using Godot;
using Godot.Collections;
using MoDueler.Camera;

using MoDueler.Nodes;
using MoDueler.Resources;
using System;
using System.Linq;

namespace MoDueler.Duel {
    /// <summary>
    /// The <see cref="Node2D"/> that contains all for visual field elements.
    /// <para>The creature zones are sub children of this and creature when summoned will becomes children of these zones. </para>
    /// </summary>
    public class BattleField : Node2D {

        /// <summary>
        /// Keep a track of zones and their position for easier and faster access.
        /// </summary>
        private readonly Dictionary<int, Node2D> Zones = new Dictionary<int, Node2D>();

        /// <summary>
        /// 1/4 of the height of the bf image. This is used to position creatures.
        /// </summary>
        private float quarterBFHeight = 0;
        /// <summary>
        /// The width of the bf image. This is used to determine which zone a point or cursor is in.
        /// </summary>
        private float bfWidth = 0;


        public System.Collections.Generic.Dictionary<int, int> IndexMap { get; private set; } = new System.Collections.Generic.Dictionary<int, int>();
        public System.Collections.Generic.Dictionary<int, int> InvertedIndexMap { get; private set; } = new System.Collections.Generic.Dictionary<int, int>();

        public BattleField() {
            Name = nameof(BattleField);
        }

        public static BattleField CreateBattleField(bool flipped, int subfield1Index, int subfield2Index, System.Collections.Generic.Dictionary<int, int> mapping) {
            var Inverted = new System.Collections.Generic.Dictionary<int, int>();
            foreach (var pair in mapping) {
                Inverted.Add(pair.Value, pair.Key);
            }
            
            var bf = new BattleField {
                //TODO: SubFields.
                IndexMap = mapping,
                InvertedIndexMap = Inverted
            };
            bf.CreateZones(flipped, mapping);
            return bf;
        }


        /// <summary>
        /// Moves a sprite to a new zone remove an old parent if it had one.
        /// </summary>
        /// <param name="sprite">The sprite that is being moved.</param>
        /// <param name="index">The new index of the sprite.</param>
        public void SpriteToZone(Sprite sprite, int index) {
            if (sprite.GetParent() != null)
                // To remove a parent we have to tell the parent to remove the child.
                sprite.GetParent().RemoveChild(sprite);
            // As there are only 5 zones we can ignore which row the sprite is moving to.
            var xcount = index % 5;
            Zones[xcount].AddChild(sprite);
            // But then we use the row information to determine where it sits in the zone.
            var y = index >= 5 ? -quarterBFHeight : quarterBFHeight;
            sprite.Position = new Vector2(0, y);
        }

        /// <summary>
        /// Removes a sprite from the zone it's in and any other refrences lying about.
        /// </summary>
        /// <param name="sprite">The sprite to kill off.</param>
        public void KillSprite(Sprite sprite) {
            sprite.QueueFree();
        }

        /// <summary>
        /// Creates the battlefield zone sprites and add's them as children and to the <see cref="Zones"/> dictionary.
        /// </summary>
        private void CreateZones(bool flipped, System.Collections.Generic.Dictionary<int, int> mapping) {

            // Find the bf map and is sub image whcih the zones will use.
            var bf = ResourceFiles.LoadImage("@bf_images.png:battlefield.png");
            // Record some of the sizing details of the image.
            quarterBFHeight = bf.GetSize().y / 4f;
            bfWidth = bf.GetSize().x;
            // Create a texture from the image.
            var tex = ResourceFiles.TexFromImage(bf);
            // Get the default shader and create a material out of it.
            Shader shader = GD.Load<Shader>("res://Shaders/Default.shader");
            ShaderMaterial mat = new ShaderMaterial {
                Shader = shader
            };
            // Make the material somewhat tranparent.
            mat.SetShaderParam("color_main", new Color(1, 1, 1, .7f));

            // Create all 5 columns with 2 zones each..
            for (int i = 0; i < 5; i++) {
                var sprite = SpriteCreator.CreateSprite("Slot" + i + "", bf, shader);
                // Repositions the zone based of index.
                sprite.Position += new Vector2((i - 2) * tex.GetWidth(), 0);
                Zones[i] = sprite;
                AddChild(sprite);


                CollisionShape2D lowCollider = new CollisionShape2D() {
                    Shape = new RectangleShape2D() {
                        Extents = new Vector2(bf.GetWidth() / 2f, quarterBFHeight)
                    },
                    Name = "Collider"
                };

                CollisionShape2D highCollider = new CollisionShape2D() {
                    Shape = new RectangleShape2D() {
                        Extents = new Vector2(bf.GetWidth() / 2f, bf.GetHeight() / 4f)
                    },
                    Name = "Collider"
                };

                // The positon is offset by one because it was created in lua.
                int position = i + 1;
                // If the local player is player 2 the battlefield indices are flipped.
                if (flipped)
                    position += 5;
                // Handle wrapping.
                if (position > 10)
                    position -= 10;
                // Get the index from the mapping using the calculated position.
                int index = mapping[position];

                Area2D low = new Area2D() { 
                    Name = "fieldslot" + index,
                    ZIndex = 1,
                    Position = new Vector2(0, quarterBFHeight),
                    ZAsRelative = true
                };

                // Do the other halve of the field. The same as before but we add 5 to start the next slots.
                position += 5;
                if (position > 10)
                    position -= 10;
                index = mapping[position];

                Area2D high = new Area2D() {
                    Name = "fieldslot" + index,
                    ZIndex = 1,
                    Position = new Vector2(0, -quarterBFHeight),
                    ZAsRelative = true
                };

                // Add the colliders.
                low.AddChild(lowCollider);
                high.AddChild(highCollider);
                // Add the areas to the scene.
                sprite.AddChild(low);
                sprite.AddChild(high);

            }
        }

        /// <summary>
        /// Gets a slot index from a given point in the 2d space.
        /// </summary>
        /// <param name="position">The position to be converted to a slot.</param>
        /// <returns></returns>
        public int GetSlot(Vector2 position) {
            // Get the slot number in the x plane.
            float x = (position.x - GlobalPosition.x) / bfWidth + 2.5f;
            // If the position is lower than the middle it is on the second row and needs to be incremented as such.
            if (position.y > Position.y)
                return (int)x;
            return (int)x + 5;
        }

    }
}