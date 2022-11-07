using System;
using System.Threading.Tasks;
using Godot;
using MoDueler.Resources;
using MoDueler.Lua;
using MoDueler.Backend;
using MoDueler.Nodes;

namespace MoDueler.Nodes {

    /// <summary>
    /// The screen that appears when the application is opened. Set in the Godot engine.
    /// </summary>
    public class SplashScreenMaster : Control {

        public override void _Ready() {

            // TODO: Actual Splash Screen.
            // TODO: Preloading.

            // Apply any stored or default settings.
            GlobalSettings.ApplySettings();
            // Update the audio controllers volume based on the loaded settings.
            Audio.AudioController.Instance.SetVolume(GlobalSettings.Volume);

            // The size of the lobby button.
            var rectSize = new Vector2(200, 69);

            // Create a button to the lobby.
            Button toDuel = new Button {
                RectSize = rectSize,
                RectPosition = (GetViewport().Size / 2f) - (rectSize/2f),
                Text = "Lobby"
            };

            toDuel.Connect("pressed", this, "MoveToLobby");

            AddChild(toDuel);
        }

        /// <summary>
        /// Changes the scene to the lobby.
        /// </summary>
        public void MoveToLobby() {
            SceneManager.ChangeScene(SceneManager.LoadScene("Lobby"));
        }

    }
}
