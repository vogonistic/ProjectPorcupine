using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace JobUtils
{
    public class Goal
    {
        public string Name;
        public NeedsDictionary Requires;
        public Vector2 Location;

        public Goal(string name, Vector2 location, NeedsDictionary needs)
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