using System.Collections.Generic;
using Godot;
using MoonSharp.Interpreter;

namespace MoDueler.Resources {

    /// <summary>
    /// Provides resources for loading and creating fonts.
    /// <para>Also can be used non-statically for a more feature housed <see cref="DynamicFont"/>.</para>
    /// </summary>
    [MoonSharpUserData]
    public class FontResource : DynamicFont {

        /// <summary>
        /// Dictionary that maps already loaded font data to the key that looked for them.
        /// </summary>
        public static Dictionary<string, DynamicFontData> LoadedFontData = new Dictionary<string, DynamicFontData>();

        /// <summary>
        /// Dictionary that maps fonts to a fixed key so they dont have to be recreated if that font already exists.
        /// </summary>
        public static Dictionary<string, FontResource> SharedFonts = new Dictionary<string, FontResource>();

        /// <summary>
        /// Loads a font data with a given key or loads the font with the <see cref="GlobalSettings.FallbackFontName"/>.
        /// </summary>
        public static DynamicFontData LoadFontData(string fontKey) {
            // Tries to see if data already exists and if it does returns it.
            if (LoadedFontData.ContainsKey(fontKey))
                return LoadedFontData[fontKey];
            // Try to find the file with the given key.
            var path = ResourceFiles.FindFile(fontKey);
            // If the path wasn't found we use the fallback.
            if (path == null)
                path = ResourceFiles.FindFile(GlobalSettings.FallbackFontName);
            // Load the data at the found path.
            DynamicFontData fontData = ResourceFiles.LoadFile(path) as DynamicFontData;
            // Apply hinting and antialiasing to the font data.
            fontData.Hinting = DynamicFontData.HintingEnum.None;
            fontData.Antialiased = true;
            // Adds the data to a dictionary so it isn't loaded or searched for again.
            LoadedFontData.Add(fontKey, fontData);
            return fontData;
        }

        /// <summary>
        /// Gets a new font with the provided paramaters.
        /// <para>This font wont be added to <see cref="SharedFonts"/>.</para>
        /// <para>Use this for fonts that are to be manipulated and can't be shared.</para>
        /// </summary>
        /// <param name="fontKey">The substring of a <see cref="DynamicFontData"/> file.</param>
        public static FontResource GetNewFont(string fontKey, int size, Color color) => new FontResource {
            FontData = LoadFontData(fontKey),
            Size = size,
            Color = color,
            UseFilter = true,
            UseMipmaps = true
        };

        /// <summary>
        /// Gets or finds a font with the provided paramaters or sharekey.
        /// <para>If a new is created it will be added to <see cref="SharedFonts"/>.</para>
        /// <para>Use this for fonts that are not going to be manipulated OR will be fine with being manipulated together.</para>
        /// </summary>
        /// <param name="shareKey">The key that is looked for in <see cref="SharedFonts"/> and the key to use upon adding newly created ones.</param>
        /// <param name="fontKey">The substring of a <see cref="DynamicFontData"/> file.</param>
        public static FontResource GetSharedFont(string shareKey, string fontKey, int size, Color color) {
            if (SharedFonts.TryGetValue(shareKey, out var font))
                return font;          
            FontResource fnt = new FontResource {
                FontData = LoadFontData(fontKey),
                Size = size,
                Color = color,
                UseFilter = true,
                UseMipmaps = true
            };
            SharedFonts.Add(shareKey, fnt);

            return fnt;
        }
        
        /// <summary>
        /// Provides get/set methods for the font color.
        /// </summary>
        public Color Color {
            get { return (Color)Get("custom_colors/font_color"); }
            set { Set("custom_colors/font_color", value); }
        }

    }
}
