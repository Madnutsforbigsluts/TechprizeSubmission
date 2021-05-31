using System;
using System.Collections.Generic;
using System.Linq; 

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Audio;


using MonoGame.Extended.Tiled;
using MonoGame.Extended.Tiled.Graphics;

using Myra;
using Myra.Graphics2D.UI;

using Glyph.Entities;
using Glyph.Utilities;
using Glyph.Combat;
using Glyph.UI;
using Glyph.Puzzles;
using Glyph.GUI;
using Glyph.Cinematics;

using Comora;

using static Glyph.Utilities.Collision;
using static Glyph.Utilities.CheckBetween;
using static Glyph.Utilities.ActionFromTileID;


// TODO
// Get player to forest (first puzzle) 
// Get player to Maritime city (second puzzle) 

public enum UIState
{
    None,
    ShowPlayerInv,
    ShowNPCInv,
    ShowDialogue
}

public enum SceneState
{
    Menu, 
    ShowOverWorld,
    ShowCombat,
    Cinematic
}

public enum PuzzleState
{
    None,
    Puzzle1,
    Puzzle2,
    Puzzle3
}

namespace Glyph
{
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        MouseState lastMousePos;
        MouseState currentMousePos;
        Dictionary<string, Texture2D> Textures = new Dictionary<string, Texture2D>();
        Dictionary<string, Song> Tracks = new Dictionary<string, Song>();
        Dictionary<string, SoundEffect> soundEffects = new Dictionary<string, SoundEffect>();
        KeyboardState currentKeyPress;
        KeyboardState lastKeyPress;
        SpriteFont spriteFont;
        Battle battle;
        public static UIState uiState;
        public static SceneState sceneState;
        public static Player player;
        public static PuzzleState puzzleState;
        Camera camera;
        TiledMap map;
        public static Dialogue dialogue;
        TiledMapRenderer mapRenderer;
        NPC interactingNPC;
        MenuGUI menu;
        Sprite background;
        Cinematic cinematic;

        Vector2 _menuCamera;
        Vector2 MenuCamera
        {
            get { return _menuCamera; }
            set
            {
                _menuCamera = !IsBetween(696, 1200, value.X) ||
                    !IsBetween(2856, 3360, value.Y) ? _menuCamera =
                    new Vector2(944, 3096) : value;
            }
        }

        TiledMapTileLayer collisionLayer;

        string lastNotePlayed;
        int noteIDPlayed;

        private int _tileID; 
        private int tileID 
        {
            get { return _tileID; }
            set
            {
                player.CollidingWithMap = _tileID != value ? true : false;
                lastNotePlayed = _tileID == value ? string.Empty : lastNotePlayed;
                string instruction = GetInstructionFromCollision(tileID);

                if(instruction != lastNotePlayed)
                {
                    switch (instruction)
                    {
                        case "Note1":
                            noteIDPlayed = 1;
                            lastNotePlayed = "Note1";
                            soundEffects["Note1"].Play();
                            break;
                        case "Note2":
                            noteIDPlayed = 2;
                            lastNotePlayed = "Note2";
                            soundEffects["Note2"].Play();
                            break;
                        case "Note3":
                            noteIDPlayed = 3;
                            lastNotePlayed = "Note3";
                            soundEffects["Note3"].Play();
                            break;
                        case "Reset":
                            puzzle1.PlayedNoteIndex = 0;
                            puzzle1.NoteIndex = 0;
                            lastNotePlayed = "Reset";
                            soundEffects["Note4"].Play();
                            break; 
                    }
                    if (puzzle1.Notes[puzzle1.StepCount][puzzle1.PlayedNoteIndex] ==
                        noteIDPlayed)
                    {
                        noteIDPlayed = 0;
                        puzzle1.PlayedNoteIndex++;
                    }
                }

                if(instruction == "Lock")
                {
                    puzzleState = PuzzleState.Puzzle2;
                }

                _tileID = value;
            }
        }

        // Points towards the current scene 

        public static int CinematicPointer;

        Puzzle1 puzzle1;
        Puzzle2 puzzle2; 

        private const float transitionTime = 3f;
        Timer locationUpdateTimer = new Timer(transitionTime);
        Timer transitionUpdatetimer = new Timer(transitionTime);

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this)
            {
                PreferredBackBufferWidth = GlobalConstants.ScreenBufferWidth,
                PreferredBackBufferHeight = GlobalConstants.ScreenBufferHeight
            };
            Window.AllowUserResizing = false; 
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            currentMousePos = new MouseState();
            lastMousePos = new MouseState();
            currentKeyPress = Keyboard.GetState();
            lastKeyPress = Keyboard.GetState();
            sceneState = SceneState.ShowOverWorld;
        }

        protected override void Initialize()
        {
            player = new Player();
            player.Initialize(GraphicsDevice);
            camera = new Camera(GraphicsDevice);
            camera.Transform.Scale = new Vector2(0.15f, 0.15f);

            background = new Sprite(GraphicsDevice, "Content/Backgrounds/BG.png", 480, 480,
                2400, 3360, 32, 0.05f, 480, 480, Color.White);

            new CreateEntityFactory(player.CurrentLocation, "NPC");

            map = Content.Load<TiledMap>("Maps/Map");
            mapRenderer = new TiledMapRenderer(GraphicsDevice);
            collisionLayer = map.GetLayer<TiledMapTileLayer>("Collisions");

            sceneState = SceneState.Menu;

            _menuCamera = new Vector2(944, 3096);

            base.Initialize();
        }


        protected override void LoadContent()
        {

            MyraEnvironment.Game = this;

            spriteBatch = new SpriteBatch(GraphicsDevice);
            spriteFont = Content.Load<SpriteFont>("Fonts/Main");

            Texture2D defaultLayer = new Texture2D(GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
            defaultLayer.SetData(new[] { Color.White });

            Tracks.Add("CyanVilleTheme", Content.Load<Song>("Music/08. Wet Tip Hen Ax"));

            Textures.Add("Default", defaultLayer);
            Textures.Add("WeaponItem", Content.Load<Texture2D>("Images/Items/Weapon"));
            Textures.Add("KeyItem", Content.Load<Texture2D>("Images/Items/Key"));
            Textures.Add("FoodItem", Content.Load<Texture2D>("Images/Items/Food"));
            Textures.Add("DialogueBox", Content.Load<Texture2D>("Images/DialogueBox"));
            Textures.Add("RouteInfo", Content.Load<Texture2D>("Images/RouteInfo"));
            Textures.Add("Cave", Content.Load<Texture2D>("Images/Cave"));
            Textures.Add("Title", Content.Load<Texture2D>("Images/Untitled-1"));
            Textures.Add("PlayerAvatar", Content.Load<Texture2D>("Images/Entities/PlayerAvatar"));
            Textures.Add("Entity1", Content.Load<Texture2D>("Images/Entities/1"));
            Textures.Add("Entity2", Content.Load<Texture2D>("Images/Entities/2"));
            Textures.Add("Entity3", Content.Load<Texture2D>("Images/Entities/3.1"));
            Textures.Add("Entity4", Content.Load<Texture2D>("Images/Entities/4"));
            Textures.Add("Entity5", Content.Load<Texture2D>("Images/Entities/5"));
            Textures.Add("Entity6", Content.Load<Texture2D>("Images/Entities/6.1"));
            Textures.Add("Entity7", Content.Load<Texture2D>("Images/Entities/7"));
            Textures.Add("Entity8", Content.Load<Texture2D>("Images/Entities/8"));
            Textures.Add("Entity9", Content.Load<Texture2D>("Images/Entities/9"));
            Textures.Add("TurnedCard", Content.Load<Texture2D>("Images/Cards/TurnedCard"));

            for(int i = 1; i < 9; i++)
            {
                soundEffects.Add($"Note{i}", Content.Load<SoundEffect>($"SoundFX/{i}"));
            }

            for(int i = 1; i < 11; i++)
            {
                Textures.Add($"Card{i}", Content.Load<Texture2D>($"Images/Cards/{i}"));
            }

            player.inventory.LoadContent(Textures, spriteFont);
            player.LoadContent(Textures);

            // puzzle1.LoadContent(soundEffects, spriteFont);
            // puzzle2.LoadContent(spriteFont);

            menu = new MenuGUI();
            menu.LoadContent(spriteFont);

            MediaPlayer.Play(Tracks["CyanVilleTheme"]);
            MediaPlayer.Volume = 0.1f; 
        }

        protected override void Update(GameTime gameTime)
        {

            // Updates based on input 
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
            else if (uiState == UIState.None && currentKeyPress.IsKeyDown(Keys.F) && lastKeyPress.IsKeyUp(Keys.F))
            {
                uiState = UIState.ShowPlayerInv;
            }
            else if (uiState == UIState.ShowPlayerInv && currentKeyPress.IsKeyDown(Keys.F) && lastKeyPress.IsKeyUp(Keys.F))
            {
                uiState = UIState.None;
            }
            else if (uiState == UIState.None && currentKeyPress.IsKeyDown(Keys.E) &&
                lastKeyPress.IsKeyUp(Keys.E) && interactingNPC != null)
            {
                uiState = UIState.ShowNPCInv;
            }
            else if (uiState == UIState.ShowNPCInv && currentKeyPress.IsKeyDown(Keys.E)
                && lastKeyPress.IsKeyUp(Keys.E))
            {
                uiState = UIState.None;
            }
            else if (uiState == UIState.None && currentKeyPress.IsKeyDown(Keys.Enter)
                && lastKeyPress.IsKeyUp(Keys.Enter) && interactingNPC != null
                && !Dialogue.Talking)
            {
                uiState = UIState.ShowDialogue;
            }
            else if (uiState == UIState.ShowDialogue && currentKeyPress.IsKeyDown(Keys.Enter)
                && lastKeyPress.IsKeyUp(Keys.Enter) && !Dialogue.Talking)
            {
                uiState = UIState.None;
            }
            else if (uiState == UIState.ShowDialogue && currentKeyPress.IsKeyDown(Keys.Enter)
                && lastKeyPress.IsKeyUp(Keys.Enter) && Dialogue.Talking)
            {
                soundEffects["Note2"].Play();
                Dialogue.NextPane = true;
            }

            // Instance updates
            lastMousePos = currentMousePos;
            currentMousePos = Mouse.GetState();
            lastKeyPress = currentKeyPress;
            currentKeyPress = Keyboard.GetState();

            // Updates based on scenes
            switch (sceneState)
            {
                case SceneState.ShowCombat:
                    background.Update(gameTime);
                    battle.Update(gameTime, currentMousePos, lastMousePos);
                    break;
                case SceneState.ShowOverWorld:
                    mapRenderer.Update(map, gameTime);
                    camera.Update(gameTime);

                    player.Controller(Keyboard.GetState(), gameTime);

                    // If the for loop is running and the NPC's are unloaded from the chunk
                    // then we don't want an exception thrown.
                    try
                    {
                        foreach (NPC npc in CreateEntityFactory.NPCS)
                        {
                            if (!npc.Initialized)
                            {
                                npc.Initialize(GraphicsDevice);
                                npc.LoadContent(spriteFont);
                            }

                            npc.Controller(gameTime);

                            if (IsColliding(player.Sprite.Reach,
                                npc.Sprite.BoundingBox))
                            {
                                interactingNPC = npc;
                                break;
                            }
                            interactingNPC = null;
                        }
                    }
                    catch (System.InvalidOperationException) { };

                    cinematic = null;

                    // player.Controller(gameTime);
                     
                    // The tiles are all 8x8
                    int x0 = (int)(player.NextPosition.X / 8) + 1;
                    int y0 = (int)(player.NextPosition.Y / 8) + 1;

                    tileID = 0;

                    if (collisionLayer.TryGetTile(x0, y0, out TiledMapTile? tile))
                    {
                        tileID = tile.Value.GlobalIdentifier;
                    }

                    switch(puzzleState)
                    {
                        case PuzzleState.Puzzle1:
                            if(puzzle1 == null)
                            {
                                puzzle1 = new Puzzle1();
                                puzzle1.LoadContent(soundEffects, spriteFont);
                            }
                            puzzle1.Update(gameTime);
                            break;
                        case PuzzleState.Puzzle2:
                            if(puzzle2 == null)
                            {
                                puzzle2 = new Puzzle2(puzzle1.puzzle2Combination);
                                puzzle2.LoadContent(spriteFont);
                            }
                            puzzle2.AttemptSolve();
                            break;
                    }

                    player.UpdateSpritePosition();
                    camera.Position = player.Sprite.Position;

                    break;
                case SceneState.Cinematic:

                    // Create a new cinematic on demand 
                    if(cinematic == null)
                    {
                        cinematic = new Cinematic(CinematicPointer,
                            spriteFont, Textures["DialogueBox"]);
                    }
                    mapRenderer.Update(map, gameTime);

                    // Initiate npcs for cinematic scene (if there are none in
                    // the scene already a null exception will be thrown.) 
                    foreach (NPC npc in CreateEntityFactory.NPCS)
                    {
                        if (!npc.Initialized)
                        {
                            npc.Initialize(GraphicsDevice);
                            npc.LoadContent(spriteFont);
                        }
                    }
                    cinematic.Update(gameTime);
                    camera.Update(gameTime);
                    uiState = UIState.ShowDialogue;
                    player.Controller(Keyboard.GetState(), gameTime);
                    player.UpdateSpritePosition();
                    camera.Position = player.Sprite.Position;
                    break;
                case SceneState.Menu:
                    MenuCamera += new Vector2(0.1f, -0.1f);
                    camera.Position = MenuCamera;
                    break;
            }
            
            // Updates based on user interface
            switch (uiState)
            {
                case UIState.ShowPlayerInv:
                    player.inventory.Update(gameTime, GraphicsDevice, lastMousePos, currentMousePos);
                    ScreenState.screenState = ScreenUpdateState.None;
                    Entity.Frozen = true; 
                    break;
                case UIState.ShowNPCInv:
                    // The player's inventory is brought up when trading with the NPC
                    player.inventory.Update(gameTime, GraphicsDevice, lastMousePos, currentMousePos);
                    background.Update(gameTime);
                    Entity.Frozen = true; 
                    break;
                case UIState.ShowDialogue:

                    if(dialogue == null)
                    {
                        if (interactingNPC != null)
                        {
                            // Show the default dialogue in the overworld 
                            dialogue = new Dialogue(new List<string> { interactingNPC.Dialogue["Greeting"] },
                            spriteFont, Textures["DialogueBox"]);

                            // a fight initiates when interacting with an enemy
                            if(interactingNPC.Friendly)
                            {
                                player.EncounteredEnemy = new Enemy(1, 1);
                                sceneState = SceneState.ShowCombat;
                                uiState = UIState.None;
                            }
                        }
                    } else 
                    {
                        dialogue.Update(gameTime);
                    }
                    // Entity.Frozen = true; 
                    break;
                case UIState.None:
                    dialogue = null;
                    Entity.Frozen = false; 
                    break;
            }

            // Updates based on screenstate
            switch (ScreenState.screenState)
            {
                case ScreenUpdateState.LocationUpdate:
                    locationUpdateTimer.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
                    break;
                case ScreenUpdateState.TransitionUpdate:

                    // Complete a full rotation  of the camera 
                    camera.Rotation = (float) ( 2 * Math.PI * Math.Sin(transitionUpdatetimer.currentTime / 2));

                    transitionUpdatetimer.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
                    Entity.Frozen = true;
                    break;

            }

            Console.WriteLine(player.Position);

            // Updates based on timers 
            if (locationUpdateTimer.timeExpired)
            {
                locationUpdateTimer.Reset();
                ScreenState.screenState = ScreenUpdateState.None;
            }

            // Once the transition is over 
            else if (transitionUpdatetimer.timeExpired)
            {
                camera.Rotation = 0;
                transitionUpdatetimer.Reset();
                ScreenState.screenState = ScreenUpdateState.None;
                Entity.Frozen = false; 
            }

            // Updates based on player information 
            if (player.EncounteredEnemy != null)
            {
                sceneState = SceneState.ShowCombat;
                ScreenState.screenState = ScreenUpdateState.TransitionUpdate;

                // Create battle based on the enemy the player encountered. 
                battle = new Battle(player, player.EncounteredEnemy);
                battle.LoadContent(Textures, spriteFont);
                player.EncounteredEnemy.LoadContent(Textures);
                player.EncounteredEnemy = null;
            }
            if (player.Dead)
            {
                sceneState = SceneState.ShowOverWorld;
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {

            // Matrix keeps the player in the centre of the screen at all times by inverting 
            Matrix mapProj = camera.Transform.InvertAbsolute;
            mapProj.Translation += new Vector3 (504,
                504, 0);

            spriteBatch.Begin(transformMatrix: mapProj, samplerState: SamplerState.PointClamp);

            // Clear all draws to screen 
            GraphicsDevice.Clear(Color.Black);

            // All the draws to be done in the overworld 
            if (sceneState == SceneState.Cinematic || sceneState == SceneState.ShowOverWorld)
            {

                player.Sprite.Draw(spriteBatch);

                // Loop over each npc in scene to draw them 
                foreach (NPC npc in CreateEntityFactory.NPCS)
                {
                    npc.Sprite.Draw(spriteBatch);
                }
                mapRenderer.Draw(map, mapProj);
            }

            // Draws based on puzzle 
            switch (puzzleState)
            {
                case PuzzleState.Puzzle1:
                    // puzzle1.Draw(spriteBatch);
                    break;
            }

            // Draws based on scene
            switch (sceneState)
            {
                case SceneState.Menu:
                    // Camera follows map in background 
                    mapRenderer.Draw(map, mapProj);
                    Desktop.Render();
                    break;
            }

            spriteBatch.End();


            // No camera when player is not in overworld
            spriteBatch.Begin(SpriteSortMode.Immediate, samplerState: SamplerState.PointClamp);

            // Draws based on scene
            switch (sceneState)
            {
                case SceneState.ShowCombat:
                    background.Draw(spriteBatch, 2.2f);
                    battle.Draw(spriteBatch);
                    Desktop.Render(); 
                    break;
                case SceneState.Menu:
                    // Title for menu 
                    spriteBatch.Draw(Textures["Title"], new Rectangle(250, 250, 500, 150),
                        Color.White);
                    break; 
            }

            // Draws based on puzzle 
            switch (puzzleState)
            {
                case PuzzleState.Puzzle2:
                    puzzle2.Draw(spriteBatch);
                    Desktop.Render();
                    break;
                case PuzzleState.Puzzle3:
                    break;
            }


            // Draws based on user interface
            switch (uiState)
            {
                case UIState.ShowPlayerInv:
                    player.inventory.Draw(spriteBatch);
                    break;
                case UIState.ShowNPCInv:
                    // The player's inventory is brought up when trading with the NPC
                    background.Draw(spriteBatch, 2.2f);
                    player.inventory.Draw(spriteBatch);
                    Desktop.Render();
                    break;
                case UIState.ShowDialogue:
                    if(dialogue != null)
                    {
                        dialogue.Draw(spriteBatch);
                    }
                    break; 
            }

            // Draws based on updates to the screen
            switch (ScreenState.screenState)
            {
                case ScreenUpdateState.LocationUpdate:
                    // Draw the drop-down location update banner 
                    spriteBatch.Draw(Textures["RouteInfo"],
                        new Rectangle(0, -25, 300, 200), Color.White);
                    spriteBatch.DrawString(spriteFont, player.CurrentLocation,
                        new Vector2(75, 20), Color.Black, 0.0f,
                        new Vector2(0, 0), 0.5f, SpriteEffects.None, 1f);
                    break;
                case ScreenUpdateState.TransitionUpdate:
                    // Draw the fading transition screen 
                    spriteBatch.Draw(Textures["Default"],
                        new Rectangle(0, 0, GlobalConstants.ScreenBufferWidth,
                        GlobalConstants.ScreenBufferHeight), Color.MediumPurple
                        * (2 - transitionUpdatetimer.currentTime));
                    break;
            }

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
