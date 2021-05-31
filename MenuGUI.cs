using System;

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework; 

using Myra.Graphics2D.UI;
using Myra.Graphics2D.Brushes;

public enum MenuButtonState
{
    saveNew
}

namespace Glyph.GUI 
{
    public class MenuGUI : IGUI 
    {
        Panel panel;
        ListBox menu;
        ListItem saveNew;
        public MenuButtonState menuButtonState;

        public MenuGUI()
        {
            Desktop.Widgets.Clear();
            panel = new Panel();
            Desktop.Widgets.Add(panel);
        }

        public void LoadContent(SpriteFont spriteFont)
        {
            menu = new ListBox
            {
                Top = 500,
                Left = 400,
                Width = 200,
                Height = 100,
                Background = new SolidBrush(Color.Black)
            };

            saveNew = new ListItem
            {
                Text = "Start new save file",
            };

            menu.TouchDown += (s, a) =>
            {
                if(menu.SelectedItem == saveNew)
                {
                    Game1.sceneState = SceneState.Cinematic;
                    //Game1.sceneState = SceneState.ShowOverWorld;
                }
            };

            menu.Items.Add(saveNew);
            Construct(); 
        }

        public void Reset() { }

        public void Construct()
        {
            panel.Widgets.Add(menu);
        }
    }
}
