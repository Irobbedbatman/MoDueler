using Godot;
using MoDueler.Resources;
using System;


namespace MoDueler.Nodes.Overlay {

    /// <summary>
    /// A "button" that is created for the bottom menu.
    /// <para>The actual <see cref="Button"/> is a child node of the <see cref="BottomMenuButton"/>,</para>
    /// </summary>
    public class BottomMenuButton : TextureRect {

        /// <summary>
        /// The <see cref="Color"/> we modulate the <see cref="TitleLabel"/> and <see cref="Icon"/> to wehn this button is selected.
        /// </summary>
        private static readonly Color Selected = new Color(1, 1, 1);
        /// <summary>
        /// The <see cref="Color"/> we modulate the <see cref="TitleLabel"/> and <see cref="Icon"/> to wehn this button is not selected.
        /// </summary>
        private static readonly Color Deselected = new Color(.75f, .75f, .75f);

        /// <summary>
        /// The creator of this button.
        /// </summary>
        private BottomMenuController Controller;

        /// <summary>
        /// THe sub-element icon of this button.
        /// </summary>
        private TextureRect Icon;
        /// <summary>
        /// The sub-element lable that displays the title of this button.
        /// </summary>
        private Label TitleLabel;

        /// <summary>
        /// The margin betwee the <see cref="TitleLabel"/> and the <see cref="Icon"/>.
        /// </summary>
        private const float ICON_LABEL_MARGIN = .1f;

        /// <summary>
        /// The original size of the <see cref="Icon"/>'s texture.
        /// </summary>
        private Vector2 originalIconSize;

        /// <summary>
        /// Setups this button. Creating any setting any nodes that are needed to display and add function to the button.
        /// </summary>
        /// <param name="controller">The creator of this button.</param>
        /// <param name="title">The text on this button.</param>
        /// <param name="icon">The image on this button.</param>
        public void Setup(BottomMenuController controller, string title, Texture icon, Theme theme, string sceneId) {
            // Store the creator this button.
            Controller = controller;

            // Allow the button texture to expand but also contract.
            Expand = true;
            // Scale the height of the bottom to the size of the bottom menu.
            SetAnchorsPreset(LayoutPreset.LeftWide);
            // Set the size of the button to that of constants in the BottomMenuController class.
            RectSize = new Vector2(BottomMenuController.BUTTON_WIDTH, BottomMenuController.MENU_HEIGHT);

            // Rename so it's easier found in the editor.
            Name = title + "Button";

            // Setup the title label on the button.
            Label lbl = new Label {
                Theme = theme,
                Text = title,
                // Center the label.
                Align = Label.AlignEnum.Center,
                Valign = Label.VAlign.Center,
                Name = title + "ButtonLabel"
            };
            // Strech the label to the size of the whole button. 
            lbl.SetAnchorsPreset(LayoutPreset.Wide);
            AddChild(lbl);
            TitleLabel = lbl;

            // Create the icon texture.
            TextureRect iconTr = new TextureRect {
                Texture = icon,
                // Allow the icon to expand and also contract.
                Expand = true,
                Name = title + "Icon"
            };
            // Store the original size of the icon.
            originalIconSize = icon.GetSize();
            AddChild(iconTr);
            Icon = iconTr;

            Button b = new Button {
                //Hide the button by making it clear.
                Modulate = new Color(0, 0, 0, 0),
                ActionMode = BaseButton.ActionModeEnum.Press,
                Name = title + "ButtonActivator"
            };
            // Strech the button to the size of the whole button. 
            b.SetAnchorsPreset(LayoutPreset.Wide);
            // When the button is pressed we call the ChangeScreen method in the parent controller.
            b.Connect("pressed", controller, nameof(controller.ChangeScreen), new Godot.Collections.Array { this, sceneId }, (uint)ConnectFlags.Deferred);


            AddChild(b);
        }

        /// <summary>
        /// Refreshes the button if the screen changes size.
        /// </summary>
        /// <param name="btnSize">The new size of the button.</param>
        /// <param name="screenwidth">The width of the screen currently.</param>
        /// <param name="sepWidth">How wide the seperators are.</param>
        /// <param name="offset">Which button is the with 0 being the center most button. ?.5f values will be used if there are an even amount of buttons.</param>
        public void Refresh(Vector2 btnSize, float screenwidth, float sepWidth, float offset) {
            // Update the size of the button.
            RectSize = btnSize;
            //Center the button in the bottom menu and then move it to the offset postion.
            RectPosition = new Vector2(
                (screenwidth / 2) - (btnSize.x / 2) + ((btnSize.x + sepWidth) * offset), 0);
            //TODO: Correct icon size when the button width constant is smaller.
            // If the screen is close to landscape mode we can hid the label and just display the icons.
            if (btnSize.y * 2.31f > btnSize.x) {
                // Hide the title label.
                TitleLabel.Visible = false;
                //Get the new icon size.
                float max = Mathf.Min(btnSize.x / BottomMenuController.BUTTON_WIDTH, btnSize.y / BottomMenuController.MENU_HEIGHT);
                // Use the new icon size.
                Icon.RectSize = new Vector2(max, max) * originalIconSize;
                // Center the icon now that there is no text.
                Icon.RectPosition = btnSize / 2f - Icon.RectSize / 2f;
            }
            else {
                // Show the label if it was hidden.
                TitleLabel.Visible = true;
                // Get the size of the title text.
                var size = Controller.GetStringSize(TitleLabel.Text);
                // Get the amout of free space on the left side of the button. (Where we will put the icon.)
                Vector2 leftSide = (btnSize - size) / 2f;
                // Get the new size of the icon.
                float max = Mathf.Min(leftSide.x / BottomMenuController.BUTTON_WIDTH, leftSide.y / BottomMenuController.MENU_HEIGHT);
                size.y = 0;
                leftSide = (btnSize - size) / 2f;
                // Use the new size of the icon.
                Icon.RectSize = new Vector2(max, max) * originalIconSize * 1.5f;
                // Position the icon such that is seperate to the title label by ICON_LABEL_MARGIN.
                var pos = (1 - ICON_LABEL_MARGIN) * leftSide - Icon.RectSize;
                // Vertically center the icon.
                pos.y = leftSide.y / 2;
                // Apply the new icon position.
                Icon.RectPosition = pos;
            }

        }

        /// <summary>
        /// Updates the way the button looks.
        /// </summary>
        /// <param name="newTex">The new texture the button will use.</param>
        /// <param name="selected">Is the button selected or not.</param>
        public void UpdateSelected(Texture newTex, bool selected) {
            Texture = newTex;
            // Change the color of the icon and title base of the selected check.
            Icon.Modulate = selected ? Selected : Deselected;
            TitleLabel.Modulate = selected ? Selected : Deselected;
        }

    }
}