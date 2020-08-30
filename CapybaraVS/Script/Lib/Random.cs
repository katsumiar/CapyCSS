using System;
using System.Collections.Generic;
using System.Text;

namespace CapybaraVS.Script.Lib
{
    class RandomLib
    {
        //------------------------------------------------------------------
        [ScriptMethod("Random" + "." + "* Random Sign", "",
            "RS=>RandomLib_PMRand"//"値をランダムで +/- の何れかで参照します。"
            )]
        public static double PMRand(double rnd)
        {
            if (FuncAssetSub.random.Next(0, 2) == 1)
            {
                return rnd * -1;
            }
            return rnd;
        }

        //------------------------------------------------------------------
        [ScriptMethod("Random" + "." + "±Random", "",
            "RS=>RandomLib_RandPMRand"//"<min> から <max> のランダムな値をランダムで +/- の何れかで参照します。"
            )]
        public static double RandPMRand(int min, int max)
        {
            double rnd = FuncAssetSub.random.Next(min, max + 1);
            if (FuncAssetSub.random.Next(0, 2) == 1)
            {
                return rnd * -1;
            }
            return rnd;
        }

        //------------------------------------------------------------------
        [ScriptMethod("Random" + "." + "Random Double", "",
            "RS=>RandomLib_RandomDouble"//"0 から 1.0 の間の乱数を参照します。"
            )]
        public static double RandomDouble()
        {
            return FuncAssetSub.random.NextDouble();
        }

        //------------------------------------------------------------------
        [ScriptMethod("Random" + "." + "Random Mul Double", "" ,
            "RS=>RandomLib_RandomMulDouble"//"0 から value の間の乱数を参照します。"
            )]
        public static double RandomMulDouble(double value)
        {
            return FuncAssetSub.random.NextDouble() * value;
        }

        //------------------------------------------------------------------
        [ScriptMethod("Random" + "." + "±Random Double", "",
            "RS=>RandomLib_PmRandomDouble"//"0 から 1.0 の間の乱数をランダムで +/- の何れかで参照します。"
            )]
        public static double PmRandomDouble()
        {
            return PMRand(FuncAssetSub.random.NextDouble());
        }

        //------------------------------------------------------------------
        [ScriptMethod("Random" + "." + "±Random Mul Double", "",
            "RS=>RandomLib_PmRandomMulDouble"//"0 から value の間の乱数をランダムで +/- の何れかで参照します。"
            )]
        public static double PmRandomMulDouble(double value)
        {
            return PMRand(FuncAssetSub.random.NextDouble()) * value;
        }

        //------------------------------------------------------------------
        [ScriptMethod("Random" + "." + "Probability True", "",
            "RS=>RandomLib_ProbabilityTrue"//"100 * <probability> の百分率で True を返します。"
            )]
        public static bool ProbabilityTrue(double probability)
        {
            return (RandomDouble() * 100) < 100 * probability;
        }
    }
}
