using Godot;
using MoDueler.Duel;
using MoonSharp.Environment;
using MoonSharp.Interpreter;
using MoDueler.Nodes;
using MoDueler.Resources;
using MoDueler.Cards;
using MoDuel.Data;
using System.Collections.Generic;
using MoDueler.Scripts.Duel.Connection;
using MoDueler.Network;

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
    /// The targeter and image for the player.
    /// </summary>
    public Area2D LocalPlayer;

    /// <summary>
    /// The targeter and image for discarding.
    /// </summary>
    public Area2D Grave;

    /// <summary>
    /// The <see cref="Node"/> that displays the background.
    /// </summary>
    public Sprite Background;

    /// <summary>
    /// The tool used to link the remote duel flow's targets with the targets on the client.
    /// </summary>
    public NetworkLinker Linker = new NetworkLinker();

    /// <summary>
    /// The provider used to connect to either a local or remote duel instances.
    /// </summary>
    public GameProvider Provider;

    public DuelMaster() { }

    public void SetProvider(GameProvider provider) {
        Provider = provider;
        provider.RecieveCommand += FlowListener;
    }

    public HandCard CreateCard(CardMetaData cardData) {
        Table results = MoDueler.Lua.ClientSideLua.Environment.TemporaryTable();
        MoDueler.Lua.ClientSideLua.Environment.AsScript.DoFile(ResourceFiles.FindFile("*Card.lua"), results);
        var method = results["Create"] as Closure;
        return method.Call(cardData).ToObject<HandCard>();
    }

    public Sprite CreateCreature(CardMetaData cardData) {
        Table results = MoDueler.Lua.ClientSideLua.Environment.TemporaryTable();
        MoDueler.Lua.ClientSideLua.Environment.AsScript.DoFile(ResourceFiles.FindFile("*Creature.lua"), results);
        var method = results["Create"] as Closure;
        return method.Call(cardData).ToObject<Sprite>();
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready() {
        PrepareScene();

        // Implement the player hand card played.
        PlayerHand.CardPlayed = (card, target, position) => {
            if (card == null)
                return;
            // Get the server side index of played card.
            var linked = Linker.TryGetID(card, out int cardIndex);
            if (!linked)
                return;
            // Call PlayCard Targeting the Opponent.
            if (target == Advisarry) {
                if (Linker.TryGetID(Advisarry, out int targetIndex)) {
                    Provider.SendCommand("CMDPlayCard", DynValue.NewNumber(cardIndex), DynValue.NewNumber(targetIndex));
                }
            }
            else if (target == LocalPlayer) {
                if (Linker.TryGetID(LocalPlayer, out int targetIndex)) {
                    Provider.SendCommand("CMDPlayCard", DynValue.NewNumber(cardIndex), DynValue.NewNumber(targetIndex));
                }
            }
            else if (target == Grave) {
                System.Console.WriteLine(cardIndex);

                Provider.SendCommand("CMDDiscard", DynValue.NewNumber(cardIndex));
            }
            // If the player selects the field we tell the flow to play the card in the hovered slot.
            else if (target.Name.Contains("fieldslot")) {
                int slotIndex = int.Parse(target.Name.Remove(0, 9));
                Provider.SendCommand("CMDPlayCard", DynValue.NewNumber(cardIndex), DynValue.NewNumber(slotIndex));
            }
        };

    }

    /// <summary>
    /// The method that is called when the client recieves flow output.
    /// </summary>
    /// <param name="command">the command to execute.</param>
    /// <param name="args">The arguments for the command</param>
    public void FlowListener(string command, DynValue[] args) {
        System.Console.WriteLine("Test Commands : " + command);
        switch (command) {

            case "SetHeroes":
                var hero1 = args[0].Table;
                var hero2 = args[1].Table;
                Advisarry = CreateAdvisarry("char_23.png");
                LocalPlayer = CreateLocalPlayer("face_129.png");
                if (hero1["UserId"].ToString() == "") {
                    Linker.Refrence((int)(double)hero1["Index"], LocalPlayer);
                    Linker.Refrence((int)(double)hero2["Index"], Advisarry);
                }
                else {
                    Linker.Refrence((int)(double)hero2["Index"], LocalPlayer);
                    Linker.Refrence((int)(double)hero1["Index"], Advisarry);
                }
                break;
            case "LinkField": {
                    string player1Id = args[0].String;
                    bool flipped = player1Id != Provider.LocalID;
                    Dictionary<int, int> fieldLinking = new Dictionary<int, int>();
                    int player1fieldIndex = (int)args[1].Number;
                    int player2fieldIndex = (int)args[2].Number;
                    Table slots = args[3].Table;
                    foreach (var pair in slots.Pairs) {
                        fieldLinking.Add((int)pair.Key.Number, (int)pair.Value.Number);
                    }
                    Field = BattleField.CreateBattleField(flipped, player1fieldIndex, player2fieldIndex, fieldLinking);
                    AddChild(Field);
                }
                break;
            case "GainMana":

                break;
            case "DrainMana":

                break;
            case "AddCardToHand": {
                    var cardId = args[0].String;
                    int index = (int)args[1].Number;
                    Shader shader = ResourceFiles.LoadFile("HandCardNew.shader") as Shader;
                    HandCard card = HandCard.CreateNewHandCard("HandCard" + index.ToString(), ResourceFiles.LoadImage("@bf_images.png:card_hand_creature(soul)_02.png"), shader);
                    card.SetShaderParam("card_is_creature", true);
                    card.SetShaderParam("card_art", ResourceFiles.TexFromImage(ResourceFiles.LoadImage("c_" + cardId + ".png")));
                    card.SetShaderParam("card_mask", ResourceFiles.TexFromImage(ResourceFiles.LoadImage("@bf_images.png:card_back_blue.png")));
                    PlayerHand.AddCard(card);
                    Linker.Refrence(index, card);
                }
                break;

            case "RemHandCard": {
                    var index = args[0].Number;
                    var obj = Linker.GetRefrence((int)index);
                    if (obj is HandCard remcard) {
                        PlayerHand.RemoveCard(remcard);
                        Linker.Derefrence(obj);
                    }
                }
                break;
            case "SummonCreature": {
                    var cardId = args[0].String;
                    int index = (int)args[1].Number;
                    var slotIndex = (int)args[2].Number;

                    Shader shader = ResourceFiles.LoadFile("FieldCreature.shader") as Shader;
                    Sprite card = SpriteCreator.CreateSprite("Creature" + index.ToString(), ResourceFiles.LoadImage("@bf_images.png:card_battlefield_02.png"), shader);
                    var mat = card.Material as ShaderMaterial;
                    mat.SetShaderParam("creature_art", ResourceFiles.TexFromImage(ResourceFiles.LoadImage("c_" + cardId + ".png")));
                    mat.SetShaderParam("creature_mask", ResourceFiles.TexFromImage(ResourceFiles.LoadImage("card_hero_frame.png")));

                    Field.SpriteToZone(card, Field.InvertedIndexMap[slotIndex] - 1);
                    Linker.Refrence(index, card);

                }
                break;
            case "CreatureKilled": {
                    var index = args[0].Number;
                    var obj = Linker.GetRefrence((int)index);
                    System.Console.WriteLine(obj);
                    if (obj is Sprite remcard) {
                        Field.KillSprite(remcard);
                        Linker.Derefrence(obj);
                    }
                }
                break;
            case "GameOver":
                var winner = args[0].String;
                var splashScreen = new SplashScreen();
                if (winner == Provider.LocalID)
                    splashScreen.AddChild(new Label() { Text = "You Win!" });
                else {
                    splashScreen.AddChild(new Label() { Text = "You Lost!" });
                }
                MoDueler.Scenes.SceneManager.ChangeScene(splashScreen);
                break;
        
        }


    }

    #region FlowOutputEvent Methods

    /// <summary>
    /// Adds a card to player's hand.
    /// </summary>
    public void AddHandCard(string[] args) {
        var meta = new CardMetaData(uint.Parse(args[1]), args[2], args[3], args[4]);
        var result = CreateCard(meta);
        PlayerHand.AddCard(result);
        Linker.Refrence(int.Parse(args[1]), result);
    }

    /// <summary>
    /// Removes a card from the player's hand.
    /// </summary>
    public void RemHandCard(string[] args) {
        if (Linker.TryGetRefrence(int.Parse(args[1]), out var obj)){
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
       // Field.SpriteToZone(result, BadSlotLinking[slot]);
        Linker.Refrence(int.Parse(args[1]), result);
    }

    /// <summary>
    /// Kills a creature from the field.
    /// </summary>
    public void KillCreature(string[] args) {
        if (Linker.TryGetRefrence(int.Parse(args[1]), out var obj)) {
            if (obj is Sprite creature) {
                Field.KillSprite(creature);
            }
        }
    }

    /// <summary>
    /// Returns the program to the splash screen and display a message telling the player if they won or lost. 
    /// </summary>
    public void EndGame(string[] args) {
        var start = new SplashScreen();

        var font = FontResource.GetNewFont("mtcg_ui.ttf", 60, new Color(1, 0, 1));
        var text = "You Won !!!!";
        //if (uint.Parse(args[1]) != LocalPlayer.Index) {
        //    text = "You Lost ????";
        //}
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
        AddChild(PlayerHand);
        CreateBackground();
        CreateDiscard();

        // Create charge, revive, levelup buttons.
        Button ChargeBtn = new Button() {
            Text = "Charge",
            RectPosition = new Vector2(400, 0)
        };

        Button ReviveBtn = new Button() {
            Text = "Revive",
            RectPosition = new Vector2(400, 20)
        };

        Button LevelBtn = new Button() {
            Text = "Level Up",
            RectPosition = new Vector2(400, 40)
        };

        ChargeBtn.Connect("pressed", this, nameof(Charge));
        ReviveBtn.Connect("pressed", this, nameof(Revive));
        LevelBtn.Connect("pressed", this, nameof(LevelUp));

        AddChild(ChargeBtn);
        AddChild(ReviveBtn);
        AddChild(LevelBtn);

    }

    public void Charge() => Provider.SendCommand("CMDCharge");

    public void Revive() => Provider.SendCommand("CMDRevive");

    public void LevelUp() => Provider.SendCommand("CMDLevelUp");


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


    private void CreateDiscard() {
        var img = ResourceFiles.LoadImage("arena_deco_01.png");
        var sprite = SpriteCreator.CreateAreaSprite<Area2D>("DiscardArea", img);
        sprite.ZIndex = -50;
        sprite.Position = new Vector2(-500, 200);
        Grave = sprite;
        AddChild(sprite);
    }

    private Area2D CreateLocalPlayer(string imgKey) {
        Shader s = GD.Load<Shader>("res://Shaders/LocalPlayer.shader");
        var mat = new ShaderMaterial() { 
            Shader =s,
        };
        var img = ResourceFiles.LoadImage("player_status.png");
        var mask = ResourceFiles.LoadImage("binder_icon_box.png");
        var art = ResourceFiles.LoadImage(imgKey);
        var sprite = SpriteCreator.CreateAreaSprite<Area2D>("LocalPlayer", img, mat);

        mat.SetShaderParam("mask_tex", ResourceFiles.TexFromImage(mask));
        mat.SetShaderParam("char_art", ResourceFiles.TexFromImage(art));
        AddChild(sprite);
        sprite.Position = new Vector2(-500, 0);
        return sprite;

    }




}
