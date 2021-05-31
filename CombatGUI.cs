using Myra.Graphics2D.UI;
using Myra.Graphics2D.Brushes;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Glyph.GUI
{

    public enum CombatButtonState
    {
        None, 
        Attack,
        Defend,
        Idle,
        Flee,
        Shuffle
    }

    public class CombatGUI : IGUI 
    {
        Panel panel;
        public VerticalProgressBar playerHealthBar;
        public VerticalProgressBar enemyHealthBar; 
        TextButton attackButton;
        TextButton defendButton;
        TextButton idleButton;
        TextButton fleeButton;
        TextButton shuffleCards;

        private const int buttonWidth = 100;
        private const int buttonHeight = 100;

        public CombatButtonState buttonState { get; set; }

        public CombatGUI()
        {
            Desktop.Widgets.Clear();
            panel = new Panel();
            Desktop.Widgets.Add(panel);
        }

        public void LoadContent(SpriteFont font)
        {
            attackButton = new TextButton
            {
                Background = new SolidBrush(Color.Transparent),
                OverBackground = new SolidBrush(Color.Gray),
                Font = font,
                Top = 130,
                Left = 250,
                Width = buttonWidth,
                Height = buttonHeight,
                TextColor = Color.White,
                Text = "Attack"
            };


            defendButton = new TextButton
            {
                Background = new SolidBrush(Color.Transparent),
                OverBackground = new SolidBrush(Color.Gray),
                Font = font,
                Top = 130,
                Left = 350,
                Width = buttonWidth,
                Height = buttonHeight,
                TextColor = Color.White,
                Text = "Defend"
            };

            idleButton = new TextButton
            {
                Background = new SolidBrush(Color.Transparent),
                OverBackground = new SolidBrush(Color.Gray),
                Font = font,
                Top = 130,
                Left = 450,
                Width = buttonWidth,
                Height = buttonHeight,
                TextColor = Color.White,
                Text = "Skip turn"
            };

            fleeButton = new TextButton
            {
                Background = new SolidBrush(Color.Transparent),
                OverBackground = new SolidBrush(Color.Gray),
                Font = font,
                Top = 130,
                Left = 550,
                Width = buttonWidth,
                Height = buttonHeight,
                TextColor = Color.White,
                Text = "Flee"
            };

            shuffleCards = new TextButton
            {
                Background = new SolidBrush(Color.Transparent),
                OverBackground = new SolidBrush(Color.Gray),
                Font = font,
                Top = 130,
                Left = 650,
                Width = buttonWidth,
                Height = buttonHeight,
                TextColor = Color.White,
                Text = "Shuffle Cards"
            };
            playerHealthBar = new VerticalProgressBar
            {
                Filler = new SolidBrush(Color.Green),
                Top = 250,
                Width = 25,
                Height = 350
            };

            enemyHealthBar = new VerticalProgressBar
            {
                Filler = new SolidBrush(Color.Red),
                Top = 250,
                Width = 25,
                Left = 975,
                Height = 350
            };

            attackButton.TouchDown += (s, a) =>
            {
                buttonState = CombatButtonState.Attack;
            };
            defendButton.TouchDown += (s, a) =>
            {
                buttonState = CombatButtonState.Defend;
            };
            idleButton.TouchDown += (s, a) =>
            {
                buttonState = CombatButtonState.Idle;
            };
            fleeButton.TouchDown += (s, a) =>
            {
                buttonState = CombatButtonState.Flee;
            };
            shuffleCards.TouchDown += (s, a) =>
            {
                buttonState = CombatButtonState.Shuffle;
            };
            Construct();
        }

        public void Reset()
        {
            panel.Widgets.Clear(); 
        }

        public void Construct()
        {
            panel.Widgets.Add(attackButton);
            panel.Widgets.Add(defendButton);
            panel.Widgets.Add(idleButton);
            panel.Widgets.Add(fleeButton);
            panel.Widgets.Add(shuffleCards);
            panel.Widgets.Add(enemyHealthBar);
            panel.Widgets.Add(playerHealthBar);
        }
    }
}
