using CapyCSSattribute;
using System;
using System.Collections.Generic;
using System.Text;

namespace CapyCSSbase
{
    [ScriptClass]
    public static class MathLib
    {
        public const string LIB_MATH_NAME = "Math";
        private const string LIB_NAME2 = LIB_MATH_NAME + ".Literal";
        private const string LIB_NAME3 = LIB_MATH_NAME + ".Trigonometric";

        [ScriptMethod(path: LIB_NAME2)]
        public static double PI()
        {
            return Math.PI;
        }

        //------------------------------------------------------------------
        [ScriptMethod(path: LIB_NAME2)]
        public static double E()
        {
            return Math.E;
        }

        //------------------------------------------------------------------
        [ScriptMethod(path: LIB_MATH_NAME)]
        public static double ToRadian(double angle)
        {
            return angle * Math.PI / 180;
        }

        //------------------------------------------------------------------
        [ScriptMethod(path: LIB_MATH_NAME)]
        public static double ToAngle(double radian)
        {
            return radian * 180 / Math.PI;
        }

        //------------------------------------------------------------------
        [ScriptMethod(path: LIB_MATH_NAME, methodName: "1/n")]
        public static double Reciprocal(double value)
        {
            return 1 / value;
        }

        //------------------------------------------------------------------
        [ScriptMethod(path: LIB_MATH_NAME, methodName: "ax+b")]
        public static double DimFunc(double a, double x, double b)
        {
            return a * x + b;
        }

        //------------------------------------------------------------------
        [ScriptMethod(path: LIB_MATH_NAME, methodName: "kx^n")]
        public static double DimFunc2(double k, double x, double n)
        {
            return k * Math.Pow(x, n);
        }

        //------------------------------------------------------------------
        [ScriptMethod(path: LIB_MATH_NAME, methodName: "(m/d)x^n")]
        public static double DimFunc3(double m, double d, double x, double n)
        {
            return (m * Math.Pow(x, n)) / d;
        }

        //------------------------------------------------------------------
        [ScriptMethod(path: LIB_MATH_NAME, methodName: "n!")]
        public static int Factorial(int n)
        {
            return Permutation(n, n);
        }

        //------------------------------------------------------------------
        [ScriptMethod(path: LIB_MATH_NAME, methodName: "nPr")]
        public static int Permutation(int n, int r)
        {
            int ret = 1;
            for (int i = n; i > (n - r); --i)
            {
                ret *= i;
            }
            return ret;
        }

        //------------------------------------------------------------------
        [ScriptMethod(path: LIB_MATH_NAME, methodName: "nCr")]
        public static int Combination(int n, int r)
        {
            return Permutation(n, r) / Factorial(r);
        }
    }
}
