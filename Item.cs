namespace Glyph.Inventories
{
    public class InvItem : ObtainableItem
    {
        public const int Width = 64;
        public const int Height = 64;

        public InvItem(int? ID = 0, int Quantity = 0)
        {
            this.ID = ID;
            this.Quantity = Quantity;
            GetItemFromID();

            switch(Type)
            {
                case "Weapon":
                    CanStack = false;
                    MaxStackable = 1;
                    break;
                case "Food":
                    CanStack = true;
                    MaxStackable = 32;
                    break;
                case "Key":
                    CanStack = false;
                    MaxStackable = 1;
                    break;
                default:
                    CanStack = true;
                    MaxStackable = 32;
                    break;
            }
        }

        protected override void GetItemFromID()
        {
            base.GetItemFromID(); 
        }

        public InvItem() { }
    }
}
