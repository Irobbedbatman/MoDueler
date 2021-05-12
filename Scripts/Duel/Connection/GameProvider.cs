using MoonSharp.Interpreter;
using System;

namespace MoDueler.Scripts.Duel.Connection {
    public abstract class GameProvider {

        /// <summary>
        /// The id of the player on this machine.
        /// </summary>
        public readonly string LocalID = "";

        protected GameProvider(string id) {
            LocalID = id;
        }

        /// <summary>
        /// The method used to send commands to a <see cref="MoDuel.DuelFlow"/>
        /// </summary>
        public abstract void SendCommand(string commandId, params DynValue[] args);

        /// <summary>
        /// The action that is invoked when a message is recieved from a <see cref="MoDuel.DuelFlow"/>.
        /// </summary>
        public Action<string, DynValue[]> RecieveCommand;

    }
}
