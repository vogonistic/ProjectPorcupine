using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace JobUtils
{
    public class NeedsDictionary : IEnumerable
    {
        private Dictionary<string, int> data;

        public NeedsDictionary() 
        {
            data = new Dictionary<string, int>();
        }
            
        public NeedsDictionary(NeedsDictionary other) 
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

        public static NeedsDictionary operator +(NeedsDictionary a, NeedsDictionary b) 
        {
            NeedsDictionary ret = new NeedsDictionary(a);
            foreach (string key in b)
            {
                ret.AddN(key, b.data[key]);
            }

            return ret;
        }

        public static NeedsDictionary operator -(NeedsDictionary a, NeedsDictionary b) 
        {
            NeedsDictionary ret = new NeedsDictionary(a);
            foreach (string key in b)
            {
                ret.RemoveN(key, b.data[key]);
            }

            return ret;
        }

        public static NeedsDictionary Intersection(NeedsDictionary a, NeedsDictionary b) 
        {
            NeedsDictionary ret = new NeedsDictionary();
            foreach (string key in a)
            {
                if (b.Value(key) > 0)
                {
                    ret.AddN(key, a.data[key] + b.data[key]);
                }
            }

            return ret;
        }
            
        public NeedsDictionary Add(string key) 
        {
            AddN(key, 1);
            return this;
        }

        public NeedsDictionary AddN(string key, int n) 
        {
            if (data.ContainsKey(key))
            {
                data[key] += n;
            }
            else
            {
                data.Add(key, n);
            }

            return this;
        }

        public NeedsDictionary Remove(string key) 
        {
            RemoveN(key, 1);
            return this;
        }

        public NeedsDictionary RemoveN(string key, int n) 
        {
            if (data.ContainsKey(key))
            {
                data[key] -= n;

                if (data[key] <= 0)
                {
                    data.Remove(key);
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

        public IEnumerator GetEnumerator()
        {
            return data.Keys.GetEnumerator();
        }
            
        public override string ToString() 
        {
            string ret = "Needs: [\n";
            foreach (string key in this)
            {
                ret += "Key: " + key + ", value: " + data[key] + "\n";
            }

            return ret + "]";
        }
    }
}