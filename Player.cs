using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using static Glyph.Utilities.Collision;
using static Glyph.Map.ActionFromLocation;
using Glyph.Map;
using Glyph.Utilities;
using Glyph.Inventories;

namespace Glyph.Entities
{
    public class Player : Entity
    {
        private Location location;
        private LocationNotifier locationNotifier;
        public static int Money; 

        public Inventory inventory;

        // Todo Instantiate this to be where the player spawns in

        CreateEntityFactory enemyFactory;

        public string CurrentLocation;

        private string _oldDirection;
        private string _currentDirection;

        /*
        private float _frameTime; 
        private float FrameTime
        {
            get { return _frameTime;  }
            set
            {
                _frameTime = value + 0.1f; 
            }
        }
        */

        public bool CollidingWithMap { get; set; }

        public new bool Frozen; 

        GameTime gameTime;

        private Vector2 _oldPosition;
        private Vector2 _currentPosition;
        public new Vector2 Position
        {
            get { return _currentPosition; }
            set
            {
                if (value != _oldPosition)
                {
                    UpdateLocation();

                    // Commits the battle
                    new CreateEntityFactory(CurrentLocation, "Enemy");
                    // EncounteredEnemy = CreateEntityFactory.EncounteredEnemy; 
                }
                _currentPosition = value; 
            }
        }

        private bool _stopped; 
        public bool Stopped
        {
            get { return _stopped; }
            set
            {
                if (value)
                {
                    Sprite.Update(gameTime, true);
                }
                _stopped = value; 
            }
        }

        protected const float maxWalkingSpeed = 0.6f;

        public bool DisableWalk = false; 

        public Sprite Sprite;

        private GraphicsDevice graphicsDevice;

        public Enemy EncounteredEnemy { get; set; }

        private Vector2 _velocity;
        private Vector2 velocity
        {
            get { return _velocity; }
            set
            {
                // Sum of vector components is 0 so velocity is 0 
                Stopped = value.X + value.Y == 0 ? true : false;
                _velocity = value; 
            }
        }

        Timer walkTimer;

        public bool FinishedWalking { get; set; }

        public Vector2 NextPosition;

        public List<Dictionary<string, string>> instructions { get; set; }

        public Player()
        {
            Health = 50;
            Level = 5;

            // Player ID is 0 by default 
            ID = 0;

            inventory = new Inventory();
            location = new Location();

            // The _oldPosition is the starting position of the player. 
            _oldPosition = new Vector2(944, 3096);
            // _oldPosition = new Vector2(2225, 3713);
            Position = _oldPosition;
            locationNotifier = new LocationNotifier(location.GetLocation(Position));
            UpdateLocation();

        }

        public override void LoadContent(Dictionary<string, Texture2D> textures)
        {
            Avatar = textures["PlayerAvatar"];
        }

        public virtual void LoadContent(SpriteFont spriteFont) { }

        public virtual void Initialize(GraphicsDevice graphicsDevice)
        {
            this.graphicsDevice = graphicsDevice;
            Sprite = new Sprite(graphicsDevice, "Content/Images/Sprites/Player.png",
                32, 30, 90, 128, 6, 0.2, 30, 32, Color.White, 3);
        }

        public void UpdateLocation()
        {
            CurrentLocation = location.GetLocation(Position);

            // Changes in location trigger the appropriate event / action.
            locationNotifier.Location = CurrentLocation;

        }


        /// <summary>
        /// Override of default Controller. Accepts a set of instructions and moves according to them. 
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="instructions"></param>
        /// <param name="duration"></param>
        public virtual void Controller(GameTime gameTime)
        {
            if (!Frozen && !_stopped)
            {
                if ( (walkTimer == null || walkTimer.timeExpired) && instructions.Count > 0)
                {
                    walkTimer = new Timer(float.Parse(instructions[0]["duration"]));
                    UpdateSpriteDirection(instructions[0]["direction"]);
                    _oldDirection = _currentDirection;
                    instructions.RemoveAt(0);
                }
                else if (walkTimer.timeExpired && instructions.Count == 0)
                {
                    _stopped = true;
                    FinishedWalking = true; 
                }

                UpdateSpritePosition();
                walkTimer.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
                NextPosition = GetNextPosition(velocity);
                Sprite.Update(gameTime);

            }
            else if (_stopped)
            {
                Sprite.Update(gameTime, true);
            }
        }

        /// <summary>
        /// Default controller for player movement. Updates player's position based on key presses. 
        /// </summary>
        /// <param name="keyboard"></param>
        /// <param name="gameTime"></param>
        public void Controller(KeyboardState keyboard, GameTime gameTime)
        {
            this.gameTime = gameTime;

            if (!Entity.Frozen && Game1.sceneState != SceneState.Cinematic)
            {
                _oldDirection = _currentDirection;

                if (keyboard.IsKeyDown(Keys.W))
                {
                    _currentDirection = "North";
                }
                else if (keyboard.IsKeyDown(Keys.A))
                {
                    _currentDirection = "West";
                }
                else if (keyboard.IsKeyDown(Keys.S))
                {
                    _currentDirection = "South";
                }
                else if (keyboard.IsKeyDown(Keys.D))
                {
                    _currentDirection = "East";
                }
                else
                {
                    velocity = Vector2.Zero;
                    _currentDirection = null;
                }

                if (_oldDirection != _currentDirection)
                {
                    UpdateSpriteDirection(_currentDirection);
                    _oldDirection = _currentDirection;
                }

                if(!Stopped)
                {
                    // timer.Update(gameTime.ElapsedGameTime.Milliseconds);
                    NextPosition = GetNextPosition(velocity);
                    Sprite.Update(gameTime);
                }
            } else
            {
                velocity = Vector2.Zero;
            }
        }

        private Vector2 GetNextPosition(Vector2 velocity)
        {
            _oldPosition = Position;

            // Uncomment for ~more~ continuous movement. 
            /* 
            acceleration = (float)(-maxWalkingSpeed / Math.Pow(Math.E,
                Math.Abs(elapsedTimeInSameDir) / 750)) + maxWalkingSpeed;
            return Position + acceleration * velocity;
            */
            return Position + velocity * new Vector2(8, 8); 
        }

        public virtual void UpdateSpritePosition() 
        {
            // Teleports the player if they are in a certain
            // location 
            DoFromLocation(CurrentLocation);

            // If there are no collisions position will be set to this

            // Position += acceleration * velocity;

            // Translates to 1 tile moved every 8/60 frames or 7.5 tiles / second

            Position += velocity;

            if (CollidingWithMap)
            {
                // Knock the player back

                Position -= velocity;
            }
            foreach (NPC npc in CreateEntityFactory.NPCS)
            {

                if (IsColliding(new Rectangle((int) Position.X,
                    (int) Position.Y, Sprite.Width,
                    Sprite.Height), npc.Sprite.BoundingBox))
                {
                    // Player cannot move forward

                    Position -= velocity;
                } 
            }
            Sprite.Position = Position;
        }

        protected virtual void UpdateSpriteDirection(string direction)
        {
            switch (direction)
            {
                case "North":
                    velocity = new Vector2(0, -1);
                    Sprite = new Sprite(graphicsDevice, "Content/Images/Sprites/Player.png",
                        32, 30, 90, 128, 12, 0.2, 30, 32, Color.White, 9);
                    break;
                case "East":
                    velocity = new Vector2(1, 0);
                    Sprite = new Sprite(graphicsDevice, "Content/Images/Sprites/Player.png",
                        32, 30, 90, 128, 9, 0.2, 30, 32, Color.White, 6);
                    break;
                case "South":
                    velocity = new Vector2(0, 1);
                    Sprite = new Sprite(graphicsDevice, "Content/Images/Sprites/Player.png",
                        32, 30, 90, 128, 4, 0.2, 30, 32, Color.White, 0);
                    break;
                case "West":
                    velocity = new Vector2(-1, 0);
                    Sprite = new Sprite(graphicsDevice, "Content/Images/Sprites/Player.png",
                        32, 30, 90, 128, 6, 0.2, 30, 32, Color.White, 3);
                    break;
            }
        }
    }
}