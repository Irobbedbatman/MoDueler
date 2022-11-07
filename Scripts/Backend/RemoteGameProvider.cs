using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoDueler.Backend {
    public class RemoteGameProvider : GameProvider {

        public Action<string, DynValue[]> SendAction;

        public RemoteGameProvider(string id) : base(id) {}

        public override void SendCommand(string commandId, params DynValue[] args) {
            if (SendAction != null) {
                SendAction(commandId, CleanArray(args));
            }
        }

    }
}
