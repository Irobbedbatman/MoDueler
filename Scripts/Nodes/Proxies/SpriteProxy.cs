using Godot;
using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoDueler.Nodes {
    public class SpriteProxy : Node2DProxy<Sprite> {

        [MoonSharpHidden]
        public SpriteProxy(Sprite realSprite) : base(realSprite) { }

        public Vector2 Size => RealNode.GetRect().Size;

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
