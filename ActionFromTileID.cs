using System.Collections.Generic;

namespace Glyph.Utilities
{
    public static class ActionFromTileID
    {
        static Dictionary<string, List<int>> tileCollisionSets =
        new Dictionary<string, List<int>>
        {
            { "Triangle", new List<int>{ 1510, 1511, 1570, 1571 } },
            { "Circle", new List<int>{ 1508, 1509, 1568, 1569 } },
            { "MiddleCircle", new List<int>{ 1506, 1507, 1566, 1567 } },
            { "InvertedTriangle", new List<int>{ 1511, 1512, 1571, 1572 } },
            { "Lock", new List<int>{ 2414, 2415, 2416 } }
        };

        public static string GetInstructionFromCollision(int tileID)
        {
            if (tileCollisionSets["Triangle"].Contains(tileID))
            {
                // Play the first note
                return "Note1";
            }
            else if(tileCollisionSets["Circle"].Contains(tileID))
            {
                // Play the second note
                return "Note2";
            }
            else if(tileCollisionSets["InvertedTriangle"].Contains(tileID))
            {
                // Play the third note
                return "Note3";
            }
            else if (tileCollisionSets["MiddleCircle"].Contains(tileID))
            {
                // Reset the notes
                return "Reset";
            }
            else if (tileCollisionSets["Lock"].Contains(tileID))
            {
                // Enter the lock of puzzle 2
                return "Lock";
            }
            return string.Empty; 
        }
    }
}
