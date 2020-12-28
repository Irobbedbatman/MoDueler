using Godot;
using System.Collections.Generic;

namespace MoDueler.Resources {

    /// <summary>
    /// An <see cref="Image"/> that is comprised of smaller images and the information to retrieve them.
    /// <para>Only splits the image when required to save memory.</para>
    /// </summary>
    public class ImageMap : Image {

        /// <summary>
        /// The set of keys used to split the map and the rect that define each subimage.
        /// </summary>
        private readonly Dictionary<string, Rect2> _keyLines = new Dictionary<string, Rect2>();

        /// <summary>
        /// The sub-<see cref="Image"/>s that have been loaded. So we can create them on a need by need basis.
        /// TODO: Consider WeakRefrences.
        /// </summary>
        private readonly Dictionary<string, Image> _loadedSubImages = new Dictionary<string, Image>();

        public ImageMap() { }

        public ImageMap(string[] keylines) {

            // Get the data from each keyline.
            foreach (var key in keylines) {
                string[] keyparts = key.Split(',');
                string name = keyparts[0];
                // Get Rect co-ords.
                int x = int.Parse(keyparts[1]);
                int y = int.Parse(keyparts[2]);
                int w = int.Parse(keyparts[3]);
                int h = int.Parse(keyparts[4]);
                _keyLines.Add(name, new Rect2(x, y, w, h));
            }

        }

        /// <summary>
        /// Indexer for shorthand accesibilty. Simply calls <see cref="GetSubImage(string)"/>
        /// </summary>
        public Image this[string key] => GetSubImage(key);

        /// <summary>
        /// Get's a sub image with the given key. Loading it if it wasn't loaded yet.
        /// <para>Returns <c>null</c> if the key is invalid.</para>
        /// </summary>
        public Image GetSubImage(string key) {
            if (_loadedSubImages.TryGetValue(key, out var img))
                return img;
            else
                return LoadSubImage(key);
        }

        /// <summary>
        /// Get's the region of a sub image. useful if you simply want it for a UV without creating a completly new image.
        /// <para>Returns <c>null</c> if the key is invalid.</para>
        /// </summary>
        public Rect2? GetRect(string key) {
            if (_keyLines.TryGetValue(key, out Rect2 rect))
                return rect;
            else
                return null;
        }

        /// <summary>
        /// Get's the sub image using the provided key, adds it to <see cref="_loadedSubImages"/> and returns the subimage.
        /// <para>Returns <c>null</c> if the key doesn't exist.</para>
        /// </summary>
        private Image LoadSubImage(string key) {
            // Ensure the key is valid.
            if (!_keyLines.ContainsKey(key))
                return null;
            var img = ResourceFiles.SubImage(this, _keyLines[key]);
            _loadedSubImages.Add(key, img);
            return img;

        }
    }
}
