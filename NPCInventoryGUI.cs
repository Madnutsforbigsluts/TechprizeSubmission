using Myra.Graphics2D.UI;
using Myra.Graphics2D.Brushes;
using Glyph.Entities;
using Glyph.Inventories;
using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

namespace Glyph.GUI
{
    public class NPCInventoryGUI : NPC, IGUI
    {

        Panel panel; 
        Tree tree;
        SpinButton spinButton;
        private bool typing;
        private BagItem viewedItem;

        public NPCInventoryGUI(int _, Vector2 __, SpriteFont spriteFont) : base(_, __) 
        {
            Desktop.Widgets.Clear();
            panel = new Panel();
            Desktop.Widgets.Add(panel);

            tree = new Tree
            {
                Width = 448,
                Left = 448,
                TextColor = Color.Black,
                Text = "Buy",
                Font = spriteFont,
                Background = new SolidBrush(Color.White)
            };

            spinButton = new SpinButton
            {
                Width = 50,
                Height = 50,
                Top = 300,
                Left = tree.Left,
                Visible = false
            };

            spinButton.TouchDown += (s, a) =>
            {
                typing = true;
            };

            spinButton.ValueChanged += (s, a) =>
            {
                if (spinButton.Value > viewedItem.Quantity)
                {
                    spinButton.Value = viewedItem.Quantity; 
                }
                else if (spinButton.Value < 0) 
                {
                    spinButton.Value = 0;
                }
            };

            Desktop.KeyDown += (s, a) =>
            {
                if (a.Data == Keys.Enter && typing && spinButton.Value > 0)
                {

                    // Get the amount bought and subtract it from the quantity left 

                    int quantityBought = (int) spinButton.Value;

                    inventory.Add(new InvItem(viewedItem.ID, quantityBought));

                    viewedItem.Quantity -= quantityBought;
                    spinButton.Value = 0;
                }
            };

            tree.TouchEntered += (s, a) =>
            {
                spinButton.Visible = false;
                typing = false;
                viewedItem = null;
            };


            // Add sub nodes to the trading tree

            TreeNode weaponNode = tree.AddSubNode("Weapons:");
            weaponNode.TextColor = Color.Black;
            weaponNode.Font = spriteFont;
            TreeNode foodNode = tree.AddSubNode("Foodstuffs:");
            foodNode.TextColor = Color.Black;
            foodNode.Font = spriteFont;
            TreeNode potionNode = tree.AddSubNode("Other:");
            potionNode.TextColor = Color.Black;
            potionNode.Font = spriteFont;

            void closeNodes()
            {
                foreach (TreeNode node in tree.AllNodes)
                {
                    node.IsExpanded = false;
                }
            }

            weaponNode.TouchEntered += (s, a) =>
            {
                closeNodes();
                weaponNode.IsExpanded = true; 
            };

            foodNode.TouchEntered += (s, a) =>
            {
                closeNodes();
                foodNode.IsExpanded = true;
            };

            potionNode.TouchEntered += (s, a) =>
            {
                closeNodes();
                potionNode.IsExpanded = true;
            };

            // Loop over the bag and assign each sub node a place in the tree based on the item's type. 

            foreach (BagItem bagItem in bag)
            {
                TreeNode itemNode;

                switch (bagItem.Type)
                {
                    case "Weapon":
                        itemNode = weaponNode.AddSubNode($"{bagItem.Name}        ${bagItem.Price}");
                        break;
                    case "Food":
                        itemNode = foodNode.AddSubNode($"{bagItem.Name}          ${bagItem.Price}");
                        break; 
                    case "Key":
                        itemNode = potionNode.AddSubNode($"{bagItem.Name}        ${bagItem.Price}");
                        break;
                    default:
                        itemNode = tree;
                        break;
                }
                itemNode.TextColor = Color.Black;
                itemNode.Font = spriteFont; 

                itemNode.TouchEntered += (s, a) =>
                {
                    spinButton.Visible = true;
                    viewedItem = bagItem;
                };
            }
            Construct();
        }

        public void Reset()
        {
            panel.Widgets.Clear(); 
        }

        public void Construct()
        {
            panel.Widgets.Add(tree);
            panel.Widgets.Add(spinButton);
        }
    }
}
