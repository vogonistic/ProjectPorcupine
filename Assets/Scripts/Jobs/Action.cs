using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace JobUtils
{
    public class Action
    {
        public string Name;
        public int CostInTime;
        public Vector2 Location;

        public NeedsDictionary Requires = new NeedsDictionary();
        public NeedsDictionary Provides = new NeedsDictionary();

        public Action(string name, int costInTime, Vector2 location)
        {
            Name = name;
            CostInTime = costInTime;
            Location = location;
        }

        public Action Copy()
        {
            return new Action(Name, CostInTime, Location);
        }

        public void AddRequirement(string c)
        {
            Requires.Add(c);
        }

        public void AddNRequirements(string c, int n)
        {
            Requires.AddN(c, n);
        }

        public void AddProvides(string c)
        {
            Provides.Add(c);
        }

        public void AddNProvides(string c, int n)
        {
            Provides.AddN(c, n);
        }

        public override string ToString()
        {
            return string.Format("[Action name: {0}, cost: {1}, location: {2}]", Name, CostInTime, Location.ToString());
        }
    }
}