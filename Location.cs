using System.Collections.Generic;
using System; 
using Microsoft.Xna.Framework;
using static Glyph.Utilities.JsonParser;

namespace Glyph.Map
{
    public class Location
    {
        private readonly dynamic RegionsContainingVertices = FetchJson("Jsons/RegionVertices.json");

        public Location()
        {
        }

        public string GetLocation(Vector2 playerPos)
        {
            foreach (var region in RegionsContainingVertices)
            {
                if (PlayerInRegion(playerPos, region["Vertices"].ToObject<List<List<float>>>()))
                {
                    return region["Name"];
                }
            }
            // Player is not in a defined region. Could create an event or action based on the fact that the player is not in a defined region.
            // I.E make the player Teleport back to spawn based on the returned string. 
            return "Unknown location";
        }

        private static bool PlayerInRegion(Vector2 playerPos, List<List<float>> regionVertices)
        {
            bool result = false;
            int j = regionVertices.Count - 1;
            for (int i = 0; i < regionVertices.Count; i++)
            {
                // Dot product with vertices 
                if (regionVertices[i][1] < playerPos.Y && regionVertices[j][1] >= playerPos.Y ||
                    regionVertices[j][1] < playerPos.Y && regionVertices[i][1] >= playerPos.Y)
                {
                    if (regionVertices[i][0] + (playerPos.Y - regionVertices[i][1]) /
                        (regionVertices[j][1] - regionVertices[i][1]) *
                        (regionVertices[j][0] - regionVertices[i][0]) < playerPos.X)
                    {
                        result = !result;
                    }
                }
                j = i;
            }
            return result;
        }
    }
}
