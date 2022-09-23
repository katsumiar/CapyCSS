using CapyCSSattribute;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapyCSSbase
{
    [ScriptClass]
    public static class FlowLib
    {
        public const string LIB_FLOW_NAME = "Flow";
        public const string LIB_Fx_NAME = "f(x)";
        public const string LIB_LOGICAL_NAME = "Logical";
        public const string LIB_OPERATION_NAME = "Comparison";
        public const string LIB_FUNC_NAME = LIB_Fx_NAME + ".Func";
        public const string LIB_ACTION_NAME = LIB_Fx_NAME + ".Action";
        public const string LIB_NULLABLE_NAME = LIB_FLOW_NAME + ".Nullable";

        //====================================================================================
        /// <summary>
        /// サンプルリストのすべての要素が true のとき、true を返します。
        /// ただし、invert が true のときは、要素の評価を逆にします。
        /// </summary>
        /// <param name="samples">評価対象リスト</param>
        /// <param name="invert">要素の評価を逆にするか？</param>
        /// <returns>すべての要素が true のとき true</returns>
        [ScriptMethod(path: LIB_LOGICAL_NAME)]
        public static bool And(IEnumerable<bool> samples, bool invert)
        {
            if (samples is null || samples.Count() == 0)
                return false;
            bool result = true;
            if (invert)
            {
                foreach (var sample in samples)
                {
                    result = result && !sample;
                }
            }
            else
            {
                foreach (var sample in samples)
                {
                    result = result && sample;
                }
            }
            return result;
        }

        //------------------------------------------------------------------
        /// <summary>
        /// サンプルリストの要素が一つでも true のとき、true を返します。
        /// ただし、invert が true のときは、要素の評価を逆にします。
        /// </summary>
        /// <param name="samples">評価対象リスト</param>
        /// <param name="invert">要素の評価を逆にするか？</param>
        /// <returns>要素が一つでも true のとき true</returns>
        [ScriptMethod(path: LIB_LOGICAL_NAME)]
        public static bool Or(IEnumerable<bool> samples, bool invert)
        {
            if (samples is null || samples.Count() == 0)
                return false;
            bool result = false;
            if (invert)
            {
                foreach (var sample in samples)
                {
                    result = result || !sample;
                }
            }
            else
            {
                foreach (var sample in samples)
                {
                    result = result || sample;
                }
            }
            return result;
        }

        //------------------------------------------------------------------
        /// <summary>
        /// 評価を逆転します。
        /// </summary>
        /// <param name="sample">真偽値</param>
        /// <returns>sampleの逆の真偽値</returns>
        [ScriptMethod(path: LIB_LOGICAL_NAME)]
        public static bool Not(bool sample)
        {
            return !sample;
        }

        //====================================================================================
        [ScriptMethod(path: LIB_OPERATION_NAME, methodName: "==")]
        public static bool Eq(IComparable a, IComparable b)
        {
            return a.CompareTo(b) == 0;
        }

        //------------------------------------------------------------------
        [ScriptMethod(path: LIB_OPERATION_NAME, methodName: "!=")]
        public static bool Ne(IComparable a, IComparable b)
        {
            return a.CompareTo(b) != 0;
        }

        //------------------------------------------------------------------
        [ScriptMethod(path: LIB_OPERATION_NAME, methodName: ">")]
        public static bool Gt(IComparable a, IComparable b)
        {
            return a.CompareTo(b) > 0;
        }

        //------------------------------------------------------------------
        [ScriptMethod(path: LIB_OPERATION_NAME, methodName: ">=")]
        public static bool Ge(IComparable a, IComparable b)
        {
            return a.CompareTo(b) >= 0;
        }

        //------------------------------------------------------------------
        [ScriptMethod(path: LIB_OPERATION_NAME, methodName: "<")]
        public static bool Lt(IComparable a, IComparable b)
        {
            return a.CompareTo(b) < 0;
        }

        //------------------------------------------------------------------
        [ScriptMethod(path: LIB_OPERATION_NAME, methodName: "<=")]
        public static bool Le(IComparable a, IComparable b)
        {
            return a.CompareTo(b) <= 0;
        }

        //====================================================================================
        /// <summary>
        /// Func<TResult> を Invoke して結果を返します。
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="func"></param>
        /// <returns></returns>
        [ScriptMethod(path: LIB_FUNC_NAME)]
        public static TResult Invoke<TResult>(Func<TResult> func)
        {
            return func();
        }

        //------------------------------------------------------------------
        /// <summary>
        /// 一つの引数を渡して Func<T, TResult> を Invoke して結果を返します。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="arg"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        [ScriptMethod(path: LIB_FUNC_NAME)]
        public static TResult Invoke<T, TResult>(T arg, Func<T, TResult> func)
        {
            return func(arg);
        }

        //------------------------------------------------------------------
        /// <summary>
        /// 一つの引数を渡して Func<T1, T2, TResult> を Invoke して結果を返します。
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        [ScriptMethod(path: LIB_FUNC_NAME)]
        public static TResult Invoke<T1, T2, TResult>(T1 arg1, T2 arg2, Func<T1, T2, TResult> func)
        {
            return func(arg1, arg2);
        }

        //====================================================================================
        /// <summary>
        /// Action を Invoke します。
        /// </summary>
        /// <param name="action"></param>
        [ScriptMethod(path: LIB_ACTION_NAME)]
        public static void Invoke(Action action)
        {
            action?.Invoke();
        }

        //------------------------------------------------------------------
        /// <summary>
        /// 一つの引数を渡して Action<T> を Invoke します。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="arg"></param>
        /// <param name="action"></param>
        [ScriptMethod(path: LIB_ACTION_NAME)]
        public static void Invoke<T>(T arg, Action<T> action)
        {
            action?.Invoke(arg);
        }

        //------------------------------------------------------------------
        /// <summary>
        /// 二つの引数を渡して Action<T1, T2> を Invoke します。
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        /// <param name="action"></param>
        [ScriptMethod(path: LIB_ACTION_NAME)]
        public static void Invoke<T1, T2>(T1 arg1, T2 arg2, Action<T1, T2> action)
        {
            action?.Invoke(arg1, arg2);
        }

        //====================================================================================
        /// <summary>
        /// null許容型の変数を受け取り、値があれば true を返します。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sample"></param>
        /// <returns></returns>
        [ScriptMethod(path: LIB_NULLABLE_NAME)]
        public static bool HasValue<T>(T? sample) where T : struct
        {
            if (sample.HasValue)
            {
                return true;
            }
            return false;
        }

        //------------------------------------------------------------------
        /// <summary>
        /// null許容型の変数を受け取り、値があればその値を引数に Action<T> を呼びます。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sample"></param>
        /// <param name="hasValueAction"></param>
        [ScriptMethod(path: LIB_NULLABLE_NAME)]
        public static void HasValue<T>(T? sample, Action<T> hasValueAction) where T : struct
        {
            if (sample.HasValue)
            {
                hasValueAction?.Invoke(sample.Value);
            }
        }

        //------------------------------------------------------------------
        /// <summary>
        /// sample が true のとき、value の値を持った null許容型の値を返します。
        /// sample が false のときは、null 状態の null許容型の値を返します。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sample"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        [ScriptMethod(path: LIB_NULLABLE_NAME)]
        public static T? If_ResultNullable<T>(bool sample, T value) where T : struct
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
        /// <summary>
        /// null許容型の変数を受け取り、値があればその値を引数に Action<T> を呼びます。
        /// 値がなければ Action を呼びます。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sample"></param>
        /// <param name="hasValueAction"></param>
        /// <param name="othersAction"></param>
        [ScriptMethod(path: LIB_NULLABLE_NAME)]
        public static void If_HasValue<T>(T? sample, Action<T> hasValueAction, Action othersAction) where T : struct
        {
            if (sample.HasValue)
            {
                hasValueAction?.Invoke(sample.Value);
            }
            else
            {
                othersAction?.Invoke();
            }
        }

        //------------------------------------------------------------------
        [ScriptMethod(path: LIB_NULLABLE_NAME)]
        public static T? If_ResultNullable<T>(bool sample, Func<T> trueFunction, Func<T> falseFunction) where T : struct
        {
            if (sample)
            {
                if (trueFunction is null)
                    return null;

                return trueFunction();
            }
            else
            {
                if (falseFunction is null)
                    return null;

                return falseFunction();
            }
        }

        //====================================================================================
        /// <summary>
        /// null チェックし null なら true を返します。
        /// </summary>
        /// <param name="sample">null チェック対象</param>
        /// <returns>null なら true</returns>
        [ScriptMethod(path: LIB_FLOW_NAME)]
        public static bool IsNull(object sample)
        {
            return sample is null;
        }

        //------------------------------------------------------------------
        [ScriptMethod(path: LIB_FLOW_NAME)]
        public static T IsNull<T>(object sample, T trueValue, T falseValue)
        {
            if (sample is null)
            {
                return trueValue;
            }
            else
            {
                return falseValue;
            }
        }

        //------------------------------------------------------------------
        [ScriptMethod(path: LIB_FLOW_NAME)]
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
        [ScriptMethod(path: LIB_FLOW_NAME)]
        public static void NotNull<T>(T sample, Action<T> action)
        {
            if (sample != null)
            {
                action?.Invoke(sample);
            }
        }

        //------------------------------------------------------------------
        [ScriptMethod(path: LIB_FLOW_NAME)]
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
        [ScriptMethod(path: LIB_FLOW_NAME)]
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
        [ScriptMethod(path: LIB_FLOW_NAME)]
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
        /// <summary>
        /// Func<bool> が true を返すまで Func<bool> を呼び続けます。
        /// </summary>
        /// <param name="func"></param>
        [ScriptMethod(path: LIB_FLOW_NAME)]
        public static void DoWhile(Func<bool> func)
        {
            if (func is null)
                return;
            do { } while (func());
        }

        //------------------------------------------------------------------
        /// <summary>
        /// サンプルリストの各要素に対して要素を引数に Action を呼びます。
        /// </summary>
        /// <typeparam name="T">サンプルリストの型</typeparam>
        /// <param name="samples">サンプルリスト</param>
        /// <param name="action">サンプルリストの要素に対する処理</param>
        [ScriptMethod(path: LIB_FLOW_NAME)]
        public static void Foreach<T>(IEnumerable<T> samples, Action<T> action)
        {
            if (samples is null || action is null)
                return;
            foreach (var sample in samples)
            {
                action(sample);
            }
        }

        //------------------------------------------------------------------
        /// <summary>
        /// サンプルリストの各要素に対して要素を引数に Predicate を呼びます。
        /// Predicate が false を返したら抜けます。
        /// </summary>
        /// <typeparam name="T">サンプルリストの型</typeparam>
        /// <param name="samples">サンプルリスト</param>
        /// <param name="predicate">サンプルリストの要素に対する処理</param>
        [ScriptMethod(path: LIB_FLOW_NAME)]
        public static void BreakableForeach<T>(IEnumerable<T> samples, Predicate<T> predicate)
        {
            if (samples is null || predicate is null)
                return;
            foreach (var sample in samples)
            {
                if (!predicate(sample))
                {
                    break;
                }
            }
        }

        //------------------------------------------------------------------
        /// <summary>
        /// サンプルリストに登録されている Action を順番に実行します。
        /// </summary>
        /// <param name="samples">サンプルリスト</param>
        [ScriptMethod(path: LIB_FLOW_NAME)]
        public static void ForeachAction(IEnumerable<Action> samples)
        {
            if (samples is null)
                return;
            foreach (var sample in samples)
            {
                sample?.Invoke();
            }
        }

        //------------------------------------------------------------------
        /// <summary>
        /// サンプルリストに登録されている Func<bool> を順番に実行します。
        /// Func<bool> が false を返したら終了します。
        /// </summary>
        /// <param name="samples">サンプルリスト</param>
        [ScriptMethod(path: LIB_FLOW_NAME)]
        public static void BreakableForeachAction(IEnumerable<Func<bool>> samples)
        {
            if (samples is null)
                return;
            foreach (var sample in samples)
            {
                if (sample != null)
                {
                    if (!sample.Invoke())
                    {
                        break;
                    }
                }
            }
        }

        //------------------------------------------------------------------
        /// <summary>
        /// サンプルリストの各要素に対して値を返すまで要素を引数にfuncを呼びます。
        /// funcがnull以外を返したら抜けます。
        /// func が返した値かデフォルト値が返ります。
        /// </summary>
        /// <typeparam name="T">サンプルリストの型</typeparam>
        /// <typeparam name="TReturn">返し値の型</typeparam>
        /// <param name="samples">サンプルリスト</param>
        /// <param name="func">サンプルリストの要素に対する処理</param>
        /// <param name="defalutReturn">デフォルトの返し値</param>
        /// <returns>funcの返した値かデフォルト値</returns>
        [ScriptMethod(path: LIB_FLOW_NAME)]
        public static TReturn ForeachReturn<T, TReturn>(IEnumerable<T> samples, Func<T, Nullable<TReturn>> func, TReturn defalutReturn)
            where TReturn : struct
        {
            if (samples is null || func is null)
                return defalutReturn;
            TReturn result = defalutReturn;
            foreach (var sample in samples)
            {
                var ret = func(sample);
                if (ret.HasValue)
                {
                    return ret.Value;
                }
            }
            return result;
        }
    }
}
