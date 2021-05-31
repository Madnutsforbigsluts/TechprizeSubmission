using System;
using System.Linq;
using System.Text; 
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;

using Glyph.Utilities;
using static Glyph.Utilities.NumToWords;

namespace Glyph.Puzzles
{
    public class Puzzle1 : IPuzzle
    {
        // Create three substiution squares for the cipher 
        private char[,] square1 = new char[3, 3];
        private char[,] square2 = new char[3, 3];
        private char[,] square3 = new char[3, 3];

        // The three subsiution squares form a cube 
        List<char[,]> cube;

        Dictionary<string, SoundEffect> soundEffects;
        float timeBetweenNotes = 1f;
        private int _playedNoteIndex;
        public int PlayedNoteIndex
        {
            get { return _playedNoteIndex; }
            set
            {
                if (value >= Notes[StepCount].Count)
                {
                    nextStep = true;
                    _playedNoteIndex = 0;
                } else
                {
                    _playedNoteIndex = value;
                }
            }
        }

        private bool finished;

        Color keyColor;

        SpriteFont spriteFont;

        private bool nextStep;
        public int NoteIndex;
        public int StepCount;
        public List<List<int>> Notes;


        string key;
        Random random = new Random();

        Timer noteTimer;

        private string encryptedMessage;

        private string msgToEncrypt => SetMessageToEncrypt(); 
        
        private bool _solved = false;
        public bool Solved
        {
            get { return _solved; }
            set
            {
                _solved = value;
                if(value)
                {
                }
            }
        }

        // The amount between 'splits' to the encrypted string 
        private const int period = 5;

        public int[] puzzle2Combination = new int[5];

        public Puzzle1()
        {
            cube = new List<char[,]> { square1, square2, square3 };
            CreateKey();
            PopulateSquares();

            encryptedMessage = GetEncryptedMessage(msgToEncrypt);
            noteTimer = new Timer(timeBetweenNotes);

        }

        public string SetMessageToEncrypt()
        {
            StringBuilder stringBuilder = new StringBuilder();

            for (int i = 0; i < 5; i++)
            {
                puzzle2Combination[i] = random.Next(11);
                stringBuilder.Append(GetNumInWords(puzzle2Combination[i]));
            }

            return $"The code to the lock is {stringBuilder}";
        }

        /// <summary>
        /// Shuffles a given cipher alphabet into a random order to create the key
        /// </summary>
        private void CreateKey()
        {
            StringBuilder stringBuilder = new StringBuilder();

            List<char> alphabet = GlobalConstants.TrifidKeyAlphabet.ToList();

            for (int i = 0; i < 27; i++)
            {
                int randomIndex = random.Next(alphabet.Count);
                stringBuilder.Append(alphabet[randomIndex]);
                alphabet.RemoveAt(randomIndex);
            }
            key = stringBuilder.ToString();
        }

        /// <summary>
        /// Populates a character from the key in each cell of the cube 
        /// </summary>
        private void PopulateSquares()
        {
            int loopCount = 0; 
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    for (int k = 0; k < 3; k++)
                    {
                        cube[i][j, k] = key[loopCount];
                        loopCount++;
                    }
                }
            }
        }

        /// <summary>
        /// Encrypts the given message using the cipher cube 
        /// </summary>
        /// <param name="message"></param>
        /// <returns> A string representing the encrypted value</returns>
        private string GetEncryptedMessage(string message)
        {
            StringBuilder result = new StringBuilder();

            // short for 'substitutedMessage' this string represents
            // the encrypted message before it is subed back into the cube 
            StringBuilder subMsg = new StringBuilder();

            // One string for each row of the table 
            StringBuilder string1 = new StringBuilder();
            StringBuilder string2 = new StringBuilder();
            StringBuilder string3 = new StringBuilder();

            int charCount = 0;

            foreach (char character in message)
            {
                if (!char.IsWhiteSpace(character))
                {
                    if (charCount % period == 0
                        && charCount != 0)
                    {
                        // Every period dumo all the subtiutions from each row
                        // into one concatenated substiution string 
                        subMsg.Append(string1);
                        subMsg.Append(string2);
                        subMsg.Append(string3);
                        subMsg.Append(" ");
                        string1 = new StringBuilder();
                        string2 = new StringBuilder();
                        string3 = new StringBuilder(); 
                    }
                    charCount++;
                }

                // Loop through every character in the cube for each character
                // in the message 
                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        for (int k = 0; k < 3; k++)
                        {
                            // The first row is substituted with the square
                            // the char was found in, the second row with the
                            // row it was found in and the third with the
                            // column it was found in
                            // Keep in mind these values correspond to a randomized key
                            if (char.ToUpper(character) == cube[i][j, k])
                            {
                                string1.Append(i);
                                string2.Append(j);
                                string3.Append(k);
                            }
                        }
                    }
                }
            }

            // dump the remaining characters not picked up by the period 
            subMsg.Append(string1);
            subMsg.Append(string2);
            subMsg.Append(string3);

            // Now for each char in the subbed message lookup the
            // values in the cube and sub those in 
            for (int i = 0; i < subMsg.Length; i++)
            {
                if (char.IsWhiteSpace(subMsg[i]))
                {
                    result.Append(" ");
                    subMsg.Remove(i, 1);
                }

                if (i % 3 == 0)
                {
                    int squareNum = int.Parse(subMsg[i].ToString());
                    int colNum = int.Parse(subMsg[i + 2].ToString());
                    int rowNum = int.Parse(subMsg[i + 1].ToString());

                    result.Append(cube[squareNum][rowNum, colNum]);
                }
            }
            return result.ToString(); 
        }

        public void LoadContent(Dictionary<string, SoundEffect> soundEffects,
            SpriteFont spriteFont)
        {
            this.spriteFont = spriteFont;

            this.soundEffects = soundEffects;
            Notes = new List<List<int>>
            {
               new List<int>
               {
                   1, 2, 1
               },
               new List<int>
               {
                   1, 3, 3, 3, 1
               },
               new List<int>
               {
                   1, 3, 3, 2, 2, 1
               },
               new List<int>
               {
                   2, 3, 2, 3, 2, 3, 2
               },
                new List<int>
               {
                    1, 2, 3, 2, 1, 1, 2, 6
               },
            };
        }

        public void Update(GameTime gameTime)
        {
            noteTimer.Update((float)gameTime.ElapsedGameTime.TotalSeconds);

            if(!finished)
            {
                if (StepCount >= Notes.Count - 1 &&
                    PlayedNoteIndex >= Notes[StepCount].Count - 1)
                {
                    keyColor = Color.Red;
                    soundEffects["Note6"].Play();
                    finished = true;
                }

                if (NoteIndex <= Notes[StepCount].Count - 1 && noteTimer.timeExpired)
                {
                    noteTimer = new Timer(timeBetweenNotes);
                    soundEffects[$"Note{Notes[StepCount][NoteIndex]}"].Play();
                    NoteIndex++;
                }
                else if (NoteIndex >= Notes[StepCount].Count &&
                    noteTimer.timeExpired && StepCount < Notes.Count - 1)
                {
                    if (nextStep)
                    {
                        NoteIndex = 0;
                        StepCount++;
                        timeBetweenNotes *= 0.9f;
                        nextStep = false;
                    }
                }
            }
        }

        public void AttemptSolve()
        {

        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawString(spriteFont, key, new Vector2(2100, 3713),
                keyColor, 0f, Vector2.Zero, 0.5f, SpriteEffects.None, 0);
            spriteBatch.DrawString(spriteFont, encryptedMessage, new Vector2(2000, 3750),
                Color.SkyBlue, 0f, Vector2.Zero, 0.5f, SpriteEffects.None, 0);
        }

    }
}
