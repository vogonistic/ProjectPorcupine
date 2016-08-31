#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion

using System.Collections.Generic;
using UnityEngine.Assertions;

namespace ProjectPorcupine.Jobs
{
    public class InventoryManagerOverride
    {
        private InventoryManager inventoryManager;

        private string characterHoldsType;
        private int characterHoldsAmount;

        private Dictionary<Tile, int> overrides;

        public InventoryManagerOverride(InventoryManager inventoryManager)
        {
            this.inventoryManager = inventoryManager;
            this.overrides = new Dictionary<Tile, int>();
        }

        protected InventoryManagerOverride(InventoryManagerOverride other)
        {
            characterHoldsType = other.characterHoldsType;
            characterHoldsAmount = other.characterHoldsAmount;
            inventoryManager = other.inventoryManager;
            overrides = new Dictionary<Tile, int>(other.overrides);
        }

        public int Count
        {
            get
            {
                return overrides.Count;
            }
        }

        public InventoryManagerOverride Clone()
        {
            return new InventoryManagerOverride(this);
        }
            
        public void AddAction(Action action)
        {
            Assert.IsTrue(action.Provides.Count <= 1 && action.Requires.Count <= 1);

            if (action.Tile.Inventory != null)
            {
                string objectType = action.Provides.FirstKey();
                PickUp(action.Tile, action.Provides.Value(objectType));
            }
            else
            {
                string objectType = action.Requires.FirstKey();
                PutDown(action.Tile, action.Requires.Value(objectType));
            }
        }

        public void PickUp(Tile tile, int amount)
        {
            // If the character isn't holding anything, set the type
            if (characterHoldsType == null)
            {
                characterHoldsType = tile.Inventory.objectType;
            }

            // Must never be different
            Assert.IsTrue(characterHoldsType == tile.Inventory.objectType);

            // Add to inventory
            characterHoldsAmount += amount;

            // Remove from tile
            if (overrides.ContainsKey(tile))
            {
                overrides[tile] -= amount;
            }
            else
            {
                overrides[tile] = tile.Inventory.StackSize - amount;
            }
        }

        public void PutDown(Tile tile, int amount)
        {
            Assert.IsTrue(tile.Inventory == null || tile.Inventory.objectType == characterHoldsType);
            Assert.IsTrue(characterHoldsAmount >= amount);

            // Figure out how much is in the tile right now
            int tileStackSize = 0;
            if (overrides.ContainsKey(tile))
            {
                tileStackSize = overrides[tile]; 
            }
            else if (tile.Inventory != null)
            {
                tileStackSize = tile.Inventory.StackSize;
            }

            // Add the new count to the tile
            overrides[tile] = tileStackSize + amount;

            // Remove from inventory
            characterHoldsAmount -= amount;

            // If we don't hold anything, remove the type
            if (characterHoldsAmount == 0)
            {
                characterHoldsType = null;
            }
        }

        public Path_AStar FindNeed(Tile startTile, string needType, bool canTakeFromStockpile = true)
        {
            // Don't start a search if there is nothing to find. Those searches are very, very slow
            if (inventoryManager.InventoriesOfTypeIsAccessibleSomewhere(needType, canTakeFromStockpile) == false)
            {
                return null;
            }

            // return new Path_AStar(World.Current, startTile, needType, needAmount, true);
            Path_AStar.GoalReachedEvaluator hasReachedGoal = pathNode =>
            {
                // It's in the overrides, so we are skipping other checks
                if (overrides.ContainsKey(pathNode.data))
                {
                    // If it has items, we've found it, otherwise it's not the tile we are looking for
                    return overrides[pathNode.data] > 0;
                }

                // If there is no inventory, we can't be in the right place.
                if (pathNode.data.Inventory == null)
                {
                    return false;
                }

                // Is the inventory of the right type and not locked?
                if (pathNode.data.Inventory.objectType == needType && pathNode.data.Inventory.locked == false)
                {
                    // Type is correct and we are allowed to pick it up
                    if (canTakeFromStockpile || pathNode.data.Furniture == null || pathNode.data.Furniture.IsStockpile() == false)
                    {
                        // Stockpile status is fine
                        return true;
                    }
                }

                return false;
            };
            Path_AStar.CostEstimator costHeuristic = currentPathNode => 0f;

            return new Path_AStar(World.Current, startTile, hasReachedGoal, costHeuristic);
        }

        public override string ToString()
        {
            string ret = "[Overrides\n";
            foreach (Tile tile in overrides.Keys)
            {
                ret += "  " + tile + ": " + overrides[tile] + ",\n";
            }

            return ret + "]";
        }
    }
}