using Godot;
using Godot.Collections;
using MoDueler.Camera;
using System;

namespace MoDueler.Scenes {
    public class SceneManager : RefreshableViewport {


        private Vector2 Screen = new Vector2(1920, 1080);

        private static SceneManager Instance;

        private Node currentScene;

        public static readonly Dictionary<string, Node> StoredScenes = new Dictionary<string, Node>();

        public override void _Ready() {
            base._Ready();
            Instance = this;
            currentScene = GetNode<Node>("SplashScreen");
            StoredScenes.Add("Home", currentScene);
        }

        public static void ChangeScene(Node newScene) {
            Instance.ChangeSceneHidden(newScene);
        }

        private void ChangeSceneHidden(Node newScene) {
            GD.Print(currentScene);
            if (currentScene != null)
                RemoveChild(currentScene);
            if (newScene != null)
                AddChild(newScene);
            currentScene = newScene;
        }

        public override void Refresh() {
            Vector2 rate = Vector2.One / (Size / Screen);
            float rateMax = Mathf.Max(rate.x, rate.y);
            CameraPointer.Instance.Zoom = new Vector2(rateMax, rateMax);
        }
    }
}
