using Glyph.Inventories;

namespace Glyph
{
    public abstract class Itemfactory
    {
        public abstract BagItem CreateBagItem(int ID);
        public abstract InvItem CreateInvItem(int ID, string ItemType);
    }
}
