using Godot;
using MoDueler.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoDueler.Nodes {

    /// <summary>
    /// Wrapper around <see cref="AdjustedRichTextLabel"/> to have node like behaviour.
    /// <para>Also includes a helper creation tool in <see cref="CreateLabel(string, string, FontResource, Vector2)"/></para>
    /// </summary>
    [MoonSharp.Interpreter.MoonSharpUserData]
    public class NodeRichTextLabel : ControlContainterNode<AdjustedRichTextLabel> {

        /// <summary>
        /// Flag to draw the rect the text is set in.
        /// </summary>
        public bool DrawBoundingRect = false;

        /// <summary>
        /// Sets up the <see cref="ControlContainterNode{T}.EmbeddedControl"/> with the provided paramters.
        /// </summary>
        /// <param name="name">The name this container <see cref="Node"/> will have.</param>
        /// <param name="text">The text the label will initially have.</param>
        /// <param name="font">The font that the label will use.</param>
        /// <param name="size">The size of the label for use in centering and other adjustments.</param>
        public void Setup(string name, string text, FontResource font, Vector2 size) {
            Name = name;
            EmbeddedControl.Setup(text, font, size);
        }

        /// <summary>
        /// Creates a new contained label.
        /// </summary>
        /// <param name="name">The name that the container node will use.</param>
        /// <param name="text">The initial text that label will display.</param>
        /// <param name="font">The font the label will use.</param>
        /// <param name="size">The size of the label for use in centering and other adjustments.</param>
        public static NodeRichTextLabel CreateLabel(string name, string text, FontResource font, Vector2 size) {
            // Create a simple label.
            var lbl = new NodeRichTextLabel();
            // Do setup.
            lbl.Setup(name, text, font, size);
            // Make it ignored by the cursor so nodes underneath can be clicked..
            // TODO: What if I want a clickable label?
            lbl.EmbeddedControl.MouseFilter = Control.MouseFilterEnum.Ignore;
            return lbl;
        }

        public override void _Draw() {

            // Only draw if the flag is set.
            if (!DrawBoundingRect)
                return;

            // Create a rect to better position the draw call.s
            var Rect = new Rect2 {
                Size = EmbeddedControl.RectSize,
                Position = EmbeddedControl.RectPosition - EmbeddedControl.ValignMove
            };

            // Draw the inside.
            DrawRect(Rect, new Color(1, 0, 1, .25f), true);
            // Draw an outline.
            DrawRect(Rect, new Color(1, 0, 1, 1f), false, 3);

            // Get the left and right side centers to draw a centre line.
            Vector2 left = new Vector2(Rect.Position.x, Rect.Position.y + Rect.Size.y / 2);
            Vector2 right = new Vector2(Rect.Position.x + Rect.Size.x, Rect.Position.y + Rect.Size.y / 2);

            // Draw that centre line.
            DrawLine(left, right, new Color(new Color(1, 1, 1, .5f)), 2);

            // Draw the text physical size rect.
            var RectText = new Rect2 {
                Size = EmbeddedControl.Font.GetWordwrapStringSize(EmbeddedControl.Text, Rect.Size.x),
                Position = EmbeddedControl.RectPosition
            };
            DrawRect(RectText, new Color(1, 1, 0, .25f), true);
            DrawRect(RectText, new Color(1, 1, 0, .5f), false, 3);

            // Draw the center line for the physical size rect.
            left = new Vector2(RectText.Position.x, RectText.Position.y + RectText.Size.y / 2);
            right = new Vector2(RectText.Position.x + RectText.Size.x, RectText.Position.y + RectText.Size.y / 2);
            DrawLine(left, right, new Color(new Color(0, 1, 1, .5f)), 2);

        }

    }
}
