#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion

using UnityEngine;

namespace ProjectPorcupine.Jobs
{
    public class Action
    {
        public string Name;
        public int Cost;
        public Vector2 Location;

        public Needs Requires = new Needs();
        public Needs Provides = new Needs();

        public Action(string name, int costInTime, Vector2 location)
        {
            Name = name;
            Cost = costInTime;
            Location = location;
        }

        public Action Copy()
        {
            return new Action(Name, Cost, Location);
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
            return string.Format("[Action name: {0}, cost: {1}, location: {2}]", Name, Cost, Location.ToString());
        }
    }
}