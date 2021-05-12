using MoDuel;
using MoonSharp.Interpreter;
using System.Linq;

namespace MoDueler.Scripts.Duel.Connection {
    public class LocalGameProvider : GameProvider {

        public readonly DuelFlow Flow;
        public readonly Player LocalPlayer;

        public LocalGameProvider(DuelFlow flow, Player localPlayer) : base(localPlayer.UserId) {
            Flow = flow;
            LocalPlayer = localPlayer;
            // Read the events to all players and the local player.
            Flow.OutBoundDelegate += (s, e) => {
                RecieveCommand?.Invoke(e.RequestId, e.Arguments);
            };
            LocalPlayer.OutBoundDelegate += (s, e) => {
                RecieveCommand?.Invoke(e.RequestId, e.Arguments);
            };
        }

        /// <inheritdoc/>
        public override void SendCommand(string commandId, params DynValue[] args) {
            Flow.EnqueueCommand(commandId, LocalPlayer, args.Cast<object>().ToArray());
        }
    }
}
