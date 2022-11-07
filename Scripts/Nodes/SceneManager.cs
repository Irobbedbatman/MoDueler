using Godot;
using Godot.Collections;
using System;

namespace MoDueler.Nodes {
    public class SceneManager : Viewport {

        private static SceneManager Instance;

        private Node currentScene;

        public static readonly Dictionary<string, Node> StoredScenes = new Dictionary<string, Node>();

        public override void _Ready() {
            Instance = this;

            // Apply any stored or default settings.
            GlobalSettings.ApplySettings();

            PackedScene scene = GD.Load<PackedScene>("res://Scenes/SplashScreen.tscn");
            var node = scene.Instance();

            ChangeScene(node);

            GD.Print("SceneManger _Ready PrintTree");
            PrintTreePretty();

            //StoredScenes.Add("Home", currentScene);
        }

        public static void ChangeScene(Node newScene) {
            Instance.ChangeSceneHidden(newScene);
        }

        private void ChangeSceneHidden(Node newScene) {

            // TODO: Change scene non-destuctive.

            GD.Print("Change Scene Current Scene: " + currentScene);
            if (currentScene != null) {
                RemoveChild(currentScene);
                currentScene.QueueFree();
            }
            if (newScene != null)
                AddChild(newScene);
            currentScene = newScene;
        }

        public void Refresh() {
            //Vector2 rate = Vector2.One / (Size / Screen);
            //float rateMax = Mathf.Max(rate.x, rate.y);
            //CameraPointer.Instance.Zoom = new Vector2(rateMax, rateMax);
        }


        public static Node LoadScene(string sceneName) {

            PackedScene scene = GD.Load<PackedScene>(System.IO.Path.Combine("res://Scenes/" + sceneName + ".tscn"));
            var node = scene.Instance();
            return node;

        }
    }
}
