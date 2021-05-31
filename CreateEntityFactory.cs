using System; 
using System.Collections.Generic;

using static Glyph.Utilities.CheckBetween;
using static Glyph.Utilities.JsonParser;

using Microsoft.Xna.Framework;

namespace Glyph.Entities
{
    public class CreateEntityFactory : EntityFactory
    {
        private static readonly dynamic EnemySpawnData = FetchJson("Jsons/EnemySpawnTable.json");
        private static readonly dynamic NPCSpawnData = FetchJson("Jsons/NPCSpawnTable.json");
        private string currentLocation;
        Random randGenerator = new Random();

        public static List<NPC> NPCS = new List<NPC>();
        public static Enemy EncounteredEnemy;

        public CreateEntityFactory(string currentLocation, string type)
        {
            this.currentLocation = currentLocation; 

            // Game should keep running after the player goes into an unknown location. 
            try
            {

                if(type == "Enemy")
                {
                    foreach (var enemy in EnemySpawnData[currentLocation])
                    {
                        int ChanceToSpawn = randGenerator.Next(0, 10001);

                        if (IsBetween(0, Convert.ToInt32(enemy["ChanceToEncounter"]),
                            ChanceToSpawn))
                        {
                            CreateEnemy(enemy);
                        }
                    }
                }
                else if(type == "NPC")
                {
                    foreach (var npc in NPCSpawnData[currentLocation])
                    {
                        CreateNPC(npc);
                    }
                }
            }
            catch (ArgumentNullException e)
            {
                Console.WriteLine("Exception caught: {0} \n " +
                    "Region undefined or not found in RegionVertices.Json", e.Message);
            }
            catch (NullReferenceException e)
            {
                Console.WriteLine("Exception caught: {0} \n " +
                    "Region found in RegionVertices.Json but not yet implemented in " +
                    "LocationSpawnTable.json", e.Message);
            }
        }

        public override void CreateEnemy(dynamic enemy)
        {
            EncounteredEnemy = new Enemy(Convert.ToInt32(enemy["ID"]), randGenerator.Next(
                Convert.ToInt32(enemy["MinLevel"]), Convert.ToInt32(enemy["MaxLevel"])));
        }

        public override void CreateNPC(dynamic npc)
        {
            var positionData = npc["Position"].ToObject<List<float>>();
            NPC _npc = new NPC(Convert.ToInt32(npc["ID"]),
                new Vector2(positionData[0], positionData[1]));
            NPCS.Add(_npc);
        }

    }
}
