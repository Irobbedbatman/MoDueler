using Godot;
using MoDuel.Cards;
using MoDueler.Nodes;
using MoDueler.Resources;
using MoonSharp.Environment;
using MoonSharp.Interpreter;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using static MoDueler.Lua.Proxies.GodotProxies;

namespace MoDueler.Lua {

    /// <summary>
    /// Registers everything we need for Lua operation. Should be run at the start of program.
    /// </summary>
    public class ClientSideLua {


        private static void SetupLuaRegisterTypes() {

            // Register Proxies
            //UserData.RegisterProxyType<ShaderProxy, Shader>(s => new ShaderProxy(s));
            UserData.RegisterProxyType<ImageProxy, Image>(s => new ImageProxy(s));

            UserData.RegisterProxyType<SpriteProxy, Sprite>(s => new SpriteProxy(s));
            UserData.RegisterProxyType<Node2DProxy<Node2D>, Node2D>(s => new Node2DProxy<Node2D>(s));
            UserData.RegisterProxyType<DeletableNodeProxy<Node>, Node>(s => new DeletableNodeProxy<Node>(s));
            UserData.RegisterProxyType<Area2DProxy<Area2D>, Area2D>(s => new Area2DProxy<Area2D>(s));
            UserData.RegisterProxyType<Polygon2DProxy<Polygon2D>, Polygon2D>(s => new Polygon2DProxy<Polygon2D>(s));
            UserData.RegisterProxyType<InteractableAreaProxy<InteractableArea>, InteractableArea>(s => new InteractableAreaProxy<InteractableArea>(s));
            UserData.RegisterProxyType<LabelProxy,NodeRichTextLabel>(s => new LabelProxy(s));


            UserData.RegisterType<Texture>();
            UserData.RegisterType<Shader>();
            UserData.RegisterType<ShaderMaterial>();
            UserData.RegisterType<Material>();
            UserData.RegisterType<Vector2>();
            UserData.RegisterType<Color>();


            UserData.RegisterType<JToken>();
            UserData.RegisterType<JArray>();
            UserData.RegisterType<JContainer>();
           

            // Register Type Convertors
            Proxies.DelegateProxies.RegisterActionConverter();

            // Register Assemblies
            UserData.RegisterAssembly(includeExtensionTypes: true);
            UserData.RegisterAssembly(System.Reflection.Assembly.GetAssembly(typeof(Card)));

        }

        public static readonly LuaEnvironment Environment = SetupLua();

        private static void SetupLuaGlobals(LuaEnvironment environment) {

            // Register Global Functions
            environment.AsScript.Options.DebugPrint = (s) => { GD.Print(s); };
            environment.AsScript.Globals["Print"] = (Action<object>)((obj) => { GD.Print(obj); });

            environment.AsScript.Globals["PrintTableKeys"] = (Action<Table>)((t) => {
                if (t != null) foreach (var key in t.Keys) GD.Print(key);
            });

            environment.AsScript.Globals["PrintTable"] = (Action<Table>)PrintTable;

                environment.AsScript.Globals["IsInstanceValid"] = (Func<Godot.Object, bool>)((n) => Godot.Object.IsInstanceValid(n));
            //  -   Object Construction.
            environment.AsScript.Globals["Vec2"] = (Func<float, float, Vector2>)((x, y) => { return new Vector2(x, y); });
            environment.AsScript.Globals["Color"] = (Func<float, float, float, float, Color>)ColorCreation.CreateColor;
            //  -   Element Creation
            environment.AsScript.Globals["CreateEmptyNode"] = (Func<string, Node2D>)((name) => { return new Node2D() { Name = name }; });
            environment.AsScript.Globals["CreateHandCard"] = (Func<string, Image, Shader, HandCard>)HandCard.CreateNewHandCard;

            environment.AsScript.Globals["CreateSprite"] = (Func<string, object, object, Sprite>)SpriteCreator.CreateSprite;

            environment.AsScript.Globals["CreateAreaSprite"] = (Func<string, Image, object, Area2D>)SpriteCreator.CreateAreaSprite<Area2D>;
            environment.AsScript.Globals["CreateInterSprite"] = (Func<string, Image, object, InteractableArea>)SpriteCreator.CreateAreaSprite<InteractableArea>;
            environment.AsScript.Globals["CreateRectSprite"] = (Func<string, object, object, Area2D>)SpriteCreator.CreateRectAreaSprite<Area2D>;
            environment.AsScript.Globals["CreateInterRectSprite"] = (Func<string, object, object, InteractableArea>)SpriteCreator.CreateRectAreaSprite<InteractableArea>;
       
            environment.AsScript.Globals["CreateLabel"] = (Func<string, string, FontResource, Vector2, NodeRichTextLabel>)NodeRichTextLabel.CreateLabel;

            //  - Loading And Resources
            environment.AsScript.Globals["FindFile"] = (Func<string, string>)ResourceFiles.FindFile;
            environment.AsScript.Globals["LoadFile"] = (Func<string, object>)ResourceFiles.LoadFile;
            environment.AsScript.Globals["LoadImage"] = (Func<string, Image>)ResourceFiles.LoadImage;
            environment.AsScript.Globals["ImgToTex"] = (Func<Image, Texture>)ResourceFiles.TexFromImage;
            environment.AsScript.Globals["ShaderToMat"] = (Func<Shader, Material>)ResourceFiles.MaterialFromShader;
            environment.AsScript.Globals["LoadJson"] = (Func<string, JsonResource>)((k) => { return new JsonResource(k); });
            //      - Fonts
            environment.AsScript.Globals["GetNewFont"] = (Func<string, int, Color, FontResource>)FontResource.GetNewFont;
            environment.AsScript.Globals["GetSharedFont"] = (Func<string, string, int, Color, FontResource>)FontResource.GetSharedFont;
            //      - Settings
            // TODO: Add Settings to Lua
            environment.AsScript.Globals["Settings"] = (Func<GlobalSettings>)(() => { return new GlobalSettings(); });
            // TODO: Add Player Profile to Lua
            environment.AsScript.Globals["Profile"] = ProfileAccessor.Profile;
            //      - Audio
            environment.AsScript.Globals["Audio"] = Audio.AudioController.Instance;

            // TODO: Lua Create Animations
            environment.AsScript.Globals["Animator"] = Animation.Animator.Instance;

            // Simplifier to make simple objects using a single class table.
            environment.AsScript.Globals["NewObject"] = (Func<Table, Table>)((_class) => {
                Table t = new Table(environment.AsScript);
                t.MetaTable = new Table(environment.AsScript);
                t.MetaTable["__index"] = _class;
                return t;
            });

        }

        public static void PrintTable(Table table) {
            if (table != null && table.Pairs.Count() > 0) {
                var line = new string('-', 25 * 2 + 3);
                GD.Print(line);
                foreach (var pair in table.Pairs) {

                    var str = string.Format("|{0,-24} | {1,24}|", pair.Key, pair.Value);

                    GD.Print(str);
                    GD.Print(line);
                }
            }
            else
                GD.Print("|Empty Table|");
        }


        private static LuaEnvironment SetupLua() {
            var environment = new LuaEnvironment();
            SetupLuaRegisterTypes();
            SetupLuaGlobals(environment);
            return environment;
        }

    }
}
