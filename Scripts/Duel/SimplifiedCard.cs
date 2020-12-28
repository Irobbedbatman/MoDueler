using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoDueler.Duel {

    /// <summary>
    /// Server side representaion of a card.
    /// </summary>
    public class SimplifiedCard : SimplifiedIndexer {

        /// <summary>
        /// The id of the card; that is used to get the name, image and default parameters of the card.
        /// </summary>
        public readonly string CardID = "";

        /// <summary>
        /// The player that owns this card and when this card is added to the grave it will be sent to that player's grave.
        /// </summary>
        public readonly SimplifiedPlayer Owner;

        /// <summary>
        /// How mnay hits a creature made from this card could take.
        /// </summary>
        public int CurrentHp = 6;
        /// <summary>
        /// How much hp is removed from other creatures of heroes it attacks.
        /// </summary>
        public int CurrentAtk = 1;

        /// <summary>
        /// Summoning Sickness; if true it can't attack this turn and Sickness will be set to false for next turn.
        /// </summary>
        public bool Sickness = true;

        public SimplifiedCard(string cardiId, SimplifiedPlayer owner) {
            CardID = cardiId;
            Owner = owner;
        }

    }
}
