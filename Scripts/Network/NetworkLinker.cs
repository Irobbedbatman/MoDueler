using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoDueler.Network {
    public class NetworkLinker {

        /// <summary>
        /// Provides linking a unique uint to a object (usually a node) across the network.
        /// </summary>
        private readonly Dictionary<int, object> Refrences = new Dictionary<int, object>();

        /// <summary>
        /// Provides the inverse of <see cref="Refrences"/> getting the unique identifier of a object if it has one.
        /// </summary>
        private readonly Dictionary<object, int> InverseRefrences = new Dictionary<object, int>();

        /// <summary>
        /// Adds a refrence and by extension it's inverse.
        /// </summary>
        public void Refrence(int id, object obj) {
            Refrences.Add(id, obj);
            InverseRefrences.Add(obj, id);
        }

        /// <summary>
        /// Calls <see cref="Dictionary{TKey, TValue}.TryGetValue(TKey, out TValue)"/> on the refrences.
        /// </summary>
        public bool TryGetRefrence(int id, out object obj) => Refrences.TryGetValue(id, out obj);

        /// <summary>
        /// Calls <see cref="Dictionary{TKey, TValue}.TryGetValue(TKey, out TValue)"/> on the inverse refrences.
        /// </summary>
        public bool TryGetID(object obj, out int id) => InverseRefrences.TryGetValue(obj, out id);

        /// <summary>
        /// Returns a refrence if it exist otherwise it return null.
        /// </summary>
        public object GetRefrence(int id) {
            if (TryGetRefrence(id, out var result))
                return result;
            else
                return null;
        }

        /// <summary>
        /// Returns a inverse refrence if it exist otherwise it return null.
        /// </summary>
        public int? GetID(object obj) {
            if (TryGetID(obj, out var result))
                return result;
            else
                return null;
        }


        public void Derefrence(object obj) {
            var id = GetID(obj);
            if (id == null)
                return;
            Refrences.Remove(id.Value);
            InverseRefrences.Remove(obj);

        }

    }
}
