using MoDuel;
using MoDuel.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MoDueler.Backend {
    public class AIPlayer {

        public Player Player;
        public DuelFlow Flow;
        public ManagedRandom Random;

        public AIPlayer(DuelFlow flow, Player player, ManagedRandom random) {
            Player = player;
            Flow = flow;
            Random = random;
        }

        public void ThreadStart() {

            while (Flow.State.Unfinished) {

                if (Flow.State.CurrentTurn.Owner != Player) {
                    Thread.Sleep(500);
                    continue;
                }


                if (Player.Hand.Count == 0 && Player.Grave.Count > 0) {
                    Flow.EnqueueCommand("CMDRevive", Player);
                    continue;
                }

                // The list of ranomd commands the ai might take. Weighting charge as more common.
                List<Action> commands = new List<Action> {
                    () => Flow.EnqueueCommand("CMDCharge", Player),
                    () => Flow.EnqueueCommand("CMDCharge", Player),
                    () => Flow.EnqueueCommand("CMDCharge", Player)
                };

                //TODO: Leveling

                foreach (var card in Player.Hand) {
                    // Get the cost of the card.
                    int cost = card.ImprintCost + card.GraveCost;
                    var manatype = card.Imprint.ManaType;
                    // If the ai doesn't have the mana type they can't play the card at all.
                    if (!Player.ManaPool.HasType(manatype))
                        continue;
                    // If the ai can't play the cost no reason to try and play it.
                    if (Player.ManaPool[manatype].ManaCount < cost)
                        continue;

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

                var command = Random.NextItem(commands);
                command.Invoke();

            }

        }


    }
}
