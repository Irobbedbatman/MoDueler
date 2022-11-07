using LiteNetLib;
using LiteNetLib.Utils;
using MoDueler.Backend;
using MoonSharp.Interpreter;
using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace MoDueler.Network {

    class NetClient {

        /// <summary>
        /// The manager of network functionity on the client side.
        /// </summary>
        private readonly NetManager client;

        /// <summary>
        /// The <see cref="NetPeer"/> that is hosting the duel and this cleint has connected to.
        /// </summary>
        private NetPeer server;

        /// <summary>
        /// The action that will execute when the duel is ready.
        /// </summary>
        public Action DuelReady = null;

        /// <summary>
        /// The game provider that can be used by a <see cref="MoDueler.Nodes.DuelMaster"/> to converse with the backend.
        /// </summary>
        private RemoteGameProvider Provider = null;

        // Wether the poll thread is running. Set to false to terminate the host.
        public bool ThreadRunning { get; private set; } = true;

        public NetClient() {
            
            // Create a listener that the net manager will use.
            EventBasedNetListener listener = new EventBasedNetListener();
            listener.NetworkReceiveEvent += Listener_NetworkReceiveEvent;
            listener.PeerConnectedEvent += Listener_PeerConnectedEvent;

            client = new NetManager(listener) {
                // TODO: In godot 4 test to see if IPv6 is supported.
                IPv6Enabled = IPv6Mode.Disabled
            };

            client.Start();
            
            // TODO: connect to server without using global settings.
            server = client.Connect(GlobalSettings.HostAddress, GlobalSettings.HostPort, new NetDataWriter());

            // Start polling for any network events.
            Thread thread = new Thread(PollThread);
            thread.Start();
        }

        private void Listener_PeerConnectedEvent(NetPeer peer) {
            Console.WriteLine("Client connected to host.");

            // TODO: Check if setting peer on connect worked.
            server = peer;
            // No that the client has connected it can send it's player info.
            SendPlayerInfo();

        }

        /// <summary>
        /// The thread that will poll for network events.
        /// </summary>
        public void PollThread() {

            while (ThreadRunning) {
                // TODO: See if there is a way to poll events without thread.sleep.
                client.PollEvents();
                Thread.Sleep(15);
            }

        }

        /// <summary>
        /// Stop any networking and also terminates the polling thread.
        /// </summary>
        public void Terminate() {
            //TODO: Check to see if there is a better termination.
            ThreadRunning = false;
            Godot.GD.Print("Terminating Client");
            client.Stop();
        }


        private void Listener_NetworkReceiveEvent(NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod) {

            // The first part of the packet determines what the packet is.
            MessagesID messageID = (MessagesID)reader.GetInt();

            Godot.GD.Print("Message Recieved: " + messageID.ToString());

            switch (messageID) {

                // If it is a request perform that request through the provider.
                case MessagesID.Request:
                    var request = reader.GetString();

                    Godot.GD.Print("Request Recieved: " + request);

                    var args = MoDuel.Networking.DynValueSerialization.DeserializeArray(reader);
                    Provider?.RecieveCommand?.Invoke(request, args);
                    break;
                // If it is a game ready notification move to the duel screen.
                case MessagesID.GameReady:
                    Provider = new RemoteGameProvider(reader.GetString()) {
                        SendAction = SendCommand
                    };
                    DuelReady?.Invoke();
                    break;

            }

            // TODO: Consider using reader/writer Recycling elsewhere.
            reader.Recycle();


        }


        public RemoteGameProvider GetRemoteGameProvider() => Provider;

        /// <summary>
        /// Sends a command and any arguments to the server.
        /// </summary>
        public void SendCommand(string command, DynValue[] args) {

            NetDataWriter writer = new NetDataWriter();
            writer.Put((int)MessagesID.Request);
            writer.Put(command);
            MoDuel.Networking.DynValueSerialization.SerializeArray(args, writer);
            server.Send(writer, DeliveryMethod.ReliableUnordered);

        }

        /// <summary>
        /// Sends all the information the backend needs to know about this player.
        /// <c>Information is taken from their player profile.</c>
        /// </summary>
        public void SendPlayerInfo() {

            string id = PlayerProfile.UserId;
            string heroId = PlayerProfile.CurrentHeroId;
            string[] cards = PlayerProfile.DeckCardNames;
            string[] manas = PlayerProfile.ManaNames;

            NetDataWriter writer = new NetDataWriter();
            writer.Put((int)MessagesID.SendPlayerSettings);
            writer.Put(id);
            writer.Put(heroId);
            writer.PutArray(cards);
            writer.PutArray(manas);

            server.Send(writer, DeliveryMethod.ReliableUnordered);

        }



    }
}
