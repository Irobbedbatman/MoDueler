using Godot;
using System;

namespace MoDueler.Nodes {

    /// <summary>
    /// A button that needs to have interactactivity added. Also contains a helper <see cref="SpriteButton"/> creator.
    /// </summary>
    public class SpriteButton : InteractableArea {

        public Vector2 Size { get; private set; }

        public SpriteButton() { }

        /// <summary>
        /// Adds a label to the button by simply adding it as a child.
        /// </summary>
        public void AddText(NodeRichTextLabel label) {
            //TODO: Button.AddText needs more cool stuff.
            AddChild(label);
        }

        /// <summary>
        /// Creates a new <see cref="SpriteButton"/>.
        /// </summary>
        /// <param name="name">The name the new <see cref="SpriteButton"/> <see cref="Node"/> will have.</param>
        /// <param name="image">The image used to display the button.</param>
        /// <param name="mat">Optional material for cooler buttons.</param>
        /// <returns></returns>
        public static SpriteButton CreateButton(string name, Image image, Material mat = null) {
            // Create an area sprite using the sprite creator and have the resulting type be a sprite button.
            var btn = SpriteCreator.CreateAreaSprite<SpriteButton>(name, image, mat);
            // Record the size of image.
            btn.Size = image.GetSize();
            return btn;

        }

    }
}
