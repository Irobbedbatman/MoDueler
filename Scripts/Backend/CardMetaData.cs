using MoDuel.Cards;
using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoDueler.Backend {

    /// <summary>
    /// The information stored locally for representing hand cards and creatures from data provided from the server.
    /// </summary>
    [MoonSharpUserData]
    public class CardMetaData {

        /// <summary>
        /// The lua code name used to create a visual instance of this data.
        /// </summary>        
        public string CardBase;

        /// <summary>
        /// The identifier for every object on the server.
        /// </summary>
        public uint Index;

        /// <summary>
        /// Teh many this card uses.
        /// </summary>
        public string ManaType;

        /// <summary>
        /// The identifier for each unqiue card.
        /// </summary>
        public string CardId;

        /// <summary>
        /// Is the base Card of this card a creature and should use card_is_creature shader param.
        /// </summary>
        public bool IsCreature = true;
        /// <summary>
        /// Is this opponent's card's details (ID, Atk, Hp etc...) revealed yet.
        /// </summary>
        public bool known;

        /// <summary>
        /// The visual instance of this data. Could be a HandCard or a sprite.
        /// //TODO: Not yet implemented.
        /// </summary>
        public object Instance = null;

        public CardMetaData(uint index, string cardBase, string cardID, string manaType) {
            Index = index;
            CardBase = cardBase;
            CardId = cardID;
            ManaType = manaType;
        }


    }
}
