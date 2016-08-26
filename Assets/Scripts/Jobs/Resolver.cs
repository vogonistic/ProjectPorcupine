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

        // Function delegate for returning possible actions.
        public delegate List<Action> ActionDelegate(Needs conditions, Path currentPath);

        Dictionary<Tile, Inventory> inventoryBeforeChanges = new Dictionary<Tile, Inventory>();

        public Resolver(InventoryManager invManager)
        {
            inventoryManagerReference = invManager;
        }

        // One stop shop. Runs the whole thing.
        public void DoThings()
        {

            // Add the fetch action
            possibleActions.Add(FetchAction);
            possibleActions.Add(SmeltAction);

            // Get needs steel plates
            Goal buildAWall = new Goal("Build a Wall", World.current.GetTileAt(0, 0), new Needs()
                {
                    { steelPlateResource, 5 },
                    { iceResource, 20 } //Made Ice a little less so that there definitelly is enough in the world to fetch
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

                // Rollback inventory changes made to the lastpath
                RollbackInventoryChanges();
                // Get the new changes made to the currentpath
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
                            RollbackInventoryChanges();
                            return newPath;
                        }

                        // Nope, not finished. Put it on the queue.
                        pathsToExplore.Enqueue(newPath, newPath.Cost);
                    }
                }
            }
            while (pathsToExplore.Count > 0);

            // We've exhausted the search.
            RollbackInventoryChanges();
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

            // Loop through the Inventory Manager
            foreach (string resourceWeNeed in conditions)
            {
                if (inventoryManagerReference.QuickCheck(resourceWeNeed))
                {
                    int toFetch = conditions.Value(resourceWeNeed);

                    // There is probably a beter way then setting the desired amount to the needed resource but for now I am testing if this works
                    // TODO: Currently I let this run from the center location --> this should be the position of the character running this code
                    Path_AStar path = inventoryManagerReference.GetPathToClosestInventoryOfType(resourceWeNeed, World.current.GetCenterTile(), toFetch, true);

                    Action ac = new Action(
                                    "Fetch " + resourceWeNeed,
                                    path.Length(),
                                    path.EndTile()
                                    );

                    //Save changes in inventoryBeforeChanges so that we can rollback to them after we are done resolving
                    Inventory newInventory = path.EndTile().Inventory;
                    if (newInventory != null)
                    {
                        newInventory = path.EndTile().Inventory.Clone();
                        newInventory.tile = path.EndTile();
                    }

                    if (inventoryBeforeChanges.ContainsKey(path.EndTile()) == false)
                    {
                        Trace(newInventory.objectType + " - " + newInventory.stackSize);
                        inventoryBeforeChanges.Add(path.EndTile(), newInventory);
                    }

                    //Look for the needed amount and update the inventory
                    if (toFetch >= path.EndTile().Inventory.stackSize)
                    {
                        toFetch = path.EndTile().Inventory.stackSize;
                        path.EndTile().Inventory.stackSize = 0;
                        inventoryManagerReference.CleanupInventory(path.EndTile().Inventory);
                    }
                    else
                    {
                        path.EndTile().Inventory.stackSize -= toFetch;
                    }

                    //Save Changes in the Current Path so that we can put them back if we continue along this path
                    Inventory newInventory2 = path.EndTile().Inventory;
                    if (newInventory2 != null)
                    {
                        newInventory2 = path.EndTile().Inventory.Clone();
                        newInventory2.tile = path.EndTile();
                    }

                    if (currentPath.inventoryChanges.ContainsKey(path.EndTile()))
                    {
                        currentPath.inventoryChanges[path.EndTile()] = newInventory2;
                    }
                    else
                    {
                        currentPath.inventoryChanges.Add(path.EndTile(), newInventory2);
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
                Action smelt = new Action("Forge Iron to Steel", (value * 2) + 20, World.current.GetTileAt(20, 20));
                smelt.AddProvides(steelPlateResource, value);
                smelt.AddRequirement(ironOreResource, value);
                actions.Add(smelt);
            }

            Trace(string.Format(" - Found {0} actions", actions.Count));
            return actions;
        }

        private void RollbackInventoryChanges()
        {
            if (inventoryBeforeChanges == null)
                return;

            foreach(Tile tile in inventoryBeforeChanges.Keys)
            {
                if(tile.Inventory != null)
                {
                    tile.Inventory.stackSize = 0;
                    inventoryManagerReference.CleanupInventory(tile.Inventory);
                }

                inventoryManagerReference.PlaceInventory(tile, inventoryBeforeChanges[tile].Clone());
            }
        }

        private void GetInventoryChanges(Path currentPath)
        {
            if (currentPath == null || currentPath.inventoryChanges == null)
                return;

            foreach (Tile tile in currentPath.inventoryChanges.Keys)
            {
                if (tile.Inventory != null)
                {
                    tile.Inventory.stackSize = 0;
                    inventoryManagerReference.CleanupInventory(tile.Inventory);
                }

                if(currentPath.inventoryChanges[tile] != null)
                    inventoryManagerReference.PlaceInventory(tile, currentPath.inventoryChanges[tile].Clone());
            }
        }
    }
}