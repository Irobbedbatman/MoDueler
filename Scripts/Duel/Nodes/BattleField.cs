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
        /// Keep a track of creatures and their zones for easier and faster access.
        /// </summary>
        private readonly Dictionary<Sprite, int> ZoneFromCreature = new Dictionary<Sprite, int>();

        /// <summary>
        /// 1/4 of the height of the bf image. This is used to position creatures.
        /// </summary>
        private float quarterBFHeight = 0;
        /// <summary>
        /// The width of the bf image. This is used to determine which zone a point or cursor is in.
        /// </summary>
        private float bfWidth = 0;

        public BattleField() {
            Name = nameof(BattleField);
        }

        public override void _Ready() {
            CreateZones();
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
            ZoneFromCreature.Add(sprite, index);
        }

        /// <summary>
        /// Removes a sprite from the zone it's in and any other refrences lying about.
        /// </summary>
        /// <param name="sprite">The sprite to kill off.</param>
        public void KillSprite(Sprite sprite) {
            var zone = ZoneFromCreature[sprite];
            Zones[zone % 5].RemoveChild(sprite);
            ZoneFromCreature.Remove(sprite);
        }

        /// <summary>
        /// Removes a sprite from a given slow position by calling the other <see cref="KillSprite(Sprite)"/>
        /// </summary>
        /// <param name="position"></param>
        public void KillSprite(int position) {
            // TODO: Kill sprite of position currently searches all the zones and is therfore slow.
            foreach (var pair in ZoneFromCreature) {
                if (pair.Value == position) {
                    KillSprite(pair.Key);
                }
            }
        }

        /// <summary>
        /// Creates the battlefield zone sprites and add's them as children and to the <see cref="Zones"/> dictionary.
        /// </summary>
        private void CreateZones() {

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

            // Create all 5 zones.
            for (int i = 0; i < 5; i++) {
                var sprite = SpriteCreator.CreateAreaSpriteFullRect<Area2D>("Slot" + i + "", bf, mat);
                // Repositions the zone based of index.
                sprite.Position += new Vector2((i - 2) * tex.GetWidth(), 0);
                Zones[i] = sprite;
                AddChild(sprite);
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