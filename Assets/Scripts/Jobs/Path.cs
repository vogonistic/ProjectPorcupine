#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion

using System.Collections.Generic;
using System.Linq;

namespace ProjectPorcupine.Jobs
{
    public class Path
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectPorcupine.Jobs.Path"/> class.
        /// </summary>
        /// <param name="goal">Initial Goal.</param>
        public Path(Goal goal, InventoryManager inventoryManager)
        {
            Fulfilled = new Needs();
            Actions = new Queue<Action>();

            Goal = goal;
            Unfulfilled = goal.Requires;
            InventoryOverride = new InventoryManagerOverride(inventoryManager);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectPorcupine.Jobs.Path"/> class from another path by adding the new action.
        /// </summary>
        /// <param name="other">Other PathToGoal that we are basing this on.</param>
        /// <param name="action">Action to add to the PathToGoal.</param>
        public Path(Path other, Action action)
        {
            Fulfilled = new Needs();
            Actions = new Queue<Action>();

            Goal = other.Goal;
            Cost = other.Cost + action.Cost;

            // We start with the requirements of the action so as to not have to loop afterwards to add them.
            Unfulfilled = new Needs(other.Unfulfilled);

            // Not entierly sure about this code...
            Unfulfilled -= action.Provides;
            Unfulfilled += action.Requires;
            Fulfilled = new Needs(other.Fulfilled);
            Fulfilled += action.Provides;
            Actions = new Queue<Action>(other.Actions); // Should this also be value based?

            InventoryOverride = other.InventoryOverride.Clone();
            InventoryOverride.AddAction(action);

            if (other.CurrentItem == null)
                CurrentItem = null;
            else
                CurrentItem = other.CurrentItem.Clone();

            Actions.Enqueue(action);
        }

        /// Final goal.
        public Goal Goal { get; private set; }

        /// Total cost in time for all actions.
        public int Cost { get; private set; }

        /// Conditions not yet fulfilled.
        public Needs Unfulfilled { get; private set; }

        /// Conditions that are fulfilled.
        public Needs Fulfilled { get; private set; }

        /// List of found actions
        public Queue<Action> Actions { get; private set; }

        /// Dictionaries with all the inventorie changes how they are now for this path
        public InventoryManagerOverride InventoryOverride { get; private set; }

        /// The tile that all further pathfinding should start from.
        public Tile CurrentTile
        {
            get
            {
                if (Actions.Count == 0)
                {
                    return Goal.StartTile;
                }

                Action action = Actions.Last();
                return action != null ? action.Tile : null;
            }
        }

        public Inventory CurrentItem;

        /// <summary>
        /// Determines whether we have fulfilled every requirement.
        /// </summary>
        /// <returns><c>true</c> if this instance is done; otherwise, <c>false</c>.</returns>
        public bool IsDone()
        {
            return Unfulfilled.Count == 0;
        }

        public override string ToString()
        {
            string output = "[PathToGoal cost: " + Cost + "\n  " + Goal + "\n";
            if (Unfulfilled.Count > 0)
            {
                output += "  Unfulfilled: " + Unfulfilled.ToString().Replace("\n", "\n  ") + "\n";
            }

            if (Fulfilled.Count > 0)
            {
                output += "  Fulfilled: " + Fulfilled.ToString().Replace("\n", "\n  ") + "\n";
            }

            if (Actions.Count > 0)
            {
                output += "  Actions:\n    " + string.Join("\n    ", Actions.Select(x => x.ToString()).ToArray()) + "\n";
            }

            if (InventoryOverride.Count > 0)
            {
                output += InventoryOverride.ToString() + "\n";
            }

            output += "]";

            return output;
        }
    }
}
