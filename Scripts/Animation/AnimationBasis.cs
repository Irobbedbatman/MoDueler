using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace MoDueler.Animation {

    /// <summary>
    /// The raw data from an animation file. Used to construct <see cref="Curves.AnimationCurve{T}"/>s and <see cref="AnimationInstance"/>s.
    /// <para><see cref="AnimationBasis"/>' should only be loaded once as they can be reused.</para>
    /// </summary>
    public struct AnimationBasis {

        //TODO: Unload animations.

        /// <summary>
        /// The already loaded basis' so that they don't need reloading.
        /// </summary>
        public static readonly Dictionary<string, AnimationBasis> LoadedBasis = new Dictionary<string, AnimationBasis>();

        /// <summary>
        /// Loads the animation from the <paramref name="filePath"/> unless it is already loaded.
        /// </summary>
        /// <param name="filePath">The location of the animation file to load.</param>
        /// <returns>The <see cref="AnimationBasis"/> at the filePath.</returns>
        public static AnimationBasis GetOrCreate(string filePath) {
            if (LoadedBasis.TryGetValue(filePath, out var loaded))
                return loaded;
            var basis = CreateFromJson(filePath);
            LoadedBasis.Add(filePath, basis);
            return basis;
        }

        /// <summary>
        /// Loads and creates a <see cref="AnimationBasis"/> from the file located at <paramref name="filePath"/>.
        /// </summary>
        public static AnimationBasis CreateFromJson(string filePath) {

            // Load and parse the animation file.
            string fileText = System.IO.File.ReadAllText(filePath);
            JObject anim = JObject.Parse(fileText);

            return new AnimationBasis() {
                // The duration of the animation defaults to 0. All the animations will occur instantly.
                Duration = (double?)anim["duration"] ?? 0,
                // Wether the animation loops defaulting to false.
                Loop = (bool?)anim["loop"] ?? false,
                // The names the animation uses to get variable names. Deafults to an empty array.
                ArgumentNames = anim["args"]?.Values<string>().ToArray() ?? new string[0],
                // The name/path of any bones tha the animation will mainpulate. Deafults to an empty array.
                Bones = anim["bones"]?.Values<string>().ToArray() ?? new string[0],
                // The data for animations stored in a json token. Defaults to null.
                Animations = anim["animations"]
            };
        }

        /// <summary>
        /// The length the animation will play for.
        /// </summary>
        public double Duration { get; private set; }
        /// <summary>
        /// Wether the time is reset upon reaching it's <see cref="Duration"/>.
        /// </summary>
        public bool Loop { get; private set; }
        /// <summary>
        /// The names of arguments used in the animation file. Each should be unique.
        /// </summary>
        public string[] ArgumentNames { get; private set; }
        /// <summary>
        /// The nodepaths to nodes to manipulate with the animation.
        /// </summary>
        public string[] Bones { get; private set; }
        /// <summary>
        /// The token that stores all the animation curve values. 
        /// </summary>
        public JToken Animations { get; private set; }

    }
}
