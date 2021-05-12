using Godot;
using MoDueler.Resources;
using MoonSharp.Interpreter;
using System;

namespace MoDueler.Nodes {

    /// <summary>
    /// The <see cref="Node"/> that is added to players hands to be moved around played.
    /// </summary>
    [MoonSharpUserData]
    public class HandCard : InteractableArea, IComparable<HandCard> {

        /// <summary>
        /// The z index of the cards. They need to display above most elements.
        /// </summary>
        public static readonly int CARD_Z_INDEX = 500;

        /// <summary>
        /// THe controller of the this card. Used in pressed and released event for picking up and playing.
        /// </summary>
        public HandController controller;

        /// <summary>
        /// The <see cref="Sprite"/> that is displaying the card.
        /// </summary>
        public Sprite Renderer { get; private set; }
        /// <summary>
        /// The <see cref="CollisionShape2D"/> that determines if a card is being hovered or not.
        /// </summary>
        public CollisionShape2D Collider { get; private set; }

        public HandCard() {
            // Provide the default behaviour for clicking.
            OnPressed = () => { controller?.SelectCard(this); };
            OnReleased = (hovered) => { controller?.DeselectCard(); };
        }

        /// <summary>
        /// If we want to sort the cards in the hand we can change the way they are compared.
        /// </summary>
        public int CompareTo(HandCard other) {
            // TODO: Hand Card CompareTo for hand sorting.
            return 0;
        }

        /// <summary>
        /// Sets a uniform on the shader with the given name to the provided value.
        /// </summary>
        public void SetShaderParam(string paramName, object value) {
            (Renderer.Material as ShaderMaterial).SetShaderParam(paramName, value);
        }

        /// <summary>
        /// Creates a new card.
        /// </summary>
        /// <param name="cardIndex">The unqiue index of the card and consiquently the name of the card too.</param>
        /// <param name="border">The image that defines the shape and clickable area of the card.</param>
        /// <param name="shader">The shader we use to render the details of the card.</param>
        /// <returns></returns>
        public static HandCard CreateNewHandCard(string cardIndex, Image border, Shader shader) {
            // Create a new sprite. FullRect is used here but dynamic cards could be considrerd in the future.
            var sprite = SpriteCreator.CreateAreaSpriteFullRect<HandCard>(cardIndex, border, ResourceFiles.MaterialFromShader(shader));
            // Set the zindex of the card to the constant.
            sprite.ZIndex = CARD_Z_INDEX;
            // Get the child nodes that are defined as the collider and renderer.
            sprite.Collider = sprite.GetNode(cardIndex + "Collider") as CollisionShape2D;
            sprite.Renderer = sprite.GetNode(cardIndex + "Renderer") as Sprite;
            return sprite;
        }


    }

}
