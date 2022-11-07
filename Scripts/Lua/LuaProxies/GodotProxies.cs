using Godot;
using MoDueler.Nodes;
using MoonSharp.Interpreter;
using System;

namespace MoDueler.Lua.Proxies {
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

            public Vector2 Size => RealImage.GetSize();


            // TODO: Resize image affects globally loaded images; this can affect certain properties.
            public void Resize(int x, int y, Image.Interpolation interpolation = Image.Interpolation.Bilinear) => RealImage.Resize(x, y, interpolation);

            public void Resize(Vector2 size, Image.Interpolation interpolation = Image.Interpolation.Bilinear) {
                RealImage.Resize((int)size.x, (int)size.y, interpolation);
            }


            /// <summary>
            /// Uses a realative scale multipler to <see cref="Resize(int, int, Image.Interpolation)"/> the <see cref="Image"/>.
            /// <para>Returns a floored value when rounding.</para>
            /// </summary>
            public void Rescale(Vector2 scale, Image.Interpolation interpolation = Image.Interpolation.Bilinear) {
                Vector2 res = Size * scale;
                RealImage.Resize(Mathf.FloorToInt(res.x), Mathf.FloorToInt(res.x), interpolation);
            }

            [MoonSharpHidden]
            public ImageProxy(Image realImage) {
                RealImage = realImage;
            }
        }


    }
}
