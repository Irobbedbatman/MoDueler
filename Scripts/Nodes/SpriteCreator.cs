using Godot;
using MoDueler.Resources;
using System.Collections.Generic;

namespace MoDueler.Nodes {

    /// <summary>
    /// Tools for creating sprites and polygons from images.
    /// </summary>
    public static class SpriteCreator {

        /// <summary>
        /// Creates a new <see cref="Sprite"/> without needed the correct argument types.
        /// <para>Useful for use in lua as varaibles are typeless.</para>
        /// </summary>
        /// <param name="name">The name of the <see cref="Sprite"/> in the tree.</param>
        /// <param name="texOrImg">The <see cref="Texture"/> the sprite will use. An <see cref="Image"/> can be supplied instead and the texture will be created using <see cref="ResourceFiles.TexFromImage(Image)"/>.</param>
        /// <param name="matOrShader">The <see cref="Material"/> the sprite will use. A <see cref="Shader"/> can be supplied instead and the texture will be created material using <see cref="ResourceFiles.MaterialFromShader(Shader)"/>.</param>
        public static Sprite CreateSprite(string name, object texOrImg = null, object matOrShader = null) {
            ResourceFiles.TextureAndMaterialAdapter(texOrImg, matOrShader, out var texture, out var material);
            return new Sprite() {
                Name = name,
                Texture = texture,
                Material = material
            };
        }

        /// <summary>
        /// Get's the polygon points from a <see cref="Image"/> by using it's <see cref="BitMap"/>.
        /// </summary>
        public static Vector2[][] GetPolygonPoints(Image image) {
            // Get the bitmap from the image.
            var bMap = new BitMap();
            bMap.CreateFromImageAlpha(image);

            // Use Godot insane polygon tool for bitmaps. Unforunalty it does return a godot array and needs to be converted.
            var polys = bMap.OpaqueToPolygons(new Rect2(0, 0, bMap.GetSize()));

            //TODO: See if there is a better way of utilizing the Array in GetPolygonPoints();

            // Create a list for storing the positions outside of the godot array.
            List<Vector2[]> polylist = new List<Vector2[]>();

            // Simply move them to the new list.
            foreach (var poly in polys)
                polylist.Add(poly as Vector2[]);

            // Return the list as a clean array.
            return polylist.ToArray();
        }

        /// <summary>
        /// Creates a sprite that is actually a physics object that holds sprites and a colliders.
        /// <para>The area's hitbox will be created using <see cref="GetPolygonPoints(Image)"/>.</para>
        /// <para>There is one renderer and one collider added per polygon.</para>
        /// </summary>
        /// <typeparam name="T">A type that derives from <see cref="Area2D"/> typically used in physics calculations.</typeparam>
        /// <param name="name">The name the created <see cref="Area2D"/> will have.</param>
        /// <param name="image">The <see cref="Image"/> for the rendering components.</param>
        /// <param name="mat">A material can be provided for shader effects. Only applies to the renderer portion; doesn't affect the collider.</param>
        /// <returns>A new <see cref="Area2D"/> of type <typeparamref name="T"/> wuth a renderer and collider for each polygon.</returns>
        public static T CreateAreaSprite<T>(string name, Image image, Material mat) where T : Area2D, new() {

            // Create a new obj from the provided type.
            T obj = new T { Name = name };

            // The img paramater is required. We can still return the area and we display an error.
            if (image == null) {
                GD.PrintErr("Failure in: " + nameof(CreateAreaSprite) + " image not provided and is required. Name provided: " + name);
                return obj;
            }

            // TODO: Cache Polygon points with an image.

            // Get the polygons from the image,
            var polys = GetPolygonPoints(image);
            // Get a new texutre from the image.
            var tex = ResourceFiles.TexFromImage(image);

            // Keep track of the number of polys so each child has a unique name.
            int count = 0;
            foreach (var poly in polys) {
                // Create a new collider for the polygon.
                var collider = new CollisionPolygon2D {
                    Polygon = poly,
                    Name =  "Collider" + ((polys.Length > 1) ? count.ToString() : "")
                };
                // Add the collider to the object.
                obj.AddChild(collider);

                // Center the new collider.
                collider.Position += new Vector2(-tex.GetWidth() / 2, -tex.GetHeight() / 2);

                // Create a new render for the polygon.
                var renderer = new Polygon2D {
                    Polygon = poly,
                    Texture = tex,
                    Name =  "Renderer" + ((polys.Length > 1) ? count.ToString() : ""),
                    Material = mat
                };
                // Add the renderer to the object.
                obj.AddChild(renderer);
                // Center the new renderer.
                renderer.Position += new Vector2(-tex.GetWidth() / 2, -tex.GetHeight() / 2);
                ++count;
            }

            return obj;
        }

        /// <summary>
        /// Creates a sprite that is actually a physics object that holds sprites and a colliders without specific types for the <see cref="Material"/> argument.
        /// <para>The area's hitbox will be created using <see cref="GetPolygonPoints(Image)"/>.</para>
        /// <para>There is one renderer and one collider added per polygon.</para>
        /// </summary>
        /// <typeparam name="T">A type that derives from <see cref="Area2D"/> typically used in physics calculations.</typeparam>
        /// <param name="name">The name the created <see cref="Area2D"/> will have.</param>
        /// <param name="image">The <see cref="Image"/> for the rendering components.</param>
        /// <param name="matOrShader">The <see cref="Material"/> the sprite will use. A <see cref="Shader"/> can be supplied instead and the texture will be created material using <see cref="ResourceFiles.MaterialFromShader(Shader)"/>.</param>
        /// <returns>A new <see cref="Area2D"/> of type <typeparamref name="T"/> wuth a renderer and collider for each polygon.</returns>
        public static T CreateAreaSprite<T>(string name, Image image, object matOrShader) where T : Area2D, new() {
            Material mat = ResourceFiles.MaterialAdapter(matOrShader);
            return CreateAreaSprite<T>(name, image, mat);
        }

        /// <summary>
        /// Creates a sprite that is actually a physics object that holds a sprite and a collider.
        /// <para>The area's hitbox will be created from <paramref name="extents"/>.</para>
        /// </summary>
        /// <typeparam name="T">A type that derives from <see cref="Area2D"/> typically used in physics calculations.</typeparam>
        /// <param name="name">The name the created <see cref="Area2D"/> will have.</param>
        /// <param name="tex">The texture for the rendering components.</param>
        /// <param name="extents">The half size of the hitbox.</param>
        /// <param name="mat">A material can be provided for shader effects. Only applies to the renderer portion; doesn't affect the collider.</param>
        /// <returns>A new <see cref="Area2D"/> of type <typeparamref name="T"/> and 2 children: one the collider and the other the <see cref="Sprite"/> used as a renderer.</returns>
        public static T CreateRectAreaSprite<T>(string name, Texture tex, Material mat, Vector2 extents) where T : Area2D, new() {
            // Create a new obj from the provided type.
            T obj = new T { Name = name };
            // Using the shape we created we can now create a collider.
            var collider = new CollisionShape2D {
                // Create a rectagle to define the rect.
                Shape = new RectangleShape2D() {
                    Extents = extents
                },
                Name = "Collider"
            };
            // Create a sprite as a renderer.
            var renderer = new Sprite {
                Name = "Renderer",
                Material = mat,
                Texture = tex
            };
            // Add the child components.
            obj.AddChild(collider);
            obj.AddChild(renderer);
            return obj;
        }

        /// <summary>
        /// Creates a sprite that is actually a physics object that holds a sprite and a collider without needing the correct argument types.
        /// <para>The area's hitbox will be created from <paramref name="extents"/>.</para>
        /// <para><paramref name="texOrImg"/> and <paramref name="matOrShader"/> use the adapter pattern.</para>
        /// </summary>
        /// <typeparam name="T">A type that derives from <see cref="Area2D"/> typically used in physics calculations.</typeparam>
        /// <param name="name">The name the created <see cref="Area2D"/> will have.</param>
        /// <param name="texOrImg">The <see cref="Texture"/> the sprite will use. An <see cref="Image"/> can be supplied instead and the texture will be created using <see cref="ResourceFiles.TexFromImage(Image)"/>.</param>
        /// <param name="matOrShader">The <see cref="Material"/> the sprite will use. A <see cref="Shader"/> can be supplied instead and the texture will be created material using <see cref="ResourceFiles.MaterialFromShader(Shader)"/>.</param>
        /// <param name="extents">The half size of the hitbox.</param>
        /// <returns>A new <see cref="Area2D"/> of type <typeparamref name="T"/> and 2 children: one the collider and the other the <see cref="Sprite"/> used as a renderer.</returns>
        public static T CreateRectAreaSprite<T>(string name, object texOrImg, object matOrShader, Vector2 extents) where T : Area2D, new() {
            ResourceFiles.TextureAndMaterialAdapter(texOrImg, matOrShader, out var texture, out var material);
            return CreateRectAreaSprite<T>(name, texture, material, extents);
        }

        /// <summary>
        /// Creates a sprite that is actually a physics object that holds a sprite and a collider without needing the correct argument types.
        /// <para>The area's hitbox will be created from the <see cref="Image"/> or <see cref="Texture"/> provided in <paramref name="texOrImg"/>.</para>
        /// <para><paramref name="texOrImg"/> and <paramref name="matOrShader"/> use the adapter pattern.</para>
        /// </summary>
        /// <typeparam name="T">A type that derives from <see cref="Area2D"/> typically used in physics calculations.</typeparam>
        /// <param name="name">The name the created <see cref="Area2D"/> will have.</param>
        /// <param name="texOrImg">The <see cref="Texture"/> the sprite will use. An <see cref="Image"/> can be supplied instead and the texture will be created using <see cref="ResourceFiles.TexFromImage(Image)"/>.</param>
        /// <param name="matOrShader">The <see cref="Material"/> the sprite will use. A <see cref="Shader"/> can be supplied instead and the texture will be created material using <see cref="ResourceFiles.MaterialFromShader(Shader)"/>.</param>
        /// <returns>A new <see cref="Area2D"/> of type <typeparamref name="T"/> and 2 children: one the collider and the other the <see cref="Sprite"/> used as a renderer.</returns>
        public static T CreateRectAreaSprite<T>(string name, object texOrImg, object matOrShader) where T : Area2D, new() {
            Vector2 extents = default;
            if (texOrImg is Texture tex)
                extents = tex.GetSize();
            else if (texOrImg is Image img)
                extents = img.GetSize() / 2f;
            ResourceFiles.TextureAndMaterialAdapter(texOrImg, matOrShader, out var texture, out var material);
            return CreateRectAreaSprite<T>(name, texture, material, extents);
        }

    }
}