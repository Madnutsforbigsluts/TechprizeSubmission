using System;
using System.Collections.Generic;
using System.Linq; 

namespace Glyph.Puzzles
{
    public class Puzzle3 : IPuzzle
    {
        List<ushort> attempts = new List<ushort>(3);
        bool[] _oldCircuitInput = new bool[16];

        // Get the list of booleans (I.e of the 16 switches are either on or off) and convert to a decimal number 
        bool[] _circuitInput
        {
            get { return _oldCircuitInput; }
            set
            {
                if (!Enumerable.SequenceEqual(_oldCircuitInput, value))
                {
                    circuitInput = (ushort)value
                                        .Where(v => v == true)
                                        .Select((v, i) => Math.Pow(2, i))
                                        .Sum();
                    _oldCircuitInput = value;
                };

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

        // Reset the puzzle every 5 attempts 
        private int _numAttempts; 
        private int numAttempts
        {
            get { return _numAttempts; }
            set
            {
                if (value > 3)
                {
                    numAttempts = 0;
                    attempts.RemoveRange(0, attempts.Count);
                }
                _numAttempts = value;
            }
        }

        ushort output = 0;

        public ushort circuitInput;

        private byte numOns => GetNumOns();

        // Get the number of bits which are turned on 
        private byte GetNumOns()
        {
            byte numOns = 0; 
            foreach(bool bit in _circuitInput)
            {
                if(bit)
                {
                    numOns++; 
                }
            }
            return numOns; 
        }

        public Puzzle3()
        {
        }

        public void Update(bool[] _circuitInput)
        {
            this._circuitInput = _circuitInput;
        }

        /// <summary>
        /// Solves the puzzles circuit based on a 16 bit input 
        /// </summary>
        /// <param name="input"></param>
        /// <returns>A boolean value (solved or not solved)</returns>
        public void AttemptSolve()
        {
            numAttempts++;

            if(numAttempts % 2 == 0)
            {
                attempts.Add((ushort) (circuitInput << 2));
            }
        }


    }
}
