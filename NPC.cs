using System.Collections.Generic;
using System; 

using Glyph.GUI; 
using Glyph.Utilities;
using Glyph.Inventories;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using static Glyph.Utilities.JsonParser;

namespace Glyph.Entities
{
    public class NPC : Player
    {
        private readonly dynamic jsonData = FetchJson("jsons/NPCS.json");

        private Vector2 velocity;
        private Vector2 _currentPosition;

        private GraphicsDevice graphicsDevice;

        public bool Initialized { get; set; } = false;

        public readonly Dictionary<string, string> Dialogue;

        private bool _stopped;

        private string spriteFilePath;

        public NPCInventoryGUI InventoryGUI;

        public bool Friendly; 

        protected List<BagItem> bag = new List<BagItem>();

        Timer walkTimer;

        public new Sprite Sprite;

        public new List<Dictionary<string, string>> instructions { get; set; }

        public NPC(int ID, Vector2 _currentPosition)
        {
            this.ID = ID;
            this._currentPosition = _currentPosition;
            spriteFilePath = $"Content/Images/Sprites/NPC{ID}.png";

            // Get name and dialogue from ID 
            Name = jsonData["NPCS"][ID - 1]["Name"];
            Dialogue = jsonData["NPCS"][ID - 1]["Dialogue"].ToObject<Dictionary<string, string>>();
            Friendly = jsonData["NPCS"][ID - 1]["Enemy"];

            // Get the items the NPC sells by looking up ID in json 
            var NPCItems = jsonData["NPCS"][ID - 1]["Items"].ToObject<List<dynamic>>();

            // Add the items to the NPC's bag 
            foreach(dynamic item in NPCItems)
            {
                bag.Add(new BagItem(Convert.ToInt32(item["ItemID"]),
                    Convert.ToInt32(item["Quantity"])));
            }

            instructions = new List<Dictionary<string, string>>
            {
                new Dictionary<string, string>
                {
                    { "Direction", "South" },
                    { "Duration", "0" }
                }
            };

            // Loaded in with no visible Sprite by default 
            Sprite = new Sprite();
            Sprite.Position = _currentPosition;
        }

        public override void Initialize(GraphicsDevice graphicsDevice)
        {
            this.graphicsDevice = graphicsDevice;
            Sprite = new Sprite(graphicsDevice, spriteFilePath,
            84, 84, 672, 420, 5, 0.1, 84, 84, Color.White, -1);
            Initialized = true;
        }

        public override void LoadContent(SpriteFont spriteFont)
        {
            InventoryGUI = new NPCInventoryGUI(ID, _currentPosition, spriteFont);
        }

        public override void Controller(GameTime gameTime)
        {
            if (!Frozen && !_stopped)
            {
                if ((walkTimer == null || walkTimer.timeExpired) && instructions.Count > 0)
                {
                    walkTimer = new Timer(float.Parse(instructions[0]["Duration"]));
                    UpdateSpriteDirection(instructions[0]["Direction"]);
                    instructions.RemoveAt(0);
                }
                else if (walkTimer.timeExpired && instructions.Count == 0)
                {
                    _stopped = true;
                }
                UpdateSpritePosition();
                walkTimer.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
                Sprite.Position = _currentPosition;
                Sprite.Update(gameTime);
            }
            else if (_stopped)
            {
                Sprite.Update(gameTime, true);
                FinishedWalking = true;
            }
        }

        public override void UpdateSpritePosition()
        {
            _currentPosition += velocity * 0.25f;
        }

        protected override void UpdateSpriteDirection(string direction)
        {

            switch (direction) 
            {
                case "North":
                    velocity = new Vector2(0, -1);
                    Sprite = new Sprite(graphicsDevice, spriteFilePath,
                        32, 30, 90, 128, 12, 0.15, 30, 32, Color.White, 9);
                    break;
                case "East":
                    velocity = new Vector2(1, 0);
                    Sprite = new Sprite(graphicsDevice, spriteFilePath,
                        32, 30, 90, 128, 9, 0.15, 30, 32, Color.White, 6);
                    break;
                case "South":
                    velocity = new Vector2(0, 1);
                    Sprite = new Sprite(graphicsDevice, spriteFilePath,
                        32, 30, 90, 128, 4, 0.15, 30, 32, Color.White, 0);
                    break;
                case "West":
                    velocity = new Vector2(-1, 0);
                    Sprite = new Sprite(graphicsDevice, spriteFilePath,
                        84, 84, 672, 420, 20, 0.1, 84, 84, Color.White, 26);
                    break;
                case null:
                    velocity = Vector2.Zero;
                    Sprite = new Sprite(graphicsDevice, spriteFilePath,
                        32, 30, 90, 128, 6, 0.15, 30, 32, Color.White, 3);
                    break;
            }
        }
    }
}

