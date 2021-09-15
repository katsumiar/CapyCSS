using CapybaraVS.Script;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapyCSS.Script.Lib
{
    public class FlowLib
    {
        public const string LIB_FLOW_NAME = "Flow";
        public const string LIB_Fx_NAME = "f(x)";
        public const string LIB_OPERATION_NAME = "Comparison";
        public const string LIB_FUNC_NAME = LIB_Fx_NAME + ".Func";
        public const string LIB_ACTION_NAME = LIB_Fx_NAME + ".Action";

        [ScriptMethod(LIB_OPERATION_NAME, "==")]
        public static bool Eq(IComparable a, IComparable b)
        {
            return a.CompareTo(b) == 0;
        }

        //------------------------------------------------------------------
        [ScriptMethod(LIB_OPERATION_NAME, ">")]
        public static bool Gt(IComparable a, IComparable b)
        {
            return a.CompareTo(b) > 0;
        }

        //------------------------------------------------------------------
        [ScriptMethod(LIB_OPERATION_NAME, ">=")]
        public static bool Ge(IComparable a, IComparable b)
        {
            return a.CompareTo(b) >= 0;
        }

        //------------------------------------------------------------------
        [ScriptMethod(LIB_OPERATION_NAME, "<")]
        public static bool Lt(IComparable a, IComparable b)
        {
            return a.CompareTo(b) < 0;
        }

        //------------------------------------------------------------------
        [ScriptMethod(LIB_OPERATION_NAME, "<=")]
        public static bool Le(IComparable a, IComparable b)
        {
            return a.CompareTo(b) <= 0;
        }

        //------------------------------------------------------------------
        [ScriptMethod(LIB_OPERATION_NAME)]
        public static bool IsNullable<T>(Nullable<T> target) where T : struct
        {
            if (target.HasValue)
            {
                return true;
            }
            return false;
        }

        //------------------------------------------------------------------
        [ScriptMethod(LIB_FUNC_NAME)]
        public static TResult Invoke<TResult>(Func<TResult> func)
        {
            return func();
        }

        //------------------------------------------------------------------
        [ScriptMethod(LIB_FUNC_NAME)]
        public static TResult Invoke<T, TResult>(T arg, Func<T, TResult> func)
        {
            return func(arg);
        }

        //------------------------------------------------------------------
        [ScriptMethod(LIB_FUNC_NAME)]
        public static TResult Invoke<T1, T2, TResult>(T1 arg1, T2 arg2, Func<T1, T2, TResult> func)
        {
            return func(arg1, arg2);
        }

        //------------------------------------------------------------------
        [ScriptMethod(LIB_ACTION_NAME)]
        public static void Invoke(Action action)
        {
            action?.Invoke();
        }

        //------------------------------------------------------------------
        [ScriptMethod(LIB_ACTION_NAME)]
        public static void Invoke<T>(T arg, Action<T> action)
        {
            action?.Invoke(arg);
        }

        //------------------------------------------------------------------
        [ScriptMethod(LIB_ACTION_NAME)]
        public static void Invoke<T1, T2>(T1 arg1, T2 arg2, Action<T1, T2> action)
        {
            action?.Invoke(arg1, arg2);
        }

        //------------------------------------------------------------------
        [ScriptMethod(LIB_ACTION_NAME)]
        public static void IsNullable<T>(Nullable<T> target, Action<T> action) where T : struct
        {
            if (target.HasValue)
            {
                action?.Invoke(target.Value);
            }
        }

        //------------------------------------------------------------------
        [ScriptMethod(LIB_FLOW_NAME)]
        public static T If_Value<T>(bool sample, T trueValue, T falseValue)
        {
            if (sample)
            {
                return trueValue;
            }
            else
            {
                return falseValue;
            }
        }

        //------------------------------------------------------------------
        [ScriptMethod(LIB_FLOW_NAME)]
        public static Nullable<T> If_ResultNullable<T>(bool sample, T value) where T : struct
        {
            if (sample)
            {
                return value;
            }
            else
            {
                return null;
            }
        }

        //------------------------------------------------------------------
        [ScriptMethod(LIB_FLOW_NAME)]
        public static void If(bool sample, Action trueAction, Action falseAction)
        {
            if (sample)
            {
                trueAction?.Invoke();
            }
            else
            {
                falseAction?.Invoke();
            }
        }

        //------------------------------------------------------------------
        [ScriptMethod(LIB_FLOW_NAME)]
        public static T If<T>(bool sample, Func<T> trueFunction, Func<T> falseFunction) where T : class
        {
            if (sample)
            {
                if (trueFunction is null)
                    return null;

                return trueFunction();
            }
            else
            {
                if (trueFunction is null)
                    return null;

                return falseFunction();
            }
        }

        //------------------------------------------------------------------
        [ScriptMethod(LIB_FLOW_NAME)]
        public static Nullable<T> If_ResultNullable<T>(bool sample, Func<T> trueFunction, Func<T> falseFunction) where T : struct
        {
            if (sample)
            {
                if (trueFunction is null)
                    return null;

                return trueFunction();
            }
            else
            {
                if (trueFunction is null)
                    return null;

                return falseFunction();
            }
        }

        //------------------------------------------------------------------
        [ScriptMethod(LIB_FLOW_NAME)]
        public static void If_Nullable<T>(Nullable<T> target, Action<T> trueAction, Action falseAction) where T : struct
        {
            if (target.HasValue)
            {
                trueAction?.Invoke(target.Value);
            }
            else
            {
                falseAction?.Invoke();
            }
        }

        //------------------------------------------------------------------
        [ScriptMethod(LIB_FLOW_NAME)]
        public static void For(int begin, int end, int step, Action<int> action)
        {
            if (action is null)
                return;
            for (int i = begin; i < end; i += step)
            {
                action(i);
            }
        }

        //------------------------------------------------------------------
        [ScriptMethod(LIB_FLOW_NAME)]
        public static void DoWhile<T>(Predicate<T> predicate, Func<T> func)
        {
            if (predicate is null || func is null)
                return;
            do { } while (predicate(func()));
        }

        //------------------------------------------------------------------
        [ScriptMethod(LIB_FLOW_NAME)]
        public static void Foreach<T>(IEnumerable<T> sample, Action<T> action)
        {
            if (sample is null || action is null)
                return;
            foreach (var node in sample)
            {
                action(node);
            }
        }

        //------------------------------------------------------------------
        [ScriptMethod(LIB_FLOW_NAME)]
        public static T ForeachReturn<T>(IEnumerable<T> sample, Func<T, T> func, T defalutReturn)
        {
            if (sample is null || func is null)
                return defalutReturn;
            T result = defalutReturn;
            foreach (var node in sample)
            {
                result = func(node);
            }
            return result;
        }
    }
}
