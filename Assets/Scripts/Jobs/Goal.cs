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
        public Tile Tile;

        public Goal(string name, Tile tile, Needs needs)
        {
            Name = name;
            Tile = tile;
            Requires = needs;
        }

        public Goal(Goal other)
        {
            this.Name = other.Name;
            this.Tile = other.Tile;
            this.Requires = other.Requires;
        }

        public override string ToString()
        {
            return string.Format("[Goal name: {0}, tile: {1}, requires: ({2})]", Name, Tile, Requires);
        }
    }
}