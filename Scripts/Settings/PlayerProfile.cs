using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MoDuel.Mana;
using Newtonsoft.Json.Linq;

namespace MoDueler {

    public static class PlayerProfile {

        // The user id this player has chosen.
        public static string UserId = "PlayerUserId";

        // The deck of cards this player currently has active.
        public static object Deck;
        // The list of all cards the player has.
        public static object CardCollection;
        // The hero the player is using.
        public static string CurrentHeroId = "Godette";
        // TODO: Use deck instead of just names.
        public static string[] DeckCardNames;
        // TODO: Derive mana from deck.
        public static ManaType[] Manas;
        public static string[] ManaNames;

        /// <summary>
        /// Load profile settings from JSON.
        /// </summary>
        public static void ApplyProfile(JObject settings) {

            if (settings.TryGetValue("UserID", out var userId)) {
                UserId = userId.Value<string>();
            }

            if (settings.TryGetValue("HeroId", out var heroId)) {
                CurrentHeroId = heroId.Value<string>();
            }

            if (settings.TryGetValue("Deck", out var deck)) {
                JArray cards = deck.Value<JArray>();
                DeckCardNames = cards.Select((card) => { return card.Value<string>(); }).ToArray();
            }

            if (settings.TryGetValue("Mana", out var mana)) {
                JArray manas = mana.Value<JArray>();
                Manas = manas.Select((m) => { return new ManaType(m.Value<string>()); }).ToArray();
                ManaNames = manas.Select((m) => { return m.Value<string>(); }).ToArray();
            }

        }


    }
}
