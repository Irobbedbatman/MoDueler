using Godot;
using System;
using MoDueler.Resources;
using MoDueler.Audio;

namespace MoDueler.Nodes.Overlay {
    public class OverlayController : Viewport {

        public static OverlayController Instance;

        BottomMenuController BottomMenu;

        public override void _Ready() {
            Instance = this;

            //BottomMenu = new BottomMenuController();
            //BottomMenu.Setup();
            //AddChild(BottomMenu);
            //HideBottomMenu(false);
        }

        public void HideBottomMenu(bool playSound = true) {
            if (!BottomMenu.Visible)
                return;
            if (playSound)
            AudioController.Instance.PlaySFX("UI_panel_show.wav");
            BottomMenu.Visible = false;
        }

        public void ShowBottomMenu(bool playSound = true) {
            if (BottomMenu.Visible)
                return;
            if (playSound)
                AudioController.Instance.PlaySFX("UI_panel_hide.wav");
            BottomMenu.Visible = true;
            //Vector2 rate = Vector2.One / (Size / Screen);
            //BottomMenu.Refresh(Size, rate);
        }

        /*
        public override void Refresh() {

            Vector2 rate = Vector2.One / (Size / Screen);
            BottomMenu.Refresh(Size, rate);
        }
        */

    }
}
