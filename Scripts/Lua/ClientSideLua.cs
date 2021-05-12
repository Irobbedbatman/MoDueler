using Godot;
using MoDuel.Cards;
using MoDueler.Nodes;
using MoDueler.Resources;
using MoonSharp.Environment;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop.RegistrationPolicies;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MoDueler.Lua.Proxies.GodotProxies;

namespace MoDueler.Lua {

    /// <summary>
    /// Registers everything we need for Lua operation. Should be run at the start of program.
    /// </summary>
    public class ClientSideLua {

        private static void SetupLuaRegisterTypes() {

            // Register Proxies
            UserData.RegisterProxyType<ShaderProxy, Shader>(s => new ShaderProxy(s));
            UserData.RegisterProxyType<ImageProxy, Image>(s => new ImageProxy(s));
            UserData.RegisterProxyType<SpriteProxy, Sprite>(s => new SpriteProxy(s));
            UserData.RegisterType<Texture>();
            UserData.RegisterType<ShaderMaterial>();
            UserData.RegisterType<Material>();
            UserData.RegisterType<Vector2>();
            UserData.RegisterType<Color>();
            UserData.RegisterType<Node2D>();

            UserData.RegisterType<JToken>();
            UserData.RegisterType<JArray>();
            UserData.RegisterType<JContainer>();

            // Register Type Convertors
            Proxies.DelegateProxies.RegisterActionConverter();

            // Register Assemblies
            UserData.RegisterAssembly(includeExtensionTypes: true);
            UserData.RegisterAssembly(System.Reflection.Assembly.GetAssembly(typeof(Card)));

        }

        public static LuaEnvironment Environment;

        private static void SetupLuaGlobals() {

            // Register Global Functions
            Environment.AsScript.Options.DebugPrint = (s) => { GD.Print(s); };
            Environment.AsScript.Globals["Print"] = (Action<object>)((obj) => { GD.Print(obj); });
            //  -   Object Construction.
            Environment.AsScript.Globals["Vec2"] = (Func<float, float, Vector2>)((x, y) => { return new Vector2(x, y); });
            Environment.AsScript.Globals["Color"] = (Func<float, float, float, float, Color>)Colors.ColorCreation.CreateColor;
            //  -   Element Creation
            Environment.AsScript.Globals["CreateHandCard"] = (Func<string, Image, Shader, HandCard>)HandCard.CreateNewHandCard;
            Environment.AsScript.Globals["CreateSprite"] = (Func<string, Image, Shader, Sprite>)SpriteCreator.CreateSprite;
            Environment.AsScript.Globals["CreateSpriteTex"] = (Func<string, Texture, Shader, Sprite>)SpriteCreator.CreateSprite;
            Environment.AsScript.Globals["CreateLabel"] = (Func<string, string, FontResource, Vector2, NodeRichTextLabel>)NodeRichTextLabel.CreateLabel;
            //  -   Transfomations
            //Environment.AsScript.Globals["Translate"] = (Action<Node2D, Vector2, bool>)TransformExtensions.Translate;
            //Environment.AsScript.Globals["Scale"] = (Action<Node2D, Vector2, bool>)TransformExtensions.Scale;
            //Environment.AsScript.Globals["Rotate"] = (Action<Node2D, float, bool>)TransformExtensions.Rotate;
            //  - Loading And Resources
            Environment.AsScript.Globals["LoadFile"] = (Func<string, object>)ResourceFiles.LoadFile;
            Environment.AsScript.Globals["LoadImage"] = (Func<string, Image>)ResourceFiles.LoadImage;
            Environment.AsScript.Globals["ImgToTex"] = (Func<Image, Texture>)ResourceFiles.TexFromImage;
            Environment.AsScript.Globals["LoadJson"] = (Func<string, JsonResource>)((k) => { return new JsonResource(k); });
            //      - Fonts
            Environment.AsScript.Globals["GetNewFont"] = (Func<string, int, Color, FontResource>)FontResource.GetNewFont;
            Environment.AsScript.Globals["GetSharedFont"] = (Func<string, string, int, Color, FontResource>)FontResource.GetSharedFont;
            //      - Language
            Environment.AsScript.Globals["GetLanguageCode"] = (Func<string>)(() => { return "en"; });
            //      - Audio
            Environment.AsScript.Globals["Audio"] = Audio.AudioController.Instance;





        }

        public static void SetupLua() {
            Environment = new LuaEnvironment();
            SetupLuaRegisterTypes();
            SetupLuaGlobals();
        }



    }
}
