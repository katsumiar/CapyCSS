using System;
using System.Collections.Generic;
using System.Text;

namespace CapybaraVS.Script.Lib
{
    public class MathLib
    {
        [ScriptMethod("Math.Literal." + nameof(PI))]
        public static double PI()
        {
            return Math.PI;
        }

        //------------------------------------------------------------------
        [ScriptMethod("Math.Literal." + nameof(E))]
        public static double E()
        {
            return Math.E;
        }

        //------------------------------------------------------------------

        [ScriptMethod("Math.Trigonometric." + nameof(Sin))]
        public static double Sin(double radian)
        {
            return Math.Sin(radian);
        }

        //------------------------------------------------------------------

        [ScriptMethod("Math.Trigonometric." + nameof(Cos))]
        public static double Cos(double radian)
        {
            return Math.Cos(radian);
        }

        //------------------------------------------------------------------

        [ScriptMethod("Math.Trigonometric." + nameof(Tan))]
        public static double Tan(double radian)
        {
            return Math.Tan(radian);
        }

        //------------------------------------------------------------------

        [ScriptMethod("Math.Trigonometric." + nameof(Asin))]
        public static double Asin(double radian)
        {
            return Math.Asin(radian);
        }

        //------------------------------------------------------------------

        [ScriptMethod("Math.Trigonometric." + nameof(Acos))]
        public static double Acos(double radian)
        {
            return Math.Acos(radian);
        }

        //------------------------------------------------------------------

        [ScriptMethod("Math.Trigonometric." + nameof(Atan))]
        public static double Atan(double radian)
        {
            return Math.Atan(radian);
        }

        //------------------------------------------------------------------

        [ScriptMethod("Math.Trigonometric." + nameof(Atan2))]
        public static double Atan2(double y, double x)
        {
            return Math.Atan2(y, x);
        }

        //------------------------------------------------------------------

        [ScriptMethod("Math.Trigonometric." + nameof(Sinh))]
        public static double Sinh(double radian)
        {
            return Math.Sinh(radian);
        }

        //------------------------------------------------------------------

        [ScriptMethod("Math.Trigonometric." + nameof(Cosh))]
        public static double Cosh(double radian)
        {
            return Math.Cosh(radian);
        }

        //------------------------------------------------------------------

        [ScriptMethod("Math.Trigonometric." + nameof(Tanh))]
        public static double Tanh(double radian)
        {
            return Math.Tanh(radian);
        }

        //------------------------------------------------------------------
        [ScriptMethod("Math." + nameof(Sqrt))]
        public static double Sqrt(double value)
        {
            return Math.Sqrt(value);
        }

        //------------------------------------------------------------------
        [ScriptMethod("Math." + nameof(Sign))]
        public static int Sign(double value)
        {
            return Math.Sign(value);
        }

        //------------------------------------------------------------------
        [ScriptMethod("Math." + nameof(Round))]
        public static double Round(double value)
        {
            return Math.Round(value);
        }

        //------------------------------------------------------------------
        [ScriptMethod("Math." + nameof(Ceiling))]
        public static double Ceiling(double value)
        {
            return Math.Ceiling(value);
        }

        //------------------------------------------------------------------
        [ScriptMethod("Math." + nameof(Floor))]
        public static double Floor(double value)
        {
            return Math.Floor(value);
        }

        //------------------------------------------------------------------
        [ScriptMethod("Math." + nameof(Truncate))]
        public static double Truncate(double value)
        {
            return Math.Truncate(value);
        }

        //------------------------------------------------------------------
        [ScriptMethod("Math." + nameof(Max))]
        public static double Max(double value1, double value2)
        {
            return Math.Max(value1, value2);
        }

        //------------------------------------------------------------------
        [ScriptMethod("Math." + nameof(Min))]
        public static double Min(double value1, double value2)
        {
            return Math.Min(value1, value2);
        }

        //------------------------------------------------------------------
        [ScriptMethod("Math." + nameof(ToRadian))]
        public static double ToRadian(double angle)
        {
            return angle * Math.PI / 180;
        }

        //------------------------------------------------------------------
        [ScriptMethod("Math." + nameof(ToAngle))]
        public static double ToAngle(double radian)
        {
            return radian * 180 / Math.PI;
        }

        //------------------------------------------------------------------
        [ScriptMethod("Math." + "1/n")]
        public static double Reciprocal(double value)
        {
            return 1 / value;
        }

        //------------------------------------------------------------------
        [ScriptMethod("Math." + "ax+b")]
        public static double DimFunc(double a, double x, double b)
        {
            return a * x + b;
        }

        //------------------------------------------------------------------
        [ScriptMethod("Math." + "kx^n")]
        public static double DimFunc2(double k, double x, double n)
        {
            return k * Math.Pow(x, n);
        }

        //------------------------------------------------------------------
        [ScriptMethod("Math." + "(m/d)x^n")]
        public static double DimFunc3(double m, double d, double x, double n)
        {
            return (m * Math.Pow(x, n)) / d;
        }

        //------------------------------------------------------------------
        [ScriptMethod("Math." + "n!")]
        public static int Factorial(int n)
        {
            return Permutation(n, n);
        }

        //------------------------------------------------------------------
        [ScriptMethod("Math." + "nPr")]
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
        [ScriptMethod("Math." + "nCr")]
        public static int Combination(int n, int r)
        {
            return Permutation(n, r) / Factorial(r);
        }
    }
}
