#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion

namespace ProjectPorcupine.Jobs
{
    public class Goal
    {
        public string Name;
        public Needs Requires;
        public Tile StartTile;
        public Tile GoalTile;

        public Goal(string name, Tile startingTile, Tile goalTile, Needs needs)
        {
            Name = name;
            StartTile = startingTile;
            GoalTile = goalTile;
            Requires = needs;
        }

        public Goal(Goal other)
        {
            this.Name = other.Name;
            this.StartTile = other.StartTile;
            this.GoalTile = other.GoalTile;
            this.Requires = other.Requires;
        }

        public override string ToString()
        {
            return string.Format("[Goal name: {0}, tile: {1}, requires: ({2})]", Name, GoalTile, Requires);
        }
    }
}