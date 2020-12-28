using Godot;
using MoDueler.Resources;
using System;
using System.Collections.Generic;

namespace MoDueler.Nodes {

    /// <summary>
    /// Tools for creating sprites and polygons from images.
    /// </summary>
    public static class SpriteCreator {

        /// <summary>
        /// Creates a simple <see cref="Sprite"/> using a <see cref="Texture"/>.
        /// </summary>
        /// <param name="name">The name the <see cref="Sprite"/> <see cref="Node"/> will have.</param>
        /// <param name="tex">The <see cref="Texture"/> to apply to the sprite.</param>
        /// <param name="shader">The <see cref="Shader"/> to apply to sprite. Uses the default <see cref="ShaderMaterial"/> if not provided.</param>
        public static Sprite CreateSprite(string name, Texture tex, Shader shader = null) {
            return new Sprite() {
                Name = name,
                Texture = tex,
                Material = ResourceFiles.MaterialFromShader(shader)
            };
        }

        /// <summary>
        /// Creates a simple <see cref="Sprite"/> by creating it's <see cref="Texture"/> using a provided <see cref="Image"/>.
        /// </summary>
        /// <param name="name">The name the <see cref="Sprite"/> <see cref="Node"/> will have.</param>
        /// <param name="image">The image the <see cref="Texture"/> to apply to the sprite is created from.</param>
        /// <param name="shader">The <see cref="Shader"/> to apply to sprite. Uses the default <see cref="ShaderMaterial"/> if not provided.</param>
        public static Sprite CreateSprite(string name, Image image, Shader shader = null) {
            return new Sprite() {
                Name = name,
                Texture = ResourceFiles.TexFromImage(image),
                Material = ResourceFiles.MaterialFromShader(shader)
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
        /// <param name="name">The name the area <see cref="Node"/> will have.</param>
        /// <param name="img">The img for the rendering components and also determines the rect size.</param>
        /// <param name="mat">A material can be provided for shader effects. Only applies to the renderer portion; doesn't affect the collider.</param>
        /// <returns>A new <see cref="Node"/> of type <typeparamref name="T"/> wuth a renderer and collider for each polygon.</returns>
        public static T CreateAreaSprite<T>(string name, Image img, Material mat = null) where T : Area2D, new() {

            // Create a new obj from the provided type.
            T obj = new T {
                Name = name
            };

            // The img paramater is required. We can still return the area and we display an error.
            if (img == null) {
                GD.PrintErr("Failure in: " + nameof(CreateAreaSprite) + " image not provided and is required. Name provided: " + name);
                return obj;
            }

            // Get the polygons from the image,
            var polys = GetPolygonPoints(img);
            // Get a new texutre from the image.
            var tex = ResourceFiles.TexFromImage(img);

            // Keep track of the number of polys so each child has a unique name.
            int count = 0;
            foreach (var poly in polys) {
                // Create a new collider for the polygon.
                CollisionPolygon2D collider = new CollisionPolygon2D {
                    Polygon = poly,
                    Name = name + "Collider" + count
                };
                // Add the collider to the object.
                obj.AddChild(collider);

                // Center the new collider.
                collider.Position += new Vector2(-tex.GetWidth() / 2, -tex.GetHeight() / 2);

                // Create a new render for the polygon.
                Polygon2D renderer = new Polygon2D {
                    Polygon = poly,
                    Texture = tex,
                    Name = name + "Renderer" + count,
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
        /// Creates a sprite that is actually a physics object that holds a sprite and a collider.
        /// <para>The area's hitbox will be simply the size of the image provided.</para>
        /// </summary>
        /// <typeparam name="T">A type that derives from <see cref="Area2D"/> typically used in physics calculations.</typeparam>
        /// <param name="name">The name the area <see cref="Node"/> will have.</param>
        /// <param name="img">The img for the rendering components and also determines the rect size.</param>
        /// <param name="mat">A material can be provided for shader effects. Only applies to the renderer portion; doesn't affect the collider.</param>
        /// <returns>A new <see cref="Node"/> of type <typeparamref name="T"/> and 2 children: one the collider and the other the sprite used as a renderer.</returns>
        public static T CreateAreaSpriteFullRect<T>(string name, Image img, Material mat = null) where T : Area2D, new() {


            // Create a new obj from the provided type.
            T obj = new T {
                Name = name
            };

            // The img paramater is required. We can still return the area and we display an error.
            if (img == null) {
                GD.PrintErr("Failure in: " + nameof(CreateAreaSpriteFullRect) + " image not provided and is required. Name provided: " + name);
                return obj;
            }

            // Create a rectagle to define the rect.
            var shape = new RectangleShape2D() {
                Extents = new Vector2(img.GetWidth() / 2f, img.GetHeight() / 2f)
            };

            // Using the shape we created we can now create a collider.
            CollisionShape2D collider = new CollisionShape2D {
                Shape = shape,
                Name = name + "Collider"
            };

            // Create a sprite as a renderer.
            Sprite renderer = new Sprite {
                Name = name + "Renderer",
                Material = mat,
                Texture = ResourceFiles.TexFromImage(img)
            };

            // Add the child components.
            obj.AddChild(collider);
            obj.AddChild(renderer);
            return obj;
        }


    }
}