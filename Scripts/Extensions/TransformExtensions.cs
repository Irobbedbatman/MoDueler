using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MoDueler.Extensions {
    public static class TransformExtensions {

        public static void Translate(this Node2D node, Vector2 vect, bool relative = true) {
            node.Position = relative ? node.Position + vect : vect;
        }

        public static void Rotate(this Node2D node, float radAngle, bool relative = true) {
            if (relative)
                node.Rotate(radAngle);
            else
                node.Rotation = radAngle;
        }

        public static void Scale(this Node2D node, Vector2 vect, bool relative = true) {
            if (relative)
                node.ApplyScale(vect);
            else
                node.Scale = vect;
        }

        public static void SetParent(this Node2D node, Node2D parent) {
            parent.AddChild(node);
        }

        public static Node2D GetChild(this Node2D node, string childName) {
            return node.GetNode(childName) as Node2D;
        }

        public static IEnumerable<Node2D> GetChildren(this Node2D node) {
            return node.GetChildren().Cast<Node2D>();
        }



    }
}
