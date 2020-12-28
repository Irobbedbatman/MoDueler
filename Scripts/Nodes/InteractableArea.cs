using Godot;
using MoDueler.Camera;
using System;


namespace MoDueler.Nodes {

    /// <summary>
    /// An <see cref="Area2D"/> that <see cref="CameraPointer.Instance"/> can supply with click, release and hovered information.
    /// </summary>
    public class InteractableArea : Area2D {

        /// <summary>
        /// Check to see if the current <see cref="CameraPointer"/> considers this hovered.
        /// </summary>
        public bool IsHovered => CameraPointer.Instance.Hovered.Contains(this);

        /// <summary>
        /// Invoked on mouse entry without previously being present.
        /// </summary>
        public Action OnHovered;
        /// <summary>
        /// Invoked on mouse exit with the mouse previously being present.
        /// </summary>
        public Action OffHovered;
        /// <summary>
        /// Invoked on mouse click while hovered.
        /// </summary>
        public Action OnPressed;
        /// <summary>
        /// Invoked on mouse realease regardless of being hovered when it is the last clicked object.
        /// <para>The Provided <c>bool</c> is the state of <see cref="IsHovered"/>.</para>
        /// </summary>
        public Action<bool> OnReleased;

    }
}