using System.Collections.Generic;
using System;

using static Glyph.Utilities.ArraySwap;
using Glyph.GUI; 

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework; 

namespace Glyph.Puzzles
{
    public class Puzzle2 : IPuzzle
    {
        SpriteFont font; 
        LockGUI lockGUI;

        private const int ringSize = 10;

        // One list rotates while the other stays fixed
        int[] fixedOuterRing = new int[ringSize];
        int[] dynamicInnerRing = new int[ringSize];

        private int _rotationOffset; 
        private int rotationOffset
        {
            get { return _rotationOffset;  }
            set
            {
                _rotationOffset = value >= 10 ? 0 : value; 
            }
        }

        private int[] combination;

        private int combinationLength => combination.Length; 

        private int _numCorrect; 
        private int numCorrect
        {
            get { return _numCorrect; }
            set
            {
                _numCorrect = value >= combinationLength ? 0 : value;
                Solved = value == combinationLength ? true : false; 
            }
        }

        private bool _solved = false;
        public bool Solved
        {
            get { return _solved; }
            set
            {
                _solved = value; 
                if (value)
                {
                }
            }
        }

        // Amount each point around circle is equidistant by 
        const int dist = 100;

        public Puzzle2(int[] combination)
        {
            this.combination = combination; 
            PopulateRings();
        }

        public void AttemptSolve()
        {
            if (lockGUI.buttonState == LockButtonState.Rotate)
            {
                rotationOffset++; 
                RotateInnerRing();
            }
            
            // Determine if the 'pointer' matches 
            if (fixedOuterRing[rotationOffset] == combination[numCorrect] &&
                lockGUI.buttonState == LockButtonState.Submit)
            {
                numCorrect++;
            }
            else if (fixedOuterRing[rotationOffset] != combination[numCorrect] &&
                lockGUI.buttonState == LockButtonState.Submit)
            {
                numCorrect = 0; 
            }

            lockGUI.buttonState = LockButtonState.None;
        }
        
        private void PopulateRings()
        {
            for (int i = 0; i < ringSize; i++)
            {
                fixedOuterRing[i] = i + 1;
                dynamicInnerRing[i] = i + 1; 
            }
        }

        /// <summary>
        /// Shifts font member of the list to the back (I.E an infinite queue) 
        /// </summary>
        private void RotateInnerRing()
        {
            for (int i = ringSize - 1; i > 0; i--)
            {
                Swap(dynamicInnerRing, i - 1, i);
            }
        }

        public void LoadContent(SpriteFont font)
        {
            this.font = font;
            lockGUI = new LockGUI();
            lockGUI.LoadContent(font); 
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            for (int i = 0; i < ringSize; i++)
            {
                // Ensure all the points are equidistant along a circle 
                int x = (int) (dist *  Math.Cos(2 * Math.PI * i / ringSize));
                int y = (int) (dist *  Math.Sin(2 * Math.PI * i / ringSize));

                // Draw the outer ring which doesn't rotate 
                spriteBatch.DrawString(font, fixedOuterRing[i].ToString(),
                    new Vector2(3 * x + 500, 3 * y + 500),
                    i == rotationOffset ? Color.Red : Color.White,
                    0, Vector2.Zero, 1f, SpriteEffects.None, 0);

                // Draw the inner ring which rotates 
                spriteBatch.DrawString(font, dynamicInnerRing[i].ToString(),
                    new Vector2(x + 500, y + 500), i == rotationOffset ? Color.Red : Color.Gray,
                    0, Vector2.Zero, 1f, SpriteEffects.None, 0);
            }
        }

    }
}
