using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using static Glyph.Utilities.CheckBetween;

namespace Glyph.Entities
{
    public class Entity : IEntity
    {

        public int ID { get; set; }

        public Vector2 Position;

        public Texture2D Avatar { get; set; }

        public string Name { get; set;}
        public string Description { get; set; }

        public List<string> Elements { get; set; }

        public int Turns { get; set; }
        public int Damage { get; set; }
        public int SelectionRadius { get; set; } = 1;
        public bool HasMoved { get; set; }

        public Point BattleLocation; 

        public int Level { get; set; }
        public int Health { get; set; } = 50;
        public byte Speed { get; set; } = 50;
        public byte Power { get; set; } = 50;
        public byte Intellect { get; set; } = 50;
        public byte Agility { get; set; } = 50;
        public byte Accuracy { get; set; } = 50;
        public byte Defense { get; set; } = 50; 

        public bool Dead => IsDead();

        public static bool Frozen; 

        public static Random randGenerator = new Random();

        public virtual void LoadContent(Dictionary<string, Texture2D> textures)
        {
          Avatar = textures[$"Entity{ID}"];
        }

        public bool CanEvade()
        {
            // Decide if the entity can or can't evade 
            const double exponent = 0.832;
            int chanceToEvade = Convert.ToInt16(Math.Pow(Agility, exponent));
            int randPercentile = randGenerator.Next(1, 100);
            if (IsBetween(chanceToEvade, 100, randPercentile))
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public double GetCriticalScalar()
        {
            const double linearMultiplier = 0.0039;
            int randInput = randGenerator.Next(Accuracy / 2, Accuracy);
            return Math.Round(1 + (linearMultiplier * randInput), 1);
        }

        public int GetDamageGiven()
        {
            // Random normal gaussian distribution to calculate damage 
            double u1 = 1.0 - randGenerator.NextDouble();
            double u2 = 1.0 - randGenerator.NextDouble();
            double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) *
            Math.Sin(Math.PI * u2);
            return Convert.ToInt32(GetCriticalScalar() *
            (Damage + (Level + Power / 4) *
            randStdNormal));
        }

        public void TakeDamage(int damage)
        {
            if (!CanEvade() && Health - damage > 0)
            {
                Health -= damage;
            }
            else if (Health - damage <= 0)
            {
                Health = 0;
            }
        }

        public void MoveToPosition(int posX, int posY)
        {
            // Move entity on battlefield 
            BattleLocation.X = posX;
            BattleLocation.Y = posY;
            HasMoved = true;
        }

        public void Heal(int amountToHeal)
        {
            Health += amountToHeal;
        }

        public bool IsDead()
        {
            if (Health <= 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}

