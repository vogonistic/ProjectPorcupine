using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
    public class PathToGoal
    {
        /// Final goal.
        public Goal Goal;

        /// Total cost in time for all actions.
        public int CostInTime;

        /// Conditions not yet fulfilled.
        public NeedsDictionary Unfulfilled;

        /// Conditions that are fulfilled.
        public NeedsDictionary Fulfilled = new NeedsDictionary();

        // Should we make this a priority queue?
        // Then it would automatically find the shortest set of actions? ... Not sure about that.

        /// List of found actions
        public Queue<Action> Actions = new Queue<Action>();

        /// <summary>
        /// Initializes a new instance of the <see cref="JobUtils.PathToGoal"/> class.
        /// </summary>
        /// <param name="goal">Goal.</param>
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
            Unfulfilled = new NeedsDictionary(other.Unfulfilled);

            // Not entierly sure about this code...
            Unfulfilled -= action.Provides;
            Unfulfilled += action.Requires;
            Fulfilled = new NeedsDictionary(other.Fulfilled);
            Actions = new Queue<Action>(other.Actions); // Should this also be value based?

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
                output += "  Unfulfilled:\n    " + Unfulfilled.ToString() + "\n";
            }

            if (Fulfilled.Count > 0)
            {
                output += "  Fulfilled:\n    " + Fulfilled.ToString() + "\n";
            }

            if (Actions.Count > 0)
            {
                output += "  Actions:\n    " + string.Join("\n    ", Actions.Select(x => x.ToString()).ToArray()) + "\n";
            }

            output += "]";

            return output;
        }
    }
}
