using LiteNetLib;
using LiteNetLib.Utils;
using MoDuel;
using MoDuel.Data;
using MoDuel.Heroes;
using MoDuel.Mana;
using MoDueler.Backend;
using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MoDueler.Network {
    public class NetHost {

        /// <summary>
        /// The connection manager.
        /// </summary>
        private readonly NetManager host;
        /// <summary>
        /// One of the players of the duel.
        /// </summary>
        private Player player1, player2;
        /// <summary>
        /// The NetPeer of the remote player.
        /// </summary>
        private NetPeer player2Peer = null;
        /// <summary>
        /// The cards that player2 will use in their deck.
        /// </summary>
        private string[] player2cards;
        /// <summary>
        /// The environment the duel will run from.
        /// </summary>
        private EnvironmentContainer environment;

        /// <summary>
        /// The action that will execute when the duel is ready.
        /// </summary>
        public Action DuelReady = null;

        /// <summary>
        /// The duel flow instance. Not created until both players are setup.
        /// </summary>
        private DuelFlow Flow = null;

        /// <summary>
        /// Wether the poll thread is running. Set to false to terminate the thread.
        /// <para><see cref="Terminate"/> will set the value to false.</para>
        /// </summary>
        public bool ThreadRunning { get; private set; } = true;

        public NetHost(int? port = null) {

            // Enable asset loading and setup the environemnt.
            DuelFlowSetup.SetupLoading();
            environment = DuelFlowSetup.SetupEnvironment();

            // Create the local player.
            CreatePlayer1();

            // Listen to network events.
            EventBasedNetListener listener = new EventBasedNetListener();
            listener.NetworkReceiveEvent += Listener_NetworkReceiveEvent;
            listener.PeerConnectedEvent += Listener_PeerConnectedEvent;
            listener.ConnectionRequestEvent += Listener_ConnectionRequestEvent;
            listener.PeerDisconnectedEvent += Listener_PeerDisconnectedEvent;

            // Create the host with the created listener.
            host = new NetManager(listener) {
                IPv6Enabled = IPv6Mode.Disabled
            };

            // Check to see if a port was and start the host process.
            if (port == null)
                host.Start();
            else
                host.Start(port.Value);

            // Start the network thread.
            var thread = new Thread(PollThread);
            thread.Start();
        }

        /// <summary>
        /// The thread that will poll for network events.
        /// </summary>
        public void PollThread() {

            while (ThreadRunning) {
                // TODO: See if there is a way to poll events without thread.sleep.
                host.PollEvents();
                Thread.Sleep(15);
            }

        }

        public void Terminate() {
            //TODO: Check to see if there is a better termination.
            ThreadRunning = false;
            Godot.GD.Print("Terminating Host");
            host.Stop();
        }

        public void StartFlow() {

            if (player1 == null || player2 == null) {
                Godot.GD.Print("Can't start hosted duel as one of the players has not been defined yet.");
                return;
            }

            Flow = DuelFlowSetup.CreateFlow(environment, player1, player2);

            // Handle the backend messages to specific players or everyone listening.
            Flow.OutBoundDelegate += (s, e) => {
                SendToAllPlayers(e.RequestId, e.Arguments);
            };

            player2.OutBoundDelegate += (s, e) => {
                SendToPlayer(player2Peer, e.RequestId, e.Arguments);
            };

            // Inform listeners that the game is now ready.
            DuelReady?.Invoke();

            // Tell all clients that the game is ready.
            NetDataWriter writer = new NetDataWriter();
            writer.Put((int)MessagesID.GameReady);
            // TODO: FInd a better spot to confirm player2's userid.
            writer.Put(player2.UserId);
            host.SendToAll(writer, DeliveryMethod.ReliableOrdered);

            DuelFlowSetup.StartThread(Flow, environment,
                DuelFlowSetup.BlankCards(PlayerProfile.DeckCardNames),
                DuelFlowSetup.BlankCards(player2cards));

            // Disable loading is recommended to keep the game 100% preloaded. Might not be kept longterm.
            ContentLoader.DeregisterLoads(environment.Lua);


        }

        /// <summary>
        /// Sends a request to anyone listening.
        /// </summary>
        public void SendToAllPlayers(string request, DynValue[] args) {
            // TODO: Keep a track of all sent event so that new arrivals can get all messages.
            NetDataWriter writer = new NetDataWriter();
            writer.Put((int)MessagesID.Request);
            writer.Put(request);
            MoDuel.Networking.DynValueSerialization.SerializeArray(args, writer);
            host.SendToAll(writer, DeliveryMethod.ReliableOrdered);
        }

        /// <summary>
        /// Sends the data to <see cref="player2Peer"/>.
        /// </summary>
        public void SendToPlayer(NetPeer playerPeer, string request, DynValue[] args) {
            NetDataWriter writer = new NetDataWriter();
            writer.Put((int)MessagesID.Request);
            writer.Put(request);
            MoDuel.Networking.DynValueSerialization.SerializeArray(args, writer);
            playerPeer.Send(writer, DeliveryMethod.ReliableOrdered);
        }

        public LocalGameProvider GetNewLocalProvider() => new LocalGameProvider(Flow, player1);

        private void Listener_PeerDisconnectedEvent(NetPeer peer, DisconnectInfo disconnectInfo) {
            // TODO: Handle disconnects and reconnects.
            Godot.GD.Print("Peer Disconnected");
        }

        private void Listener_ConnectionRequestEvent(ConnectionRequest request) {
            // TODO: Ensure request has unique user id.
            request.Accept();
        }

        private void Listener_NetworkReceiveEvent(NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod) {

            // Perform different actions based on the type of message.
            MessagesID messageId = (MessagesID)reader.GetInt();

            Godot.GD.Print("Message Recieved: " + messageId.ToString());

            switch (messageId) {

                case MessagesID.SendPlayerSettings:
                    RecievePlayerSettings(peer, reader);
                    break;

                case MessagesID.Request:
                    // TODO: ensure request is from player2.
                    Flow.EnqueueCommand(reader.GetString(), player2, MoDuel.Networking.DynValueSerialization.DeserializeArray(reader));
                    break;
            }

            reader.Recycle();

        }

        /// <summary>
        /// Creates the player 1 form the local players profile settings.
        /// </summary>
        public void CreatePlayer1() {

            Hero hero = ContentLoader.LoadHero(PlayerProfile.CurrentHeroId, environment.Content, environment.Lua);
            player1 = new Player(PlayerProfile.UserId, hero, new ManaPool(PlayerProfile.Manas));

        }

        /// <summary>
        /// Reads the player2s settings from the packet data.
        /// </summary>
        private void RecievePlayerSettings(NetPeer peer, NetPacketReader reader) {
            // TODO: Don't assume first connector is the player.
            player2Peer = peer;

            // Get the contents of the message.
            string userId = reader.GetString();
            string heroId = reader.GetString();
            string[] cards = reader.GetStringArray();
            string[] manaNames = reader.GetStringArray();

            // Convert the names of manas to their value.
            ManaType[] manas = manaNames.Select((m) => { return new ManaType(m); }).ToArray();

            // Load the hero information for the player.
            Hero hero = ContentLoader.LoadHero(heroId, environment.Content, environment.Lua);

            // Create the player.
            player2 = new Player(userId, hero, new ManaPool(manas));

            // Store a reference to the cards the player wants to use.
            player2cards = cards;

            // Once both players are setup the duel can commence.
            StartFlow();
        }

        private void Listener_PeerConnectedEvent(NetPeer peer) {

            // TODO: Inform the conencted client of the potential lobby state or any other details here.

            Godot.GD.Print("Peer Connected");

        }

    }
}
