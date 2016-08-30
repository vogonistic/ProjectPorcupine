using System.Collections.Generic;

namespace AssemblyCSharp
{
    public class InventoryDelta
    {
        private InventoryManager inventoryManager;
        private Dictionary<Tile, Inventory> delta = new Dictionary<Tile, Inventory>();

        public InventoryDelta(InventoryManager inventoryManager)
        {
            this.inventoryManager = inventoryManager;
            delta = new Dictionary<Tile, Inventory>();
        }

        public InventoryDelta(InventoryDelta other)
        {
            this.inventoryManager = other.inventoryManager;
            this.delta = CloneDictionary(other.delta);
        }

        public void ChangeInventoryStackSize(Tile tile, Inventory inventory, int amount)
        {
            
        }

        public void ChangeInventoryStackSize(Character character, Inventory inventory, int amount)
        {

        }

        public void ChangeInventoryStackSize(Job job, Inventory inventory, int amount)
        {

        }

        private Dictionary<Tile, Inventory> CloneDictionary(Dictionary<Tile, Inventory> dictionary)
        {
            Dictionary<Tile, Inventory> ret = new Dictionary<Tile, Inventory>();

            foreach (Tile t in dictionary.Keys)
            {
                if (dictionary[t] != null)
                {
                    ret.Add(t, dictionary[t].Clone());
                }
                else
                {
                    ret.Add(t, null);
                }
            }

            return ret;
        }
    }
}

