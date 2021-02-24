using System;
using System.Collections.Generic;
using System.Text;

namespace CapybaraVS.Script.Lib
{
    public class Union
    {
        [ScriptMethod("Union." + "∩", "", "RS=>Union_And")]
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
        [ScriptMethod("Union." + "∪", "", "RS=>Union_Or")]
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
        [ScriptMethod("Union." + "+", "", "RS=>Union_Add")]
        public static ICollection<string> Add(IEnumerable<string> A, IEnumerable<string> B)
        {
            var temp = new List<string>(A);
            temp.AddRange(B);
            return temp;
        }

        //------------------------------------------------------------------
        [ScriptMethod("Union." + "-", "", "RS=>Union_Sub")]
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
        [ScriptMethod("Union." + "=", "", "RS=>Union_IfEq")]
        public static bool IfEq(ICollection<string> A, ICollection<string> B)
        {
            if (IfSubset(A, B))
                return A.Count == B.Count;
            return false;
        }

        //------------------------------------------------------------------
        [ScriptMethod("Union." + "!=", "", "RS=>Union_IfNotEq")]
        public static bool IfNotEq(ICollection<string> A, ICollection<string> B)
        {
            return !IfEq(A, B);
        }

        //------------------------------------------------------------------
        [ScriptMethod("Union." + "⊆", "", "RS=>Union_IfSubset")]
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
        [ScriptMethod("Union." + "!⊆", "", "RS=>Union_IfNotSubset")]
        public static bool IfNotSubset(ICollection<string> A, ICollection<string> B)
        {
            return !IfSubset(A, B);
        }

        //------------------------------------------------------------------
        [ScriptMethod("Union." + "⊂", "", "RS=>Union_IfTrueSubset")]
        public static bool IfTrueSubset(ICollection<string> A, ICollection<string> B)
        {
            if (IfSubset(A, B))
                return !IfNotEq(A, B);
            return false;
        }

        //------------------------------------------------------------------
        [ScriptMethod("Union." + "!⊂", "", "RS=>Union_IfNotTrueSubset")]
        public static bool IfNotTrueSubset(ICollection<string> A, ICollection<string> B)
        {
            return !IfTrueSubset(A, B);
        }

        //------------------------------------------------------------------
        [ScriptMethod("Union." + "|A∩B|", "", "RS=>Union_AndCount")]
        public static int AndCount(IEnumerable<string> A, IEnumerable<string> B)
        {
            return And(A, B).Count;
        }

        //------------------------------------------------------------------
        [ScriptMethod("Union." + "|A∪B|", "", "RS=>Union_OrCount")]
        public static int OrCount(IEnumerable<string> A, IEnumerable<string> B)
        {
            return Or(A, B).Count;
        }

        //------------------------------------------------------------------
        [ScriptMethod("Union." + nameof(Jaccard), "", "RS=>Union_Jaccard")]
        public static double Jaccard(IEnumerable<string> A, IEnumerable<string> B)
        {
            return (double)And(A, B).Count / Or(A, B).Count;
        }

        //------------------------------------------------------------------
        [ScriptMethod("Union." + nameof(Dice), "", "RS=>Union_Dice")]
        public static double Dice(ICollection<string> A, ICollection<string> B)
        {
            return (double)(And(A, B).Count * 2) / (A.Count + B.Count);
        }

        //------------------------------------------------------------------
        [ScriptMethod("Union." + nameof(Simpson), "", "RS=>Union_Simpson")]
        public static double Simpson(ICollection<string> A, ICollection<string> B)
        {
            return (double)And(A, B).Count / Math.Min(A.Count, B.Count);
        }
    }
}
