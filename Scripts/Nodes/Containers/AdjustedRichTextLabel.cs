using Godot;
using MoDueler.Resources;
using System;


namespace MoDueler.Nodes {
    [MoonSharp.Interpreter.MoonSharpUserData]
    public class AdjustedRichTextLabel : RichTextLabel {

        /// <summary>
        /// The <see cref="DynamicFont"/> used by the currently applied <see cref="Theme"/>.
        /// </summary>
        public DynamicFont Font => Theme.DefaultFont as DynamicFont;

        /// <summary>
        /// The amout the label has moved in <see cref="ValignMove"/> so it can be reset or changed.
        /// </summary>
        public Vector2 ValignMove = Vector2.Zero;

        /// <summary>
        /// Event handler for freeing any extra resources not cleaned by the GC. Look at <see cref="AddImage(Texture, int, int)"/> for a typical use case.
        /// </summary>
        private EventHandler WhenFree;

        /// <summary>
        /// Has this element been centeredd using <see cref="Center"/>.
        /// </summary>
        public bool IsCentered {
            get;
            private set;
        } = false;

        /// <summary>
        /// Have to invoke <see cref="WhenFree"/> even if the program terminates or when cleaned by the GC.
        /// </summary>
        ~AdjustedRichTextLabel() {
            GD.Print("Rich Text Label - Freed as Finalizer");
            WhenFree?.Invoke(this, null);
            WhenFree = null;
        }

        /// <summary>
        /// Adds <see cref="WhenFree"/> invokation to the usual <see cref="Godot.Object.Free"/>.
        /// </summary>
        public new void Free() {
            WhenFree?.Invoke(this, null);
            WhenFree = null;
            base.Free();
        }

        /// <summary>
        /// Adds <see cref="WhenFree"/> invokation to the usual <see cref="Godot.Node.QueueFree"/>.
        /// </summary>
        public new void QueueFree() {
            WhenFree?.Invoke(this, null);
            WhenFree = null;
            base.QueueFree();
        }

        /// <summary>
        /// The text without any of the tags in it.
        /// </summary>
        public string RawText { get; private set; }

        public void Setup(string text, DynamicFont font, Vector2 size) {

            SetSize(size);
                
            // Centers the label.
            RectPosition -= RectSize / 2f;
            RectPivotOffset = RectSize / 2f;

            // We use bbcode text so we can set it to true.
            BbcodeEnabled = true;



            // Set both text values to the text provided even though we can't rensure raw text has no tags. TODO: Check for tags.
            BbcodeText = text;
            RawText = text;

            // Create a new theme for this label with the provided font being the only unqiue thing.
            Theme = new Theme {
                DefaultFont = font
            };

        }

        // Called when the node enters the scene tree for the first time.
        public override void _Ready() {
            BbcodeEnabled = true;
            // Rich text labels come with a scroll bar that needs to be hidden.
            GetVScroll().Visible = false;
            ScrollActive = false;
            RectClipContent = false;
        }

        /// <summary>
        /// Adds a tag before the text.
        /// </summary>
        public void PreTag(string tag) => BbcodeText = tag + BbcodeText;

        /// <summary>
        /// Adds a tag to the end of the text.
        /// </summary>
        public void PostTag(string tag) => BbcodeText += tag;

        /// <summary>
        /// Applies the '[center]' tags around the text.
        /// </summary>
        public void Center() {
            IsCentered = true;
            PreTag("[center]");
            AppendBbcode("[/center]");
        }


        /// <summary>
        /// Adds a image to the <see cref="RichTextLabel.BbcodeText"/> without clearing it due to a bug.
        /// <para>TODO: If Godot 4.0 => <c>base.AddImage()</c> should hopefully work as intended.</para>
        /// </summary>
        public new void AddImage(Texture image, int width = -1, int height = -1) {

            //The texture needs to have it's refrence incremented manually as rich text and take over path don't do this.
            image.Reference_();
            // Unrefefrence the image when the label s finished with.
            WhenFree += (sender, args) => { image.Unreference(); };

            // Construct a FIXED path for the image to be accessed. It is made unqiue using it's RID. If the same path is used images will be overwritten.
            // Also handles the fact that if the same texture is used by multiple things the RID will be the same and extra space wont be wasted.
            // .richtextimage affix is used so to avoid the unlikely situation the file would be used elsewhere. And having files simply named '123' would be pretty appalling.
            var path = image.GetRid().GetId().ToString() + ".richtextimage";

            //TODO: Sizecode from method arguments.
            string sizecode = "=<32x32>";

            BbcodeText = BbcodeText + "[img" + sizecode + "]" + path + "[/img]";

            //Remap the texture address to the path.
            //TODO: Make an approppriate and vital method that handles this as moving the path will destroy all things using a seperate path.
            image.TakeOverPath("res://" + path);
            //TODO: i.e. the following path will remove the image.
            //image.TakeOverPath("res://" + path + "___");
        }

        /// <summary>
        /// Increase font size so that it matches with the area provided.
        /// </summary>
        public void FitToLine() {
            //Get the size of the string when using the current font.
            var stringsize = Font.GetStringSize(Text);
            //Get the sca;e and ensure that it won't extend outside of the provided dimensions.
            var scale = Mathf.Min(RectSize.x / stringsize.x, RectSize.y / stringsize.y);
            //Scale Fontsize accordingly.
            Font.Size = (int)((Font.Size - Font.OutlineSize) * scale);
        }

        /// <summary>
        /// Changes the size to a new size.
        /// </summary>
        public void AdjustSize(Vector2 newSize) {
            SetSize(newSize);
            // TODO: Call FitToLine or Valign when called. 
        }

        /// <summary>
        /// Verically aligns the text.
        /// </summary>
        /// <param name="vAlign">Alignment to use.</param>
        public void VAlign(VAlign vAlign) {

            // Remove any previous movement from VAlign.
            RectPosition -= ValignMove;
            // Reset Valign.
            ValignMove = Vector2.Zero;

            // Get the size of the text.
            var textrect = Font.GetWordwrapStringSize(RawText, RectSize.x);

            switch (vAlign) {
                // Aligning with the top is default behaviour.
                case Godot.VAlign.Top:
                    break;
                // To align to the center we just objtain the center of both the rect and text and subtract them.
                case Godot.VAlign.Center:
                    ValignMove = new Vector2(0, RectSize.y / 2f - textrect.y / 2f);
                    RectPosition += ValignMove;
                    break;
                // To align to the bottom we just start from the bottom and move up equal to the size of the text.
                case Godot.VAlign.Bottom:
                    ValignMove = new Vector2(0, RectSize.y - (textrect.y));
                    RectPosition += ValignMove;
                    break;
            }

        }


    }

}