using CapyCSSattribute;
using System;
using System.Collections.Generic;
using System.Text;

namespace CapyCSS.Script.Lib
{
    [ScriptClass]
    public static class RandomLib
    {
        private const string LIB_NAME = "Random";

        //------------------------------------------------------------------
        [ScriptMethod(LIB_NAME, "* Random Sign")]
        public static double PMRand(double rnd)
        {
            if (FuncAssetSub.random.Next(0, 2) == 1)
            {
                return rnd * -1;
            }
            return rnd;
        }

        //------------------------------------------------------------------
        [ScriptMethod(LIB_NAME, "±Random")]
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
        [ScriptMethod(LIB_NAME, "Random Double")]
        public static double RandomDouble()
        {
            return FuncAssetSub.random.NextDouble();
        }

        //------------------------------------------------------------------
        [ScriptMethod(LIB_NAME, "Random Mul Double")]
        public static double RandomMulDouble(double value)
        {
            return FuncAssetSub.random.NextDouble() * value;
        }

        //------------------------------------------------------------------
        [ScriptMethod(LIB_NAME, "±Random Double")]
        public static double PmRandomDouble()
        {
            return PMRand(FuncAssetSub.random.NextDouble());
        }

        //------------------------------------------------------------------
        [ScriptMethod(LIB_NAME, "±Random Mul Double")]
        public static double PmRandomMulDouble(double value)
        {
            return PMRand(FuncAssetSub.random.NextDouble()) * value;
        }

        //------------------------------------------------------------------
        [ScriptMethod(LIB_NAME, "Probability True")]
        public static bool ProbabilityTrue(double probability)
        {
            return (RandomDouble() * 100) < 100 * probability;
        }
    }
}
