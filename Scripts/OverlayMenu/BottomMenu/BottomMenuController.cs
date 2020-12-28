using Godot;
using MoDueler.Resources;
using System;
using System.Collections.Generic;

namespace MoDueler.Overlay {

    /// <summary>
    /// A menu at the bottom of the screen.
    /// </summary>
    public class BottomMenuController : Panel {

        /// <summary>
        /// The unscaled height of the menu.
        /// </summary>
        public const float MENU_HEIGHT = 60;
        /// <summary>
        /// The unscaled width of each <see cref="BottomMenuButton"/> in the menu.
        /// </summary>
        public const float BUTTON_WIDTH = 371;
        /// <summary>
        /// The fixed width of each seperator in or around the button in the menu.
        /// </summary>
        private const int SEPERATOR_WIDTH = 2;
        /// <summary>
        /// The unscaled font size for the <see cref="BottomMenuButton"/>s.
        /// </summary>
        private const int FONT_SIZE = 22;

        /// <summary>
        /// The <see cref="Texture"/> the <see cref="BottomMenuButton"/> will use when selected,
        /// </summary>
        private Texture TexSelectOn;
        /// <summary>
        /// The <see cref="Texture"/> the <see cref="BottomMenuButton"/> will use when not selected,
        /// </summary>
        private Texture TexSelectOff;

        /// <summary>
        /// The theme the <see cref="BottomMenuButton.TitleLabel"/> will use.
        /// </summary>
        private Theme LabelTheme;

        /// <summary>
        /// The created buttons in the menu.
        /// </summary>
        private readonly List<BottomMenuButton> Buttons = new List<BottomMenuButton>();
        /// <summary>
        /// The created seperators for the menu.
        /// </summary>
        private readonly List<TextureRect> Seperators = new List<TextureRect>();

        /// <summary>
        /// Setups this button. Loading resources and creating the buttons and sepertators.
        /// </summary>
        public void Setup() {
            // The menu streches across the bottom of the screen.
            SetAnchorsPreset(LayoutPreset.BottomWide);
            // Resize the hight of the menu using its top margin.
            MarginTop = -MENU_HEIGHT;
            // Rename so it's easier found in the editor.
            Name = "BottomMenu";

            // Create a background of the menu.
            TextureRect background = new TextureRect {
                // Load the background texture.
                Texture = ResourceFiles.TexFromImage(ResourceFiles.LoadImage("bottomMenu_side.png")),
                // Allow the background to expand and contract.
                Expand = true,
                // Make it so the background tiles.
                StretchMode = TextureRect.StretchModeEnum.Tile,
                Name = "Backgorund"
            };
            // Strech the backgorund bounding box to the size of the menu.
            background.SetAnchorsPreset(LayoutPreset.Wide);
            AddChild(background);

            // Load the textures for the buttons.
            TexSelectOff = ResourceFiles.TexFromImage(ResourceFiles.LoadImage("bottomMenu_nonselected_back.png"));
            TexSelectOn = ResourceFiles.TexFromImage(ResourceFiles.LoadImage("bottomMenu_selected_back.png"));
            // Load the texture for the sepertator. (Doesn't need to be stored for longer because seperators dont change.
            Texture texSeperator = ResourceFiles.TexFromImage(ResourceFiles.LoadImage("bottomMenu_separation_line.png"));
            // Load all the icons for the buttons.
            Texture iconHome = ResourceFiles.TexFromImage(ResourceFiles.LoadImage("bottom_menu.png:bottom_icon6.png"));
            Texture iconArena = ResourceFiles.TexFromImage(ResourceFiles.LoadImage("bottom_menu.png:bottom_icon2.png"));
            Texture iconBinder = ResourceFiles.TexFromImage(ResourceFiles.LoadImage("bottom_menu.png:bottom_icon4.png"));
            Texture iconStore = ResourceFiles.TexFromImage(ResourceFiles.LoadImage("bottom_menu.png:bottom_icon5.png"));
            Texture iconLab = ResourceFiles.TexFromImage(ResourceFiles.LoadImage("bottom_menu.png:bottom_icon1.png"));

            // Create a theme thats only relevant feature is the new font.
            LabelTheme = new Theme {
                DefaultFont = FontResource.GetNewFont("*ui.ttf", FONT_SIZE, new Color(1, 1, 1))
            };

            // Create all the menu buttons.
            //TODO: Localization bottom menu.
            NewButton("Home", iconHome, "Home");
            NewButton("Arena", iconArena, "Arena");
            NewButton("Binder", iconBinder, "Binder");
            NewButton("Shop", iconStore, "Shop");
            NewButton("Developer", iconLab, "Developer");


            // Create seperators. There is a new seperator left of each button and one more at the very end.
            for (int i = 0; i < Buttons.Count + 1; ++i)
                CreateSeperator(texSeperator);

            // Initalize the menu to the first button.
            ChangeScreen(Buttons[0], "Home");

        }

        /// <summary>
        /// Accessor for <see cref="Font.GetStringSize(string)"/> for the <see cref="LabelTheme"/>.
        /// </summary>
        public Vector2 GetStringSize(string text) => LabelTheme.DefaultFont.GetStringSize(text);

        /// <summary>
        /// Creates a new menu button.
        /// <para>Adds it to <see cref="Buttons"/> and as a child to this <see cref="BottomMenuController"/>.</para>
        /// </summary>
        /// <param name="title">The title of this button.</param>
        /// <param name="icon">The icon the button will use.</param>
        private void NewButton(string title, Texture icon, string sceneId) {
            var btn = new BottomMenuButton();
            btn.Setup(this, title, icon, LabelTheme, sceneId);
            Buttons.Add(btn);
            AddChild(btn);
        }

        /// <summary>
        /// Creates a new menu button seperator..
        /// <para>Adds it to <see cref="Seperators"/> and as a child to this <see cref="BottomMenuController"/>.</para>
        /// </summary>
        /// <param name="sepTex">The texture every seperator uses.</param>
        private void CreateSeperator(Texture sepTex) {
            TextureRect sep = new TextureRect {
                // Allow the texture rect to expand and contract.
                Expand = true,
                StretchMode = TextureRect.StretchModeEnum.ScaleOnExpand,
                RectSize = new Vector2(SEPERATOR_WIDTH, MENU_HEIGHT),
                Texture = sepTex,
                Name = "Serperator" + Seperators.Count 
            };
            // Seperators have a fixed width and strech from the bottom of the menu to the top.
            sep.SetAnchorsPreset(LayoutPreset.LeftWide);
            Seperators.Add(sep);
            AddChild(sep);
        }

        /// <summary>
        /// Refreshes the menu if the screen changes size.
        /// </summary>
        /// <param name="viewportSize">The size of the viewport the menu is in. Usually the current screen size.</param>
        /// <param name="rate">The rate the screen size is at compared to the default.</param>
        public void Refresh(Vector2 viewportSize, Vector2 rate) {

            // Adjust the size of the menu in correlation with the new size of the screem.
            MarginTop = -MENU_HEIGHT / rate.y;

            // Get the highest max change of rate.
            float max = Mathf.Max(rate.x, rate.y);

            // Affect the size of the font based on the higher rate of,
            (LabelTheme.DefaultFont as DynamicFont).Size = (int)(FONT_SIZE / max);

            // Get the new size of the buttons. There y value is equal to the height of the menu.
            var btnSize = new Vector2(BUTTON_WIDTH / rate.x, RectSize.y);

            // Readjust all the button poitions by calling there own Refresh method.
            float btnIndex = -(Buttons.Count - 1) / 2f;
            Buttons.ForEach((btn) => {
                btn.Refresh(btnSize, viewportSize.x, SEPERATOR_WIDTH, btnIndex++);
            });

            // Readjust all the seperators to there new locations.
            float sepIndex = -(Seperators.Count) / 2f;
            Seperators.ForEach((sep) => {
                sep.RectPosition = new Vector2(
                (viewportSize.x / 2) - (btnSize.x / 2) + ((SEPERATOR_WIDTH + btnSize.x) * (++sepIndex) - SEPERATOR_WIDTH), 0);
            });

        }

        /// <summary>
        /// Changes from one screen to another.
        /// <para>Called on the button pressed signal.</para>
        /// </summary>
        /// <param name="btn">The button that called this method.</param>
        public void ChangeScreen(BottomMenuButton btn, string sceneId) {
            // Deselected all the other buttons.
            Buttons.ForEach((btn) => {
                btn.UpdateSelected(TexSelectOff, false);
            });
            // Select the new button.
            btn.UpdateSelected(TexSelectOn, true);

            if (Scenes.SceneManager.StoredScenes.TryGetValue(sceneId, out var scene))
                Scenes.SceneManager.ChangeScene(scene);
            else
                Scenes.SceneManager.ChangeScene(null);

        }
    }
}