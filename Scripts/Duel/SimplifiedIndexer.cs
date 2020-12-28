using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoDueler.Duel {

    /// <summary>
    /// A tool used to make all child classes have a unqiue Index property. Also stores all of them in <see cref="Indices"/> for fast and easy accessing.
    /// </summary>
    public abstract class SimplifiedIndexer {

        /// <summary>
        /// The unqiue number given to this object. Can be used to get this object from <see cref="Indices"/>.
        /// 
        /// </summary>
        public readonly uint Index;

        /// <summary>
        /// How many <see cref="Index"/> values have been created so they continue being unique.
        /// </summary>
        private static uint IndexCount = 0;

        /// <summary>
        /// A dictionary that provides simple access to objects that inherit this class.
        /// </summary>
        public static readonly Dictionary<uint, SimplifiedIndexer> Indices = new Dictionary<uint, SimplifiedIndexer>();

        public SimplifiedIndexer() {
            //  Get the next unique index.
            Index = IndexCount++;
            // Add the new object to the Indices dict.
            Indices.Add(Index, this);
        }
    
    }
}
