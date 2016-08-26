#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion

using System.Collections.Generic;
using UnityEngine;

// In XML version, add parameter for max on input and output (inventory prefab has default max stack size)
// Need to take walking into account. I.e. if you need ice and steel, go fetch one, drop it off, fetch second, drop it off. Different path from being able to carry both.
// Add code that can figure out if we need to drop off inventory or can pick up more
// Let the Character modify cost for path and action
// Integrate into game
namespace ProjectPorcupine.Jobs
{
    public class Resolver
    {
        private static string ironOreResource = "Iron Ore";
        private static string iceResource = "Ice";
        private static string steelPlateResource = "Steel Plate";

        // Contains a list of functions that checks for possible actions.
        private List<ActionDelegate> possibleActions = new List<ActionDelegate>();

        // Function delegate for returning possible actions.
        public delegate List<Action> ActionDelegate(Needs conditions);

        // One stop shop. Runs the whole thing.
        public void DoThings()
        {
            // Add the fetch action
            possibleActions.Add(FetchAction);
            possibleActions.Add(SmeltAction);

            // Get needs steel plates
            Needs needs = new Needs();
            needs.Add(steelPlateResource, 5);
            needs.Add(iceResource, 50);
            Goal buildAWall = new Goal("Build a Wall", new Vector2(0, 0), needs);
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

            int maxIteration = 20;
            int currentInteration = 0;

            if (path.IsDone())
            {
                return path;
            }

            // Should we rename this so that it is a bit more general?
            // Because it does not contain anything inherently pathfindy.
            PathfindingPriorityQueue<Path> pathsToExplore = new PathfindingPriorityQueue<Path>();
            pathsToExplore.Enqueue(path, path.Cost);

            Path currentPath;
            do
            {
                if (++currentInteration >= maxIteration)
                {
                    break;
                }

                // Fetch the top path.
                currentPath = pathsToExplore.Dequeue();
                Trace("dequeued:\n" + currentPath);

                // Loop over each actionFinder.
                foreach (ActionDelegate actionFinder in possibleActions)
                {
                    // Find all the possible paths they return.
                    List<Action> actions = actionFinder(currentPath.Unfulfilled);

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
                            return newPath;
                        }

                        // Nope, not finished. Put it on the queue.
                        pathsToExplore.Enqueue(newPath, newPath.Cost);
                    }
                }
            }
            while (pathsToExplore.Count > 0);

            // We've exhausted the search.
            return null;
        }

        /// <summary>
        /// Looks for places where we can find the requested materials.
        /// </summary>
        /// <returns>A list of actions.</returns>
        /// <param name="conditions">Conditions to fulfill.</param>
        private List<Action> FetchAction(Needs conditions)
        {
            Trace("Testing Fetch Actions");

            // Crazy hardcoded
            string resourceWeHave = ironOreResource;
            string resourceWeHave2 = iceResource;
            List<Action> actions = new List<Action>();

            // Check if they want a steel plate
            if (conditions.Value(resourceWeHave) > 0)
            {
                Action ac = new Action(
                                "Fetch " + resourceWeHave,
                                20,
                                new Vector2(20, 0));

                ac.AddProvides(resourceWeHave, 50);
                actions.Add(ac);
            }

            if (conditions.Value(resourceWeHave2) > 0)
            {
                Action ac = new Action(
                                "Fetch " + resourceWeHave2,
                                20,
                                new Vector2(20, 0));

                ac.AddProvides(resourceWeHave2, 50);
                actions.Add(ac);
            }

            Trace(string.Format(" - Found {0} actions", actions.Count));
            return actions;
        }

        private List<Action> SmeltAction(Needs conditions)
        {
            Trace("Testing Smelt Actions");
            List<Action> actions = new List<Action>();
            int value = conditions.Value(steelPlateResource);
            if (value > 0)
            {
                Action smelt = new Action("Forge Iron to Steel", (value * 2) + 20, new Vector2(0, 20));
                smelt.AddProvides(steelPlateResource, value);
                smelt.AddRequirement(ironOreResource, value);
                actions.Add(smelt);
            }

            Trace(string.Format(" - Found {0} actions", actions.Count));
            return actions;
        }
    }
}