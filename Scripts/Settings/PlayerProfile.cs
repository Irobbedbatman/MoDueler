using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MoDuel.Mana;
using Newtonsoft.Json.Linq;

namespace MoDueler {
    public static class PlayerProfile {

        public static string UserId = "PlayerUserId";

        public static object Deck;
        public static object CardCollection;

        public static string CurrentHeroId = "Mai";

        public static string[] DeckCardNames;

        public static ManaType[] Manas;


        public static void ApplyProfile(JObject settings) {

            if (settings.TryGetValue("UserId", out var userId)) {
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
            }

        }


    }
}
