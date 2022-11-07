using MoonSharp.Interpreter;

namespace MoDueler.Nodes {


    /// <summary>
    /// Proxy class for <see cref="InteractableArea"/> that allows for the use of lua functions.
    /// <para> Can use setters instead of properties to provide default arguments.</para>
    /// </summary>
    [MoonSharpUserData]
    public class InteractableAreaProxy<T> : Area2DProxy<T> where T : InteractableArea {

        [MoonSharpHidden]
        public InteractableAreaProxy(T node) : base(node) { }

        public Closure OnPressed {
            set => RealNode.OnPressed = delegate { value.Call(this); };
        }

        public Closure OnReleased {
            set => RealNode.OnReleased = (hovered) => { value?.Call(this, hovered); };
        }

        public Closure OnHovered {
            set => RealNode.OnHovered = delegate { value?.Call(this); };
        }

        public Closure OffHovered {
            set => RealNode.OffHovered = delegate { value?.Call(this); };
        }

        public Closure OnClicked {
            set => RealNode.OnClicked = delegate { value?.Call(this); };

        }
        public bool IsHovered => RealNode.IsHovered;




    }
}
