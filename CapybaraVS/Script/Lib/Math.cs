using System;
using System.Collections.Generic;
using System.Text;

namespace CapybaraVS.Script.Lib
{
    public class MathLib
    {
        public const string LIB_MATH_NAME = "Math";
        private const string LIB_NAME2 = LIB_MATH_NAME + ".Literal";
        private const string LIB_NAME3 = LIB_MATH_NAME + ".Trigonometric";

        [ScriptMethod(LIB_NAME2)]
        public static double PI()
        {
            return Math.PI;
        }

        //------------------------------------------------------------------
        [ScriptMethod(LIB_NAME2)]
        public static double E()
        {
            return Math.E;
        }

        //------------------------------------------------------------------

        [ScriptMethod(LIB_NAME3)]
        public static double Sin(double radian)
        {
            return Math.Sin(radian);
        }

        //------------------------------------------------------------------

        [ScriptMethod(LIB_NAME3)]
        public static double Cos(double radian)
        {
            return Math.Cos(radian);
        }

        //------------------------------------------------------------------

        [ScriptMethod(LIB_NAME3)]
        public static double Tan(double radian)
        {
            return Math.Tan(radian);
        }

        //------------------------------------------------------------------

        [ScriptMethod(LIB_NAME3)]
        public static double Asin(double radian)
        {
            return Math.Asin(radian);
        }

        //------------------------------------------------------------------

        [ScriptMethod(LIB_NAME3)]
        public static double Acos(double radian)
        {
            return Math.Acos(radian);
        }

        //------------------------------------------------------------------

        [ScriptMethod(LIB_NAME3)]
        public static double Atan(double radian)
        {
            return Math.Atan(radian);
        }

        //------------------------------------------------------------------

        [ScriptMethod(LIB_NAME3)]
        public static double Atan2(double y, double x)
        {
            return Math.Atan2(y, x);
        }

        //------------------------------------------------------------------

        [ScriptMethod(LIB_NAME3)]
        public static double Sinh(double radian)
        {
            return Math.Sinh(radian);
        }

        //------------------------------------------------------------------

        [ScriptMethod(LIB_NAME3)]
        public static double Cosh(double radian)
        {
            return Math.Cosh(radian);
        }

        //------------------------------------------------------------------

        [ScriptMethod(LIB_NAME3)]
        public static double Tanh(double radian)
        {
            return Math.Tanh(radian);
        }

        //------------------------------------------------------------------
        [ScriptMethod(LIB_MATH_NAME)]
        public static double Sqrt(double value)
        {
            return Math.Sqrt(value);
        }

        //------------------------------------------------------------------
        [ScriptMethod(LIB_MATH_NAME)]
        public static int Sign(double value)
        {
            return Math.Sign(value);
        }

        //------------------------------------------------------------------
        [ScriptMethod(LIB_MATH_NAME)]
        public static double Round(double value)
        {
            return Math.Round(value);
        }

        //------------------------------------------------------------------
        [ScriptMethod(LIB_MATH_NAME)]
        public static double Ceiling(double value)
        {
            return Math.Ceiling(value);
        }

        //------------------------------------------------------------------
        [ScriptMethod(LIB_MATH_NAME)]
        public static double Floor(double value)
        {
            return Math.Floor(value);
        }

        //------------------------------------------------------------------
        [ScriptMethod(LIB_MATH_NAME)]
        public static double Truncate(double value)
        {
            return Math.Truncate(value);
        }

        //------------------------------------------------------------------
        [ScriptMethod(LIB_MATH_NAME)]
        public static double Max(double value1, double value2)
        {
            return Math.Max(value1, value2);
        }

        //------------------------------------------------------------------
        [ScriptMethod(LIB_MATH_NAME)]
        public static double Min(double value1, double value2)
        {
            return Math.Min(value1, value2);
        }

        //------------------------------------------------------------------
        [ScriptMethod(LIB_MATH_NAME)]
        public static double ToRadian(double angle)
        {
            return angle * Math.PI / 180;
        }

        //------------------------------------------------------------------
        [ScriptMethod(LIB_MATH_NAME)]
        public static double ToAngle(double radian)
        {
            return radian * 180 / Math.PI;
        }

        //------------------------------------------------------------------
        [ScriptMethod(LIB_MATH_NAME, "1/n")]
        public static double Reciprocal(double value)
        {
            return 1 / value;
        }

        //------------------------------------------------------------------
        [ScriptMethod(LIB_MATH_NAME, "ax+b")]
        public static double DimFunc(double a, double x, double b)
        {
            return a * x + b;
        }

        //------------------------------------------------------------------
        [ScriptMethod(LIB_MATH_NAME, "kx^n")]
        public static double DimFunc2(double k, double x, double n)
        {
            return k * Math.Pow(x, n);
        }

        //------------------------------------------------------------------
        [ScriptMethod(LIB_MATH_NAME, "(m/d)x^n")]
        public static double DimFunc3(double m, double d, double x, double n)
        {
            return (m * Math.Pow(x, n)) / d;
        }

        //------------------------------------------------------------------
        [ScriptMethod(LIB_MATH_NAME, "n!")]
        public static int Factorial(int n)
        {
            return Permutation(n, n);
        }

        //------------------------------------------------------------------
        [ScriptMethod(LIB_MATH_NAME, "nPr")]
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
        [ScriptMethod(LIB_MATH_NAME, "nCr")]
        public static int Combination(int n, int r)
        {
            return Permutation(n, r) / Factorial(r);
        }
    }
}
