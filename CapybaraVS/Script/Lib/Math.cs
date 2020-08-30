using MathNet.Numerics;
using System;
using System.Collections.Generic;
using System.Text;

namespace CapybaraVS.Script.Lib
{
    class MathLib
    {
        [ScriptMethod("Math.Literal." + nameof(PI), "",
            "RS=>MathLib_PI"//"円周率を参照します。"
            )]
        public static double PI()
        {
            return Math.PI;
        }

        //------------------------------------------------------------------
        [ScriptMethod("Math.Literal." + nameof(E), "",
            "RS=>MathLib_E"//"ネイピア数（自然対数の底）を参照します。"
            )]
        public static double E()
        {
            return Math.E;
        }

        //------------------------------------------------------------------

        [ScriptMethod("Math.Trigonometric." + nameof(Sin), "",
            "RS=>MathLib_Sin"//"正弦を求めます。"
            )]
        public static double Sin(double radian)
        {
            return Math.Sin(radian);
        }

        //------------------------------------------------------------------

        [ScriptMethod("Math.Trigonometric." + nameof(Cos), "",
            "RS=>MathLib_Cos"//"余弦を求めます。"
            )]
        public static double Cos(double radian)
        {
            return Math.Cos(radian);
        }

        //------------------------------------------------------------------

        [ScriptMethod("Math.Trigonometric." + nameof(Tan), "",
            "RS=>MathLib_Tan"//"正接を求めます。"
            )]
        public static double Tan(double radian)
        {
            return Math.Tan(radian);
        }

        //------------------------------------------------------------------

        [ScriptMethod("Math.Trigonometric." + nameof(Asin), "",
            "RS=>MathLib_Asin"//"逆正弦を求めます。"
            )]
        public static double Asin(double radian)
        {
            return Math.Asin(radian);
        }

        //------------------------------------------------------------------

        [ScriptMethod("Math.Trigonometric." + nameof(Acos), "",
            "RS=>MathLib_Acos"//"逆余弦を求めます。"
            )]
        public static double Acos(double radian)
        {
            return Math.Acos(radian);
        }

        //------------------------------------------------------------------

        [ScriptMethod("Math.Trigonometric." + nameof(Atan), "",
            "RS=>MathLib_Atan"//"逆正接を求めます。"
            )]
        public static double Atan(double radian)
        {
            return Math.Atan(radian);
        }

        //------------------------------------------------------------------

        [ScriptMethod("Math.Trigonometric." + nameof(Atan2), "",
            "RS=>MathLib_Atan2"//"<y> と <x> からラジアンを求めます。"
            )]
        public static double Atan2(double y, double x)
        {
            return Math.Atan2(y, x);
        }

        //------------------------------------------------------------------

        [ScriptMethod("Math.Trigonometric." + nameof(Sinh), "",
            "RS=>MathLib_Sinh"//"双曲線正弦を求めます。"
            )]
        public static double Sinh(double radian)
        {
            return Math.Sinh(radian);
        }

        //------------------------------------------------------------------

        [ScriptMethod("Math.Trigonometric." + nameof(Cosh), "",
            "RS=>MathLib_Cosh"//"双曲線余弦を求めます。"
            )]
        public static double Cosh(double radian)
        {
            return Math.Cosh(radian);
        }

        //------------------------------------------------------------------

        [ScriptMethod("Math.Trigonometric." + nameof(Tanh), "",
            "RS=>MathLib_Tanh"//"双曲線正接を求めます。"
            )]
        public static double Tanh(double radian)
        {
            return Math.Tanh(radian);
        }

        //------------------------------------------------------------------
        [ScriptMethod("Math." + nameof(Sqrt), "",
            "RS=>MathLib_Sqrt"//"平方根を求めます。"
            )]
        public static double Sqrt(double value)
        {
            return Math.Sqrt(value);
        }

        //------------------------------------------------------------------
        [ScriptMethod("Math." + nameof(Sign), "",
            "RS=>MathLib_Sign"//"値の負号が正か負かを表す +/- 1 を求めます。"
            )]
        public static int Sign(double value)
        {
            return Math.Sign(value);
        }

        //------------------------------------------------------------------
        [ScriptMethod("Math." + nameof(Round), "",
            "RS=>MathLib_Round"//"値の四捨五入を求めます。"
            )]
        public static double Round(double value)
        {
            return Math.Round(value);
        }

        //------------------------------------------------------------------
        [ScriptMethod("Math." + nameof(Ceiling), "",
            "RS=>MathLib_Ceiling"//"値の切り上げを求めます。"
            )]
        public static double Ceiling(double value)
        {
            return Math.Ceiling(value);
        }

        //------------------------------------------------------------------
        [ScriptMethod("Math." + nameof(Floor), "",
            "RS=>MathLib_Floor"//"値の切り下げを求めます。"
            )]
        public static double Floor(double value)
        {
            return Math.Floor(value);
        }

        //------------------------------------------------------------------
        [ScriptMethod("Math." + nameof(Truncate), "",
            "RS=>MathLib_Truncate"//"値を 0 に近い方に丸めた値を求めます。"
            )]
        public static double Truncate(double value)
        {
            return Math.Truncate(value);
        }

        //------------------------------------------------------------------
        [ScriptMethod("Math." + nameof(Max), "",
            "RS=>MathLib_Max"//"最大値を求めます。"
            )]
        public static double Max(double value1, double value2)
        {
            return Math.Max(value1, value2);
        }

        //------------------------------------------------------------------
        [ScriptMethod("Math." + nameof(Min), "",
            "RS=>MathLib_Min"//"最小値を求めます。"
            )]
        public static double Min(double value1, double value2)
        {
            return Math.Min(value1, value2);
        }

        //------------------------------------------------------------------
        [ScriptMethod("Math." + nameof(ToRadian), "",
            "RS=>MathLib_ToRadian"//"角度からラジアンを求めます。"
            )]
        public static double ToRadian(double angle)
        {
            return angle * Math.PI / 180;
        }

        //------------------------------------------------------------------
        [ScriptMethod("Math." + nameof(ToAngle), "",
            "RS=>MathLib_ToAngle"//"ラジアンから角度を求めます。"
            )]
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
        public static double DimFunc(double x, double a, double b)
        {
            return a * x + b;
        }

        //------------------------------------------------------------------
        [ScriptMethod("Math." + "kx^n")]
        public static double DimFunc2(double x, double k, double n)
        {
            return k * Math.Pow(x, n);
        }

        //------------------------------------------------------------------
        [ScriptMethod("Math." + "(m/d)x^n")]
        public static double DimFunc3(double x, double m, double d, double n)
        {
            return (m * Math.Pow(x, n)) / d;
        }

        //------------------------------------------------------------------
        [ScriptMethod("Math." + "Polynomial", "",
            "RS=>MathLib_Polynomial"//"多項式の解を求めます。"
            )]
        public static double Polynomial(double x, List<double> list)
        {
            List<double> order = new List<double>();
            for (int i = list.Count - 1; i >= 0; --i)
                order.Add(Math.Pow(x, i));

            Vector listVector = new Vector(list);
            Vector orderVector = new Vector(order);

            return listVector.mul(orderVector);
        }

        //------------------------------------------------------------------
        [ScriptMethod("Math." + "n!", "",
            "RS=>MathLib_Factorial"//"階乗を求めます。"
            )]
        public static int Factorial(int n)
        {
            return Permutation(n, n);
        }

        //------------------------------------------------------------------
        [ScriptMethod("Math." + "nPr", "",
            "RS=>MathLib_Permutation"//"順列を求めます。"
            )]
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
        [ScriptMethod("Math." + "nCr", "",
            "RS=>MathLib_Combination"//"組み合わせを求めます。"
            )]
        public static int Combination(int n, int r)
        {
            return Permutation(n, r) / Factorial(r);
        }
    }
}
