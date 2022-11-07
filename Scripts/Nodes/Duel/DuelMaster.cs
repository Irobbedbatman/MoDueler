using Godot;
using MoDueler.Backend;
using MoDueler.Lua;
using MoDueler.Network;
using MoDueler.Resources;
using MoonSharp.Environment;
using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;

namespace MoDueler.Nodes {

    [MoonSharpUserData]
    public class DuelMaster : Node {

        /// <summary>
        /// The <see cref="Node"/> that contains all the cards the player can use.
        /// <para>This is always for the local player as we never display the opponent's hand.</para>
        /// </summary>
        public HandController PlayerHand { get; private set; } = null;

        /// <summary>
        /// The function listeners subscribed to messages sent by <see cref="HandController.CardPlayed"/> through <see cref="PlayerHand"/>.
        /// </summary>
        private readonly Dictionary<Area2D, ClosureList> HandListeners = new Dictionary<Area2D, ClosureList>();

        /// <summary>
        /// The tool used to link the remote duel flow's targets with the targets on the client.
        /// </summary>
        public readonly NetworkLinker Linker = new NetworkLinker();

        /// <summary>
        /// The table to store current use lua state without certain methods needing to be reloaded..
        /// </summary>
        public Table CurrentLua = ClientSideLua.Environment.TemporaryTable(false);

        /// <summary>
        /// Cehck to ensure setup is only called once.
        /// </summary>
        private bool IsSetup = false;

        /// <summary>
        /// A list of actions that will be deferred to the next frame on the main thread.
        /// </summary>
        private readonly List<Action> deferredActions = new List<Action>();

        /// <summary>
        /// Action invoked in <see cref="EndDuel"/> to cleanup anything neccasary.
        /// </summary>
        public Action CleanUp = null; 


        public DuelMaster() { }

        /// <summary>
        /// Assign the provider that will allow this duel instance to respond to the backend duelflow.
        /// </summary>
        public void SetProvider(GameProvider provider) {
            // Listen to backend actions.
            provider.RecieveCommand += (cmd, args) => {             
                FlowListener( cmd, args);
            };
            // Update the lua to use the information provided by the provider.
            CurrentLua["LocalId"] = provider.LocalID;
            CurrentLua["SendCommand"] = (System.Action<string, DynValue[]>)provider.SendCommand;
        }

        /// <summary>
        /// Loads the specified lua file and executes the specified function.
        /// </summary>
        /// <param name="callName">The name of the callback to execute. This is parially the name of the file and the full name of the function.</param>
        /// <param name="args">RThe arguments passed to the lua function.</param>
        /// <returns>The return value of the lua function; <c>null</c> if there no result or failed to execute.</returns>
        public DynValue CallBack(string callName, params DynValue[] args) {

            // Note time to check callback performance.
            var elapsedBefore = OS.GetTicksMsec();

            GD.Print("Callback [" + callName + "]  Start");

            // Create an execution envornment.
            var embededTable = LuaEnvironment.TemporaryEmbededTable(CurrentLua, true);
            // Find the lua file to exeucte.
            var file = ResourceFiles.FindFile("Callback_" + callName + ".lua");

            if (file == null) {
                GD.Print("Callback_" + callName + " couldn't be found. Perhaps it hasn't been impmenented yet.");
                return null;
            }

            DynValue result;
            try {
                // Parse the file.
                embededTable.OwnerScript.DoFile(file, embededTable, "Callback: " + callName);
                // Look for the function expected to run.
                var mainFunction = embededTable[callName] as Closure;
                result = mainFunction.Call(args);
            }
            // Display an error if the callback failed during execution.
            catch (ScriptRuntimeException e) {
                GD.Print("Call Runtime[" + callName + "]  Failed");
                if (e.DecoratedMessage == null)
                    GD.PrintErr(e.StackTrace);
                else
                    GD.PrintErr("Reason:" + e.DecoratedMessage);
                return null;
            }
            // Display an error if the callback failed during parsing.
            catch (SyntaxErrorException e) {
                GD.PrintErr("Call Syntax[" + callName + "]  Failed");
                GD.PrintErr("Reason:" + e.DecoratedMessage);
                return null;
            }

            // Display the callback performance.
            var elaspedAfter = OS.GetTicksMsec();
            GD.Print("Call [" + callName + "] Finshed Time Taken: (ms): " + (elaspedAfter - elapsedBefore));

            return result;
        }


        public void EndDuel() {

            CleanUp?.Invoke();

            // TODO: EndDUel();
            GD.Print("Returning to Lobby");
            Node lobby = ResourceLoader.Load<PackedScene>("res://Scenes/Lobby.tscn").Instance();

            // Change scene to the duel.
            // TODO: Use SceneManager
            GetTree().CurrentScene.QueueFree();
            GetTree().Root.AddChild(lobby);
            GetTree().CurrentScene = lobby;

        }

        // Called when the node enters the scene tree for the first time.
        public override void _Ready() {
            // Calls setup for testing purposes if not done so by set provided earlier.
            Setup();
        }

        public override void _Process(float delta) {

            // Clone out the deferred actions so that they can manipulated during execution.
            Action[] clone = null;
            lock (deferredActions) {
                clone = deferredActions.ToArray();
                deferredActions.Clear();
            }

            // Calls action on the main thread.
            foreach (var action in clone) {
                action?.Invoke();
            }

        }

        /// <summary>
        /// Set's up the lua for the current duel.
        /// </summary>
        public void Setup() {

            // Ensure Setup is only run once.
            if (IsSetup)
                return;
            else
                IsSetup = true;

            // Add the player hand controller.
            PlayerHand = new HandController();
            AddChild(PlayerHand);
            CurrentLua["PlayerHand"] = PlayerHand;
            // Add trigger support for when a card is played.
            PlayerHand.CardPlayed = (card, target, position) => {
                if (HandListeners.TryGetValue(target, out ClosureList events))
                    events.CallAll(card, target);
            };

            // Add access to some of the methods and properties of this class inside lua.
            CurrentLua["Include"] = (Func<string, Table>)IncludeLuaFile;
            CurrentLua["Linker"] = Linker;
            CurrentLua["EndDuel"] = (Action)EndDuel;
            CurrentLua["IsBoardFlippped"] = false;
            CurrentLua["Scene"] = new NodeProxy<Node>(GetNode("Scene"));
            CurrentLua["GetAllMetaTables"] = (Func<Table>)(delegate { return CurrentLua.MetaTable; });

            // Adds a listener to the hand that executes when the card is fropped on the specified target.
           CurrentLua["AddHandListener"] = (Action<Area2D, Closure>)((target, function) => {
                if (!HandListeners.TryGetValue(target, out var list)){
                    list = new ClosureList();
                    HandListeners.Add(target, list);
                }
                list.Add(function);
            });

            // Removes a listener added thorugh 'AddHandListener'.
            CurrentLua["RemoveHandListener"] = (System.Action<Area2D, Closure>)((target, function) => { 
                if (HandListeners.TryGetValue(target, out var list)){
                    list.Remove(function);
                }
            });

            // Run the callback that will populate the scene with assets.
            CallBack("DuelSetup");

            // TODO: Infrom game that the duel is setup and scene is ready.
        }

        /// <summary>
        /// Returns the global table from a provided lua file.
        /// </summary>
        public Table IncludeLuaFile(string fileName) {
            // Load the lua file in a closed environment.
            var tempTable = LuaEnvironment.TemporaryEmbededTable(CurrentLua, true);
            var file = ResourceFiles.FindFile(fileName);
            try {
                // Execute the code in the file; populating the environement with any globals.
                ClientSideLua.Environment.AsScript.DoFile(file, tempTable);
            }
            // Display a message if the there was an issue running the lua file.
            catch (ScriptRuntimeException e) {
                GD.PrintErr("Include Runtime[" + fileName + "]  Failed");
                if (e.DecoratedMessage == null)
                    GD.PrintErr(e.StackTrace);
                else
                    GD.PrintErr("Reason:" + e.DecoratedMessage);
            }
            // DIsplay a message if there was an issue parsing the lua file.
            catch (SyntaxErrorException e){
                GD.PrintErr("Include Syntax[" + fileName + "]  Failed");
                GD.PrintErr("Reason:" + e.DecoratedMessage);
            }
            return tempTable;
        }

        /// <summary>
        /// The method that is called when the client recieves flow output.
        /// </summary>
        /// <param name="command">the command to execute.</param>
        /// <param name="args">The arguments for the command</param>
        public void FlowListener(string command, DynValue[] args) {
            Console.WriteLine("Command Recieved: " + command);
            // Defer the callback until the next game frame on the main thread.
            lock (deferredActions) {
                deferredActions.Add(delegate {
                    CallBack(command, args);
                });
            }

        }

    }
}
