using Godot;
using MoonSharp.Interpreter;
using System;

namespace MoDueler.LuaProxies {
    public static class GodotProxies {

        public class ShaderProxy {

            public readonly Shader RealShader;

            [MoonSharpHidden]
            public ShaderProxy(Shader realShader) {
                RealShader = realShader;
            }
        }

        public class ImageProxy {

            public readonly Image RealImage;

            [MoonSharpHidden]
            public ImageProxy(Image realImage) {
                RealImage = realImage;
            }
        }

        public class SpriteProxy {

            public readonly Sprite RealSprite;

            [MoonSharpHidden]
            public SpriteProxy(Sprite realSprite) {
                RealSprite = realSprite;
            }

            public void SetShaderParam(string param, object value) {
                (RealSprite.Material as ShaderMaterial).SetShaderParam(param, value);
            }
        }



    }
}
