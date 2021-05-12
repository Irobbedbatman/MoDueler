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
    public static class DuelFlowSetup {

        public static void SetupLoading() {
            ContentLoader.LogLoads = true;
            ContentLoader.SetContentDirectory(GlobalSettings.ContentDirectory);
        }

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
                    TimeOutPlayers = false,
                    ForceIdToGoFirst = "Computer"
                }
            };

            ContentLoader.LoadAction("CMDCharge", content, lua);
            ContentLoader.LoadAction("CMDRevive", content, lua);
            ContentLoader.LoadAction("CMDDiscard", content, lua);
            ContentLoader.LoadAction("CMDPlayCard", content, lua);
            ContentLoader.LoadAction("CMDLevelUp", content, lua);

            return container;
        }

        public static DuelFlow FlowStart(EnvironmentContainer env, Player player1, Player player2) {
            DuelFlow flow = new DuelFlow(env, player1, player2);
            return flow;
        }

        public static Table BlankHero(Player player) {
            Table table = DynValue.NewPrimeTable().Table;
            table["HeroId"] = player.Hero.Imprint.HeroId;
            return table;
        }

        public static Table[] BlankCards() {
            return new Table[] { BlankCard("HollowMage"), BlankCard("ArmedSoldier"), BlankCard("DarkMagGirl") };
        }


        public static Table[] BlankCardsPlayer() {
            return PlayerProfile.DeckCardNames.Select((cardName) => { return BlankCard(cardName); }).ToArray();
        }

        public static Table[] BlankAiCards() {
            if (GlobalSettings.SettingsObject.TryGetValue("AI", out var aiInfoQuery)) {
                var deck = aiInfoQuery["Deck"].Value<JArray>();
                return deck.Select((cardName) => { return BlankCard(cardName.Value<string>()); }).ToArray();
            }
            return BlankCards();
        }

        public static Table BlankCard(string CardId) {
            Table table = DynValue.NewPrimeTable().Table;
            table["CardId"] = CardId;
            return table;
        }


        public static Player CreatePlayer1(EnvironmentContainer env) {
            return new Player(PlayerProfile.UserId, 
                ContentLoader.LoadHero(PlayerProfile.CurrentHeroId, env.Content, env.Lua),
                new ManaPool(PlayerProfile.Manas));
        }

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

        private static Player DefaltAI(EnvironmentContainer env) {
            return new Player("Computer",
                ContentLoader.LoadHero("Mai", env.Content, env.Lua),
                new ManaPool(new ManaType[] {
                                new ManaType("Funds"),
                                new ManaType("Hearts"),
                                new ManaType("Souls")
            }));
        }

        public static Thread StartThread(DuelFlow flow, EnvironmentContainer env, Player player1, Player player2) {

            ContentLoader.LoadAction("SYSDuelInit", env.Content, env.Lua);
            flow.DoAction("SYSDuelInit", player1, player2, BlankHero(player1), BlankHero(player2), BlankCardsPlayer(), BlankAiCards());
            ContentLoader.DeregisterLoads(env.Lua);


            Thread loop = flow.StartLoop();
            return loop;
        }

    }
}
