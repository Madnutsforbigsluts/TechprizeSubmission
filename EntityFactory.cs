namespace Glyph.Entities
{
    public abstract class EntityFactory 
    {
        public abstract void CreateEnemy(dynamic entity);
        public abstract void CreateNPC(dynamic entity);
    }
}
