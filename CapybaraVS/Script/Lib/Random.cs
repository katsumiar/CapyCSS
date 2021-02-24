using System;
using System.Collections.Generic;
using System.Text;

namespace CapybaraVS.Script.Lib
{
    public class RandomLib
    {
        //------------------------------------------------------------------
        [ScriptMethod("Random" + "." + "* Random Sign", "", "RS=>RandomLib_PMRand")]
        public static double PMRand(double rnd)
        {
            if (FuncAssetSub.random.Next(0, 2) == 1)
            {
                return rnd * -1;
            }
            return rnd;
        }

        //------------------------------------------------------------------
        [ScriptMethod("Random" + "." + "±Random", "", "RS=>RandomLib_RandPMRand")]
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
        [ScriptMethod("Random" + "." + "Random Double", "", "RS=>RandomLib_RandomDouble")]
        public static double RandomDouble()
        {
            return FuncAssetSub.random.NextDouble();
        }

        //------------------------------------------------------------------
        [ScriptMethod("Random" + "." + "Random Mul Double", "" , "RS=>RandomLib_RandomMulDouble")]
        public static double RandomMulDouble(double value)
        {
            return FuncAssetSub.random.NextDouble() * value;
        }

        //------------------------------------------------------------------
        [ScriptMethod("Random" + "." + "±Random Double", "", "RS=>RandomLib_PmRandomDouble")]
        public static double PmRandomDouble()
        {
            return PMRand(FuncAssetSub.random.NextDouble());
        }

        //------------------------------------------------------------------
        [ScriptMethod("Random" + "." + "±Random Mul Double", "", "RS=>RandomLib_PmRandomMulDouble")]
        public static double PmRandomMulDouble(double value)
        {
            return PMRand(FuncAssetSub.random.NextDouble()) * value;
        }

        //------------------------------------------------------------------
        [ScriptMethod("Random" + "." + "Probability True", "", "RS=>RandomLib_ProbabilityTrue")]
        public static bool ProbabilityTrue(double probability)
        {
            return (RandomDouble() * 100) < 100 * probability;
        }
    }
}
