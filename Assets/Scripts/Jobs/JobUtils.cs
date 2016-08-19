using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace JobUtils
{
    public class GoalResolver
    {
        #if true
        private void Trace(string message)
        {
            Debug.Log("JOB UTILS: " + message);
            // Doesn't do multi line messages:
//            Debug.ULogChannel("JobUtils", message);
        }
        #else
        private void Trace(string message) {
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
            // Get needs steel plates
            Goal buildAWall = new Goal("Build a Wall", new Vector2(0, 0), new Condition("inventory", "Steel Plate", 5));
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

            if (path.IsDone())
                return path;

            // Should probably use a priority queue here. Based on CostInTime?
            Queue<PathToGoal> pathsToExplore = new Queue<PathToGoal>();
            pathsToExplore.Enqueue(path);

            PathToGoal currentPath;
            do
            {
                // Fetch the top path.
                currentPath = pathsToExplore.Dequeue();
                Trace("dequeued:\n" + currentPath);

                // Loop over each actionFinder.
                foreach (ActionDelegate actionFinder in PossibleActions)
                {
                    // Find all the possible paths they return.
                    List<Action> actions = actionFinder(currentPath.Unfulfilled);
                    foreach (Action nextAction in actions)
                    {
                        // Create a new path for each.
                        PathToGoal newPath = new PathToGoal(currentPath, nextAction);

                        // Yay! Found it!
                        if (newPath.IsDone())
                            return newPath;

                        // Nope, not finished. Put it on the queue.
                        pathsToExplore.Enqueue(newPath);
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
            // Crazy hardcoded
            string type = "inventory";
            string resourceWeHave = "Steel Plate";
            List<Action> actions = new List<Action>();
            // Check if they want a steel plate
            foreach (Condition c in conditions)
            {
                if (c.Type == type && c.Name == resourceWeHave)
                {
                    Action ac = new Action(
                                    "Fetch " + resourceWeHave,
                                    20,
                                    new Vector2(20, 0)
                                );

                    ac.AddProvides(c);
                    actions.Add(ac);
                }
            }
                
            return actions.Count > 0 ? actions : null;
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
        public List<Action> Actions = new List<Action>();

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

            // We start with a clean copy
            Unfulfilled = other.Unfulfilled;
            Fulfilled = other.Fulfilled;
            Actions = other.Actions;

            // Lets apply the new action
            foreach (Condition provided in action.Provides)
            {
                foreach (Condition want in Unfulfilled.ToArray())
                {
                    if (want.Type == provided.Type && want.Name == provided.Name)
                    {
                        // TODO: We don't deal with the amount
                        Unfulfilled.Remove(provided);
                        Fulfilled.Add(provided);
                    }
                }
            }
            
            foreach (Condition required in action.Requires)
            {
                Unfulfilled.Add(required);
            }

            Actions.Add(action);
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

            return string.Format("[Condition type: {0}, name: {1}, details: ({2})", Type, Name, detailsString);
        }
    }

}
