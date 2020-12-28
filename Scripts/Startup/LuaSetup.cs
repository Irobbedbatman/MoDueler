using Godot;
using MoDuel.Cards;
using MoDueler.Extensions;
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
using static MoDueler.LuaProxies.GodotProxies;

namespace MoDueler.Startup {

    /// <summary>
    /// Registers everything we need for Lua operation. Should be run at the start of program.
    /// </summary>
    public class LuaSetup {

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
            LuaProxies.DelegateProxies.RegisterActionConverter();

            // Register Assemblies
            UserData.RegisterAssembly(includeExtensionTypes: true);
            UserData.RegisterAssembly(System.Reflection.Assembly.GetAssembly(typeof(Card)));

        }

        private static void SetupLuaGlobals() {

            var luaScript = LuaEnvironment.AsScript;

            // Register Global Functions
            luaScript.Options.DebugPrint = (s) => { GD.Print(s); };
            luaScript.Globals["Print"] = (Action<object>)((obj) => { GD.Print(obj); });
            //  -   Object Construction.
            luaScript.Globals["Vec2"] = (Func<float, float, Vector2>)((x, y) => { return new Vector2(x, y); });
            luaScript.Globals["Color"] = (Func<float, float, float, float, Color>)Colors.ColorCreation.CreateColor;
            //  -   Element Creation
            luaScript.Globals["CreateHandCard"] = (Func<string, Image, Shader, HandCard>)HandCard.CreateNewHandCard;
            luaScript.Globals["CreateSprite"] = (Func<string, Image, Shader, Sprite>)SpriteCreator.CreateSprite;
            luaScript.Globals["CreateSpriteTex"] = (Func<string, Texture, Shader, Sprite>)SpriteCreator.CreateSprite;
            luaScript.Globals["CreateLabel"] = (Func<string, string, FontResource, Vector2, NodeRichTextLabel>)NodeRichTextLabel.CreateLabel;
            //  -   Transfomations
            luaScript.Globals["Translate"] = (Action<Node2D, Vector2, bool>)TransformExtensions.Translate;
            luaScript.Globals["Scale"] = (Action<Node2D, Vector2, bool>)TransformExtensions.Scale;
            luaScript.Globals["Rotate"] = (Action<Node2D, float, bool>)TransformExtensions.Rotate;
            //  - Loading And Resources
            luaScript.Globals["LoadFile"] = (Func<string, object>)ResourceFiles.LoadFile;
            luaScript.Globals["LoadImage"] = (Func<string, Image>)ResourceFiles.LoadImage;
            luaScript.Globals["ImgToTex"] = (Func<Image, Texture>)ResourceFiles.TexFromImage;
            luaScript.Globals["LoadJson"] = (Func<string, JsonResource>)((k) => { return new JsonResource(k); });
            //      - Fonts
            luaScript.Globals["GetNewFont"] = (Func<string, int, Color, FontResource>)FontResource.GetNewFont;
            luaScript.Globals["GetSharedFont"] = (Func<string, string, int, Color, FontResource>)FontResource.GetSharedFont;
            //      - Language
            luaScript.Globals["GetLanguageCode"] = (Func<string>)(() => { return "en"; });
            //      - Sounds



        }

        public static void SetupLua() {
            SetupLuaRegisterTypes();
            SetupLuaGlobals();
        }



    }
}
