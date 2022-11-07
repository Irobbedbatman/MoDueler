using Godot;
using Godot.Collections;
using MoDueler.Nodes;
using System.Collections.Generic;
using System.Linq;

namespace MoDueler.Nodes {
    public class CameraPointer : Camera2D {

        /// <summary>
        /// Staticly accessible <see cref="CameraPointer"/>.
        /// <para>
        /// Assigned in constructor.
        /// </para>
        /// </summary>
        public static CameraPointer Instance;

        /// <summary>
        /// The list of currently hovered <see cref="CollisionObject2D"/>.
        /// <para>
        /// Sorted by <see cref="Node2D.ZIndex"/> using <see cref="ZDepthSort"/>. 
        /// </para>
        /// </summary>
        public List<CollisionObject2D> Hovered = new List<CollisionObject2D>();

        /// <summary>
        /// The position the mouse is on the screen.
        /// </summary>
        public Vector2 PointerPos => GetGlobalMousePosition();

        /// <summary>
        /// The top most <see cref="CollisionObject2D"/> in the <see cref="Hovered"/> <see cref="List"/>;
        /// </summary>
        public CollisionObject2D TopHovered { get; private set; }

        /// <summary>
        /// The last clicked interactable so we can send <see cref="InteractableArea.OnReleased"/> when the mouse is released anywhere.
        /// </summary>
        private InteractableArea _lastClicked = null;

        /// <summary>
        /// Assign the singleton instance when this camera enters visible space.
        /// </summary>
        public override void _EnterTree() {

            // Warn developers when using more than one of this singleton.
            if (Instance != null) {
                GD.PrintErr("Only one camera pointer instance should be availbe at a time,");
            }

            Instance = this;
        }

        /// <summary>
        /// Remove the singleton instance if this ever leaves visible space.
        /// </summary>
        public override void _ExitTree() {
            Instance = null;
        }

        public override void _Process(float delta) {
            UpdatePointer();
        }

        /// <summary>
        /// Updates the pointer with the new hovered changes.
        /// </summary>
        private void UpdatePointer() {

            var lastHovered = TopHovered as InteractableArea;
            Hovered = GetIntersectColliders(GetGlobalMousePosition());
            TopHovered = Hovered.Count > 0 ? Hovered[0] : null;
            var newHovered = TopHovered as InteractableArea;

            if (newHovered != lastHovered) {
                lastHovered?.OffHovered?.Invoke();
                newHovered?.OnHovered?.Invoke();
            }

        }

        /// <summary>
        /// The input call that informs us when to tell <see cref="InteractableArea"/>s that they have been clicked.
        /// </summary>
        /// <param name="ievent">The event that is provided. We only need mouse events.</param>
        public override void _UnhandledInput(InputEvent ievent) {

            //We only care about mouse events.
            if (ievent is InputEventMouseButton mevent) {
                //Left click.
                if (mevent.ButtonIndex == (int)ButtonList.Left && mevent.Pressed) {
                    //If we click on an interactable we invokes in onclick and stores it for on left mouse released.
                    if (typeof(InteractableArea).IsAssignableFrom(TopHovered?.GetType())) {
                        (TopHovered as InteractableArea)?.OnPressed?.Invoke();
                        _lastClicked = TopHovered as InteractableArea;
                    }
                }
                //Left click released.
                else if (mevent.ButtonIndex == (int)ButtonList.Left && !mevent.Pressed) {
                    // The on released action provides the current hovered state.
                    _lastClicked?.OnReleased?.Invoke(_lastClicked == TopHovered);
                    // The on clicked action requires to be hovered.
                    if (_lastClicked != null) {
                        if (_lastClicked == TopHovered) {
                            _lastClicked?.OnClicked?.Invoke();
                        }
                    }
                    _lastClicked = null;
                }
            }

        }

        /// <summary>
        /// Notificaton singal listener. Only listens for <see cref="MainLoop.NotificationWmFocusOut"/> so we can disable any clicked buttons.
        /// </summary>
        public override void _Notification(int notif) {
            switch (notif) {
                case MainLoop.NotificationWmFocusOut:
                    _lastClicked?.OnReleased?.Invoke(false);
                    _lastClicked = null;
                    TopHovered = null;
                    break;
            }
        }

        /// <summary>
        /// Gets all the <see cref="CollisionObject2D"/> at the given position.
        /// <para>
        /// Sorted by <see cref="Node2D.ZIndex"/> using <see cref="ZDepthSort"/>. 
        /// </para>
        /// </summary>
        public List<CollisionObject2D> GetIntersectColliders(Vector2 position, Godot.Collections.Array exclude = null) {

            //TODO: GetWorld2d() can cause a leak. This is an issue with Godot.

            //Check the position for any colliders.
            var colliders = GetWorld2d().DirectSpaceState.IntersectPoint(position, collideWithAreas: true, exclude: exclude).Cast<Dictionary>();

            //Select the collider portion only as that pertains to the node data.
            var nodes = colliders.Select((c) => c["collider"]);
            //Make the nodes accessible by type.
            var castednodes = nodes.Cast<CollisionObject2D>().ToList();
            //Sort the nodes by their zindex.
            castednodes.Sort(new ZDepthSort());
            return castednodes;
        }

    }
}