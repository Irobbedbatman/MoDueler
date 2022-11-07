using Godot;
using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoDueler.Nodes {

    /// <summary>
    /// <see cref="NodeProxy{T}"/> that applies to <see cref="Node2D"/> and makes important features public.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [MoonSharpUserData]
    public class Node2DProxy<T> : DeletableNodeProxy<T> where T : Node2D {

        [MoonSharpHidden]
        public Node2DProxy(T node) : base(node) { }

        public Vector2 Position {
            get => RealNode.Position;
            set => RealNode.Position = value;
        }

        public void Translate(Vector2 offset) {
            Position += offset;
        }

        public Vector2 GlobalPosition {
            get => RealNode.GlobalPosition;
            set => RealNode.GlobalPosition = value;
        }

        public Vector2 Scale {
            get => RealNode.Scale;
            set => RealNode.Scale = value;
        }

        public Vector2 GlobalScale {
            get => RealNode.GlobalScale;
            set => RealNode.GlobalScale = value;
        }

        public float Rotation {
            get => RealNode.Rotation;
            set => RealNode.Rotation = value;
        }

        public void Rotate(float angle) {
            RealNode.Rotation += angle;
        }

        public float GlobalRotation {
            get => RealNode.GlobalRotation;
            set => RealNode.GlobalRotation = value;
        }

        public bool ZAsRelative {
            get => RealNode.ZAsRelative;
            set => RealNode.ZAsRelative = value;
        }

        public int ZIndex {
            get => RealNode.ZIndex;
            set => RealNode.ZIndex = value;
        }

        public bool Visible {
            get => RealNode.Visible;
            set => RealNode.Visible = value;
        }

    }
}
