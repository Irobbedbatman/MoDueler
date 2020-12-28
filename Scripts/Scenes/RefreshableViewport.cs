using Godot;
using System;

namespace MoDueler.Scenes {

    /// <summary>
    /// A viewport that calls <see cref="Refresh"/> when it's size changes. Mainly used for readjusting the size of nodes when the screen is ajusted.
    /// </summary>
    public abstract class RefreshableViewport : Viewport {

        public override void _Ready() {
            Connect("size_changed", this, nameof(Refresh), null, (uint)ConnectFlags.Deferred);
        }

        /// <summary>
        /// Called when the screen changes size but also to apply changes upon scene change.
        /// <para>Only called natively if <c>base._Ready()</c> is called in <c>_Ready()</c>.</para>
        /// </summary>
        public abstract void Refresh();

    }
}
