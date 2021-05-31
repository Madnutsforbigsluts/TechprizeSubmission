using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using static Glyph.Utilities.JsonParser;


namespace Glyph.Entities
{
    public class Enemy : Entity
    {
        private readonly dynamic jsonData = FetchJson("jsons/Enemies.json");

        public Enemy(int ID, int Level)
        {
            // TODO create enemy inventory based on items it may spawn with (move inventory to entity class)
            this.ID = ID;
            this.Level = Level;

            // Entity ID starts at 1 so to access the array subtract 1 from the index. 
            Elements = jsonData["Enemies"][ID - 1]["Elements"].ToObject<List<string>>();
            Name = jsonData["Enemies"][ID - 1]["Name"];
            Description = jsonData["Enemies"][ID - 1]["Description"];
            Health = Level * randGenerator.Next(10, 15);
            Speed = GetStatisticFromLevel(Level);
            Power = GetStatisticFromLevel(Level);
            Intellect = GetStatisticFromLevel(Level);
            Agility = GetStatisticFromLevel(Level);
            Accuracy = GetStatisticFromLevel(Level);
            Defense = GetStatisticFromLevel(Level);
        }

        /// <summary>
        /// Get the statistics from the enemy's level 
        /// </summary>
        /// <param name="Level"></param>
        /// <returns></returns>
        private byte GetStatisticFromLevel(int Level)
        {
            double linearMultiplier = 2.55 * randGenerator.NextDouble();
            if (Level < 100)
            {
                return Convert.ToByte(Math.Round(Level * linearMultiplier, 1));
            }
            return byte.MaxValue;
        }
    }
}
