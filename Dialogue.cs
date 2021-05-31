using System.Text;
using System; 
using System.Collections.Generic; 

using Glyph.Utilities; 

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Glyph.UI 
{
    /// <summary>
    ///  Formats a string to fit a box and draws the string in the dialogue box.
    /// </summary>
    public class Dialogue
    {
        Texture2D dialogueBox;
        SpriteFont spriteFont;
        List<string> formattedStrings = new List<string>();
        string[] words;
        private float spaceWidth;

        public static bool Talking;
        public static bool NextPane;

        int _colorCount; 
        int colorCount
        {
            get { return _colorCount; }
            set
            {
                _colorCount = value >= roygbiv.Count ? 0 : value; 
            }
        }

        List<Color> roygbiv = new List<Color>
        {
            Color.Red,
            Color.Orange,
            Color.Yellow,
            Color.Green,
            Color.Blue,
            Color.Indigo,
            Color.Violet
        };
        Color currentColor => roygbiv[colorCount];


        Timer timer = new Timer(0.03f);
        int charCount = 0;
        public static int paneCount = 0;
        public static int paneLength;
        StringBuilder appearingString = new StringBuilder();

        public Dialogue(List<string> dialogue, SpriteFont spriteFont, Texture2D dialogueBox)
        {
            this.spriteFont = spriteFont;
            this.dialogueBox = dialogueBox;
            spaceWidth = spriteFont.MeasureString(" ").X;

            for (int i = 0; i < dialogue.Count; i++)
            {
                formattedStrings.Add(GetFormattedString(dialogue[i]));
            }
            paneCount = 0;
            Talking = true;
            paneLength = formattedStrings.Count;
        }

        private string GetFormattedString(string stringToFormat)
        {
            words = stringToFormat.Split(' ');
            StringBuilder result = new StringBuilder();
            float lineWidth = 0f;

            foreach(string word in words)
            {
                Vector2 sizeOfWord = spriteFont.MeasureString(word);
                if(sizeOfWord.X + lineWidth < GlobalConstants.DialogueBoxWidth - 150)
                {
                    result.Append(word + " ");
                    lineWidth += sizeOfWord.X + spaceWidth;
                }
                else
                {
                    result.Append("\n" + word + " ");
                    lineWidth = sizeOfWord.X + spaceWidth;
                }
            }
            return result.ToString();
        }

        public void Update(GameTime gameTime)
        {

            if (paneCount < formattedStrings.Count)
            {
                timer.Update((float)gameTime.ElapsedGameTime.TotalSeconds);

                // Get the next dialogue pane 
                if(NextPane)
                {
                    paneCount++;
                    charCount = 0;
                    NextPane = false;
                    appearingString = paneCount < formattedStrings.Count ?
                        new StringBuilder() :
                        new StringBuilder(formattedStrings[paneCount - 1]);
                }
                // Gives the effect of the 'typed' out text 
                else if (timer.timeExpired && appearingString.Length <
                    formattedStrings[paneCount].Length)
                {
                    appearingString.Append(formattedStrings[paneCount][charCount]);
                    charCount++;
                    colorCount++; 
                    timer = new Timer(0.03f);
                }
            }
            else
            {
                Talking = false; 
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(dialogueBox, new Rectangle(GlobalConstants.DialoguePositionX,
                GlobalConstants.DialoguePositionY, GlobalConstants.DialogueBoxWidth,
                GlobalConstants.DialogueBoxHeight), Color.White);

                spriteBatch.DrawString(spriteFont, appearingString.ToString(),
                    new Vector2( GlobalConstants.DialoguePositionX + 80,
                    GlobalConstants.DialoguePositionY + 150),
                    Color.Black, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0);
        }
    }
}