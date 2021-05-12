using Godot;
using System;
using System.Collections.Generic;

namespace MoDueler.Resources {
    public static class ResourceFiles {

        /// <summary>
        /// The dictionary of currently loaded <see cref="Image"/>s so they dont need to be reloaded.
        /// </summary>
        public static Dictionary<string, WeakReference<Image>> LoadedImages = new Dictionary<string, WeakReference<Image>>();
        /// <summary>
        /// The dictionary of currently loaded <see cref="ImageMap"/>s so they dont need to be reloaded.
        /// </summary>
        public static Dictionary<string, ImageMap> LoadedImageMaps = new Dictionary<string, ImageMap>();

        /// <summary>
        /// The dictionary of created textures created by each image so that we don't recreate if we already have it.
        /// </summary>
        public static Dictionary<Image, ImageTexture> CreatedTextures = new Dictionary<Image, ImageTexture>();

        /// <summary>
        /// The image to use when a certain image can't be found.
        /// </summary>
        private static Image _fallBackImage = null;

        /// <summary>
        /// The image to use when a certain image can't be found.
        /// </summary>
        public static Image FallBackImage {
            get {
                if (_fallBackImage != null)
                    return _fallBackImage;
                var image = new Image();
                if (!TryFindFile(GlobalSettings.FallBackImageName, out var img)) {
                    _fallBackImage = new Image(); //To stop loading each time if the fallback cant be found we just give it a blank image.
                    return _fallBackImage;
                }
                var loadok = image.Load(img);
                if (loadok == Error.Ok) {
                    _fallBackImage = image;
                }
                else {
                    _fallBackImage = new Image(); //To stop loading each time if the fallback cant be found we just give it a blank image.
                }
                return _fallBackImage;
            }
        }

        /// <summary>
        /// Creates a new <see cref="Image"/> with the contents from <paramref name="img"/> at the <paramref name="rect"/> position.
        /// </summary>
        /// <param name="img">The image to get a subimage from</param>
        /// <param name="rect">The pixedl position.</param>
        /// <returns></returns>
        public static Image SubImage(Image img, Rect2 rect) {
            //TODO: Constraint checking.
            Image n = new Image();
            n.Create((int)rect.Size.x, (int)rect.Size.y, false, img.GetFormat());
            n.BlitRect(img, rect, new Vector2(0, 0));
            return n;

        }

        /// <summary>
        /// Loads a file afer finding it. Beware this cannot load images. But can load exported textures.
        /// <para>Returns <c>null</c> if the key is invalid.</para>
        /// </summary>
        public static object LoadFile(string fileKey) {
            if (TryFindFile(fileKey, out var filePath))
                return GD.Load(filePath);
            else
                return null;
        }

        /// <summary>
        /// Looks for a file with the provided name (or key). 
        /// TODO: Fasters search method and other such ideas.
        /// <para>Returen <c>null</c> if no file was found.</para>
        /// </summary>
        public static string FindFile(string name) {
            //If the file's full path is provided we don't neeed to find it. Also would crash.
            if (System.IO.Path.IsPathRooted(name))
                return name;
            var files = System.IO.Directory.GetFiles(GlobalSettings.ContentDirectory, name, System.IO.SearchOption.AllDirectories);
            // We only care about the first file found.
            return files.Length > 0 ? files[0] : null;
        }

        /// <summary>
        /// Calls <see cref="FindFile(string)"/> but returns false if the file wasn't found. Outputs the path result through <paramref name="file"/>.
        /// </summary>
        public static bool TryFindFile(string name, out string file) {
            file = FindFile(name);
            return file != null;
        }

        public static Image LoadImage(string fileRequested) {

            // Check to see if the file request is from a map.
            // File requests split the map file path and the key with a colon ':'.
            // Esnuring we skip the Drive Colon (C:/ , D:/, E:/)
            int colPos = fileRequested.Find(":", 2);


            string key = "";

            // If the file is a map we update the file name.
            if (colPos != -1) {
                // Getthe key part from the file request.
                key = fileRequested.Substring(colPos + 1);
                fileRequested = fileRequested.Substring(0, colPos);
            }

            // If the file can't be found we return the fallback image.
            if (!TryFindFile(fileRequested, out var imgPath))
                return FallBackImage;


            // If the file isn't a image map we can get the image now.
            if (colPos == -1)
                return GetImage(imgPath);

            // Otherwise we get an image map.
            var map = GetImageMap(imgPath);

            // If the map couldn't be loaded we return the fallback.
            if (map == null)
                return FallBackImage;

            // Use the key to get the part of the map we want. Of course if in the end the key isn't part of the map we return the fallback.
            return map[key] ?? FallBackImage;
        }

        /// <summary>
        /// Get's an image map from the dictionary if possible. Otherwise; loads it from the given path.
        /// </summary>
        public static ImageMap GetImageMap(string filePath) {

            if (LoadedImageMaps.TryGetValue(filePath, out var refmap)) {
                return refmap;
            }
            // If the image map hasn't been loaded yet we load it now.
            //Get the file that will contain the keys. That is a file with the same name, same folder but with ".txt" as it's last file exstension.
            string mapkeysFile = System.IO.Path.ChangeExtension(filePath, ".txt");
            //Remove @ symbols from the key file path.
            mapkeysFile = mapkeysFile.Replace("@", "");

            // If we cant find the image map keys we return null.
            if (!System.IO.File.Exists(mapkeysFile))
                return null;

            var map = new ImageMap(System.IO.File.ReadAllLines(mapkeysFile));
            var loadok = map.Load(filePath);
            if (loadok == Error.Ok) {
                // Add the image to the dictionary so we don't need to load it again.
                LoadedImageMaps.Add(filePath, map);
                return map;
            }
            else {
                // If the map couldn't be loaded we return null. We cant return FallBackImage here but we can in LoadImage.
                return null;
            }
        }

        /// <summary>
        /// Get's an image from the dictionary if possible. Otherwise; loads it from the given path.
        /// </summary>
        public static Image GetImage(string filePath) {

            if (LoadedImages.ContainsKey(filePath)) {
                if (LoadedImages[filePath].TryGetTarget(out var refimage)) {
                    return refimage;
                }
            }
            // If Image hasn't been loaded before we load it now.
            var image = new Image();
            var loadok = image.Load(filePath);
            if (loadok == Error.Ok) {
                // Add the image to the dictionary so we don't need to load it again.
                if (LoadedImages.ContainsKey(filePath))
                    LoadedImages[filePath].SetTarget(image);
                else
                    LoadedImages.Add(filePath, new WeakReference<Image>(image));
                return image;
            }
            else {
                // If there was an error we return the fallback image.
                return FallBackImage;
            }
        }


        /// <summary>
        /// Constructs a <see cref="Texture"/> from the provided <see cref="Image"/>.
        /// <para>Uses <see cref="CreatedTextures"/> to only ever create one.</para>
        /// </summary>
        public static Texture TexFromImage(Image image) {

            // Cant create a texture from nothing.
            if (image == null)
                return null;

            // If a texture has alrady been created for this image return that one.
            if (CreatedTextures.ContainsKey(image))
                return CreatedTextures[image];

            var tex = new ImageTexture();
            tex.CreateFromImage(image, (uint)(Texture.FlagsEnum.AnisotropicFilter | Texture.FlagsEnum.Default | Texture.FlagsEnum.Filter));

            CreatedTextures.Add(image, tex);

            return tex;
        }

        /// <summary>
        /// Constructs a <see cref="Texture"/> from the provided <see cref="Image"/>.
        /// <para>Doesn't use <see cref="CreatedTextures"/> so each Texture created is unique.</para>
        /// </summary>
        public static Texture NewTexFromImage(Image image) {
            var tex = new ImageTexture();
            tex.CreateFromImage(image, (uint)(Texture.FlagsEnum.AnisotropicFilter | Texture.FlagsEnum.Default | Texture.FlagsEnum.Filter));
            return tex;
        }


        /// <summary>
        /// Constructs a <see cref="BitMap"/> from the provided <see cref="Image"/>.
        /// </summary>
        public static BitMap BitmapFromImage(Image image) {
            var bMap = new BitMap();
            bMap.CreateFromImageAlpha(image);
            return bMap;
        }


        /// <summary>
        /// Constructs a new <see cref="Material"/> from the provied <see cref="Shader"/>.
        /// </summary>
        public static ShaderMaterial MaterialFromShader(Shader shader) {
            if (shader != null)
                return new ShaderMaterial() { Shader = shader };
            else
                return null;
        }
    }
}
