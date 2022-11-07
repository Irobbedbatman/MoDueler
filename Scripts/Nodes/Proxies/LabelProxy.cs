using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoDueler.Nodes{
    internal class LabelProxy : ControlContainerNodeProxy<NodeRichTextLabel, AdjustedRichTextLabel> {

        public LabelProxy(NodeRichTextLabel node) : base(node) { }

        

        public void Center() => RealNode.EmbeddedControl.Center();

        public void VAlign(Godot.VAlign align) => RealNode.EmbeddedControl.VAlign(align);


        public void SetText(string newText) {
            RealNode.EmbeddedControl.BbcodeText = newText;
            //if (RealNode.EmbeddedControl.IsCentered)
            //    Center();
        }

        public void SetTextNormal(string newText) => RealNode.EmbeddedControl.Text = newText;

        public void Debug() {
            RealNode.DrawBoundingRect = true;
        }

        public int GetLines() => RealNode.EmbeddedControl.GetLineCount();


    }
}
