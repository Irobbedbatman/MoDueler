using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoDueler.Duel {

    /// <summary>
    /// This is a roung around the edges quick implementation of my MoDuel DuelFlow class. Recomend reading that instead of this.
    /// <para>Handles the flow and data of a duel. Outputs message for players through the <see cref="Output"/> evnthandler.</para>
    /// </summary>
    public class SimplifiedFlow {

        /// <summary>
        /// The player who can currently do actions.
        /// </summary>
        public SimplifiedPlayer TurnOwner;
        
        /// <summary>
        /// How many actions the turn owner has left this turn.
        /// </summary>
        public int ActionsRemaining = 1;

        /// <summary>
        /// The player provided first.
        /// </summary>
        public SimplifiedPlayer Player1;
        /// <summary>
        /// The player provided second.
        /// </summary>
        public SimplifiedPlayer Player2;

        /// <summary>
        /// The slot of field.
        /// </summary>
        public SimplifiedSlot[] Slots = new SimplifiedSlot[10] {
            new SimplifiedSlot(0),
            new SimplifiedSlot(1),
            new SimplifiedSlot(2),
            new SimplifiedSlot(3),
            new SimplifiedSlot(4),
            new SimplifiedSlot(5),
            new SimplifiedSlot(6),
            new SimplifiedSlot(7),
            new SimplifiedSlot(8),
            new SimplifiedSlot(0),
        };

        /// <summary>
        /// The outout handler that players and spectators can listen to.
        /// </summary>
        public EventHandler<FlowOutputEventArgs> Output;

        /// <summary>
        /// The linking of objects from the networ.
        /// TODO: Remove this from flow.
        /// </summary>
        public Network.NetworkLinker Linker = new Network.NetworkLinker();

        public void Start(SimplifiedPlayer p1, SimplifiedPlayer p2) {
            // Store the provided players.
            Player1 = p1;
            Player2 = p2;
            //TODO: Random player goes first.
            TurnOwner = p1;
            // Tell the players the index of each slot.
            InitSlots();
            // Tell the player the cards that are in their hands.
            InitHands();
        }

        /// <summary>
        /// Informs the connected players what index the slots are using.
        /// </summary>
        private void InitSlots() {
            int i = 0;
            foreach (var slot in Slots) {
                Output.Invoke(this, new FlowOutputEventArgs(null, "LinkSlot", i.ToString(), slot.Index.ToString()));
                ++i;
            }
        }

        /// <summary>
        /// Informs the players what cards they have in their hands at the start of the game and the index those cards are using.
        /// </summary>
        private void InitHands() {
            // Tell player1 their cards.
            foreach (var card in Player1.Hand) {
                Output.Invoke(this, new FlowOutputEventArgs(Player1, "AddHandCard", card.Index.ToString(), "HandCardNew", card.CardID, "Soul"));
            }
            // Then tell player2.
            foreach (var card in Player2.Hand) {
                Output.Invoke(this, new FlowOutputEventArgs(Player2, "AddHandCard", card.Index.ToString(), "HandCardNew", card.CardID, "Soul"));
            }
        }

        /// <summary>
        /// Switches turn owner to the other player.
        /// </summary>
        public void SwapTurn() {
            if (TurnOwner == Player2)
                TurnOwner = Player1;
            else
                TurnOwner = Player2;
            ActionsRemaining = 1;
        }

        /// <summary>
        /// Method use d by players to tell the flow that they are trying to play a card.
        /// </summary>
        /// <param name="actor">The player that called this method.</param>
        /// <param name="cardIndex">The index of the card they trying to play.</param>
        /// <param name="target">WHat the card they are trying to play is targeting.</param>
        public void PlayCard(SimplifiedPlayer actor, uint cardIndex, uint target) {
            if (TurnOwner != actor)
                return;
            // Check to see if the target is a slot.
            if (SimplifiedIndexer.Indices[target] is SimplifiedSlot slot) {
                // Can only play creatures in empty slots.
                if (slot.IsEmpty) {
                    // Esnure the player is playing a card and not refrence some other index.
                    if (SimplifiedIndexer.Indices[cardIndex] is SimplifiedCard card) {
                        // Remove the card from the players hand.
                        Output.Invoke(this, new FlowOutputEventArgs(Player1, "RemHandCard", cardIndex.ToString()));
                        actor.Hand.Remove(card);
                        // Create the creature.
                        var creature = new SimplifiedCreature(card.CardID, card.Owner);
                        // Tell the players a creature has spawned.
                        Output.Invoke(this, new FlowOutputEventArgs(null, "SpawnCreature", creature.Index.ToString(), "BattlefFiledCreature", creature.CardID, "Soul", slot.Index.ToString()));
                        slot.Creature = creature;
                    }
                    ActionsRemaining--;
                }
            }
            CheckEndTurn();
        }

        /// <summary>
        /// Would add resources to the players resource pools but for now it wastes an action point.
        /// </summary>
        /// <param name="actor">The player trying to charge.</param>
        public void Charge(SimplifiedPlayer actor) {
            if (TurnOwner != actor)
                return;
            //TODO: Resources.
            ActionsRemaining--;
            CheckEndTurn();
        }

        /// <summary>
        /// Returns all the cards from the player's grave to the player's hand.
        /// </summary>
        /// <param name="actor"></param>
        public void Revive(SimplifiedPlayer actor) {
            if (TurnOwner != actor)
                return;
            // Can't revive if the grave is empty.
            if (actor.Grave.Count == 0)
                return;
            var grave = actor.Grave.ToArray();
            foreach (var card in grave) {
                // Remove the card from the graveyard.
                actor.Grave.Remove(card);
                var c = new SimplifiedCard(card.CardID, card.Owner);
                // Tell the player that a new card has been added to the hand.
                Output.Invoke(this, new FlowOutputEventArgs(actor, "AddHandCard", c.Index.ToString(), "HandCardNew", c.CardID, "Soul"));
                actor.Hand.Add(c);
            }
            ActionsRemaining--;
            CheckEndTurn();
        }


        /// <summary>
        /// Check to see if the turn is over and call <see cref="SwapTurn"/> if it is.
        /// </summary>
        public void CheckEndTurn() {
            if (ActionsRemaining == 0) {
                Attackers();
                SwapTurn();

                // If it's the AI's trun we call the AI method.
                if (TurnOwner == Player2) {
                    BrainDeadAI();
                }
                else {

                    // If the player can't play because of the game state we play for them.
                    bool free = false;
                    foreach (var slot in Slots) {
                        if (slot.IsEmpty) {
                            free = true;
                            break;
                        }
                    }
                    if (!free)
                        Charge(TurnOwner);
                    else if (TurnOwner.Hand.Count == 0) {
                        if (TurnOwner.Grave.Count > 0) {
                            Revive(TurnOwner);
                        }
                        else {
                            Charge(TurnOwner);
                        }
                    }

                }
            }
            // If it's still the AI's turn we tell them to continue.
            else if (TurnOwner == Player2)
                BrainDeadAI();
        }

        /// <summary>
        /// A random number generator fot the AI to peform diffrent actions. 
        /// </summary>
        private readonly Godot.RandomNumberGenerator rand = new Godot.RandomNumberGenerator();

        /// <summary>
        /// Peforms dummy actions for when it is the ai's turn.
        /// </summary>
        private void BrainDeadAI() {
            
            // Ensure there is a free slot to play.
            bool free = false;
            foreach (var slotcheck in Slots) {
                if (slotcheck.IsEmpty) {
                    free = true;
                    break;
                }
            }
            // IF there isn't we try to skip to the next turn.
            if (!free) {
                Charge(TurnOwner);
                CheckEndTurn();
                return;
            }
            // IF the AI has no cards in hand we add them from the grave.
            else if (TurnOwner.Hand.Count == 0) {
                if (TurnOwner.Grave.Count > 0) {
                    Revive(TurnOwner);
                    CheckEndTurn();
                    return;
                }
                // But if the grave is empty we just waste actions.
                else {
                    Charge(TurnOwner);
                    CheckEndTurn();
                    return;
                }
            }
            
            // Grab the first most card in the AI's hand.
            var card = TurnOwner.Hand[0];
            // Grab any slot.
            var slot = rand.RandiRange(0, 9);

            // Try to play in that slot. Note this becomes recursive as we don't check to see if the slot is empty and it could loop forever.
            PlayCard(TurnOwner, card.Index, Slots[slot].Index);

            CheckEndTurn();
        }

        /// <summary>
        /// Grabs any creatures in the turn owners row and attacks with them.
        /// </summary>
        private void Attackers() {
            List<SimplifiedSlot> slots;
            // Based of whoever's turn it is only their row attacks.
            if (TurnOwner == Player1) {
                slots = new List<SimplifiedSlot>() {
                    Slots[0],
                    Slots[1],
                    Slots[2],
                    Slots[3],
                    Slots[4]
                };
            }
            else {
                slots = new List<SimplifiedSlot>() {
                    Slots[5],
                    Slots[6],
                    Slots[7],
                    Slots[8],
                    Slots[9]
                };
            }

            foreach (var slot in slots) {
                // Attack with the slot and if it returns true the opponent is dead.
                if (IndivAttacker(slot)) {
                    // Tell the players the game is over and that someone has won.
                    Output.Invoke(this, new FlowOutputEventArgs(null, "EndGame", TurnOwner == Player1 ? Player1.Index.ToString() : Player2.Index.ToString()));
                    return;
                }
            }

        }

        /// <summary>
        /// Performs the attacks check for a provided slot and if it occupied the creature attacks.
        /// </summary>
        /// <param name="slot"></param>
        /// <returns></returns>
        private bool IndivAttacker(SimplifiedSlot slot) {
            // If there is no creature there is no attacker.
            if (slot.IsEmpty)
                return false;

            // If the creature has summoning sickness it can't attack this turn but can sickness can be removed.
            if (slot.Creature.Sickness) {
                slot.Creature.Sickness = false;
                return false;
            }

            // Get the slot number opposite the provided slot.
            var otherSlot = slot.SlotNumber + 5;
            if (otherSlot > 9)
                otherSlot -= 10;

            // Get the slot from the slot number.
            var oppSlot = Slots[otherSlot];

            // If the slot is empty we attack the other player.
            if (oppSlot.IsEmpty)
                return AttackPlayer(slot.Creature, oppSlot.SlotNumber >= 5 ? Player2 : Player1);
            // Otherwise we attackt the opposite creature.
            else {
                AttackCreature(slot.Creature, oppSlot);
                return false;
            }
        }

        /// <summary>
        /// Reduces the health of a targeted <see cref="SimplifiedPlayer"/> by the attack of a <see cref="SimplifiedCreature"/>
        /// <para>If a player dies to this method it returns true.</para>
        /// </summary>
        /// <param name="attacker">The creature attacking.</param>
        /// <param name="target">The player  being attcked.</param>
        /// <returns>true if the player is dead.</returns>
        private bool AttackPlayer(SimplifiedCreature attacker, SimplifiedPlayer target) {
            target.Health -= attacker.CurrentAtk;
            // Check the player's health to see if they have died.
            return target.Health <= 0;
        }

        /// <summary>
        /// Reduces the health of a <see cref="SimplifiedCreature"/> in a targeted slot by the attack of a <see cref="SimplifiedCreature"/>
        /// </summary>
        /// <param name="attacker">The creature attacking.</param>
        /// <param name="targetSlot">The slot  being attcked.</param>
        private void AttackCreature(SimplifiedCreature attacker, SimplifiedSlot targetSlot) {
            // Get the creature in the other slot.
            var target = targetSlot.Creature;

            // Reduce the targets hp by the attacker's attack.
            target.CurrentHp -= attacker.CurrentAtk;

            // If the target has no health left it dies.
            if (target.CurrentHp <= 0) {
                // The creature goes to the grave.
                target.Owner.Grave.Add(target);
                // Tell the players a creature has died.
                Output.Invoke(this, new FlowOutputEventArgs(null, "KillCreature", target.Index.ToString()));
                // Clear the target slot.
                targetSlot.Creature = null;
            }
        }
    }
}
