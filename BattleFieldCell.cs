using System; 

namespace Glyph.Combat
{
    public abstract class BattleFieldCell
    {
        public int EventID { get; set; }
        public int CellID { get; set; }
        public int ChanceForEvent { get; set; }
        public int EventsLeft { get; set; }
        public bool CanHoldEntities { get; set; }
        public virtual void StartEvent(Action action) { }
    }
}
