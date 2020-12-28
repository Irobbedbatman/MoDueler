using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoDueler.Duel {


    /// <summary>
    /// Server side representaion of a player.
    /// </summary>
    public class SimplifiedPlayer : SimplifiedIndexer {

        /// <summary>
        /// The cards curently in this <see cref="SimplifiedPlayer"/>'s hand.
        /// </summary>
        public List<SimplifiedCard> Hand = new List<SimplifiedCard>();
        /// <summary>
        /// The cards curently in this <see cref="SimplifiedPlayer"/>'s grave.
        /// </summary>
        public List<SimplifiedCard> Grave = new List<SimplifiedCard>();

        /// <summary>
        /// The health players start at. Currently just a good round number.
        /// </summary>
        private const int INITIAL_HEALTH = 20;

        /// <summary>
        /// How much health this player has remaining.
        /// </summary>
        public int Health = INITIAL_HEALTH;


        public SimplifiedPlayer() {

            // Add the initial cards to the player hand.
            Hand.Add(new SimplifiedCard("succubus", this));
            Hand.Add(new SimplifiedCard("alraune", this));
            Hand.Add(new SimplifiedCard("merchantOfDarkness", this));
            Hand.Add(new SimplifiedCard("assistantAnna", this));
            Hand.Add(new SimplifiedCard("wandererSarah", this));
            Hand.Add(new SimplifiedCard("cloLevia", this));
            Hand.Add(new SimplifiedCard("elf", this));
            Hand.Add(new SimplifiedCard("wolfKingBoro", this));
            Hand.Add(new SimplifiedCard("magicianJurh", this));
            Hand.Add(new SimplifiedCard("blacksmithFerghus", this));
            Hand.Add(new SimplifiedCard("vampireVesh", this));
        }

    }
}
