using LiteNetLib.Utils;
using MoonSharp.Interpreter;
using System;

namespace MoDueler.Backend {
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


        /// <summary>
        /// Cleans Dynvalues of their ownership so the values can be sent from backend to frontend and vise-versa.
        /// </summary>
        protected DynValue[] CleanArray(DynValue[] values) {
            var writer = new NetDataWriter();
            MoDuel.Networking.DynValueSerialization.SerializeArray(values, writer);
            return MoDuel.Networking.DynValueSerialization.DeserializeArray(new NetDataReader(writer));
        }

    }
}
