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

                // Fallback image stored in local res directory.
                _fallBackImage = LoadImage("res://FileNotFoundFallback.png");
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
        /// <para>Returns <c>null</c> if no file was found.</para>
        /// </summary>
        public static string FindFile(string name) {
            // If the file is found within the game itself it requires the full path.
            if (name.StartsWith("res://"))
                return name;
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
        /// <para>To create a unique texture use <see cref="NewTexFromImage(Image)"/>.</para>
        /// </summary>
        public static Texture TexFromImage(Image image) {

            // Cant create a texture from nothing.
            if (image == null)
                return null;

            // If a texture has alrady been created for this image return that one.
            if (CreatedTextures.ContainsKey(image))
                return CreatedTextures[image];

            var tex = new ImageTexture();

            // TODO: Optional Texture Flags

            tex.CreateFromImage(image, (uint)(Texture.FlagsEnum.AnisotropicFilter | Texture.FlagsEnum.Mipmaps));

            CreatedTextures.Add(image, tex);

            return tex;
        }

        /// <summary>
        /// Constructs a <see cref="Texture"/> from the provided <see cref="Image"/>.
        /// <para>Doesn't use <see cref="CreatedTextures"/> so each Texture created is unique.</para>
        /// </summary>
        public static Texture NewTexFromImage(Image image) {
            var tex = new ImageTexture();
            // If a image wasn't provided we return a blank texture.
            if (image == null)
                return tex; 
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

        /// <summary>
        /// Adapter to allow <see cref="Texture"/>s and <see cref="Image"/>s as the same paramter.
        /// <para>Convertes provided <see cref="Image"/>s to <see cref="Texture"/>s using <see cref="TexFromImage(Image)"/>.</para>
        /// </summary>
        /// <param name="texOrImg">A texture or an image to be converted to a texture.</param>
        /// <returns>The result of <paramref name="texOrImg"/>.</returns>
        public static Texture TextureAdapter(object texOrImg) {
            if (texOrImg is Texture tex)
                return tex;
            else if (texOrImg is Image img)
                return TexFromImage(img);
            return null;
        }

        /// <summary>
        /// Adapter to allow <see cref="Material"/>s and <see cref="Shader"/>s as the same paramter.
        /// <para>Convertes provided <see cref="Material"/>s to <see cref="Shader"/>s using <see cref="MaterialFromShader(Shader)"/>.</para>
        /// </summary>
        /// <param name="matOrShader">A material or a shader to be converted to a material.</param>
        /// <returns>The result of <paramref name="matOrShader"/>.</returns>
        public static Material MaterialAdapter(object matOrShader) {
            if (matOrShader is Material mat)
                return mat;
            else if (matOrShader is Shader shader)
                return MaterialFromShader(shader);
            return null;
        }

        /// <summary>
        /// Adapter to allow texture and images together and/or materials and shader together.
        /// <para>Providing an <see cref="Image"/> in <paramref name="texOrImg"/> will be converted to <see cref="Texture"/>s.</para>
        /// <para>Providing a <see cref="Shader"/> in <paramref name="matOrShader"/> will be converted to <see cref="ShaderMaterial"/>s.</para>
        /// </summary>
        /// <param name="texOrImg">A texture or an image to be converted to a texture.</param>
        /// <param name="matOrShader">A material or a shader to be converted to a material.</param>
        /// <param name="texture">The result of <paramref name="texOrImg"/>.</param>
        /// <param name="material">The result fo <paramref name="matOrShader"/>.</param>
        public static void TextureAndMaterialAdapter(object texOrImg, object matOrShader, out Texture texture, out Material material) {
            texture = TextureAdapter(texOrImg);
            material = MaterialAdapter(matOrShader);
        }
    }
}
