using System.Collections.Generic;
using System.Linq;
using System; 

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Glyph.UI;
using Glyph.Entities;
using static Glyph.Utilities.JsonParser;

namespace Glyph.Cinematics
{
    public class Cinematic
    {
        private readonly dynamic jsonData = FetchJson("jsons/Scenes.json");
        List<Dictionary<string, string>> targetedNPCInstructions;
        public NPC targetedNPC;
        private int instructionCount;
        private float duration;
        List<Dictionary<string, string>> NPCInstructions;
        public int sceneNum;
        SpriteFont spriteFont;
        Texture2D texture2D;
        List<List<string>> NPCDialogue; 

        public Cinematic(int sceneNum, SpriteFont spriteFont, Texture2D texture2D)
        {
            this.spriteFont = spriteFont;
            this.texture2D = texture2D; 
            GetScene(sceneNum); 
        }

        private void GetScene(int sceneNum)
        {
            SetScene(sceneNum);
            SetTargetNPC();
        }

        private void SetScene(int sceneNum)
        {
            NPCInstructions = jsonData[sceneNum]["NPCInstructions"]
                .ToObject<List<Dictionary<string, string>>>();
            NPCDialogue = jsonData[sceneNum]["NPCDialogue"]
                .ToObject<List<List<string>>>();
            Game1.dialogue = new Dialogue(NPCDialogue[instructionCount],
                spriteFont, texture2D);
        }

        private void SetTargetNPC()
        {
            int npcTargetID = 0; 
            if(instructionCount < NPCInstructions.Count)
            {
                // Get NPC ID from Json instructions so we know who to move 
                npcTargetID = int.Parse(NPCInstructions[instructionCount]["ID"]);
            }

            // 
            targetedNPCInstructions = NPCInstructions
                .Select(n => n)
                .Where(n => int.Parse(n["ID"]) == npcTargetID)
                .ToList();


            // locate the NPC in the scene from their ID
            targetedNPC = CreateEntityFactory.NPCS
                .Select(n => n)
                .Where(n => n.ID == npcTargetID)
                .ToList()[0];

            targetedNPC.FinishedWalking = false;
            targetedNPC.instructions = targetedNPCInstructions;
        }

        public void Update(GameTime gameTime)
        {
            targetedNPC.Controller(gameTime);

            if (targetedNPC.FinishedWalking &&
                instructionCount < NPCInstructions.Count)
            {

                // The new line will have a potentially different ID
                SetTargetNPC();

                Game1.dialogue = null;
                Game1.dialogue = new Dialogue(NPCDialogue[instructionCount],
                spriteFont, texture2D);

                // Move to the next line of instruction in the json 
                instructionCount++;

            }

            // The NPC has finished walking 
            else if (targetedNPC.FinishedWalking && 
                instructionCount >= NPCInstructions.Count)
            {
                // Move out of the cinematic scene 
                Game1.uiState = UIState.None;
                Game1.sceneState = SceneState.ShowOverWorld;

                // Move on to the next scene 
                Game1.CinematicPointer++;

                if(Game1.CinematicPointer == 3)
                {
                    Game1.puzzleState = PuzzleState.Puzzle1;
                    Game1.player.Position = new Vector2(2225, 3713);
                }
            }
        }
    }
}
