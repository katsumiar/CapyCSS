using CapybaraVS.Script;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapybaraVS
{
    /// <summary>
    /// イージング関数
    /// </summary>
    class EasingFunction
    {
        /// <summary>
        /// イージング関数
        /// </summary>
        [ScriptMethod("Easing" + "." + nameof(Linear))]
        public static double Linear(double t)
        {
            return t / 1.0;
        }

        /// <summary>
        /// イージング関数
        /// </summary>
        public static double OutExp(double t, double totaltime, double max, double min)
        {
            max -= min;
            return t == totaltime ? max + min : max * (-Math.Pow(2, -10 * t / totaltime) + 1) + min;
        }
        [ScriptMethod("Easing" + "." + nameof(OutExp))]
        public static double OutExp(double t)
        {
            return 1 - Math.Pow(2, -10 * t);
        }

        /// <summary>
        /// イージング関数
        /// </summary>
        public static double InOutSine(double t, double totaltime, double max, double min)
        {
            max -= min;
            return -max / 2 * (Math.Cos(t * Math.PI / totaltime) - 1) + min;
        }
        [ScriptMethod("Easing" + "." + nameof(InOutSine))]
        public static double InOutSine(double t)
        {
            return -1.0 / 2.0 * (Math.Cos(t * Math.PI) - 1.0);
        }

        /// <summary>
        /// イージング関数
        /// </summary>
        public static double OutCirc(double t, double totaltime, double max, double min)
        {
            max -= min;
            t = t / totaltime - 1;
            return max * Math.Sqrt(1 - t * t) + min;
        }
        [ScriptMethod("Easing" + "." + nameof(OutCirc))]
        public static double OutCirc(double t)
        {
            t = t - 1;
            return Math.Sqrt(1 - t * t);
        }

        /// <summary>
        /// イージング関数
        /// </summary>
        public static double OutCubic(double t, double totaltime, double max, double min)
        {
            max -= min;
            t = t / totaltime - 1;
            return max * (t * t * t + 1) + min;
        }
        [ScriptMethod("Easing" + "." + nameof(OutCubic))]
        public static double OutCubic(double t)
        {
            t = t - 1;
            return t * t * t + 1;
        }

        /// <summary>
        /// イージング関数
        /// </summary>
        public static double OutSine(double t, double totaltime, double max, double min)
        {
            max -= min;
            return max * Math.Sin(t * (Math.PI / 2) / totaltime) + min;
        }
        [ScriptMethod("Easing" + "." + nameof(OutSine))]
        public static double OutSine(double t)
        {
            return Math.Sin(t * (Math.PI / 2.0));
        }

        /// <summary>
        /// イージング関数
        /// </summary>
        public static double InCubic(double t, double totaltime, double max, double min)
        {
            max -= min;
            t /= totaltime;
            return max * t * t * t + min;
        }
        [ScriptMethod("Easing" + "." + nameof(InCubic))]
        public static double InCubic(double t)
        {
            return t * t * t;
        }

        /// <summary>
        /// イージング関数
        /// </summary>
        [ScriptMethod("Easing" + "." + nameof(InQuad))]
        public static double InQuad(double t)
        {
            return t * t;
        }

        /// <summary>
        /// イージング関数
        /// </summary>
        [ScriptMethod("Easing" + "." + nameof(OutQuad))]
        public static double OutQuad(double t)
        {
            return -1.0 * t * (t - 2.0);
        }
    }
}
