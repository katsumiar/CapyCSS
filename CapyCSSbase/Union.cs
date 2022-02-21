using CapyCSSattribute;
using System;
using System.Collections.Generic;
using System.Text;

namespace CapyCSSbase
{
    [ScriptClass]
    public static class Union
    {
        private const string LIB_NAME = "Union";

        [ScriptMethod(path: LIB_NAME, methodName: "∩")]
        public static ICollection<string> And(IEnumerable<string> A, IEnumerable<string> B)
        {
            var temp = new List<string>();
            foreach (var a in A)
            {
                foreach (var b in B)
                {
                    if (a == b && !temp.Contains(b))
                    {
                        temp.Add(b);
                    }
                }
            }
            return temp;
        }

        //------------------------------------------------------------------
        [ScriptMethod(path: LIB_NAME, methodName: "∪")]
        public static ICollection<string> Or(IEnumerable<string> A, IEnumerable<string> B)
        {
            var temp = new List<string>(A);
            foreach (var b in B)
            {
                if (!temp.Contains(b))
                {
                    temp.Add(b);
                }
            }
            return temp;
        }

        //------------------------------------------------------------------
        [ScriptMethod(path: LIB_NAME, methodName: "+")]
        public static ICollection<string> Add(IEnumerable<string> A, IEnumerable<string> B)
        {
            var temp = new List<string>(A);
            temp.AddRange(B);
            return temp;
        }

        //------------------------------------------------------------------
        [ScriptMethod(path: LIB_NAME, methodName: "-")]
        public static ICollection<string> Sub(IEnumerable<string> A, IEnumerable<string> B)
        {
            var temp = new List<string>(A);
            foreach (var b in B)
            {
                if (temp.Contains(b))
                {
                    temp.Remove(b);
                }
            }
            return temp;
        }

        //------------------------------------------------------------------
        [ScriptMethod(path: LIB_NAME, methodName: "=")]
        public static bool IfEq(ICollection<string> A, ICollection<string> B)
        {
            if (IfSubset(A, B))
                return A.Count == B.Count;
            return false;
        }

        //------------------------------------------------------------------
        [ScriptMethod(path: LIB_NAME, methodName: "!=")]
        public static bool IfNotEq(ICollection<string> A, ICollection<string> B)
        {
            return !IfEq(A, B);
        }

        //------------------------------------------------------------------
        [ScriptMethod(path: LIB_NAME, methodName: "⊆")]
        public static bool IfSubset(ICollection<string> A, IEnumerable<string> B)
        {
            foreach (var b in B)
            {
                if (!A.Contains(b))
                {
                    return false;
                }
            }
            return true;
        }

        //------------------------------------------------------------------
        [ScriptMethod(path: LIB_NAME, methodName: "!⊆")]
        public static bool IfNotSubset(ICollection<string> A, ICollection<string> B)
        {
            return !IfSubset(A, B);
        }

        //------------------------------------------------------------------
        [ScriptMethod(path: LIB_NAME, methodName: "⊂")]
        public static bool IfTrueSubset(ICollection<string> A, ICollection<string> B)
        {
            if (IfSubset(A, B))
                return !IfNotEq(A, B);
            return false;
        }

        //------------------------------------------------------------------
        [ScriptMethod(path: LIB_NAME, methodName: "!⊂")]
        public static bool IfNotTrueSubset(ICollection<string> A, ICollection<string> B)
        {
            return !IfTrueSubset(A, B);
        }

        //------------------------------------------------------------------
        [ScriptMethod(path: LIB_NAME, methodName: "|A∩B|")]
        public static int AndCount(IEnumerable<string> A, IEnumerable<string> B)
        {
            return And(A, B).Count;
        }

        //------------------------------------------------------------------
        [ScriptMethod(path: LIB_NAME, methodName: "|A∪B|")]
        public static int OrCount(IEnumerable<string> A, IEnumerable<string> B)
        {
            return Or(A, B).Count;
        }

        //------------------------------------------------------------------
        [ScriptMethod(path: LIB_NAME)]
        public static double Jaccard(IEnumerable<string> A, IEnumerable<string> B)
        {
            return (double)And(A, B).Count / Or(A, B).Count;
        }

        //------------------------------------------------------------------
        [ScriptMethod(path: LIB_NAME)]
        public static double Dice(ICollection<string> A, ICollection<string> B)
        {
            return (double)(And(A, B).Count * 2) / (A.Count + B.Count);
        }

        //------------------------------------------------------------------
        [ScriptMethod(path: LIB_NAME)]
        public static double Simpson(ICollection<string> A, ICollection<string> B)
        {
            return (double)And(A, B).Count / Math.Min(A.Count, B.Count);
        }
    }
}
