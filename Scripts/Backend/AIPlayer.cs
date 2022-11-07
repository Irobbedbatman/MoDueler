using MoDuel;
using MoDuel.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Godot;

namespace MoDueler.Backend {

    /// <summary>
    /// Automated instance of <see cref="Player"/> on a locally provided <see cref="DuelFlow"/>.
    /// </summary>
    public class AIPlayer {

        /// <summary>
        /// The player the ai will automate actions for,
        /// </summary>
        public Player Player;
        /// <summary>
        /// The duel flow the ai will need to determine decisions.
        /// </summary>
        public DuelFlow Flow;
        /// <summary>
        /// The managed random that can be used to keep the AI consistant.
        /// <para>If also used by duel flow the whole duel outcome will be fixed.</para>
        /// </summary>
        public ManagedRandom Random;

        public AIPlayer(DuelFlow flow, Player player, ManagedRandom random) {
            Player = player;
            Flow = flow;
            Random = random;
        }


        /// <summary>
        /// Method to run on a new thread.
        /// <para>Executes the ai actions.</para>
        /// </summary>
        public void ThreadStart() {

            // Stop the the thread when the duel finishes.
            while (Flow.State.Unfinished) {

                // The AI doesn't perform actions on opponent turn.
                if (Flow.State.CurrentTurn.Owner != Player) {
                    System.Threading.Thread.Sleep(500);
                    continue;
                }

                // Force a revive if have is empty and there are cards in grave.
                if (Player.Hand.Count == 0 && Player.Grave.Count > 0) {

                    Flow.EnqueueCommand("CMDRevive", Player);
                    continue;
                }

                // The list of ranomd commands the ai might take. Weighting charge as more common.
                List<Action> commands = new List<Action> {
                    //() => Flow.EnqueueCommand("CMDCharge", Player),
                    //() => Flow.EnqueueCommand("CMDCharge", Player),
                    //() => Flow.EnqueueCommand("CMDCharge", Player)
                };

                //TODO: Leveling

                // Check to see if a card could be used or should be used.
                foreach (var card in Player.Hand) {


                    // Get the cost of the card.
                    int cost = card.ImprintCost + card.GraveCost;
                    var manatype = card.Imprint.ManaType;
                    // If the ai doesn't have the mana type they can't play the card at all.
                    if (!Player.ManaPool.HasType(manatype)) {
                        continue;
                    }
                    // If the ai can't play the cost no reason to try and play it.
                    if (Player.ManaPool[manatype].ManaCount < cost) {
                        continue;
                    }

                    foreach (var slot in Player.Field.GetEmptySlots()) {
                        commands.Add(() => {
                            Flow.EnqueueCommand("CMDPlayCard", Player, card.Index, slot.Index);
                        });
                    }
                    commands.Add(() => {
                        Flow.EnqueueCommand("CMDDiscard", Player, card.Index);
                    });
                }

                // If there are no commands the ai just charges. However currently this should never be the case because commands already starts with values.
                if (commands.Count == 0) {
                    Flow.EnqueueCommand("CMDCharge", Player);
                    continue;
                }

                // Send a command to the server.
                var command = Random.NextItem(commands);
                command.Invoke();

                // Sleep before sending another command.
                // TODO: Signals
                System.Threading.Thread.Sleep(100);

            }

        }


    }
}
