using static Glyph.Utilities.CheckBetween;

using System;  

namespace Glyph.Combat
{
    public class BattleCell : BattleFieldCell
    {
        public static int InstanceCount = 0;

        public BattleCell()
        {
            InstanceCount++;
            CellID = InstanceCount;

            Random RandGenerator = new Random();

            EventsLeft = 1;

            // I.E 20% chance cell has event 
            ChanceForEvent = 10;

            if (IsBetween(RandGenerator.Next(0, 100), 101, ChanceForEvent))
            {
                EventID = RandGenerator.Next(1, 4);
            }
            else
            {
                EventID = 0; 
            }

            CanHoldEntities = true;
        }

        public override void StartEvent(Action action)
        {
            action.Invoke();
            EventsLeft--; 
        }
    }
}
