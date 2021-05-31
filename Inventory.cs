using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Glyph.Utilities; 
using static Glyph.Utilities.CheckBetween;
using static Glyph.Utilities.Initialize2DArray; 
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Glyph.Inventories
{
    public class Inventory
    {
        private bool doubleClicked; 
        CheckDoubleClick checkDoubleClick = new CheckDoubleClick(); 

        private GraphicsDevice graphics;
        private Rectangle slot;
        Dictionary<string, Texture2D> textures;

        private SpriteFont font; 

        private static InvItem ItemInHand;

        private const int NumRows = 5;
        private const int NumCols = 5;

        MouseState current;
        MouseState last;

        private readonly int[,,] MidPoints = new int[,,]
        {
            { {32, 32}, {128, 32}, {224, 32}, {320, 32}, {416, 32} },
            { {32, 128}, {128, 128}, {224, 128}, {320, 128}, {416, 128} },
            { {32, 224}, {128, 224}, {224, 224}, {320, 224}, {416, 224} },
            { {32, 320}, {128, 320}, {224, 320}, {320, 320}, {416, 320} },
            { {32, 416}, {128, 416}, {224, 416}, {320, 416}, {416, 416} },
        };

        private static InvItem[,] IdTable;

        public Inventory()
        {
            IdTable = Populate2DArray<InvItem>(NumCols, NumRows);
            ItemInHand = new InvItem();
            slot = new Rectangle();

            // Method 1 of adding to inventory table 

            /*
            IdTable[1, 1].ItemId = 2;
            IdTable[1, 1].ItemType = "Potion";
            IdTable[1, 1].Quantity = 1;
            IdTable[1, 1].CanStack = false;
            */

            // Method 2 of adding to inventory table

            /*
            obtainableItems.Add(new InvItem("Potion", 1, 100));
            AddItems(obtainableItems);
            */
        }

        public void Update(GameTime gameTime, GraphicsDevice graphics, MouseState last, MouseState current)
        {
            this.last = last;
            this.current = current;
            this.graphics = graphics;
            checkDoubleClick.Update(gameTime, current, last);
            doubleClicked = checkDoubleClick.DoubleClicked;
            ManageInventory();
        }

        public void LoadContent(Dictionary<string, Texture2D> textures, SpriteFont font)
        {
            this.font = font; 
            this.textures = textures;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            for (int i = 0; i < NumRows; i++)
            {
                for (int j = 0; j < NumCols; j++)
                {

                    Color itemColor = Color.LightGray;
                    Color itemInSlotColor = Color.WhiteSmoke;

                    slot.X = MidPoints[i, j, 0] - InvItem.Width / 2;
                    slot.Y = MidPoints[i, j, 1] - InvItem.Height / 2;

                    if (ItemInHand.ID.HasValue)
                    {
                        DrawSlot(textures["Default"], slot.X, slot.Y, itemInSlotColor, "Item");
                        DrawSlot(textures["Default"], slot.X, slot.Y, itemColor, IdTable[i, j].Type);
                        DrawSlot(textures["Default"], current.X, current.Y, itemColor, ItemInHand.Type);
                    }
                    else
                    {
                        DrawSlot(textures["Default"], slot.X, slot.Y, itemInSlotColor, "Item");
                        DrawSlot(textures["Default"], slot.X, slot.Y, itemColor, IdTable[i, j].Type);
                    }

                    void DrawSlot(Texture2D texture2D, int posX, int posY, Color color, string itemType)
                    {
                        // Draw what's in the slot based on it's type 
                        switch (itemType)
                        {
                            case "Weapon":
                                texture2D = textures["WeaponItem"];
                                break;

                            case "Key":
                                texture2D = textures["KeyItem"];
                                break;

                            case "Food":
                                texture2D = textures["FoodItem"];
                                break;

                            case "Item":
                                texture2D = textures["Default"];
                                break;

                        };

                        // Draw the slot's themselves. 
                        spriteBatch.Draw(texture2D, new Rectangle(posX, posY,
                        InvItem.Width, InvItem.Height), color);
                        spriteBatch.DrawString(font, Convert.ToString(IdTable[i, j].Quantity),
                        new Vector2(slot.X, slot.Y), Color.Black, 0f, Vector2.Zero, 0.3f,
                        SpriteEffects.None, 0);
                    }
                }
            }
        }

        public void ManageInventory()
        {
            // Loop over the entire IdTable for each tick 
            for (int i = 0; i < NumRows; i++)
            {
                for (int j = 0; j < NumCols; j++)
                {
                    // Assigns the 2nd lower half for the width and height of the slot to be drawn 
                    int xBound = InvItem.Width / 2;
                    int yBound = InvItem.Height / 2;

                    // Assigns the centre position of the slot to be drawn 
                    int xCentre = MidPoints[i, j, 0];
                    int yCentre = MidPoints[i, j, 1];

                    var currentSlot = IdTable[i, j]; 

                    // Check if the mouse is positioned in one of the slots 
                    if (IsBetween(xCentre - xBound, xCentre + xBound, current.X) && IsBetween(yCentre - yBound,
                        yCentre + yBound, current.Y))
                    {
                        // If the left click has been clicked once 
                        if (last.LeftButton == ButtonState.Released
                            && current.LeftButton == ButtonState.Pressed)
                        {
                            // If the player's hands are empty they can pick up
                            // whatever is in the current slot 
                            if (currentSlot.ID.HasValue && ItemInHand.ID == null)
                            {
                                ItemInHand = currentSlot;
                                IdTable[i, j] = new InvItem();
                            }
                            // If the player's hands aren't empty and the current slot isn't empty 
                            // and they're different items then they swap the items 
                            else if (currentSlot.ID.HasValue && ItemInHand.ID.HasValue
                                && currentSlot.ID != ItemInHand.ID)
                            {
                                InvItem temp = currentSlot;
                                IdTable[i, j] = ItemInHand;
                                ItemInHand = temp;
                            }
                            // If the player's hands are full and the slot is empty
                            // place the item in the current slot 
                            else if (ItemInHand.ID.HasValue && currentSlot.ID == null)
                            {
                                IdTable[i, j] = ItemInHand;
                                ItemInHand = new InvItem();
                            }
                            // If the held item and item in the current slot are the same and
                            // their combined quantity isn't greater than the max stack limit
                            // add the held item to the current slot 
                            else if (ItemInHand.ID.HasValue && currentSlot.ID == ItemInHand.ID &&
                                ItemInHand.Quantity + currentSlot.Quantity <= IdTable[i, j].MaxStackable &&
                                ItemInHand.CanStack == true)
                            {
                                IdTable[i, j].Quantity += ItemInHand.Quantity;
                                ItemInHand = new InvItem();
                            }
                        }
                        else if (doubleClicked)
                        {
                            if (IsBetween(0, ItemInHand.MaxStackable, ItemInHand.Quantity)
                                && currentSlot.Quantity > 1 && ItemInHand.ID == currentSlot.ID
                                && ItemInHand.CanStack)
                            {
                                IdTable[i, j].Quantity -= 1;
                                ItemInHand.Quantity += 1;
                            }
                            else if (currentSlot.ID == null && ItemInHand.Quantity > 1) 
                            {
                                IdTable[i, j] = new InvItem(ItemInHand.ID, 1);
                                ItemInHand.Quantity -= 1;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Adds items to the 2D IdTable upon request.
        /// <param name="items">items is a list of type InvItem</param>
        /// </summary>
        public void Add(List<InvItem> items)
        {
            foreach (InvItem item in items)
            {
                int itemsLeft = item.Quantity;

                // Loop over the table of items for each item to be added 
                for (int i = 0; i < NumRows; i++)
                {
                    for (int j = 0; j < NumCols; j++)
                    {
                        var currentSlot = IdTable[i, j];

                        if (itemsLeft > 0)

                            // Check if the items being added to the inventory match with another slot
                            // and they can both be added together (I.E same Id's)
                            if (currentSlot.ID == item.ID && currentSlot.ID.HasValue && item.CanStack)
                            {
                                // If the quantity of the combined items in the slot exceeds the max stack then
                                // the amount left will be the difference between the max stack and the amount
                                // already in the slot 
                                if (currentSlot.Quantity + item.Quantity >= currentSlot.MaxStackable)
                                {
                                    itemsLeft -= currentSlot.MaxStackable - currentSlot.Quantity;
                                    IdTable[i, j].Quantity = currentSlot.MaxStackable;
                                }
                                // Alternatively, if the quantity of the combined items in the slot is less
                                // than the max stack, add the remaining items because we know this
                                // will not exceed the slots max stack. 
                                else if (currentSlot.Quantity + itemsLeft <= currentSlot.MaxStackable)
                                {
                                    IdTable[i, j].Quantity += itemsLeft;
                                    itemsLeft = 0;
                                }
                            }
                            // Slot is empty 
                            else if (currentSlot.ID == null)
                            {
                                // If the quantity of the items left is greater than the items stack limit 
                                // then set the currentslots quantity to the items max stack and subtract
                                // that from the items left
                                if (itemsLeft >= item.MaxStackable || item.MaxStackable == 1)
                                {
                                    IdTable[i, j] = new InvItem(item.ID, item.MaxStackable);
                                    itemsLeft -= item.MaxStackable;
                                }
                                // If the quantity of the items Left is less than the items max stack limit
                                // than place the remaining items in the slot and there will be none remaining 
                                else if (itemsLeft <= item.MaxStackable)
                                {
                                    IdTable[i, j] = new InvItem(item.ID, itemsLeft);
                                    itemsLeft = 0;
                                }
                            }
                        }
                    }
                }
            }

        public void Add(InvItem invItem)
        {
            List<InvItem> item = new List<InvItem> { invItem }; 
            Add(item);
        }

        //TODO implement system which reads inventory from database 
        public void InitializeInventory()
        {

        }

        //TODO implement system which records the inventory in database
        public void RecordInventory()
        {

        }
    }
}