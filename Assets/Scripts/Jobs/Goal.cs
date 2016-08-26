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
    public class Goal
    {
        public string Name;
        public Needs Requires;
        public Vector2 Location;

        public Goal(string name, Vector2 location, Needs needs)
        {
            Name = name;
            Location = location;
            Requires = needs;
        }

        public Goal(Goal other)
        {
            this.Name = other.Name;
            this.Location = other.Location;
            this.Requires = other.Requires;
        }

        public override string ToString()
        {
            return string.Format("[Goal name: {0}, location: {1}, requires: ({2})]", Name, Location, Requires);
        }
    }
}