using Godot;
using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MoDueler.Nodes {

    /// <summary>
    /// Factory class that embeeds a <see cref="Control"/> under <see cref="Node2D"/> logic.
    /// </summary>
    /// <typeparam name="T">A <see cref="Control"/> that is imbedded inside the container.</typeparam>
    public class ControlContainterNode<T> : Node2D where T : Control, new() {

        /// <summary>
        /// The embedded control.
        /// </summary>
        public readonly T EmbeddedControl;

        public static implicit operator T(ControlContainterNode<T> container) => container.EmbeddedControl;

        public ControlContainterNode() {
            EmbeddedControl = new T();
            AddChild(EmbeddedControl);
            ZIndex = 1;
            ZAsRelative = true;
            //VisualServer.CanvasItemSetZIndex(EmbeddedControl.GetCanvasItem(), 500);
        }

        /// <summary>
        /// The name of the Node. Also applies to <see cref="EmbeddedControl"/>.
        /// </summary>
        public new string Name {
            get {
                return base.Name;
            }
            set {
                EmbeddedControl.Name = value;
                base.Name = value;
            }  
        }


    }
}
