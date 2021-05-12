using Godot;

namespace MoDueler.Nodes {
    public static class Transformations {

        public static void Translate(Node node, Vector2 pos, bool relative = true) {

            if (node is Node2D node2d) {
                if (relative)
                    node2d.Position += pos;
                else
                    node2d.Position = pos;
            }
            else if (node is Control control) {
                if (relative)
                    control.RectPosition += pos;
                else
                    control.RectPosition = pos;
            }
        }

        public static void Scale(Node node, Vector2 scale, bool relative = true) {
            if (node is Node2D node2d) {
                if (relative)
                    node2d.Scale *= scale;
                else
                    node2d.Scale = scale;
            }
            else if (node is Control control) {
                if (relative)
                    control.RectScale *= scale;
                else
                    control.RectScale = scale;
            }
        }

        public static void Rotate(Node node, float theta, bool relative = true) {
            if (node is Node2D node2d) {
                if (relative)
                    node2d.Rotate(theta);
                else
                    node2d.Rotation = theta;
            }
            else if (node is Control control) {
                if (relative)
                    control.RectRotation += theta;
                else
                    control.RectRotation = theta;
            }
        }

    }
}
