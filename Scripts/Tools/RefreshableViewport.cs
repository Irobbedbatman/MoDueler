using Godot;
using System;

namespace MoDueler.Tools {

    /// <summary>
    /// A viewport that calls <see cref="Refresh"/> when it's size changes. Mainly used for readjusting the size of nodes when the screen is ajusted.
    /// </summary>
    [Obsolete("Automatic Scaling is now provided through Godot.")]
    public abstract class RefreshableViewport : Viewport {

        /// <summary>
        /// The expected size of the screen.
        /// </summary>
        protected Vector2 Screen = new Vector2(1920, 1080);

        public override void _Ready() {
            //Connect("size_changed", this, nameof(Refresh), null, (uint)ConnectFlags.Deferred);
        }

        /// <summary>
        /// Called when the screen changes size but also to apply changes upon scene change.
        /// <para>Only called natively if <c>base._Ready()</c> is called in <c>_Ready()</c>.</para>
        /// </summary>
        public abstract void Refresh();

    }
}
