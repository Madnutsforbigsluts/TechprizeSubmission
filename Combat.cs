using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using Glyph.Entities;
using Glyph.GUI;
using Glyph.UI;
using static Glyph.Utilities.CheckBetween;
using static Glyph.Utilities.Initialize2DArray;

namespace Glyph.Combat
{
    public enum EntityState
    {
        Deciding,
        Idle,
        Start,
        End,
        Attack,
        Defend,
        CheckInv,
        GetCard
    }
    
    public enum DialogueState
    {
        None,
        Flee,
        Deciding,
        Missed, 
        EnemyAttackPlayer,
        PlayerAttackEnemy,
        TriggeredDamageEvent,
        TriggeredHealEvent,
        TriggeredStasisEvent
    }

    public enum EventState
    {
        None, 
        Default, 
        DamageEvent,
        HealEvent,
        StasisEvent 
    }

    public sealed class Battle
    {
        private Player player;
        private Enemy enemy;

        public EntityState playerState = EntityState.Start;
        EntityState enemyState = EntityState.Start;

        // When the dialogue changes set the current dialogue to null so the new dialogue may start 
        DialogueState _dialogueState;
        DialogueState dialogueState
        {
            get { return _dialogueState; }
            set
            {
                if(value != _dialogueState)
                {
                    dialogue = null;
                }
                _dialogueState = value;
            }
        }

        EventState eventState = EventState.None;

        // The length of each turn 
        private static readonly TimeSpan TurnBasedWaitTime = TimeSpan.FromMilliseconds(2000);
        private TimeSpan LastTimeOfTurn;
        private TimeSpan LastTimeOfEvent;
        private TimeSpan LastTimeOfEnemyTurn;

        GameTime gameTime;

        private int PaddingX = 260;
        private int PaddingY = 260;
        
        public CombatGUI combatGUI; 

        private int playerMaxHealth;
        private int enemyMaxHealth;

        private readonly int BattleFieldWidth = GlobalConstants.BattleFieldWidth;
        private readonly int BattleFieldHeight = GlobalConstants.BattleFieldHeight;
        private readonly int CellWidth = GlobalConstants.BattleCellWidth;
        private readonly int CellHeight = GlobalConstants.BattleCellHeight;

        private delegate void MethodToRun(int i, int j);

        Action<int, int, int, MethodToRun> DrawEntityRadius;
        Action<int, int, MethodToRun> DrawEntity;
        Action DrawPlayerSelection;
        Action DrawBattleField;
        Action DrawEvents;

        // Color CheckerColor1 = Color.Black;
        // Color CheckerColor2 = Color.White;

        Color BorderColor = Color.Black;
        Color SelectionColor = Color.Green;
        Color PlayerRadiusColor = Color.Lime;
        Color EnemyRadiusColor = Color.Red;
        Color PlayerColor = Color.Green;
        Color EnemyColor = Color.Red;
        Color DefaultCellColor = Color.White;

        MouseState current;
        MouseState last;

        SpriteFont font;
        SpriteBatch spriteBatch;

        Dictionary<string, Texture2D> textures;

        BattleCell[,] BattleField;

        Dialogue dialogue;

        private int playerDamage;
        private int enemyDamage;
        private int eventHealthMod;

        private List<Action> BattleEvents = new List<Action>();
        private Random RandGenerator = new Random();

        // The two card decks 
        CardSystem playersCards = new CardSystem();
        CardSystem cardStack = new CardSystem();



        // The aim is to get the sum of card's as close to possible to a
        // random integer value indicated through a random range. If achieved
        // the player's damage will be doubled 

        private int solution;

        private int randomLowerRange;
        private int randomUpperRange;

        private int _cardSum;
        private int cardSum
        {
            get { return _cardSum; }
            set
            {
                damageScalar = cardSum < solution ? 1.5f : 1f;
                Console.WriteLine(solution);
                _cardSum = value; 
            }
        }

        private float damageScalar; 

        private Card cardInHand;

        private int? playersTopCardID;
        private int? cardStacksTopCardID; 

        // The actual rectangles which represents the card decks. 
        Rectangle playerCard = new Rectangle(50, 25, 150, 150);
        Rectangle stackCard = new Rectangle(825, 25, 150, 150);

        // The card that 'demos' what the player is supposed to do with the card. 
        Point _demoCardPos;
        Point demoCardPos
        {
            get { return _demoCardPos; }
            set
            {
                _demoCardPos = value.X > stackCard.X ?
                   new Point(playerCard.X, playerCard.Y) : value;
            }
        }

        public Battle(Player player, Enemy enemy)
        {
            this.player = player;
            this.enemy = enemy;

            solution = RandGenerator.Next(1, 56);
            randomLowerRange = RandGenerator.Next(0, solution);
            randomUpperRange = RandGenerator.Next(solution, 56);

            demoCardPos = new Point(playerCard.X, playerCard.Y);
            playersCards.PopulateDeck(); 
            playersCards.Shuffle(); 

            combatGUI = new CombatGUI(); 

            player.BattleLocation = new Point(RandGenerator.Next(BattleFieldWidth), RandGenerator.Next(BattleFieldHeight));

            // Generate random starting positions for each entity until they don't overlap 
            while (enemy.BattleLocation == player.BattleLocation)
            {
                enemy.BattleLocation = new Point(RandGenerator.Next(BattleFieldWidth), RandGenerator.Next(BattleFieldHeight));
            }

            playerMaxHealth = player.Health;
            enemyMaxHealth = enemy.Health;

            // Run actions in the cells on the battlefield 

            BattleField = Populate2DArray<BattleCell>(BattleFieldWidth, BattleFieldHeight);

            DrawEvents = () => RunMethodInCells(0, BattleFieldHeight, 0, BattleFieldWidth, ColorEvents);

            DrawEntityRadius = (radius, posX, posY, ColorMethod) => RunMethodInCells(posY - radius,
                posY + radius + 1, posX - radius, posX + radius + 1, ColorMethod);

            DrawEntity = (posX, posY, ColorMethod) => RunMethodInCells(posY, posY + 1, posX,
                posX + 1, ColorMethod);

            DrawPlayerSelection = () => RunMethodInCells(0, BattleFieldHeight, 0, BattleFieldWidth, ColorSelection);

            DrawBattleField = () => RunMethodInCells(0, BattleFieldHeight, 1, BattleFieldWidth + 1, ColorField);

            // Define new events to trigger in cells below (I.E Player takes poison damage, player becomes idle for 3 turns etc...)

            Action Default = () => { };

            Action DamageEvent = () =>
            {
                eventHealthMod = RandGenerator.Next(0, player.Health / 4);
                player.TakeDamage(eventHealthMod);
                combatGUI.playerHealthBar.Value = GetPercentHealthLeft(playerMaxHealth, player.Health);
                dialogueState = DialogueState.TriggeredDamageEvent;
                eventState = EventState.DamageEvent;
                LastTimeOfEvent = gameTime.TotalGameTime;
            };

            Action HealEvent = () =>
            {
                eventHealthMod = RandGenerator.Next(0, playerMaxHealth);
                player.Heal(eventHealthMod);
                combatGUI.playerHealthBar.Value = GetPercentHealthLeft(playerMaxHealth, player.Health);
                dialogueState = DialogueState.TriggeredHealEvent;
                eventState = EventState.HealEvent;
                LastTimeOfEvent = gameTime.TotalGameTime;
            };

            Action StasisEvent = () =>
            {
                enemy.Turns = RandGenerator.Next(1, 4);
                playerState = EntityState.Idle;
                enemyState = EntityState.Attack;
                dialogueState = DialogueState.TriggeredStasisEvent;
                eventState = EventState.StasisEvent;
                LastTimeOfEvent = gameTime.TotalGameTime;
            };

            BattleEvents.Add(Default);
            BattleEvents.Add(DamageEvent);
            BattleEvents.Add(HealEvent);
            BattleEvents.Add(StasisEvent);

        }

        public void LoadContent(Dictionary<string, Texture2D> textures, SpriteFont font)
        {
            this.font = font;
            this.textures = textures;
            combatGUI.LoadContent(font);
            combatGUI.playerHealthBar.Value = GetPercentHealthLeft(playerMaxHealth, player.Health);
            combatGUI.enemyHealthBar.Value = GetPercentHealthLeft(enemyMaxHealth, enemy.Health);
        }

        public void Update(GameTime gameTime, MouseState current, MouseState last)
        {
            this.current = current;
            this.last = last;
            this.gameTime = gameTime;

            // The last turn is over 
            if (LastTimeOfTurn + TurnBasedWaitTime < gameTime.TotalGameTime)
            {
                // Create the next turn 
                DoTurn();
                LastTimeOfTurn = gameTime.TotalGameTime;
            }

            if(dialogue != null)
            {
                dialogue.Update(gameTime);
            }
            UpdateCards(); 
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            this.spriteBatch = spriteBatch;
            DrawDialogue();
            DrawEvents();
            DrawBattleField();
            DrawStatistics();
            DrawEntityRadius(player.SelectionRadius, enemy.BattleLocation.X, enemy.BattleLocation.Y, ColorEnemyRadius);

            if (playerState == EntityState.Deciding && LastTimeOfEnemyTurn + TurnBasedWaitTime < gameTime.TotalGameTime)
            {
                DrawPlayerSelection();
            }

            // Draw the player's and enemey's avatars
            spriteBatch.Draw(player.Avatar, new Rectangle(30, 250, 160, 256), Color.White);
            spriteBatch.Draw(enemy.Avatar, new Rectangle(720, 250, 256, 256), Color.White);

            DrawEntity(enemy.BattleLocation.X, enemy.BattleLocation.Y, ColorEnemy);
            DrawEntity(player.BattleLocation.X, player.BattleLocation.Y, ColorPlayer);
            DrawEntityRadius(enemy.SelectionRadius, player.BattleLocation.X, player.BattleLocation.Y, ColorPlayerRadius);

            DrawCards();
        }

        private static int GetPercentHealthLeft(int max, int current)
        {
            try
            {
                return 100 - (100 * (max - current) / max);
            }
            catch (DivideByZeroException)
            {
            }
            return 0;
        }

        private void DrawStatistics()
        {
            // Enemy statistics 
            spriteBatch.DrawString(font, $"Level: {enemy.Level}", new Vector2(750, 500), Color.White, 0.0f,
                new Vector2(0, 0), 0.5f, SpriteEffects.None, 1f);
            spriteBatch.DrawString(font, $"Health: {enemy.Health}", new Vector2(750, 525), Color.White, 0.0f,
                new Vector2(0, 0), 0.5f, SpriteEffects.None, 1f);

            // Player statistics
            spriteBatch.DrawString(font, $"Level: {player.Level}", new Vector2(30, 500), Color.White, 0.0f,
                new Vector2(0, 0), 0.5f, SpriteEffects.None, 1f);
            spriteBatch.DrawString(font, $"Health: {player.Health}", new Vector2(30, 525), Color.White, 0.0f,
                new Vector2(0, 0), 0.5f, SpriteEffects.None, 1f);
        }

        private void DrawDialogue()
        {
            if(dialogue == null)
            {
                switch (dialogueState)
                {
                    case DialogueState.PlayerAttackEnemy:
                        dialogue = new Dialogue(new List<string>()
                        { $"You Gave: {playerDamage} Dmg" },
                            font, textures["DialogueBox"]);
                        break;
                    case DialogueState.EnemyAttackPlayer:
                        dialogue = new Dialogue(new List<string>()
                        { $"You Took: {enemyDamage} Dmg" },
                            font, textures["DialogueBox"]);
                        break;
                    case DialogueState.Flee:
                        dialogue = new Dialogue(new List<string>()
                        { "You fled the battle successfully!" },
                            font, textures["DialogueBox"]);
                        break;
                    case DialogueState.TriggeredDamageEvent:
                        dialogue = new Dialogue(new List<string>()
                        { "You suffered damage from landing on a damage event!" },
                            font, textures["DialogueBox"]);
                        break;
                    case DialogueState.TriggeredHealEvent:
                        dialogue = new Dialogue(new List<string>()
                        { $"You gained: {eventHealthMod} health from landing on " +
                        $"a heal event"}, font, textures["DialogueBox"]);
                        break;
                    case DialogueState.TriggeredStasisEvent:
                        dialogue = new Dialogue(new List<string>()
                        { "You Are paralyzed from landing on a paralysis event" },
                            font, textures["DialogueBox"]);
                        break;
                    case DialogueState.Deciding:
                        dialogue = new Dialogue(new List<string>()
                        { "Waiting for your decision..." },
                            font, textures["DialogueBox"]);
                        break;
                    case DialogueState.Missed:
                        dialogue = new Dialogue(new List<string>()
                        { "You Missed your attack!" },
                            font, textures["DialogueBox"]);
                        break;
                    case DialogueState.None:
                        dialogue = new Dialogue(new List<string>()
                        { "Enemy moved!" },
                        font, textures["DialogueBox"]);
                        break;
                }
            }
            if (dialogue != null)
            {
                dialogue.Draw(spriteBatch);
            }
        }

        /// <summary>
        /// Check an entitie's radius collides with another entity 
        /// </summary>
        /// <param name="entityPosX"></param>
        /// <param name="entityPosY"></param>
        /// <param name="targetPosX"></param>
        /// <param name="targetPosY"></param>
        /// <param name="entityRadius"></param>
        /// <returns></returns>
        private bool CheckRadiusCollision(int entityPosX, int entityPosY, int targetPosX, int targetPosY, int entityRadius)
        {
            if (IsBetween(entityPosX - entityRadius, entityPosX + entityRadius, targetPosX)
                && IsBetween(entityPosY - entityRadius, entityPosY + entityRadius, targetPosY))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Trigger's the event in the cell an entity is currently in
        /// </summary>
        /// <param name="entity"></param>
        private void TriggerEvent(Entity entity)
        {
            if (entity == player && BattleField[entity.BattleLocation.X, entity.BattleLocation.Y].EventsLeft >= 0)
            {
                BattleField[entity.BattleLocation.X, entity.BattleLocation.Y].StartEvent(
                BattleEvents[BattleField[entity.BattleLocation.X, entity.BattleLocation.Y].EventID]);
            }
        }

        /// <summary>
        /// Checks the state of the buttons and sets the states accordingly
        /// </summary>
        private void CheckButtonState()
        {
            switch (combatGUI.buttonState)
            {
                case CombatButtonState.Attack:
                    playerState = EntityState.Attack;
                    enemyState = EntityState.Idle;
                    break;
                case CombatButtonState.Defend:
                    playerState = EntityState.Defend;
                    enemyState = EntityState.Attack;
                    break;
                case CombatButtonState.Idle:
                    playerState = EntityState.Idle;
                    enemyState = EntityState.Attack;
                    break;
                case CombatButtonState.Flee:
                    Game1.sceneState = SceneState.ShowOverWorld;
                    Game1.player.EncounteredEnemy = null;
                    playerState = EntityState.End;
                    enemyState = EntityState.End;
                    dialogueState = DialogueState.Flee;
                    break;
                case CombatButtonState.Shuffle:
                    playersCards.Shuffle();
                    break;
                case CombatButtonState.None:
                    playerState = EntityState.Deciding;
                    enemyState = EntityState.Idle;
                    break;
            }
            combatGUI.buttonState = CombatButtonState.None;
        }

        private void CheckMovementState()
        {
            if (player.HasMoved)
            {
                playerState = EntityState.Idle;
                enemyState = EntityState.Attack;
            }
            player.HasMoved = false;
        }

        /// <summary>
        /// Controls the call's for each turn. 
        /// </summary>
        private void DoTurn()
        {
            // The battle is still going 
            if (playerState != EntityState.End && enemyState != EntityState.End && !player.IsDead()
                && !enemy.IsDead())
            {
                // The player is making a decision or their speed is greater than the enemies on the first turn 
                if (playerState == EntityState.Deciding ||
                    (player.Speed >= enemy.Speed && playerState == EntityState.Start))
                {
                    dialogueState = DialogueState.Deciding;
                    CheckButtonState();
                    CheckMovementState();
                }
                CheckEntityStates();
            } else
            {
                Game1.sceneState = SceneState.ShowOverWorld;
                Game1.player.EncounteredEnemy = null;
            }
        }

        private void EnemyAttack(float scalar)
        {
            enemyDamage = (int)(enemy.GetDamageGiven() * scalar);
            player.TakeDamage(enemyDamage);
            combatGUI.playerHealthBar.Value = GetPercentHealthLeft(playerMaxHealth, player.Health);
        }

        private void ResetEnemyAfterAttack()
        {
            enemy.Turns = 1;
            LastTimeOfEnemyTurn = gameTime.TotalGameTime;
            dialogueState = DialogueState.EnemyAttackPlayer;
            playerState = EntityState.Deciding;
            enemyState = EntityState.Idle;
        }

        private void ResetEnemyAfterMove()
        {
            enemy.Turns = 1;
            LastTimeOfEnemyTurn = gameTime.TotalGameTime;
            dialogueState = DialogueState.None;
            playerState = EntityState.Deciding;
            enemyState = EntityState.Idle;
        }


        private void PlayerAttack(float scalar)
        {
            playerDamage = (int)(player.GetDamageGiven() * scalar) + 5;
            enemy.TakeDamage(playerDamage);
            combatGUI.enemyHealthBar.Value = GetPercentHealthLeft(enemyMaxHealth, enemy.Health);
        }

        private void ResetPlayerAfterAttack()
        {
            player.Turns = 1;
            dialogueState = DialogueState.PlayerAttackEnemy;
            playerState = EntityState.Idle;
            enemyState = EntityState.Attack;
        }

        private void MissedAttack()
        {
            playerState = EntityState.Idle;
            enemyState = EntityState.Attack;
            dialogueState = DialogueState.Missed;
        }

        /// <summary>
        /// Check the states of the enemy and player according to turn-based pattern,
        /// run the appropriate response according to each state / turn. 
        /// </summary>
        private void CheckEntityStates()
        {
            // The enemy is in range to attack the player 
            if(CheckRadiusCollision(enemy.BattleLocation.X, enemy.BattleLocation.Y,
               player.BattleLocation.X, player.BattleLocation.Y, enemy.SelectionRadius + 1))
            {
                // The player is idle, therefore the enemy attack's the player. 
                if (playerState == EntityState.Idle)
                {
                    EnemyAttack(1f);
                    ResetEnemyAfterAttack();
                }
                // The player is defending, therefore they take half as much damage from the enemy 
                else if (playerState == EntityState.Defend)
                {
                    EnemyAttack(0.5f);
                    ResetEnemyAfterAttack(); 
                }
            }
            // The enemy is not in range to attack the player 
            else if (!CheckRadiusCollision(enemy.BattleLocation.X, enemy.BattleLocation.Y,
                player.BattleLocation.X, player.BattleLocation.Y, enemy.SelectionRadius + 1))
            {
                // Could invoke an enemy AI, causing the enemy to move closer to the player
                // Not implemented yet. Using a random number around player for now.

                int potentialXDir = player.BattleLocation.X >= enemy.BattleLocation.X ?
                    1 : -1;
                int potentialYDir = player.BattleLocation.Y >= enemy.BattleLocation.Y ?
                    1 : -1;

                enemy.MoveToPosition(enemy.BattleLocation.X + potentialXDir,
                    enemy.BattleLocation.Y + potentialYDir);
                ResetEnemyAfterMove();
            }

            // The player is in range to attack the enemy  
            if (CheckRadiusCollision(player.BattleLocation.X, player.BattleLocation.Y,
                enemy.BattleLocation.X, enemy.BattleLocation.Y, player.SelectionRadius + 1))
            {
                // The player chooses to attack 
                if (playerState == EntityState.Attack)
                {
                    // The scalar for damage is based on the card's they pulled 
                    PlayerAttack(damageScalar);
                    ResetPlayerAfterAttack();
                }
            }
            // The player is not in range to attack the enemy 
            else if (!CheckRadiusCollision(player.BattleLocation.X, player.BattleLocation.Y,
                enemy.BattleLocation.X, enemy.BattleLocation.Y, player.SelectionRadius + 1))
            {
                if(playerState == EntityState.Attack)
                {
                    MissedAttack();
                }
            }
        }

     /// <summary>
     /// Runs an action in every cell on the battlefield. Used for coloring cells and
     /// Putting 'events' in each cell. 
     /// </summary>
     /// <param name="startingRow"></param>
     /// <param name="endingRow"></param>
     /// <param name="startingCol"></param>
     /// <param name="endingCol"></param>
     /// <param name="action"></param>
      private static void RunMethodInCells(int startingRow, int endingRow,
      int startingCol, int endingCol, MethodToRun action)
        {
            for (int j = startingRow; j < endingRow; j++)
            {
                for (int i = startingCol; i < endingCol; i++)
                {
                    action(i, j);
                }
            }
        }

        // Uncomment for checkerboard pattern
        /*
        private void ColorCheckerboard(int i, int j)
        {
            if ((i % 2 == 0 && j % 2 == 0) || (i % 2 != 0 && j % 2 != 0))
            {
                BattleField[i, j].CellColor = CheckerColor1;
            }
            else if ((i % 2 == 0 && j % 2 != 0) || (i % 2 != 0 && j % 2 == 0))
            {
                BattleField[i, j].CellColor = CheckerColor2; 
            }
            spriteBatch.Draw(texture, new Rectangle(i * CellWidth, j * CellHeight,
                CellWidth, CellHeight), BattleField[i, j].CellColor);
        }
        */

        private void ColorField(int i, int j)
        {
            int BorderMaxPosX = (i * CellWidth) + PaddingX;
            int BorderMinPosX = BorderMaxPosX - CellWidth;
            int BorderMaxPosY = (j * CellHeight) + PaddingY;
            int BorderMinPosY = BorderMaxPosY + CellHeight;

            // Draw the border's (I.E lines) for the battlefield 
            spriteBatch.Draw(textures["Default"], new Rectangle(BorderMaxPosX, BorderMaxPosY,
                1, CellHeight), BorderColor);
            spriteBatch.Draw(textures["Default"], new Rectangle(BorderMinPosX, BorderMinPosY,
                CellWidth, 1), BorderColor);
        }

        // Below 4 functions should probably be grouped into 2 general functions 

        private void ColorPlayerRadius(int i, int j)
        {
            if (IsBetween(-1, BattleFieldWidth, i) && IsBetween(-1, BattleFieldHeight, j))
            {
                // Draw's the player's 'radius' of attack
                spriteBatch.Draw(textures["Default"], new Rectangle((i * CellWidth) + PaddingX,
                    (j * CellHeight) + PaddingY, CellWidth, CellHeight), PlayerRadiusColor * 0.5f);
            } 
        }

        private void ColorEnemyRadius(int i, int j)
        {
            if (IsBetween(-1, BattleFieldWidth, i) && IsBetween(-1, BattleFieldHeight, j))
            {
                // Draw's the enemy's 'radius' of attack 
                spriteBatch.Draw(textures["Default"], new Rectangle((i * CellWidth) + PaddingX,
                    (j * CellHeight) + PaddingY, CellWidth, CellHeight), EnemyRadiusColor * 0.5f);
            }
        }

        private void ColorPlayer(int i, int j)
        {
            // Draws player avatar in the cell 
            spriteBatch.Draw(player.Avatar, new Rectangle((i * CellWidth) + PaddingX,
                (j * CellHeight) + PaddingY, (int) (CellWidth * 0.625), CellHeight), PlayerColor); ;
        }

        private void ColorEnemy(int i, int j)
        {
            // Draws enemy avatar in the cell
            spriteBatch.Draw(enemy.Avatar, new Rectangle((i * CellWidth) + PaddingX,
                (j * CellHeight) + PaddingY, CellWidth, CellHeight), EnemyColor);
        }

        private void ColorEvents(int i, int j)
        {
            Color EventColor = DefaultCellColor;

            if(BattleField[i, j].EventID != 0)
            {
                EventColor = Color.Black;
            };

            // Draw the cell's color as the event type
            spriteBatch.Draw(textures["Default"], new Rectangle((i * CellWidth) + PaddingX,
                (j * CellHeight) + PaddingY, CellWidth, CellHeight), EventColor);
        }

        /// <summary>
        /// 'Colors' the current selected cell on the battlefield 
        /// </summary>
        /// <param name="i"></param>
        /// <param name="j"></param>
        private void ColorSelection(int i, int j)
        {
            // The width and height required to get the required quadrants of the cell 
            int xBound = CellWidth / 2;
            int yBound = CellHeight / 2;

            // Use the midpoint of the cell to get all quadrants in the cell 
            int xCentre = (i * CellWidth) + xBound + PaddingX;
            int yCentre = (j * CellHeight) + yBound + PaddingY;


            Tuple<int, int> selectionPos = new Tuple<int, int>(i, j);

            // The player's cursor is in the cell 
            if (IsBetween(xCentre - xBound, xCentre + xBound, current.X) &&
                IsBetween(yCentre - yBound, yCentre + yBound, current.Y) &&
                !enemy.BattleLocation.Equals(selectionPos))

            {
                // Draw the cell with a different color to indicate it is being selected 
                spriteBatch.Draw(textures["Default"], new Rectangle((i * CellWidth) + PaddingX,
                    (j * CellHeight) + PaddingY, CellWidth, CellHeight), SelectionColor);

                // The player clicks in the cell
                if (last.LeftButton == ButtonState.Released
                    && current.LeftButton == ButtonState.Pressed
                    && playerState == EntityState.Deciding)
                {
                    player.MoveToPosition(i, j);
                    LastTimeOfEnemyTurn = gameTime.TotalGameTime;
                    TriggerEvent(player);
                    TriggerEvent(enemy);
                }
            }
        }

        private void UpdateCards()
        {
            playersTopCardID = playersCards.currentCardID;
            cardStacksTopCardID = cardStack.currentCardID;

            // The cursor is in the player's deck. 
            if (IsBetween(playerCard.X, playerCard.X + playerCard.Width, current.X)
                && IsBetween(playerCard.Y, playerCard.Y + playerCard.Height, current.Y))
            {
                if (last.LeftButton == ButtonState.Released
                    && current.LeftButton == ButtonState.Pressed
                    && playerState == EntityState.Deciding)
                {
                    // The card in the player's hand is empty, so the top card is picked up
                    // and put in the player's hand 
                    if (cardInHand.ID == null && playersTopCardID != null)
                    {
                        dialogue = new Dialogue(new List<string>
                        {
                            $"The key is between: {randomLowerRange} and {randomUpperRange}."
                        }, font, textures["DialogueBox"]);
                        cardInHand.ID = playersTopCardID;
                        playersCards.TakeCardFromDeck();
                    }
                    // The player places the card back on the deck 
                    else
                    {
                        dialogue = null; 
                        playersCards.PlaceCardOnDeck(cardInHand);
                        cardInHand.ID = null;
                    }
                }
            }
            // The cursor is in the other deck 
            else if(IsBetween(stackCard.X, stackCard.X + stackCard.Width, current.X)
                && IsBetween(stackCard.Y, stackCard.Y + stackCard.Height, current.Y))
            {
                if (last.LeftButton == ButtonState.Released
                    && current.LeftButton == ButtonState.Pressed
                    && playerState == EntityState.Deciding)
                {
                    // The player places the card in their hand on the other deck 
                    if (cardInHand.ID != null)
                    {
                        cardStack.PlaceCardOnDeck(cardInHand);
                        cardSum += (int)cardInHand.ID;
                        cardInHand.ID = null;
                    }
                }
            }
        }

        private void DrawCards()
        {

            // Draw the two deck placeholder rectangles 
            spriteBatch.Draw(textures["Default"], new Rectangle(50, 25, 100, 150), Color.White);
            spriteBatch.Draw(textures["Default"], new Rectangle(825, 25,100, 150), Color.White);

            // Draw the top card of the other deck 
            if(cardStacksTopCardID != null)
            {
                spriteBatch.Draw(textures[$"Card{cardStacksTopCardID}"], stackCard, Color.White);
            }

            // Draw the top card of the player's deck 
            if(playersTopCardID != null)
            {
                spriteBatch.Draw(textures["TurnedCard"], playerCard, Color.White);
            }

            // Draw the card the player picked up
            if(cardInHand.ID != null)
            {
                spriteBatch.Draw(textures[$"TurnedCard"], new Rectangle(current.X, current.Y,
                    playerCard.Width, playerCard.Height), Color.White);

                demoCardPos += new Point(8, 0);
                spriteBatch.Draw(textures[$"TurnedCard"], new Rectangle(demoCardPos.X,
                    demoCardPos.Y, playerCard.Width, playerCard.Height), Color.White * 0.6f);
            }
        }
    }
}
