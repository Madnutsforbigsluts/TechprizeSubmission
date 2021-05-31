namespace Glyph.Inventories
{
    public class BagItem : ObtainableItem
    {
        public float Price { get; set; }

        public BagItem(int ID, int Quantity)
        {
            this.ID = ID;
            this.Quantity = Quantity;
            GetItemFromID(); 
        }

        protected override void GetItemFromID()
        {
            base.GetItemFromID();
        }
    }
}