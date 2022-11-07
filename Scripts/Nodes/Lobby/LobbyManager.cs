using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;
using MoDueler.Backend;
using MoDueler.Network;

namespace MoDueler.Nodes.Lobby {
    public class LobbyManager : Control {

        /// <summary>
        /// The object that will container the duel list.
        /// </summary>
        [Export]
        private NodePath DuelsContainerPath = "";

        /// <summary>
        /// The vertical box containing all the elements. Foudn using <see cref="DuelsContainerPath"/>.
        /// </summary>
        private VBoxContainer publicDuels = null;

        /// <summary>
        /// Action used to terminate net threads cleanly.
        /// </summary>
        private Action terminateNetThreads;

        /// <summary>
        /// Actions that need to be performed in th next main thread process.
        /// </summary>
        private Action deferedActions;
        /// <summary>
        /// The lock object for <see cref="deferedActions"/>.
        /// </summary>
        private object deferedActionsLock = new object();

        /// <summary>
        /// The template  object that will be created for each availle duel.
        /// </summary>
        private static PackedScene duelEntry = GD.Load<PackedScene>("res://SceneObjects/LobbyDuelEntry.tscn");

        public override void _Ready() {

            publicDuels = GetNode<VBoxContainer>(DuelsContainerPath);
            RefreshDuelEntries();

        }

        /// <summary>
        /// The method called by the duel buttons when they are pressed.
        /// </summary>
        public void StartDuel() {

            // Create the duel scene.
            DuelMaster master = ResourceLoader.Load<PackedScene>("res://Scenes/Duel.tscn").Instance<DuelMaster>();
            master.Setup();

            // Make the local provider and start the duel.
            MakeBasicProvider(master);

            // Change scene to the duel.
            // TODO: Use SceneManager
            GetTree().CurrentScene.QueueFree();
            GetTree().Root.AddChild(master);
            GetTree().CurrentScene = master;


        }

        public void HostDuel() {
            // Create the duel scene.
            DuelMaster master = ResourceLoader.Load<PackedScene>("res://Scenes/Duel.tscn").Instance<DuelMaster>();
            master.Setup();

            // Create the network host.
            NetHost host = new NetHost(GlobalSettings.HostPort);

            // If the application ends early terminate the network threads.
            // TODO: Test if neccassary. Although it should result in cleaner termination.
            terminateNetThreads += () => { host.Terminate(); };

            // When the duel ends the network host is no longer needed.
            master.CleanUp += () => { host.Terminate(); };

            // When the duel is ready move to the duel scene.
            host.DuelReady = () => {

                master.SetProvider(host.GetNewLocalProvider());

                lock (deferedActionsLock) {

                    // Changing the scene needs to occur on the main thread.
                    deferedActions += () => {

                        // Change scene to the duel.
                        // TODO: Use SceneManager
                        GetTree().CurrentScene.QueueFree();
                        GetTree().Root.AddChild(master);
                        GetTree().CurrentScene = master;

                    };
                }

                // TODO: Termiante host when duel ends.
            };

        }

        public void JoinRemoteDuel() {
            // Create the duel scene.
            DuelMaster master = ResourceLoader.Load<PackedScene>("res://Scenes/Duel.tscn").Instance<DuelMaster>();
            master.Setup();

            // Create the network client.
            NetClient client = new NetClient();

            // If the application ends early terminate the network threads.
            // TODO: Test if neccassary. Although it should result in cleaner termination.
            terminateNetThreads += () => { client.Terminate(); };

            // When the duel ends the network client is no longer needed.
            master.CleanUp += () => { client.Terminate(); };

            // When the duel is ready move to the duel scene.
            client.DuelReady = () => {

                master.SetProvider(client.GetRemoteGameProvider());

                // Change scene to the duel.
                // TODO: Use SceneManager

                lock (deferedActionsLock) {

                    // Changing the scene needs to occur on the main thread.
                    deferedActions += () => {

                        // Change scene to the duel.
                        // TODO: Use SceneManager
                        GetTree().CurrentScene.QueueFree();
                        GetTree().Root.AddChild(master);
                        GetTree().CurrentScene = master;

                    };
                }

                // TODO: Termiante client when duel ends.

            };

        }

        public override void _Process(float delta) {
            // RUn any deferred actions.
            // TODO: Lock defered actions.

            lock (deferedActionsLock) {
                deferedActions?.Invoke();
                deferedActions = null;
            }
        }

        public override void _Notification(int what) {
            if (what == MainLoop.NotificationWmQuitRequest) {
                terminateNetThreads?.Invoke();
                terminateNetThreads = null;
            }
        }


        /// <summary>
        /// Gets the new list of duel entries and re-populates the visible list.
        /// </summary>
        private void RefreshDuelEntries() {

            // TODO: Populate list from a centralized source.

            foreach (Node child in publicDuels.GetChildren()) {
                child.QueueFree();
            }

            NewDuelEntry();
            NewDuelEntry("Host\nPort: " + GlobalSettings.HostPort, "HostDuel");
            NewDuelEntry("Join\n" + GlobalSettings.HostAddress + ":" + GlobalSettings.HostPort, "JoinRemoteDuel");

        }

        /// <summary>
        /// Create a new duel entry to be listed.
        /// </summary>
        private void NewDuelEntry(string title = "VS AI", string func = "StartDuel") {
            var newDuel = duelEntry.Instance<Button>();
            newDuel.Connect("pressed", this, func);

            var label = newDuel.GetNode<Label>("LobbyDuelEntry/Label");
            label.Text = title;

            publicDuels.AddChild(newDuel);
        }


        /// <summary>
        /// Makes the backend and starts it. Iforming the local side how to connect with it.
        /// </summary>
        private System.Threading.Thread MakeBasicProvider(DuelMaster duelMaster) {

            // Enable backend loading.
            DuelFlowSetup.SetupLoading();
            // Create the flow environement and players.
            var env = DuelFlowSetup.SetupEnvironment();
            var player1 = DuelFlowSetup.CreatePlayer1(env);
            var player2 = DuelFlowSetup.CreatePlayer2(env);

            // Creat the flow.
            var flow = DuelFlowSetup.CreateFlow(env, player1, player2);

            // Link the backend and the front end using a new provider.
            LocalGameProvider provider = new LocalGameProvider(flow, player1);
            duelMaster.SetProvider(provider);

            // Create the ai.
            AIPlayer ai = new AIPlayer(flow, player2, new MoDuel.Tools.ManagedRandom());
            System.Threading.Thread aiThread = new System.Threading.Thread(new System.Threading.ThreadStart(
                ai.ThreadStart
                ));

            // Start the ai.
            aiThread.Start();

            // STart the duel.
            var thread = DuelFlowSetup.StartThread(flow, env);
            return thread;

        }



    }
}
