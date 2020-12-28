using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoDueler.Debug {
    public static class DebugDrawing {

        /// <summary>
        /// Draws a bone from point a to point b with a border for easier positional information reading.
        /// <para>uUse this if points are local to the scene.</para>
        /// </summary>
        /// <param name="width">The width of the line including the 2 pixel wide border.</param>
        public static void DrawBone(Vector2 pointa, Vector2 pointb, int width, Color innerColor, Color outerColor) {
            var instance = DebugDrawer.Instance;

            var zoom = Camera.CameraPointer.Instance.Zoom;

            instance.DrawLine(pointa / zoom + instance.DrawOffset, pointb / zoom + instance.DrawOffset, outerColor, width: width);
            instance.DrawLine(pointa / zoom + instance.DrawOffset, pointb / zoom + instance.DrawOffset, innerColor, width: width - 2);
            instance.DrawCircle(pointa / zoom + instance.DrawOffset, width + 2, outerColor);
            instance.DrawCircle(pointa / zoom + instance.DrawOffset, width + 1, innerColor);
            instance.DrawCircle(pointb / zoom + instance.DrawOffset, width + 2, outerColor);
            instance.DrawCircle(pointb / zoom + instance.DrawOffset, width + 1, innerColor);
        }

        /// <summary>
        /// Draws a bone from point a to point b with a border for easier positional information reading.
        /// <para>uUse this if points are on the overlay layer.</para>
        /// </summary>
        /// <param name="width">The width of the line including the 2 pixel wide border.</param>
        public static void DrawBoneFromOverlay(Vector2 pointa, Vector2 pointb, int width, Color innerColor, Color outerColor) {
            var instance = DebugDrawer.Instance;

            instance.DrawLine(pointa, pointb, outerColor, width: width);
            instance.DrawLine(pointa, pointb, innerColor, width: width - 2);
            instance.DrawCircle(pointa, width + 2, outerColor);
            instance.DrawCircle(pointa, width + 1, innerColor);
            instance.DrawCircle(pointb, width + 2, outerColor);
            instance.DrawCircle(pointb, width + 1, innerColor);
        }

    }
}
