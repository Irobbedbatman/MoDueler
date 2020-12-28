using Godot;
using MoDueler.Duel;
using MoonSharp.Environment;
using MoonSharp.Interpreter;
using MoDueler.Nodes;
using MoDueler.Resources;
using MoDueler.Cards;
using MoDuel.Data;
using System.Collections.Generic;

public class DuelMaster : Node {

    /// <summary>
    /// The clientside filed that will display all the currently alive monsters.
    /// </summary>
    public BattleField Field { get; private set; } = null;

    /// <summary>
    /// The <see cref="Node"/> that contains all the cards the player can use.
    /// <para>This is always for the local player as we never display the opponent's hand.</para>
    /// </summary>
    public static HandController PlayerHand { get; private set; } = null;

    /// <summary>
    /// The targeter and image for the opponent.
    /// </summary>
    public Area2D Advisarry;

    /// <summary>
    /// The <see cref="Node"/> that displays the background.
    /// </summary>
    public Sprite Background;

    /// <summary>
    /// The object that determines the game flow. Will be moved to a server or will be run through some other system in the future,
    /// </summary>
    public SimplifiedFlow Flow = new SimplifiedFlow();

    /// <summary>
    /// The player we will use in Flow for local player.
    /// </summary>
    public SimplifiedPlayer LocalPlayer;

    /// <summary>
    /// The player we will use in Flow for the AI.
    /// </summary>
    public SimplifiedPlayer AIPlayer;

    /// <summary>
    /// A rudimenty system to link <see cref="SimplifiedSlot"/>'s Index and slots held on the local player side.
    /// </summary>
    public Dictionary<uint, int> BadSlotLinking = new Dictionary<uint, int>();
    /// <summary>
    /// A rudimenty system to link <see cref="SimplifiedSlot"/>'s Index and slots held on the local player side but this time in reverse.
    /// </summary>
    public Dictionary<int, uint> BadSlotLinkingReverse = new Dictionary<int, uint>();

    public DuelMaster() { }

    public HandCard CreateCard(CardMetaData cardData) {
        Table results = LuaEnvironment.TemporaryTable();
        LuaEnvironment.AsScript.DoFile(ResourceFiles.FindFile("*Card.lua"), results);
        var method = results["Create"] as Closure;
        return method.Call(cardData).ToObject<HandCard>();
    }

    public Sprite CreateCreature(CardMetaData cardData) {
        Table results = LuaEnvironment.TemporaryTable();
        LuaEnvironment.AsScript.DoFile(ResourceFiles.FindFile("*Creature.lua"), results);
        var method = results["Create"] as Closure;
        return method.Call(cardData).ToObject<Sprite>();
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready() {
        PrepareScene();

        // Listen to the flow output.
        Flow.Output += FlowListener;

        // Setup the players.
        LocalPlayer = new SimplifiedPlayer();
        AIPlayer = new SimplifiedPlayer();

        // Implement the player hand card played.
        PlayerHand.CardPlayed = (card, target, position) => {
            //TODO: Card played on advisarry.
            if (target == Advisarry) {
            
            }
            // If the player selects the field we tell the flow to play the card in the hovered slot.
            else if (target.GetParent() == Field) {
                uint index = BadSlotLinkingReverse[Field.GetSlot(position)];
                if (Flow.Linker.TryGetID(card, out uint cardIndex)) {
                    Flow.PlayCard(LocalPlayer, cardIndex, index);
                }
            }
        };

        // Start the duel flow.
        Flow.Start(LocalPlayer, AIPlayer);

    }

    /// <summary>
    /// Method that is called when the flow has messages for us.
    /// </summary>
    /// <param name="flow">The duelflow that sent the messages.</param>
    /// <param name="args">The messages and target for tose messages.</param>
    public void FlowListener(object flow, FlowOutputEventArgs args) {
        var messages = args.Messages;
        // We need only listen if we are the target for the messages or there is no direct target.
        if (args.Target == null || args.Target == LocalPlayer) {
            // Use the first message (if it exists) to determine which local method to use and provide that method with the messages from teh flow..
            if (messages.Length > 0) {
                switch (messages[0]) {
                    case "LinkSlot":
                        LinkSlot(messages);
                        break;
                    case "AddHandCard":
                        AddHandCard(messages);
                        break;
                    case "RemHandCard":
                        RemHandCard(messages);
                        break;
                    case "SpawnCreature":
                        SpawnCreature(messages);
                        break;
                    case "KillCreature":
                        KillCreature(messages);
                        break;
                    case "EndGame":
                        EndGame(messages);
                        break;
                }

            }
        }
    }

    #region FlowOutputEvent Methods

    /// <summary>
    /// Links the slot from the duel flow to slot created locally.
    /// </summary>
    /// <param name="args"></param>
    public void LinkSlot(string[] args) {
        BadSlotLinking.Add(uint.Parse(args[2]), int.Parse(args[1]));
        BadSlotLinkingReverse.Add(int.Parse(args[1]), uint.Parse(args[2]));
    }

    /// <summary>
    /// Adds a card to player's hand.
    /// </summary>
    public void AddHandCard(string[] args) {
        var meta = new CardMetaData(uint.Parse(args[1]), args[2], args[3], args[4]);
        var result = CreateCard(meta);
        PlayerHand.AddCard(result);
        Flow.Linker.Refrence(uint.Parse(args[1]), result);
    }

    /// <summary>
    /// Removes a card from the player's hand.
    /// </summary>
    public void RemHandCard(string[] args) {
        if (Flow.Linker.TryGetRefrence(uint.Parse(args[1]), out var obj)){
            if (obj is HandCard card) {
                PlayerHand.RemoveCard(card);
            }
        }
    }

    /// <summary>
    /// Spawns a creature to the field
    /// </summary>
    public void SpawnCreature(string[] args) {
        var meta = new CardMetaData(uint.Parse(args[1]), args[2], args[3], args[4]);
        var result = CreateCreature(meta);
        var slot = uint.Parse(args[5]);
        Field.SpriteToZone(result, BadSlotLinking[slot]);
        Flow.Linker.Refrence(uint.Parse(args[1]), result);
    }

    /// <summary>
    /// Kills a creature from the field.
    /// </summary>
    public void KillCreature(string[] args) {
        if (Flow.Linker.TryGetRefrence(uint.Parse(args[1]), out var obj)) {
            if (obj is Sprite creature) {
                Field.KillSprite(creature);
            }
        }
    }

    /// <summary>
    /// Returns the program to the splash screen and display a message telling the player if they won or lost. 
    /// </summary>
    public void EndGame(string[] args) {
        var start = new MoDueler.Startup.SplashScreen();

        var font = FontResource.GetNewFont("mtcg_ui.ttf", 60, new Color(1, 0, 1));
        var text = "You Won !!!!";
        if (uint.Parse(args[1]) != LocalPlayer.Index) {
            text = "You Lost ????";
        }
        var lbl = NodeRichTextLabel.CreateLabel("Winner_", text, font, new Vector2(400, 400));
        lbl.EmbeddedControl.VAlign(VAlign.Center);
        lbl.EmbeddedControl.Center();
        lbl.ZAsRelative = false;
        lbl.EmbeddedControl.Modulate = new Color(1, 0, 1);
        lbl.ZIndex = 2000;
        start.AddChild(lbl);
        MoDueler.Scenes.SceneManager.ChangeScene(start);
    }

    #endregion

    /// <summary>
    /// Readies the scene beofre the duel has begun.
    /// </summary>
    private void PrepareScene() {
        PlayerHand = new HandController();
        Field = new BattleField();
        AddChild(Field);
        AddChild(PlayerHand);
        CreateBackground();
        // Use a placeholder sprite as the advisarry as the opponent hasn't loaded in yet.
        Advisarry = CreateAdvisarry("char_23.png");
    }

    /// <summary>
    /// Creates the <see cref="Advisarry"/> node using the advisarry shader.
    /// </summary>
    /// <param name="imgKey">The image the opponent is using.</param>
    /// <returns></returns>
    private Area2D CreateAdvisarry(string imgKey) {
        Shader s = GD.Load<Shader>("res://Shaders/Advisarry.shader");
        var mat = new ShaderMaterial {
            Shader = s
        };
        var advImage = ResourceFiles.LoadImage(ResourceFiles.FindFile(imgKey));
        var node = SpriteCreator.CreateAreaSpriteFullRect<Area2D>("Advisarry", advImage, mat);
        node.Position += new Vector2(0, -advImage.GetHeight()/2);
        node.ZIndex = -10;
        AddChild(node);
        return node;
    }

    /// <summary>
    /// Creates the backdrop of the duel using is own shader.
    /// </summary>
    private void CreateBackground() {
        var img = ResourceFiles.LoadImage("dungeon_0.jpg");
        Shader s = GD.Load<Shader>("res://Shaders/Background.shader");
        var sprite = SpriteCreator.CreateSprite("Background", image: img, shader: s);

        // Push the background to the start of the draw calls.
        sprite.ZIndex = -1000;
        // Make the backgorund fit the screen.
        sprite.Scale = new Vector2(2, 2);

        AddChild(sprite);
    }


}
