using System;
using System.Collections.Generic;
using System.Text;

namespace CapybaraVS.Script.Lib
{
    public class RandomLib
    {
        //------------------------------------------------------------------
        [ScriptMethod("Random" + "." + "* Random Sign")]
        public static double PMRand(double rnd)
        {
            if (FuncAssetSub.random.Next(0, 2) == 1)
            {
                return rnd * -1;
            }
            return rnd;
        }

        //------------------------------------------------------------------
        [ScriptMethod("Random" + "." + "±Random")]
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
        [ScriptMethod("Random" + "." + "Random Double")]
        public static double RandomDouble()
        {
            return FuncAssetSub.random.NextDouble();
        }

        //------------------------------------------------------------------
        [ScriptMethod("Random" + "." + "Random Mul Double")]
        public static double RandomMulDouble(double value)
        {
            return FuncAssetSub.random.NextDouble() * value;
        }

        //------------------------------------------------------------------
        [ScriptMethod("Random" + "." + "±Random Double")]
        public static double PmRandomDouble()
        {
            return PMRand(FuncAssetSub.random.NextDouble());
        }

        //------------------------------------------------------------------
        [ScriptMethod("Random" + "." + "±Random Mul Double")]
        public static double PmRandomMulDouble(double value)
        {
            return PMRand(FuncAssetSub.random.NextDouble()) * value;
        }

        //------------------------------------------------------------------
        [ScriptMethod("Random" + "." + "Probability True")]
        public static bool ProbabilityTrue(double probability)
        {
            return (RandomDouble() * 100) < 100 * probability;
        }
    }
}
