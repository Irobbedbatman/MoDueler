using Godot;
using Godot.Collections;
using System;
using System.Linq;
using System.Collections.Generic;
using MoonSharp.Interpreter;

namespace MoDueler.Nodes {
 
    [MoonSharpUserData]
    public class HandController : Node2D {

        /// <summary>
        /// The card the playeer is currently dragging across the scren.
        /// </summary>
        public HandCard SelectedCard { get; private set; } = null;
        /// <summary>
        /// The action that is invoked when the player releases a card.
        /// <para>Parameters in order; The selected card, the object hovered and the position on the screen.</para>
        /// </summary>
        public Action<HandCard, Area2D, Vector2> CardPlayed;
        /// <summary>
        /// Abstract position for calculating the large circle the hand rests on.
        /// </summary>
        private const float CircleRadius = 2300;
        /// <summary>
        /// Abstract position for positioning the large circle the hand rests on.
        /// </summary>
        private static readonly Vector2 CircleOrigin = Vector2.Down * (CircleRadius + 400);
        /// <summary>
        /// The amout of degree of seperation for each card.
        /// </summary>
        private const float CardSepertation = 2.5f;
        /// <summary>
        /// The list of objects in the hand.
        /// </summary>
        private readonly List<HandCard> HeldCards = new List<HandCard>();

        public HandController() {
            Name = "PlayerHand";
        }

        /// <summary>
        /// The distnace from the <see cref="SelectedCard"/>'s centre pivot to the mouse position.
        /// </summary>
        private float anchorDist;
        /// <summary>
        /// The angle from the <see cref="SelectedCard"/>'s centre pivot to the mouse position.
        /// </summary>
        private float anchorAngle;
        /// <summary>
        /// The angle from <see cref="CircleOrigin"/> to the <see cref="SelectedCard"/>'s centre pivot on the last frame.
        /// </summary>
        private float lastSelectedAngle = 0;
        /// <summary>
        /// The angle to the mouse from <see cref="CircleOrigin"/>.
        /// </summary>
        private float lastMouseAngle;
        /// <summary>
        /// The angle from <see cref="CircleOrigin"/> to the middle of the hand cards.
        /// </summary>
        private float tangent = 0;


        /// <summary>
        /// Adds a new card to the hand and sets it's controller to this.
        /// </summary>
        /// <param name="card"></param>
        public void AddCard(HandCard card) {
            HeldCards.Add(card);
            AddChild(card);
            card.controller = this;
        }

        /// <summary>
        /// Removes a crad from the hand and removes its controller from being this.
        /// <para>Also deselects any card in the players hand for logical safety.</para>
        /// </summary>
        /// <param name="card"></param>
        public void RemoveCard(HandCard card) {
            HeldCards.Remove(card);
            RemoveChild(card);
            card.controller = null;
            SelectedCard = null;
        }


        //TODO: Card OnHovered and OffHovered Events.

        /// <summary>
        /// Method called when a card is clicked. 
        /// </summary>
        /// <param name="card">The card to be selected.</param>
        public void SelectCard(HandCard card) {

            // Can only select one card at a time.
            if (SelectedCard != null)
                return;

            // Shorthand accessor for the mouse position.
            var mousepos = CameraPointer.Instance.PointerPos;

            // Set the accesible selected card.
            SelectedCard = card;

            // Get the cards position.
            Vector2 pos = card.Position;
            // Get where on the card the mouse was clicked.
            Vector2 anchor = mousepos - pos;
            // Get the distance from the middle of the card and the clicked postion.
            anchorDist = anchor.Length();
            // Get the angle from the middle of the card to the clicked position.
            anchorAngle = AngleTo(pos, mousepos);

            lastMouseAngle = AngleTo(CircleOrigin, mousepos);
            lastSelectedAngle = AngleTo(CircleOrigin, pos);
        }

        /// <summary>
        /// Deselects the current <see cref="SelectedCard"/> amd of the card could be played be tru and play it.
        /// </summary>
        public void DeselectCard() {

            // Ensure a card is selected.
            if (SelectedCard == null)
                return;

            //Cards can't target other cards in the hand and need to excluded from the intersect check.
            //TODO: Don't create more arrays.
            Godot.Collections.Array exclude = new Godot.Collections.Array(HeldCards);


            // Get area the card is over. To determine the correct play the player is trying to make.
            var collider = CameraPointer.Instance.GetIntersectColliders(SelectedCard.Position, exclude).FirstOrDefault();

            // If there is something the card is over we try and play it.
            if (collider is Area2D area) {
                CardPlayed?.Invoke(SelectedCard, area, SelectedCard.GlobalPosition);
            }

            // Clear the selected card.
            SelectedCard = null;
        }

        public override void _Process(float delta) {
            // Every frame we need to update the hand cards and the selected cards postion and angle.
            Update(delta);
        }

        /// <summary>
        /// Recaluclates the normal angle of the hand by using the selected card and it's anchorage.
        /// <para>Also moves the cards, including the selcted card to it's new position by caclulating mouse movement delta and rotational delta.</para>
        /// </summary>
        /// <param name="timeDelta">Time since last frame</param>
        public void Update(float timeDelta) {

            // If a card is not currently selcted we don't need to update the hand.
            if (SelectedCard == null) {

                // Normalize the tanget angle of the hand so it can interoplated.
                tangent %= 360;
                if (tangent > 180)
                    tangent -= 360;
                if (tangent < -180)
                    tangent += 360;

                // Interoplate the tangent angle.
                tangent = Mathf.Lerp(tangent, 0, timeDelta / 5f);
                UpdateCards();

                return;
            }

            //Shorthand accessor for the mouse position.
            var mousepos = CameraPointer.Instance.PointerPos;

            // Draw some bones so that tesing is a bit easier.
            Debug.DebugDrawer.Instance.DrawEvents += delegate {

                if (!SelectedCard.IsInsideTree())
                    return;

                if (!IsInsideTree())
                    return;

                var pointa =  SelectedCard.GlobalPosition;
                var pointb = SelectedCard.GlobalPosition + UnitVectorDirection(anchorAngle) * anchorDist;
                // Draw the way the selected card is anchored.
                Debug.DebugDrawing.DrawBone(pointa, pointb, 5, new Color(1, 0, 1), new Color(1, 1, 1));
                // Draw a bone represent the tangent so we can see where the centre of the hand is.
                Debug.DebugDrawing.DrawBone(CircleOrigin, CircleOrigin + UnitVectorDirection(tangent-90) * CircleRadius, 5, new Color(0, 1, 1), new Color(1, 1, 1));
                //Cards can't target other cards in the hand and need to excluded from the intersect check.
                // TODO: Another pesky godot array.
                Godot.Collections.Array exclude = new Godot.Collections.Array(HeldCards);
                // Draw a bone to the current object the mouse is pointing to.
                var obj = CameraPointer.Instance.GetIntersectColliders(pointa, exclude).FirstOrDefault();
                if (obj != null && obj.IsInsideTree())
                    Debug.DebugDrawing.DrawBone(pointa, obj.GlobalPosition, 5, new Color(1, .3f, .4f), new Color(1, 1, 1));
            };

            // Get the new angle to the mouse and use it calculate the delta from last frame.
            float rot = AngleTo(CircleOrigin, mousepos);
            var deltaRot = rot - lastMouseAngle;
            lastMouseAngle = rot;

            // Now we can rotate the anchor on the card as it rotates at the same rate as the whole hand.
            anchorAngle += deltaRot;

            // Update the cards position using inverse kinematics and the new anchor angle we just found.
            Vector2 newCardPos = mousepos - UnitVectorDirection(anchorAngle) * anchorDist;

            // If the selected card is fine to move to its new position we update it after the other cards.
            bool updateSelectedPos = true;

            // If the card would be off the bottom of the screen we don't move it to the new position.
            // and instead only update with the rest of the cards.
            if (newCardPos.DistanceTo(CircleOrigin) < CircleRadius) {
               updateSelectedPos = false;
            }

            // How that we have the amount of angular change the selected card has moved we can apply the same change to the whole hand.
            float currentSelectedAngle = AngleTo(CircleOrigin, newCardPos);
            var deltaSelectedAngle = currentSelectedAngle - lastSelectedAngle;
            lastSelectedAngle = currentSelectedAngle;
            tangent += deltaSelectedAngle;

            // Now that we have the angle the hand sits on we can update all the cards.
            UpdateCards();

            // If the selected cards postion should be furthur updated we can now do that.
            if (updateSelectedPos)
                SelectedCard.Position = newCardPos;

        }

        /// <summary>
        /// Updates the position of each card.
        /// </summary>
        public void UpdateCards() {

            // Sorts and also moves the cards in the hand accoring to their IComparer.
            HeldCards.Sort();

            float posI = -((HeldCards.Count - 1) / 2f);
            foreach (var card in HeldCards.ToArray()) {

                if (card == null)
                    continue;

                // Get the angle to where the card would be.
                float angle = tangent + (posI * CardSepertation);

                // Get the new card position.
                Vector2 pos = CircleOrigin + UnitVectorDirection(angle- 90) * (CircleRadius);

                // Lerp to the new position over 1 second.
                card.Position += (pos - card.Position) * 1;

                // Get the normailzed angle of the card.
                angle %= 360;
                if (angle > 180)
                    angle -= 360;
                if (angle < -180)
                    angle += 360;
                // Set the angle of the card to the correct angle over 1 second.
                card.RotationDegrees = Mathf.Lerp(card.RotationDegrees, angle, 1);
                // Make cards render in the correct order.
                card.ZIndex = (int)(HandCard.CARD_Z_INDEX - (posI * 2));
                posI++;
            }
        }

        /// <summary>
        /// Gets the degree angle from a to b.
        /// </summary>
        public static float AngleTo(Vector2 a, Vector2 b) {

            float rad = -Mathf.Atan2(
                b.x - a.x,
                b.y - a.y
                );

            return Mathf.Rad2Deg(rad) + 90;
        }

        /// <summary>
        /// Creates a new unit vector using basic trig with the provied angle.
        /// </summary>
        public static Vector2 UnitVectorDirection(float degreeAngle) {
            return new Vector2(
                Mathf.Cos(Mathf.Deg2Rad(degreeAngle)),
                Mathf.Sin(Mathf.Deg2Rad(degreeAngle))
                );
        }


    }
}
