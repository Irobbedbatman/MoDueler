using MoDuel;
using MoDuel.Tools;
using MoDuel.Data;
using MoonSharp.Environment;
using System.Threading;
using System;
using MoDuel.Mana;
using MoonSharp.Interpreter;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace MoDueler.Backend {


    /// <summary>
    /// Tools to create a <see cref="DuelFlow"/> on the client side.
    /// <para>Currently uses non-stored information on what a players have achieved.</para>
    /// </summary>
    public static class DuelFlowSetup {

        /// <summary>
        /// Runs methods that allow the duel flow to load resources.
        /// </summary>
        public static void SetupLoading() {
            ContentLoader.LogLoads = true;
            ContentLoader.SetContentDirectory(GlobalSettings.ContentDirectory);
        }

        /// <summary>
        /// Creates a new environment container using the defaults.
        /// </summary>
        public static EnvironmentContainer SetupEnvironment() {
            LoadedContent content = new LoadedContent();
            LuaEnvironment lua = new LuaEnvironment();
            ContentLoader.RegisterLoads(content, lua);
            EnvironmentContainer container = new EnvironmentContainer() {
                AnimationBlocker = new PlaybackBlockingHandler(),
                Content = content,
                Lua = lua,
                Random = new ManagedRandom(),
                Settings = new DuelSettings() {
                    ChangeTurnAction = ContentLoader.LoadAction("SYSChangeTurn", content, lua),
                    GameEndAction = ContentLoader.LoadAction("SYSGameEnd", content, lua),
                    GameStartAction = ContentLoader.LoadAction("SYSGameStart", content, lua),
                    TimeOutPlayers = false
                    //TODO: Force ID to go First from profile.
                }
            };

            // The commands have to be loaded manually.
            ContentLoader.LoadAction("CMDCharge", content, lua);
            ContentLoader.LoadAction("CMDRevive", content, lua);
            ContentLoader.LoadAction("CMDDiscard", content, lua);
            ContentLoader.LoadAction("CMDPlayCard", content, lua);
            ContentLoader.LoadAction("CMDLevelUp", content, lua);

            return container;
        }

        /// <summary>
        /// Create the duel flow.
        /// </summary>
        public static DuelFlow CreateFlow(EnvironmentContainer env, Player player1, Player player2) {
            DuelFlow flow = new DuelFlow(env, player1, player2) {
                ENABLE_DEBUG = true
            };
            return flow;
        }

        /// <summary>
        /// Makes a hero setting table from the hero that the player has.
        /// </summary>
        public static Table BlankHero(Player player) {
            Table table = DynValue.NewPrimeTable().Table;
            table["HeroId"] = player.Hero.Imprint.HeroId;
            return table;
        }

        /// <summary>
        /// Creates and returns a set of card info that is useful if specific cards have not beenm requested.
        /// </summary>
        public static Table[] BlankCards() {
            return new Table[] { BlankCard("HollowMage"), BlankCard("ArmedSoldier"), BlankCard("DarkMagGirl") };
        }


        /// <summary>
        /// Converts a set of card names to set of simplified card meta data.
        /// </summary>
        public static Table[] BlankCards(string[] cardNames) {

            return cardNames.Select((name) => { return BlankCard(name); }).ToArray();
        }


        /// <summary>
        /// Creates and returns a set of card info based on the cards the player has asked to use.
        /// </summary>
        public static Table[] BlankCardsPlayer() {
            return PlayerProfile.DeckCardNames.Select((cardName) => { return BlankCard(cardName); }).ToArray();
        }

        /// <summary>
        /// Creates an returns a set of card info based on the cards the AI was given to use.
        /// </summary>
        public static Table[] BlankAiCards() {
            if (GlobalSettings.SettingsObject.TryGetValue("AI", out var aiInfoQuery)) {
                var deck = aiInfoQuery["Deck"].Value<JArray>();
                return deck.Select((cardName) => { return BlankCard(cardName.Value<string>()); }).ToArray();
            }
            return BlankCards();
        }

        /// <summary>
        /// Creates a card info that only contains the cardId.
        /// </summary>
        public static Table BlankCard(string CardId) {
            Table table = DynValue.NewPrimeTable().Table;
            table["CardId"] = CardId;
            return table;
        }


        /// <summary>
        /// Creates the local player for use in the duel flow.
        /// </summary>
        public static Player CreatePlayer1(EnvironmentContainer env) {
            return new Player(PlayerProfile.UserId, 
                ContentLoader.LoadHero(PlayerProfile.CurrentHeroId, env.Content, env.Lua),
                new ManaPool(PlayerProfile.Manas));
        }
        
        /// <summary>
        /// Creates the advisarrial player.
        /// </summary>
        public static Player CreatePlayer2(EnvironmentContainer env) {


            if (GlobalSettings.SettingsObject.TryGetValue("AI", out var aiInfoQuery)){

                var aiInfo = aiInfoQuery.Value<JObject>();
                if (aiInfo.TryGetValue("UserID", out var userId)
                    && aiInfo.TryGetValue("HeroID", out var heroId)
                    && aiInfo.TryGetValue("Mana", out var mana)) {
                    var manatypes = mana.Value<JArray>();
                    return new Player(userId.Value<string>(),
                        ContentLoader.LoadHero(heroId.Value<string>(), env.Content, env.Lua),
                        new ManaPool(manatypes.Select((type) => new ManaType(type.Value<string>())).ToArray())
                    );
                }
                else
                    return DefaltAI(env);
            }

            return DefaltAI(env);

        }

        /// <summary>
        /// Create a fallback AI incase no settings were provided.
        /// </summary>
        private static Player DefaltAI(EnvironmentContainer env) {
            return new Player("Computer",
                ContentLoader.LoadHero("Mai", env.Content, env.Lua),
                new ManaPool(new ManaType[] {
                                new ManaType("Funds"),
                                new ManaType("Hearts"),
                                new ManaType("Souls")
            }));
        }

        /// <summary>
        /// Start the duel flow and call the setup action.
        /// <para>Disbables the expensive loading.</para>
        /// </summary>
        public static Thread StartThread(DuelFlow flow, EnvironmentContainer env, Table[] player1Cards = null, Table[] player2Cards = null) {

            // Use card defaults if none were provided.
            if (player1Cards == null) {
                player1Cards = BlankCardsPlayer();
            }
            if (player2Cards == null) {
                player2Cards = BlankAiCards();
            }

            ContentLoader.LoadAction("SYSDuelInit", env.Content, env.Lua);
            flow.DoAction("SYSDuelInit", flow.State.Player1, flow.State.Player2, BlankHero(flow.State.Player1), BlankHero(flow.State.Player2), player1Cards, player2Cards);
            ContentLoader.DeregisterLoads(env.Lua);
            Thread loop = flow.StartLoop();
            return loop;
        }

    }
}
