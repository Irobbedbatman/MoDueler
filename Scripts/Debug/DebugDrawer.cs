using Godot;
using MoDueler.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoDueler.Debug {

    /// <summary>
    /// A node topmost on the screen so that debug information can be seen no matter what.
    /// </summary>
    public class DebugDrawer : Control {

        /// <summary>
        /// The last or only creation of a <see cref="DebugDrawer"/>.
        /// </summary>
        public static DebugDrawer Instance;

        /// <summary>
        /// What needs drawing on the next draw frame.
        /// <para>Cleared after drawing.</para>
        /// </summary>
        public EventHandler DrawEvents;

        /// <summary>
        /// The font used for display any text debug information.
        /// </summary>
        private FontResource font;

        public override void _Ready() {
            Instance = this;
            // Set the z index as high as possible.
            VisualServer.CanvasItemSetZIndex(this.GetCanvasItem(), 4096);
            // Create the font.
            font = FontResource.GetNewFont(GlobalSettings.FallbackFontName, 18, Colors.ConstantColors.White);

        }

        public override void _Process(float delta) {
            // We need to always call the draw call.
            Update();
        }

        public override void _Draw() {

            // TODO: Ensure draw events are each still valid.

            // Draw something behind the text but in front of the scene for contrast.
            DrawPanel();
            DrawFPSCounter();
            DrawHovered();
            // If anything else has requested a draw event we can invoke them now.
            DrawEvents?.Invoke(this, null);
            // Draw events are only executed once before we clear them.
            DrawEvents = null;
        }

        /// <summary>
        /// Draws a rect to provide some contrast for <see cref="DrawFPSCounter"/> and <see cref="DrawHovered"/>.
        /// </summary>
        private void DrawPanel() {
            var size = GetViewport().Size;
            Vector2 pos = new Vector2(0, size.y);
            DrawRect(new Rect2(pos, new Vector2(240, -80)), new Color(0f, 0f, 0f, .75f));
            // Draw a border around the rect so it doesn't clash as much.
            DrawRect(new Rect2(pos, new Vector2(240, -80)), new Color(1f, 1f, 1f, .75f), filled:false, width:3);
        }

        /// <summary>
        /// Display the current frame rate.
        /// </summary>
        public void DrawFPSCounter() {
            var size = GetViewport().Size;
            Vector2 pos = new Vector2(8, size.y - 17);
            DrawString(font, pos, "FPS: " + Engine.GetFramesPerSecond());
        }

        /// <summary>
        /// Displays the currently hovered object by the <see cref="Camera.CameraPointer"/>.
        /// </summary>
        public void DrawHovered() {
            var size = GetViewport().Size;
            Vector2 pos = new Vector2(8, size.y - 35);
            DrawString(font, pos, "Hovered: " + Camera.CameraPointer.Instance?.TopHovered?.Name ?? "-");
        }

        /// <summary>
        /// Gets the centre of the screen manually.
        /// <para> As this control is unaffected by the scene camera.</para>
        /// </summary>
        public Vector2 DrawOffset => GetViewport().Size / 2f;


    }
}
