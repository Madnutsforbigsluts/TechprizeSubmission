using System.Collections.Generic;
using Microsoft.Xna.Framework; 
using Microsoft.Xna.Framework.Graphics;

namespace Glyph.Entities
{
    public interface IEntity
    {
        int ID { get; set; }
        Texture2D Avatar { get; set; }
        string Name { get; set; }
        string Description { get; set; }
        int Level { get; set; }
        int Health { get; set; }
        byte Speed { get; set; }
        byte Power { get; set; }
        byte Intellect { get; set; }
        byte Agility { get; set; }
        byte Accuracy { get; set; }
        byte Defense { get; set; }
        List<string> Elements { get; set; }
        
        bool CanEvade();
        bool IsDead();
        double GetCriticalScalar();
        int GetDamageGiven();
        void TakeDamage(int damageToTake);
        void MoveToPosition(int posX, int posY);
        void Heal(int amountToHeal);
    }
}




