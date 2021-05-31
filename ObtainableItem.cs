using static Glyph.Utilities.JsonParser;

namespace Glyph.Inventories
{
    public abstract class ObtainableItem
    {

        public string Name { get; set; }
        public int? ID { get; set; }
        public string Type { get; set; }
        public int Quantity { get; set; }
        public int MaxStackable { get; set; }
        public bool CanStack { get; set; }
        public string Description { get; set; }

        public ObtainableItem() { }

        protected virtual void GetItemFromID()
        {
            dynamic jsonData = FetchJson("jsons/Items.json");
            Type = jsonData["Items"][ID - 1]["Type"];
            Name = jsonData["Items"][ID - 1]["Name"];
            Description = jsonData["Items"][ID - 1]["Description"];
        }
    }
}
