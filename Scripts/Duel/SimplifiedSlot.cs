using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoDueler.Duel {


    /// <summary>
    /// Server side representaion of a field slot.
    /// </summary>
    public class SimplifiedSlot : SimplifiedIndexer {
        
        /// <summary>
        /// The current occupant of this slot or null if the slot is empty.
        /// </summary>
        public SimplifiedCreature Creature = null;

        /// <summary>
        /// Shorthand check to see if there is no creature in this slot.
        /// </summary>
        public bool IsEmpty => Creature == null;

        /// <summary>
        /// Which position this slot is on the field.
        /// </summary>
        public readonly int SlotNumber = -1;

        public SimplifiedSlot(int slot) {
            SlotNumber = slot;
        }
        
    }
}
