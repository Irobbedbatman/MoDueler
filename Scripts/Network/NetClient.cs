using LiteNetLib;
using LiteNetLib.Utils;
using System.Runtime.InteropServices;

namespace MoDueler.Network {

    class NetClient {

        private readonly EventBasedNetListener listener = new EventBasedNetListener();
        private readonly NetManager client;
        private NetPeer server;
        private readonly NetDataWriter writer = new NetDataWriter();

        public NetClient() {
            client = new NetManager(listener);
            client.Start();

            listener.PeerConnectedEvent += (peer) => {

            };
        }

        public void Connect(string address, short port, string key = "") {
            server = client.Connect(address, port, key);
            
        }

        public void SendMessage(string packet) {
            writer.Put(packet);
            server.Send(writer, DeliveryMethod.ReliableOrdered);
        }



    }
}
