using System;
using System.Collections.Generic;
using System.Text;

namespace CapybaraVS.Script.Lib
{
    public class MathLib
    {
        [ScriptMethod("Math.Literal." + nameof(PI), "", "RS=>MathLib_PI")]
        public static double PI()
        {
            return Math.PI;
        }

        //------------------------------------------------------------------
        [ScriptMethod("Math.Literal." + nameof(E), "", "RS=>MathLib_E")]
        public static double E()
        {
            return Math.E;
        }

        //------------------------------------------------------------------

        [ScriptMethod("Math.Trigonometric." + nameof(Sin), "", "RS=>MathLib_Sin")]
        public static double Sin(double radian)
        {
            return Math.Sin(radian);
        }

        //------------------------------------------------------------------

        [ScriptMethod("Math.Trigonometric." + nameof(Cos), "", "RS=>MathLib_Cos")]
        public static double Cos(double radian)
        {
            return Math.Cos(radian);
        }

        //------------------------------------------------------------------

        [ScriptMethod("Math.Trigonometric." + nameof(Tan), "", "RS=>MathLib_Tan")]
        public static double Tan(double radian)
        {
            return Math.Tan(radian);
        }

        //------------------------------------------------------------------

        [ScriptMethod("Math.Trigonometric." + nameof(Asin), "", "RS=>MathLib_Asin")]
        public static double Asin(double radian)
        {
            return Math.Asin(radian);
        }

        //------------------------------------------------------------------

        [ScriptMethod("Math.Trigonometric." + nameof(Acos), "", "RS=>MathLib_Acos")]
        public static double Acos(double radian)
        {
            return Math.Acos(radian);
        }

        //------------------------------------------------------------------

        [ScriptMethod("Math.Trigonometric." + nameof(Atan), "", "RS=>MathLib_Atan")]
        public static double Atan(double radian)
        {
            return Math.Atan(radian);
        }

        //------------------------------------------------------------------

        [ScriptMethod("Math.Trigonometric." + nameof(Atan2), "", "RS=>MathLib_Atan2")]
        public static double Atan2(double y, double x)
        {
            return Math.Atan2(y, x);
        }

        //------------------------------------------------------------------

        [ScriptMethod("Math.Trigonometric." + nameof(Sinh), "", "RS=>MathLib_Sinh")]
        public static double Sinh(double radian)
        {
            return Math.Sinh(radian);
        }

        //------------------------------------------------------------------

        [ScriptMethod("Math.Trigonometric." + nameof(Cosh), "", "RS=>MathLib_Cosh")]
        public static double Cosh(double radian)
        {
            return Math.Cosh(radian);
        }

        //------------------------------------------------------------------

        [ScriptMethod("Math.Trigonometric." + nameof(Tanh), "", "RS=>MathLib_Tanh")]
        public static double Tanh(double radian)
        {
            return Math.Tanh(radian);
        }

        //------------------------------------------------------------------
        [ScriptMethod("Math." + nameof(Sqrt), "", "RS=>MathLib_Sqrt")]
        public static double Sqrt(double value)
        {
            return Math.Sqrt(value);
        }

        //------------------------------------------------------------------
        [ScriptMethod("Math." + nameof(Sign), "", "RS=>MathLib_Sign")]
        public static int Sign(double value)
        {
            return Math.Sign(value);
        }

        //------------------------------------------------------------------
        [ScriptMethod("Math." + nameof(Round), "", "RS=>MathLib_Round")]
        public static double Round(double value)
        {
            return Math.Round(value);
        }

        //------------------------------------------------------------------
        [ScriptMethod("Math." + nameof(Ceiling), "", "RS=>MathLib_Ceiling")]
        public static double Ceiling(double value)
        {
            return Math.Ceiling(value);
        }

        //------------------------------------------------------------------
        [ScriptMethod("Math." + nameof(Floor), "", "RS=>MathLib_Floor")]
        public static double Floor(double value)
        {
            return Math.Floor(value);
        }

        //------------------------------------------------------------------
        [ScriptMethod("Math." + nameof(Truncate), "", "RS=>MathLib_Truncate")]
        public static double Truncate(double value)
        {
            return Math.Truncate(value);
        }

        //------------------------------------------------------------------
        [ScriptMethod("Math." + nameof(Max), "", "RS=>MathLib_Max")]
        public static double Max(double value1, double value2)
        {
            return Math.Max(value1, value2);
        }

        //------------------------------------------------------------------
        [ScriptMethod("Math." + nameof(Min), "", "RS=>MathLib_Min")]
        public static double Min(double value1, double value2)
        {
            return Math.Min(value1, value2);
        }

        //------------------------------------------------------------------
        [ScriptMethod("Math." + nameof(ToRadian), "", "RS=>MathLib_ToRadian")]
        public static double ToRadian(double angle)
        {
            return angle * Math.PI / 180;
        }

        //------------------------------------------------------------------
        [ScriptMethod("Math." + nameof(ToAngle), "", "RS=>MathLib_ToAngle")]
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
        [ScriptMethod("Math." + "n!", "", "RS=>MathLib_Factorial")]
        public static int Factorial(int n)
        {
            return Permutation(n, n);
        }

        //------------------------------------------------------------------
        [ScriptMethod("Math." + "nPr", "", "RS=>MathLib_Permutation")]
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
        [ScriptMethod("Math." + "nCr", "", "RS=>MathLib_Combination")]
        public static int Combination(int n, int r)
        {
            return Permutation(n, r) / Factorial(r);
        }
    }
}
