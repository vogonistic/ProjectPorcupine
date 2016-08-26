﻿using System.Collections.Generic;
using System.Collections;
using System.Linq;

namespace JobUtils
{
    public class NeedsDictionary : IEnumerable
    {
        Dictionary<string, int> data;

        public NeedsDictionary() {
            data = new Dictionary<string, int>();
        }

        public NeedsDictionary(NeedsDictionary other) {
            data = new Dictionary<string, int>(other.data); // TODO make this better
        }

        public int Count {
            get {
                return data.Keys.Count; 
            }
        }

        public NeedsDictionary Add(string key) {
            AddN(key, 1);
            return this;
        }

        public NeedsDictionary AddN(string key, int n) {
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

        public NeedsDictionary Remove(string key) {
            RemoveN(key, 1);
            return this;
        }

        public NeedsDictionary RemoveN(string key, int n) {
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

        public int Value(string key) {
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

        public static NeedsDictionary operator +(NeedsDictionary a, NeedsDictionary b) {
            NeedsDictionary ret = new NeedsDictionary(a);
            foreach (string key in b)
            {
                ret.AddN(key, b.data[key]);
            }
            return ret;
        }

        public static NeedsDictionary operator -(NeedsDictionary a, NeedsDictionary b) {
            NeedsDictionary ret = new NeedsDictionary(a);
            foreach (string key in b)
            {
                ret.RemoveN(key, b.data[key]);
            }
            return ret;
        }

        public override string ToString() {
            string ret = "Needs: [\n";
            foreach (string key in this)
            {
                ret += "Key: " + key + ", value: " + data[key] + "\n";
            }
            return ret + "]";
        }
    }
}

