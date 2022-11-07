using System.Collections.Generic;

namespace MoDueler.Network {

    [MoonSharp.Interpreter.MoonSharpUserData]
    public class NetworkLinker {

        /// <summary>
        /// Provides linking a unique int to a object (usually a node) across the network.
        /// </summary>
        private readonly Dictionary<int, object> References = new Dictionary<int, object>();

        /// <summary>
        /// Provides the inverse of <see cref="References"/> getting the unique identifier of a object if it has one.
        /// </summary>
        private readonly Dictionary<object, int> InverseReferences = new Dictionary<object, int>();

        /// <summary>
        /// Adds a refrence and by extension it's inverse.
        /// </summary>
        public void Reference(int id, object obj) {

            // Object value is required.
            if (obj == null)
                return;

            // TODO: Consider reference overwriting.
            if (References.ContainsKey(id)) {
                Godot.GD.Print("Linking an object to an already present id. [" + id + "]");
                return;
            }

            References.Add(id, obj);
            InverseReferences.Add(obj, id);
        }

        /// <summary>
        /// Calls <see cref="Dictionary{TKey, TValue}.TryGetValue(TKey, out TValue)"/> on the refrences.
        /// </summary>
        public bool TryGetReference(int id, out object obj) => References.TryGetValue(id, out obj);

        /// <summary>
        /// Calls <see cref="Dictionary{TKey, TValue}.TryGetValue(TKey, out TValue)"/> on the inverse refrences.
        /// </summary>
        public bool TryGetID(object obj, out int id) => InverseReferences.TryGetValue(obj, out id);

        /// <summary>
        /// Returns a refrence if it exist otherwise it returns null.
        /// </summary>
        public object GetReference(int id) {
            if (TryGetReference(id, out var result))
                return result;
            else
                return null;
        }

        /// <summary>
        /// Returns a inverse refrence if it exist otherwise it returns null.
        /// </summary>
        public int? GetID(object obj) {
            if (TryGetID(obj, out var result))
                return result;
            else
                return null;
        }

        /// <summary>
        /// Removes the refrence par for the given id and it's linked object.
        /// </summary>
        public void DereferenceID(int id) {
            var obj = GetReference(id);
            References.Remove(id);
            InverseReferences.Remove(obj);

        }

        /// <summary>
        /// Removes the refrence par for the given object and it's linked id.
        /// </summary>
        public void Dereference(object obj) {
            var id = GetID(obj);
            if (id == null)
                return;
            References.Remove(id.Value);
            InverseReferences.Remove(obj);

        }

    }
}
