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
    public class Action
    {
        public string Name;
        public int Cost;
        public Tile Tile;

        public Needs Requires = new Needs();
        public Needs Provides = new Needs();

        public Action(string name, int costInTime, Tile tile)
        {
            Name = name;
            Cost = costInTime;
            Tile = tile;
        }

        public Action Copy()
        {
            return new Action(Name, Cost, Tile);
        }

        public void AddRequirement(string c, int n = 1)
        {
            Requires.Add(c, n);
        }

        public void AddProvides(string c, int n = 1)
        {
            Provides.Add(c, n);
        }

        public override string ToString()
        {
            return string.Format("[Action name: {0}, cost: {1}, tile: ({2},{3})]", Name, Cost, Tile.X, Tile.Y);
        }
    }
}