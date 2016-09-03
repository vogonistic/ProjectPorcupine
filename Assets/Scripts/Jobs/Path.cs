﻿#region License
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
        /// Final goal.
        public Goal Goal;

        /// Total cost in time for all actions.
        public int Cost;

        /// Conditions not yet fulfilled.
        public Needs Unfulfilled;

        /// Conditions that are fulfilled.
        public Needs Fulfilled = new Needs();

        /// List of found actions
        public Queue<Action> Actions = new Queue<Action>();

        // I do not like this but for now I will keep it like this... It should also only save the coordnates of the tile.
        // the idea was to buffer changes as there could be multiple paths and I tricked the inventoryManager --> This should definitely be changed as resource wise this is very inefficient
        /// Dictionaries with all the inventorie changes how they are now for this path
        public InventoryManagerOverride InventoryOverride;

        public Tile currentTile
        {
            get
            {
                if (Actions.Count == 0)
                    return Goal.StartTile;
                
                Action action = Actions.Last();
                return action != null ? action.Tile : null;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectPorcupine.Jobs.Path"/> class.
        /// </summary>
        /// <param name="goal">Initial Goal.</param>
        public Path(Goal goal, InventoryManager inventoryManager)
        {
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

            Actions.Enqueue(action);
        }

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
