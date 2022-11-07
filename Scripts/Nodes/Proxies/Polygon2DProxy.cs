using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;
using MoonSharp.Interpreter;

namespace MoDueler.Nodes {

    [MoonSharpUserData]
    public class Polygon2DProxy<T> : Node2DProxy<T> where T : Polygon2D {

        // TODO: Polygon2D based functionallity,

        [MoonSharpHidden]
        public Polygon2DProxy(T node) : base(node) {}

        /// <summary>
        /// Simplifeid access to the sprites shader parameters.
        /// </summary>
        public void SetShaderParam(string param, object value) => (RealNode.Material as ShaderMaterial).SetShaderParam(param, value);

        public Texture Texture {
            get => RealNode.Texture;
            set => RealNode.Texture = value;
        }

        public Color Modulate {

            get => RealNode.Modulate;
            set => RealNode.Modulate = value;

        }


    }
}
