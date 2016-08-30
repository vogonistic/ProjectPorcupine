#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion

using System.Collections.Generic;

// In XML version, add parameter for max on input and output (inventory prefab has default max stack size)
// Need to take walking into account. I.e. if you need ice and steel, go fetch one, drop it off, fetch second, drop it off. Different path from being able to carry both.
// Add code that can figure out if we need to drop off inventory or can pick up more
// Let the Character modify cost for path and action
// Integrate into game
namespace ProjectPorcupine.Jobs
{
    public class Resolver
    {
        private static string ironOreResource = "Raw Iron";
        private static string iceResource = "Ice";
        private static string steelPlateResource = "Steel Plate";

        private static InventoryManager inventoryManagerReference;

        // Contains a list of functions that checks for possible actions.
        private List<ActionDelegate> possibleActions = new List<ActionDelegate>();

        // Contains the normal state which we have to return to after running resolve. Out of Efficiency we only add stuff to the list that is actually changed. It is also used to reset after each Path
        private Dictionary<Tile, Inventory> inventoryBeforeChanges = new Dictionary<Tile, Inventory>();

        public Resolver(InventoryManager invManager)
        {
            inventoryManagerReference = invManager;
        }

        // Function delegate for returning possible actions.
        public delegate List<Action> ActionDelegate(Needs conditions,Path CurrentPath);  // A side note here in the CurrentPath the conditions are there --> Performance wise we do not need another reference to be created

        // One stop shop. Runs the whole thing.
        public void DoThings()
        {
            // Add the fetch action
            possibleActions.Add(FetchAction);
            possibleActions.Add(SmeltAction);

            // Get needs steel plates
            Goal buildAWall = new Goal(
                                  "Build a Wall", 
                                  World.Current.GetTileAt(0, 0), 
                                  new Needs()
                {
                    { steelPlateResource, 5 },
                    { iceResource, 20 }
                });

            Trace("Starting with goal: " + buildAWall);

            Path path = Resolve(buildAWall);
            if (path != null)
            {
                Trace("WE FOUND A PATH:\n" + path);
            }
            else
            {
                Trace("No path found :sad:");
            }
        }

        #if true
        private void Trace(string message)
        {
            Debug.Log("JOB UTILS: " + message);

            // Doesn't do multi line messages:
            // Debug.ULogChannel("JobUtils", message);
        }
        #else
        private void Trace(string message) {
        }
        #endif

        // Runs until we've solved the problem or exhausted the potential solutions.
        private Path Resolve(Goal g)
        {
            // Turn it into a PathToGoal and queue it.
            Path path = new Path(g);

            int maxIteration = 200;
            int currentInteration = 0;

            if (path.IsDone())
            {
                return path;
            }

            // Should we rename this so that it is a bit more general?
            // Because it does not contain anything inherently pathfindy.
            PathfindingPriorityQueue<Path> pathsToExplore = new PathfindingPriorityQueue<Path>();
            pathsToExplore.Enqueue(path, path.Cost);

            Path currentPath = null;
            do
            {
                if (++currentInteration >= maxIteration)
                {
                    break;
                }

                // Fetch the top path.
                currentPath = pathsToExplore.Dequeue();
                Trace("dequeued:\n" + currentPath);

                // We deleta all inventory changes made to the lastpath. We have now a blank slate again.
                RollbackInventoryChanges();

                // Get the changes made until now on the currentPath
                GetInventoryChanges(currentPath);

                // Loop over each actionFinder.
                foreach (ActionDelegate actionFinder in possibleActions)
                {
                    // Find all the possible paths they return.
                    List<Action> actions = actionFinder(currentPath.Unfulfilled, currentPath);

                    if (actions == null)
                    {
                        continue;
                    }

                    foreach (Action nextAction in actions)
                    {
                        // Create a new path for each.
                        Path newPath = new Path(currentPath, nextAction);

                        // Yay! Found it!
                        if (newPath.IsDone())
                        {
                            RollbackInventoryChanges(); // If we found a new Path reset everything so that the game is not changed.
                            return newPath;
                        }

                        // Nope, not finished. Put it on the queue.
                        pathsToExplore.Enqueue(newPath, newPath.Cost);
                    }
                }
            }
            while (pathsToExplore.Count > 0);

            // We've exhausted the search.
            RollbackInventoryChanges(); // If we found nothing, we still need to change everything back to what it was
            return null;
        }

        /// <summary>
        /// Looks for places where we can find the requested materials.
        /// </summary>
        /// <returns>A list of actions.</returns>
        /// <param name="conditions">Conditions to fulfill.</param>
        private List<Action> FetchAction(Needs conditions, Path currentPath)
        {
            Trace("Testing Fetch Actions");

            List<Action> actions = new List<Action>();

            // Loop through every condition we have and search for resources we can fetch easily without crafting
            foreach (string resourceWeNeed in conditions)
            {
                if (inventoryManagerReference.QuickCheck(resourceWeNeed)) // Check if there even is such a ressource known to ther inventoryManager if not we do not need to Pathfind
                {
                    int toFetch = conditions.Value(resourceWeNeed); //We would in the best case want to fetch everything we need from this resource

                    // There is probably a beter way then setting the desired amount to the needed resource but for now I am testing if this works
                    // TODO: Currently I let this run from the center location --> this should be the position of the character running this code
                    Path_AStar path = inventoryManagerReference.GetPathToClosestInventoryOfType(resourceWeNeed, currentPath.currentTile, toFetch, true);

                    Action ac = new Action(
                                    "Fetch " + resourceWeNeed,
                                    path.Length(),    // Use the path.Length as cost so that longer paths to a resource should not be desired
                                    path.EndTile());

                    // Save changes in inventoryBeforeChanges so that we can rollback to them after we are done resolving
                    // Just found a way to make the code a little bit faster this was edited
                    if (inventoryBeforeChanges.ContainsKey(path.EndTile()) == false) // If we haven't saved the tile yet, it should be saved as we are defintely going to change the tile in a bit.
                    {
                        Inventory oldInventory = path.EndTile().Inventory;  // Check if path.EndTile's Inventory is null
                        if (oldInventory != null)
                        {
                            oldInventory = path.EndTile().Inventory.Clone(); // If it isn't we clone it into our dictionary that contains how things were before.
                            oldInventory.tile = path.EndTile();              // Weird is that .Clone() does not copy the tile it belonged to once. If this wasn't set it could maybe break things so I set it here.
                        }

                        inventoryBeforeChanges.Add(path.EndTile(), oldInventory);
                    }

                    // Look for the needed amount and update the inventory
                    // Here we calculate and change the tile in question
                    if (toFetch >= path.EndTile().Inventory.StackSize)  // If the needed amount is larger then the amount we have there then we should take all we can and delete what is there as we took all of it.
                    {
                        toFetch = path.EndTile().Inventory.StackSize;
                        path.EndTile().Inventory.StackSize = 0;
                        inventoryManagerReference.CleanupInventory(path.EndTile().Inventory);
                    }
                    else
                    {
                        path.EndTile().Inventory.StackSize -= toFetch;
                    }

                    // Save Changes in the Current Path so that we can put them back if we continue along this path
                    Inventory newInventory = path.EndTile().Inventory;
                    if (newInventory != null)   // If the Inventory still exists then we should clone it
                    {
                        newInventory = path.EndTile().Inventory.Clone();
                        newInventory.tile = path.EndTile();
                    }


                    // Here there are 2 possibilities:
                    // 1: we have that tile already in our list, that would mean we just update the tile in there and let garbage collection cleanup
                    // 2: we have it not in our list and add the new inventory to our list.
                    if (currentPath.inventoryChanges.ContainsKey(path.EndTile()))
                    {
                        currentPath.inventoryChanges[path.EndTile()] = newInventory;
                    }
                    else
                    {
                        currentPath.inventoryChanges.Add(path.EndTile(), newInventory);
                    }

                    ac.AddProvides(resourceWeNeed, toFetch);
                    actions.Add(ac);
                }
            }

            Trace(string.Format(" - Found {0} actions", actions.Count));
            return actions;
        }

        private List<Action> SmeltAction(Needs conditions, Path currentPath)
        {
            Trace("Testing Smelt Actions");
            List<Action> actions = new List<Action>();
            int value = conditions.Value(steelPlateResource);
            if (value > 0)
            {
                Action smelt = new Action("Forge Iron to Steel", (value * 2) + 20, World.Current.GetTileAt(20, 20));
                smelt.AddProvides(steelPlateResource, value);
                smelt.AddRequirement(ironOreResource, value);
                actions.Add(smelt);
            }

            Trace(string.Format(" - Found {0} actions", actions.Count));
            return actions;
        }

        /// <summary>
        /// We take all the previous states of inventory we saved in inventoryBeforeChanges and apply them.
        /// That means the world now looks like it was before resolver was run.
        /// </summary>
        private void RollbackInventoryChanges()
        {
            if (inventoryBeforeChanges == null) // If there were no changes made then do nothing
            {
                return;
            }

            foreach (Tile tile in inventoryBeforeChanges.Keys)
            {
                if (tile.Inventory != null) //If there is an Inventory there then delete it. The reason for this is that PlaceInventory in the inventoryManager (Which also handles all callbacks) would otherwise stack the items or do nothing if there was something else there
                {
                    tile.Inventory.StackSize = 0;
                    inventoryManagerReference.CleanupInventory(tile.Inventory); //This destroys the inventory after the stacksize is zero
                }

                inventoryManagerReference.PlaceInventory(tile, inventoryBeforeChanges[tile].Clone()); // We let the game handle all callbacks and give them a clone of our previous Inventory, so If we change it later that it doesn't change it in this list.
            }
        }

        /// <summary>
        /// We take all the changes that happened until now on that path and apply them.
        /// </summary>
        /// <param name="currentPath"></param>
        private void GetInventoryChanges(Path currentPath)
        {
            if (currentPath == null || currentPath.inventoryChanges == null) // Preventing NullReference Errors by checking if anything is null
            {
                return;
            }

            foreach (Tile tile in currentPath.inventoryChanges.Keys)
            {
                if (tile.Inventory != null) // Same as in RollbackInventoryChanges
                {
                    tile.Inventory.StackSize = 0;
                    inventoryManagerReference.CleanupInventory(tile.Inventory);
                }

                if (currentPath.inventoryChanges[tile] != null) // Here we need an extra check because it could be that an Inventory is now empty and we cannot clone null
                {
                    inventoryManagerReference.PlaceInventory(tile, currentPath.inventoryChanges[tile].Clone());
                }
            }
        }
    }
}