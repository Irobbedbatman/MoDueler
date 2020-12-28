using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoDueler.Duel {

    /// <summary>
    /// The server implementation of a creature currently on the field.
    /// </summary>
    public class SimplifiedCreature : SimplifiedCard {
        public SimplifiedCreature(string cardiId, SimplifiedPlayer owner) : base(cardiId, owner) { }
    }
}
