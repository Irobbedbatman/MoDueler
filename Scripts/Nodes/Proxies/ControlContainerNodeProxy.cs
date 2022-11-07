using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoDueler.Nodes {
    internal class ControlContainerNodeProxy<C, T> : Node2DProxy<C> where C : ControlContainterNode<T> where T : Godot.Control, new() {
        public ControlContainerNodeProxy(C node) : base(node) { }

        public Godot.Control EmbeddedControl => RealNode.EmbeddedControl;

    }
}
