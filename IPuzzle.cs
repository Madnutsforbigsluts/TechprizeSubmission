using System;
namespace Glyph.Puzzles
{
    interface IPuzzle
    {
        bool Solved { get; set; }
        void AttemptSolve();
    }
}
