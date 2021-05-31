using Myra.Graphics2D.UI;
using Myra.Graphics2D.Brushes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Glyph.GUI
{
    public enum LockButtonState
    {
        None, 
        Submit,
        Rotate 
    }

    public class LockGUI : IGUI
    {
        Panel panel;
        TextButton submit;
        TextButton rotate;
        public LockButtonState buttonState { get; set; } 


        public LockGUI()
        {
            Desktop.Widgets.Clear();
            panel = new Panel();
            Desktop.Widgets.Add(panel);
        }

        public void LoadContent(SpriteFont font)
        {
            submit = new TextButton
            {
                Background = new SolidBrush(Color.White),
                OverBackground = new SolidBrush(Color.Gray),
                Font = font,
                Top = 100,
                Left = 100,
                Width = 100,
                Height = 100,
                TextColor = Color.Black,
                Text = "Submit"
            };

            rotate = new TextButton
            {
                Background = new SolidBrush(Color.White),
                OverBackground = new SolidBrush(Color.Gray),
                Font = font,
                Top = 100,
                Left = 200,
                Width = 100,
                Height = 100,
                TextColor = Color.Black,
                Text = "Rotate"
            };
            submit.TouchDown += (s, a) =>
            {
                buttonState = LockButtonState.Submit;
            };

            rotate.TouchDown += (s, a) =>
            {
                buttonState = LockButtonState.Rotate;
            };
            Construct();
        }

        public void Reset()
        {
            panel.Widgets.Clear();
        }

        public void Construct()
        {
            panel.Widgets.Add(submit);
            panel.Widgets.Add(rotate);
        }
    }
}
