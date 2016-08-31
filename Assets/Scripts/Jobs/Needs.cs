#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion

using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ProjectPorcupine.Jobs
{
    public class Needs : IEnumerable
    {
        private Dictionary<string, int> data;

        public Needs()
        {
            data = new Dictionary<string, int>();
        }

        public Needs(Needs other)
        {
            data = new Dictionary<string, int>(other.data);
        }

        public int Count
        {
            get
            {
                return data.Keys.Count; 
            }
        }

        public static Needs operator +(Needs a, Needs b)
        {
            Needs ret = new Needs(a);
            foreach (string key in b)
            {
                ret.Add(key, b.data[key]);
            }

            return ret;
        }

        public static Needs operator -(Needs a, Needs b)
        {
            Needs ret = new Needs(a);
            foreach (string key in b)
            {
                ret.Remove(key, b.data[key]);
            }

            return ret;
        }

        public static Needs Intersection(Needs a, Needs b)
        {
            Needs ret = new Needs();
            foreach (string key in a)
            {
                if (b.Value(key) > 0)
                {
                    ret.Add(key, Max(a.data[key], b.data[key]) - Min(a.data[key], b.data[key]));
                }
            }

            return ret;
        }

        public Needs Add(string key, int n = 1)
        {
            if (n > 0)
            {
                if (data.ContainsKey(key))
                {
                    data[key] += n;
                }
                else
                {
                    data.Add(key, n);
                }
            }

            return this;
        }

        public Needs Remove(string key, int n = 1)
        {
            if (n > 0)
            {
                if (data.ContainsKey(key))
                {
                    data[key] -= n;

                    if (data[key] <= 0)
                    {
                        data.Remove(key);
                    }
                }
            }

            return this;
        }

        public int Value(string key)
        {
            if (data.ContainsKey(key))
            {
                return data[key];
            }
            else
            {
                return 0;
            }
        }

        public string FirstKey()
        {
            return data.Keys.ElementAt(0);
        }

        public IEnumerator GetEnumerator()
        {
            return data.Keys.GetEnumerator();
        }

        public override string ToString()
        {
            string ret = "[Needs\n";
            foreach (string key in this)
            {
                ret += "  " + key + ": " + data[key] + ",\n";
            }

            return ret + "]";
        }

        private static int Min(int a, int b)
        {
            return a < b ? a : b;
        }

        private static int Max(int a, int b)
        {
            return a > b ? a : b;
        }
    }
}