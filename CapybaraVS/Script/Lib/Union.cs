using System;
using System.Collections.Generic;
using System.Text;

namespace CapybaraVS.Script.Lib
{
    class Union
    {
        [ScriptMethod("Union." + "∩", "",
            "RS=>Union_And"//"集合 A と集合 B の積集合を求めます。"
            )]
        public static List<string> And(List<string> A, List<string> B)
        {
            List<string> temp = new List<string>();
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
        [ScriptMethod("Union." + "∪", "",
            "RS=>Union_Or"//"集合 A と集合 B の和集合を求めます。"
            )]
        public static List<string> Or(List<string> A, List<string> B)
        {
            List<string> temp = new List<string>(A);
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
        [ScriptMethod("Union." + "+", "",
            "RS=>Union_Add"//"集合 A に集合 B の要素を追加した集合を求めます。"
            )]
        public static List<string> Add(List<string> A, List<string> B)
        {
            List<string> temp = new List<string>(A);
            temp.AddRange(B);
            return temp;
        }

        //------------------------------------------------------------------
        [ScriptMethod("Union." + "-", "",
            "RS=>Union_Sub"//"集合 A から集合 B の要素を削除した集合を求めます。"
            )]
        public static List<string> Sub(List<string> A, List<string> B)
        {
            List<string> temp = new List<string>(A);
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
        [ScriptMethod("Union." + "=", "",
            "RS=>Union_IfEq"//"集合 A と集合 B が同じ集合かを判定します。"
            )]
        public static bool IfEq(List<string> A, List<string> B)
        {
            if (IfSubset(A, B))
                return A.Count == B.Count;
            return false;
        }

        //------------------------------------------------------------------
        [ScriptMethod("Union." + "!=", "",
            "RS=>Union_IfNotEq"//"集合 A と集合 B が同じ集合ではないことを判定します。"
            )]
        public static bool IfNotEq(List<string> A, List<string> B)
        {
            return !IfEq(A, B);
        }

        //------------------------------------------------------------------
        [ScriptMethod("Union." + "⊆", "",
            "RS=>Union_IfSubset"//"集合 B が集合 A の部分集合かを判定します。"
            )]
        public static bool IfSubset(List<string> A, List<string> B)
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
        [ScriptMethod("Union." + "!⊆", "",
            "RS=>Union_IfNotSubset"//"集合 B が集合 A の部分集合ではないことを判定します。"
            )]
        public static bool IfNotSubset(List<string> A, List<string> B)
        {
            return !IfSubset(A, B);
        }

        //------------------------------------------------------------------
        [ScriptMethod("Union." + "⊂", "",
            "RS=>Union_IfTrueSubset"//"集合 B が集合 A の真部分集合かを判定します。"
            )]
        public static bool IfTrueSubset(List<string> A, List<string> B)
        {
            if (IfSubset(A, B))
                return !IfNotEq(A, B);
            return false;
        }

        //------------------------------------------------------------------
        [ScriptMethod("Union." + "!⊂", "",
            "RS=>Union_IfNotTrueSubset"//"集合 B が集合 A の真部分集合ではないことを判定します。"
            )]
        public static bool IfNotTrueSubset(List<string> A, List<string> B)
        {
            return !IfTrueSubset(A, B);
        }

        //------------------------------------------------------------------
        [ScriptMethod("Union." + "|A∩B|", "",
            "RS=>Union_AndCount"//"積集合の数を求めます。"
            )]
        public static int AndCount(List<string> A, List<string> B)
        {
            return And(A, B).Count;
        }

        //------------------------------------------------------------------
        [ScriptMethod("Union." + "|A∪B|", "",
            "RS=>Union_OrCount"//"和集合の数を求めます。"
            )]
        public static int OrCount(List<string> A, List<string> B)
        {
            return Or(A, B).Count;
        }

        //------------------------------------------------------------------
        [ScriptMethod("Union." + nameof(Jaccard), "",
            "RS=>Union_Jaccard"//"Jaccard 係数を求めます。"
            )]
        public static double Jaccard(List<string> A, List<string> B)
        {
            return (double)And(A, B).Count / Or(A, B).Count;
        }

        //------------------------------------------------------------------
        [ScriptMethod("Union." + nameof(Dice), "",
            "RS=>Union_Dice"//"Dice 係数を求めます。"
            )]
        public static double Dice(List<string> A, List<string> B)
        {
            return (double)(And(A, B).Count * 2) / (A.Count + B.Count);
        }

        //------------------------------------------------------------------
        [ScriptMethod("Union." + nameof(Simpson), "",
            "RS=>Union_Simpson"//"Simpson 係数を求めます。"
            )]
        public static double Simpson(List<string> A, List<string> B)
        {
            return (double)And(A, B).Count / Math.Min(A.Count, B.Count);
        }
    }
}
