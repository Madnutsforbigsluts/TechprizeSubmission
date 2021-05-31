using System.Collections.Generic;

using Microsoft.Xna.Framework;

namespace Glyph.Map
{
    public static class ActionFromLocation
    {
        static List<string> visited = new List<string> { "Unknown location", "Puzzle Challenge 1"};

        public static void DoFromLocation(string currentLocation) 
        {

            // Player has not visited the location 
            if(!visited.Contains(currentLocation))
            {
                visited.Add(currentLocation);
                Game1.sceneState = SceneState.Cinematic;
                // Determine state based on new location update 
                switch (currentLocation)
                {
                    case "Sacrificial mound":
                        ScreenState.screenState = ScreenUpdateState.TransitionUpdate;
                        Game1.sceneState = SceneState.Cinematic;
                        break;
                    case "Boroughed Forest":
                        ScreenState.screenState = ScreenUpdateState.TransitionUpdate;
                        Game1.player.Position = new Vector2(920, 3490);
                        break;
                }
            }
        }
    }
}
