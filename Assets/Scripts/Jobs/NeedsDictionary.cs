using System.Collections.Generic;
using System.Collections;

namespace JobUtils
{
    public class NeedsDictionary : IEnumerable
    {
        public NeedsDictionary() {}

        public NeedsDictionary(Condition[] other) {}

        public NeedsDictionary(NeedsDictionary other) {}

        public int Count {
            get { 
                return 0; 
            }
        }

        public void Add(Condition c) {}

        public IEnumerator GetEnumerator()
        {
            return null;
        }

        public static NeedsDictionary operator +(NeedsDictionary a, NeedsDictionary b) {
            return a;
        }

        public static NeedsDictionary operator -(NeedsDictionary a, NeedsDictionary b) {
            return a;
        }

        public override string ToString() {
            return "";
        }
    }
}

