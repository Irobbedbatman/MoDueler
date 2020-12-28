﻿using System;
using System.Threading.Tasks;
using Godot;
using MoDueler.Colors;
using MoDueler.Nodes;
using MoDueler.Resources;

namespace MoDueler.Startup {
    
    /// <summary>
    /// The screen that first shows and only show up when the program is run.
    /// </summary>
    public class SplashScreen : Node2D {

        public override void _Ready() {

            // Apply any stored or default settings.
            SettingsSetup.ApplySettings();
            // Load anything we need from lua.
            LuaSetup.SetupLua();

            // Delay playing the BGM in case the AudioController hasn't setup yet.
            Task.Delay(1000).ContinueWith((t) => {
                Audio.AudioController.Instance.SetVolume(GlobalSettings.Volume);
                Audio.AudioController.Instance.PlayBGM("bgm_opening.mp3", true);
            });

            // Create a background.
            var shader = GD.Load("res://Shaders/Background.shader") as Shader;
            var sprite = SpriteCreator.CreateSprite("img", ResourceFiles.LoadImage("shop_background_g2.jpg"), shader);
            AddChild(sprite);

            // Create the game title.
            sprite = SpriteCreator.CreateSprite("img", image: ResourceFiles.LoadImage("@logo_1tone.en_US.png"));
            AddChild(sprite);

            // Create a new button for opening the menu.
            var btn = SpriteButton.CreateButton("Continue", ResourceFiles.LoadImage("button_common.png"));
            var lbl = NodeRichTextLabel.CreateLabel("ContinueLabel", "Continue", FontResource.GetNewFont("*ui.ttf", 28, new Color(.7f, .7f, .7f)), btn.Size);
            lbl.EmbeddedControl.VAlign((VAlign)1);
            lbl.EmbeddedControl.Center();
            btn.AddText(lbl);
            AddChild(btn);
            btn.Position += new Vector2(0, 300);
            btn.Scale = new Vector2(.75f, .75f);

            // Create a new button that will open the duel screen.
            var duelbtn = SpriteButton.CreateButton("DuelBtn", ResourceFiles.LoadImage("button_common.png"));
            var duellbl = NodeRichTextLabel.CreateLabel("DuelLbl", "Duel", FontResource.GetNewFont("*ui.ttf", 28, new Color(.7f, .7f, .7f)), btn.Size);
            duellbl.EmbeddedControl.VAlign((VAlign)1);
            duellbl.EmbeddedControl.Center();
            duelbtn.AddText(duellbl);
            AddChild(duelbtn);
            duelbtn.Position += new Vector2(0, 400);
            duelbtn.Scale = new Vector2(.75f, .75f);


            // When a button is clicked we darkenen it and play a click sound.
            Action btnClicked = () => {
                Audio.AudioController.Instance.PlaySFX("UI_click.wav");
                btn.Modulate = ConstantColors.SlightDarken;
            };

            // When the button is released we remove any darkening. If the button was released while hovered we can open the menu.
            Action<bool> btnReleased = (hovered) => {
                if (hovered) {
                    Audio.AudioController.Instance.PlaySFX("UI_click.wav");
                    Overlay.OverlayController.Instance.ShowBottomMenu();
                }
                else
                    Audio.AudioController.Instance.PlaySFX("cancel.wav");
                btn.Modulate = ConstantColors.White;             
            };

            // When a button is clicked we darkenen it and play a click sound.
            Action continueAction4 = () => {
                Audio.AudioController.Instance.PlaySFX("UI_click.wav");
                duelbtn.Modulate = ConstantColors.SlightDarken;
            };

            // When the button is released we remove any darkening. If the button was released while hovered we can goto the lobby.
            Action<bool> continueAction3 = (hovered) => {
                if (hovered) {
                    Audio.AudioController.Instance.PlaySFX("UI_click.wav");
                    Scenes.SceneManager.ChangeScene(CreateLobby());
                    Overlay.OverlayController.Instance.HideBottomMenu();
                }
                else
                    Audio.AudioController.Instance.PlaySFX("cancel.wav");
                duelbtn.Modulate = ConstantColors.White;
            };

            // Provided the buttons with their actions.
            btn.OnPressed = btnClicked;
            btn.OnReleased += btnReleased;
            duelbtn.OnPressed = continueAction4;
            duelbtn.OnReleased += continueAction3;

        }

        /// <summary>
        /// Creates a pretty barebones lobby.
        /// </summary>
        /// <returns></returns>
        public Node2D CreateLobby() {

            // Create node for the sub elements to be added to.
            var node = new Node2D() { 
                Name = "Lobby"
            };

            // Create the carosoul of scharacters that appears in the backgorund.
            var s = SpriteCreator.CreateSprite("search", ResourceFiles.LoadImage("ui_searching.png"), GD.Load<Shader>("res://Shaders/searching_circle.shader"));
            node.AddChild(s);

            // Create the big VS icon in the middle of the screen.
            s = SpriteCreator.CreateSprite("VS", ResourceFiles.LoadImage("versus.png"));
            node.AddChild(s);

            // Create a test button that will take the player into a test match vs a AI.
            var btn = SpriteButton.CreateButton("VsAI", ResourceFiles.LoadImage("button_common.png"));
            btn.Scale = new Vector2(.75f, .75f);
            // Create the label for on top the button.
            var lbl = NodeRichTextLabel.CreateLabel("DuelLbl", "Ai", FontResource.GetNewFont("*ui.ttf", 28, new Color(.7f, .7f, .7f)), btn.Size);
            lbl.EmbeddedControl.VAlign((VAlign)1);
            lbl.EmbeddedControl.Center();
            btn.AddText(lbl);
            node.AddChild(btn);

            // When a button is clicked we darkenen it and play a click sound.
            btn.OnPressed = () => {
                Audio.AudioController.Instance.PlaySFX("UI_click.wav");
                btn.Modulate = ConstantColors.SlightDarken;
            };

            // If the button is released we can remove any darkening and if it was hovered we can move to the duel scene.
            btn.OnReleased = (hovered) => {
                if (hovered) {
                    Scenes.SceneManager.ChangeScene(new DuelMaster());
                    // Play in Duel music.
                    Audio.AudioController.Instance.PlayBGM("bgm_duel.mp3", true);
                }
                btn.Modulate = ConstantColors.White;
            };

            btn.Position += new Vector2(400, 0);

            // Play Lobby Music.
            Audio.AudioController.Instance.PlayBGM("bgm_burnt.mp3", true);
            return node;

        }
    }
}
