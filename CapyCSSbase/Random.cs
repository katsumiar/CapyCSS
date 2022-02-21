using CapyCSSattribute;
using System;
using System.Collections.Generic;
using System.Text;

namespace CapyCSSbase
{
    [ScriptClass]
    public static class RandomLib
    {
        private const string LIB_NAME = "Random";

        private static Random random = new System.Random();

        //------------------------------------------------------------------
        [ScriptMethod(path: LIB_NAME, methodName: "* Random Sign")]
        public static double PMRand(double rnd)
        {
            if (random.Next(0, 2) == 1)
            {
                return rnd * -1;
            }
            return rnd;
        }

        //------------------------------------------------------------------
        [ScriptMethod(path: LIB_NAME, methodName: "±Random")]
        public static double RandPMRand(int min, int max)
        {
            double rnd = random.Next(min, max + 1);
            if (random.Next(0, 2) == 1)
            {
                return rnd * -1;
            }
            return rnd;
        }

        //------------------------------------------------------------------
        [ScriptMethod(path: LIB_NAME, methodName: "Random Double")]
        public static double RandomDouble()
        {
            return random.NextDouble();
        }

        //------------------------------------------------------------------
        [ScriptMethod(path: LIB_NAME, methodName: "Random Mul Double")]
        public static double RandomMulDouble(double value)
        {
            return random.NextDouble() * value;
        }

        //------------------------------------------------------------------
        [ScriptMethod(path: LIB_NAME, methodName: "±Random Double")]
        public static double PmRandomDouble()
        {
            return PMRand(random.NextDouble());
        }

        //------------------------------------------------------------------
        [ScriptMethod(path: LIB_NAME, methodName: "±Random Mul Double")]
        public static double PmRandomMulDouble(double value)
        {
            return PMRand(random.NextDouble()) * value;
        }

        //------------------------------------------------------------------
        [ScriptMethod(path: LIB_NAME, methodName: "Probability True")]
        public static bool ProbabilityTrue(double probability)
        {
            return (RandomDouble() * 100) < 100 * probability;
        }
    }
}
