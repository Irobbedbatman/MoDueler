using Godot;
using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoDueler.Nodes {

    [MoonSharpUserData]
    public class Area2DProxy<T> : Node2DProxy<T> where T : Area2D {

        [MoonSharpHidden]
        public Area2DProxy(T node) : base(node) { }

        public Node Renderer => GetRenderer();
        public Node Collider => GetCollider();

        // TODO: Object caching for found renderers / colliders.

        public Node GetRenderer() {
            try {
                return RealNode.GetNode("Renderer");
            }
            catch { }
            return null;
        }

        public Node GetCollider() {
            try {
                return RealNode.GetNode("Collider");
            }
            catch { }
            return null;
        }

        // TODO: Multiple renderer or colliders.

        public Node[] GetRenderers() {
            throw new NotImplementedException();
        }

        public Node[] GetColliders() {
            throw new NotImplementedException();
        }
    }
}
