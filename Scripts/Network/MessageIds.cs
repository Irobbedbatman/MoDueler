using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoDueler.Network {

    public enum MessagesID {
    
        Request, // Send call to client or command to backend.
        SendPlayerSettings, // Send all the inital cards and hero to the backend.
        GameReady, // Send to clients to tell them that the duel has started.
        ConfirmUserId, // Tells a client that their id has bene confirmed.
    }

}
