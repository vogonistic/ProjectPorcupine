using UnityEngine;
using System.Collections.Generic;
using System.Linq;

// We need to make Condition into an interface.
// Need to be able to split InventoryConditions based on count
// In XML version, add parameter for max on input and output (inventory prefab has default max stack size)
// Need to take walking into account. I.e. if you need ice and steel, go fetch one, drop it off, fetch second, drop it off. Different path from being able to carry both.
// Add code that can figure out if we need to drop off inventory or can pick up more
// Let the Character modify cost for path and action
// Split into 1 class per file
// Integrate into game

namespace JobUtils
{
    public class GoalResolver
    {
        static string InventoryType = "inventory";
        static string IronOreResource = "Iron Ore";
        static string IceResource = "Ice";
        static string SteelPlateResource = "Steel Plate";

        #if true
        void Trace(string message)
        {
            Debug.Log("JOB UTILS: " + message);
            // Doesn't do multi line messages:
//            Debug.ULogChannel("JobUtils", message);
        }
        #else
        void Trace(string message) {
        }
        #endif

        // Function delegate for returning possible actions.
        public delegate List<Action> ActionDelegate(List<Condition> conditions);
        // Contains a list of functions that checks for possible actions.
        List<ActionDelegate> PossibleActions = new List<ActionDelegate>();

        // One stop shop. Runs the whole thing.
        public void DoThings()
        {
            // Add the fetch action
            PossibleActions.Add(FetchAction);
            PossibleActions.Add(SmeltAction);
            // Get needs steel plates
            Goal buildAWall = new Goal("Build a Wall", new Vector2(0, 0), new Condition(InventoryType, SteelPlateResource, 5), new Condition(InventoryType, IceResource, 50));
            Trace("Starting with goal: " + buildAWall);

            PathToGoal path = Resolve(buildAWall);
            if (path != null)
            {
                Trace("WE FOUND A PATH:\n" + path);
            }
            else
            {
                Trace("No path found :sad:");
            }
        }

        // Runs until we've solved the problem or exhausted the potential solutions.
        PathToGoal Resolve(Goal g)
        {
            // Turn it into a PathToGoal and queue it.
            PathToGoal path = new PathToGoal(g);

            int maxIteration = 20;
            int currentInteration = 0;

            if (path.IsDone())
                return path;

            // Should we rename this so that it is a bit more general?
            // Because it does not contain anything inherently pathfindy.
            PathfindingPriorityQueue<PathToGoal> pathsToExplore = new PathfindingPriorityQueue<PathToGoal>();
            pathsToExplore.Enqueue(path, path.CostInTime);

            PathToGoal currentPath;
            do
            {
                if (++currentInteration >= maxIteration)
                    break;

                // Fetch the top path.
                currentPath = pathsToExplore.Dequeue();
                Trace("dequeued:\n" + currentPath);

                // Loop over each actionFinder.
                foreach (ActionDelegate actionFinder in PossibleActions)
                {
                    // Find all the possible paths they return.
                    List<Action> actions = actionFinder(currentPath.Unfulfilled);

                    if (actions == null)
                        continue;

                    foreach (Action nextAction in actions)
                    {
                        // Create a new path for each.
                        PathToGoal newPath = new PathToGoal(currentPath, nextAction);

                        // Yay! Found it!
                        if (newPath.IsDone())
                            return newPath;

                        // Nope, not finished. Put it on the queue.
                        pathsToExplore.Enqueue(newPath, newPath.CostInTime);
                    }
                }
            } while (pathsToExplore.Count > 0);

            // We've exhausted the search.
            return null;
        }

        /// <summary>
        /// Looks for places where we can find the requested materials
        /// </summary>
        /// <returns>A list of actions</returns>
        /// <param name="conditions">Conditions to fulfill</param>
        List<Action> FetchAction(List<Condition> conditions)
        {
            Trace("Testing Fetch Actions");
            // Crazy hardcoded
            string resourceWeHave = IronOreResource;
            string resourceWeHave2 = IceResource;
            List<Action> actions = new List<Action>();
            // Check if they want a steel plate
            foreach (Condition c in conditions)
            {
                if (c.Type == InventoryType)
                {
                    if (c.Name == resourceWeHave)
                    {
                        Action ac = new Action(
                                        "Fetch " + resourceWeHave,
                                        20,
                                        new Vector2(20, 0)
                                    );

                        ac.AddProvides(c);
                        actions.Add(ac);
                    }// This is the really lazy test code way of doing this.
                    // DO NOT DO IT THIS WAY. :)
                    else if (c.Name == resourceWeHave2)
                    {
                        Action ac = new Action(
                                        "Fetch " + resourceWeHave2,
                                        50,
                                        new Vector2(50, 0)
                                    );

                        ac.AddProvides(c);
                        actions.Add(ac);
                    }
                }
            }

            Trace(string.Format(" - Found {0} actions", actions.Count));
            return actions;
        }


        List<Action> SmeltAction(List<Condition> conditions)
        {
            Trace("Testing Smelt Actions");
            List<Action> actions = new List<Action>();
            foreach (Condition c in conditions)
            {
                if (c.Type == InventoryType && c.Name == SteelPlateResource)
                {// we need something better than casting details [0] to an int!
                    Action smelt = new Action("Forge Iron to Steel", (int)c.Details[0] * 2 + 20, new Vector2(0, 20));
                    smelt.AddProvides(new Condition(InventoryType, SteelPlateResource, 1));
                    smelt.AddRequirement(new Condition(InventoryType, IronOreResource, 1));
                    actions.Add(smelt);
                }
            }

            Trace(string.Format(" - Found {0} actions", actions.Count));
            return actions;
        }
    }

    public class PathToGoal
    {
        // Final goal.
        public Goal Goal;
        // Total cost in time for all actions.
        public int CostInTime;

        // Conditions not yet fulfilled.
        public List<Condition> Unfulfilled;
        // Conditions that are fulfilled.
        public List<Condition> Fulfilled = new List<Condition>();
        // List of found actions
        // Should we make this a priority queue?
        // Then it would automatically find the shortest set of actions? ... Not sure about that.
        public Queue<Action> Actions = new Queue<Action>();

        /// <summary>
        /// Initializes a new instance of the <see cref="JobUtils.PathToGoal"/> class.
        /// </summary>
        /// <param name="goal">Goal</param>
        public PathToGoal(Goal goal)
        {
            Goal = goal;
            Unfulfilled = goal.Requires;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JobUtils.PathToGoal"/> class from another path by adding the new action.
        /// </summary>
        /// <param name="other">Other.</param>
        /// <param name="action">Action.</param>
        public PathToGoal(PathToGoal other, Action action)
        {
            Goal = other.Goal;
            CostInTime = other.CostInTime + action.CostInTime;

            // We start with the requirements of the action so as to not have to loop afterwards to add them.
            Unfulfilled = new List<Condition>(action.Requires);
            Fulfilled = new List<Condition>(other.Fulfilled);
            Actions = new Queue<Action>(other.Actions);

            // Start: [Goal name: Build a Wall, location: (0.0, 0.0), requires: ([Condition type: inventory, name: Steel Plate, details: (5)])]

            // End:
            // [PathToGoal cost: 42
            //   [Goal name: Build a Wall, location: (0.0, 0.0), requires: ([Condition type: inventory, name: Steel Plate, details: (5))]
            //   Fulfilled:
            //     [Condition type: inventory, name: Steel Plate, details: (1)]
            //     [Condition type: inventory, name: Iron Ore, details: (1)]
            //   Actions:
            //     [Action name: Forge Iron to Steel, cost: 22, location: (0.0, 20.0)]
            //     [Action name: Fetch Iron Ore, cost: 20, location: (20.0, 0.0)]
            // ]


            // Loop over all the things that was previously unfulfilled and test if they still are.
            foreach (Condition uf in other.Unfulfilled)
            {
                bool fulfilled = false;
                foreach (Condition provided in action.Provides)
                {// Do not add to unfulfilled in the loop as that will be bad for multiple provides (In case it is a thing.)
                    if (uf.Equals(provided))
                    {
                        fulfilled = true;
                        Fulfilled.Add(provided);
                        Debug.Log(" - Fulfilled: " + uf);
                        break;
                    }
                }
                if (fulfilled == false)
                {
                    Unfulfilled.Add(uf);
                    Debug.Log(" - Unfulfilled: " + uf);
                }
            }

            Actions.Enqueue(action);
        }

        public bool IsDone()
        {
            return Unfulfilled.Count == 0;
        }

        public override string ToString()
        {
            string output = "[PathToGoal cost: " + CostInTime + "\n  " + Goal + "\n";
            if (Unfulfilled.Count > 0)
            {
                output += "  Unfulfilled:\n    " + string.Join("\n    ", Unfulfilled.Select(x => x.ToString()).ToArray()) + "\n";
            }

            if (Fulfilled.Count > 0)
            {
                output += "  Fulfilled:\n    " + string.Join("\n    ", Fulfilled.Select(x => x.ToString()).ToArray()) + "\n";
            }

            if (Actions.Count > 0)
            {
                output += "  Actions:\n    " + string.Join("\n    ", Actions.Select(x => x.ToString()).ToArray()) + "\n";
            }

            output += "]";

            return output;
        }
    }

    public class Goal
    {
        public string Name;
        public List<Condition> Requires;
        public Vector2 Location;

        public Goal(string name, Vector2 location, params Condition[] requirements)
        {
            Name = name;
            Location = location;
            Requires = new List<Condition>(requirements);
        }

        public Goal(Goal other)
        {
            this.Name = other.Name;
            this.Location = other.Location;
            this.Requires = other.Requires;
        }

        public override string ToString()
        {
            string requiredString = "";

            if (Requires.Count > 0)
                requiredString = string.Join(", ", Requires.Select(r => r.ToString()).ToArray());

            return string.Format("[Goal name: {0}, location: {1}, requires: ({2})]", Name, Location, requiredString);
        }
    }

    public class Action
    {
        public string Name;
        public int CostInTime;
        public Vector2 Location;

        public List<Condition> Requires = new List<Condition>();
        public List<Condition> Provides = new List<Condition>();

        public Action(string name, int costInTime, Vector2 location)
        {
            Name = name;
            CostInTime = costInTime;
            Location = location;
        }

        public Action Copy()
        {
            return new Action(Name, CostInTime, Location);
        }

        public void AddRequirement(Condition c)
        {
            Requires.Add(c);
        }

        public void AddProvides(Condition c)
        {
            Provides.Add(c);
        }

        public override string ToString()
        {
            return string.Format("[Action name: {0}, cost: {1}, location: {2}]", Name, CostInTime, Location.ToString());
        }
    }

    public class Condition
    {
        public string Type;
        public string Name;
        public List<object> Details;

        public Condition(string type, string name, params object[] details)
        {
            Type = type;
            Name = name;
            Details = new List<object>(details);
        }

        public override string ToString()
        {
            string detailsString = "";

            if (Details.Count > 0)
                detailsString = string.Join(", ", Details.Select(d => d.ToString()).ToArray());

            return string.Format("[Condition type: {0}, name: {1}, details: ({2})]", Type, Name, detailsString);
        }

        public bool Equals(Condition other)
        {
            // Return true if the fields match:
            return Type == other.Type && Name == other.Name;
        }
    }

}
