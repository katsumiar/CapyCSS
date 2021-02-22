using CapybaraVS.Script;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using static CapybaraVS.Script.ScriptImplement;

namespace CbVS.Script
{
    /// <summary>
    /// CbFunc 用引数クラスです。
    /// </summary>
    public class CbFuncArguments
    {
        public enum INDEX
        {
            ARG_1,
            ARG_2,
            ARG_3,
            ARG_4,
            ARG_5,
            ARG_6,
            ARG_7,
            ARG_8,
            ARG_9,
            ARG_10,
            ARG_11,
            ARG_12,
            ARG_13,
            ARG_14,
            ARG_15,
            ARG_16,
        }
        private List<ICbValue> Values = null;
        public CbFuncArguments(List<ICbValue> values)
        {
            Values = values;
        }
        public void Set(List<ICbValue> values)
        {
            Values = values;
        }
        public void CopyFrom(List<object> values)
        {
            for (int i = 0; i < values.Count; ++i)
            {
                Values[i].Data = values[i];
            }
        }
        public ICbValue this[INDEX index]
        {
            get => Values[(int)index];
            set
            {
                Values[(int)index] = value;
            }
        }
        public void Add(ICbValue value)
        {
            Values.Add(value);
        }
        public void Clear()
        {
            Values.Clear();
        }
        public int Count => Values.Count;
    }

    public interface ICbEvent : ICbValue
    {
        /// <summary>
        /// CbEnum<T> の管理するT型の名前
        /// </summary>
        string ItemName { get; }
        /// <summary>
        /// オリジナルの型を参照します。
        /// </summary>
        //Type OriginalType { get; }
        /// <summary>
        /// イベント呼び出し用引数リストを取得します。
        /// </summary>
        List<ICbValue> EventCallArgumentValueList { get; }
        /// <summary>
        /// イベント型を判定します。
        /// </summary>
        bool IsCallBack { get; }
        /// <summary>
        /// Func<> もしくは Action<> の内包したコールバックを参照します。
        /// </summary>
        Func<DummyArgumentsStack, object> CallBack { get; set; }
        /// <summary>
        /// コールバックの返し値を参照します。
        /// </summary>
        ICbValue Value { get; set; }
        /// <summary>
        /// コールバックを呼び出します。
        /// </summary>
        /// <param name="cbPushList"></param>
        void CallCallBack(DummyArgumentsStack cbPushList);
        /// <summary>
        /// オリジナルの型でコールバックを取得します。
        /// </summary>
        /// <param name="dummyArgumentsControl"></param>
        /// <param name="cagt"></param>
        /// <returns></returns>
        object GetCallBackOriginalType(DummyArgumentsControl dummyArgumentsControl, DummyArgumentsStack cagt);
    }

    public class CbFunc
    {
        /// <summary>
        /// CbXXX型の Action<> 型の変数を作成します。
        /// </summary>
        /// <param name="original">Action型の第一引数のオリジナルの型</param>
        /// <returns>CbFunc<original, CbClass<CbVoid>>型の変数</returns>
        public static ICbValue CreateAction(string name = "")
        {
            Type cbFuncType = typeof(CbFunc<,>).MakeGenericType(
                typeof(Action),
                CbVoid.T // Action の場合はこの指定が必要
                );

            object result = cbFuncType.InvokeMember("GetCbFunc", BindingFlags.InvokeMethod,
                        null, null, new object[] { name }) as ICbValue;
            return result as ICbValue;
        }

        /// <summary>
        /// CbXXX型の Action<> 型の変数を作成します。
        /// </summary>
        /// <param name="arg">Action型の第一引数のオリジナルの型</param>
        /// <returns>CbFunc<original, CbClass<CbVoid>>型の変数</returns>
        public static ICbValue CreateAction(Type arg, string name = "")
        {
            Type cbFuncType = typeof(CbFunc<,>).MakeGenericType(
                typeof(Action<>).MakeGenericType(arg),
                CbVoid.T // Action の場合はこの指定が必要
                );

            object result = cbFuncType.InvokeMember("GetCbFunc", BindingFlags.InvokeMethod,
                        null, null, new object[] { name }) as ICbValue;
            return result as ICbValue;
        }

        /// <summary>
        /// CbXXX型の Func<> 型の変数を作成します。
        /// </summary>
        /// <param name="ret">オリジナルの返し値の型</param>
        /// <returns>CbFunc<original, CbXXX型のret型>の変数</returns>
        public static ICbValue CreateFunc(Type ret, string name = "")
        {
            Type cbFuncType = typeof(CbFunc<,>).MakeGenericType(
                typeof(Func<>).MakeGenericType(ret),
                CbST.ConvertCbType(ret)
                );

            object result = cbFuncType.InvokeMember("GetCbFunc", BindingFlags.InvokeMethod,
                        null, null, new object[] { name }) as ICbValue;
            return result as ICbValue;
        }

        /// <summary>
        /// CbXXX型の Func<> 型の変数を作成します。
        /// </summary>
        /// <param name="arg">Func型の第一引数のオリジナルの型</param>
        /// <param name="ret">オリジナルの返し値の型</param>
        /// <returns>CbFunc<original, CbXXX型のret型>の変数</returns>
        public static ICbValue CreateFunc(Type arg, Type ret, string name = "")
        {
            Type cbFuncType = typeof(CbFunc<,>).MakeGenericType(
                typeof(Func<,>).MakeGenericType(arg, ret),
                CbST.ConvertCbType(ret)
                );

            object result = cbFuncType.InvokeMember("GetCbFunc", BindingFlags.InvokeMethod,
                        null, null, new object[] { name }) as ICbValue;
            return result as ICbValue;
        }

        /// <summary>
        /// CbXXX型の Func<> 型を作成します。
        /// </summary>
        /// <param name="original">Func型の正式な型</param>
        /// <param name="ret">オリジナルの返し値の型</param>
        /// <returns>型</returns>
        public static Type GetFuncType(Type original, Type ret)
        {
            return typeof(CbFunc<,>).MakeGenericType(
                original,
                CbST.ConvertCbType(ret)
                );
        }

        /// <summary>
        /// CbXXX型の Func<> 型の変数を作成します。
        /// </summary>
        /// <param name="original">Func型の正式な型</param>
        /// <param name="ret">オリジナルの返し値の型</param>
        /// <returns>CbFunc<original, CbXXX型のret型>の変数</returns>
        public static ICbValue CreateFuncFromOriginalType(Type original, Type ret, string name = "")
        {
            object result = GetFuncType(original, ret)
                        .InvokeMember("GetCbFunc", BindingFlags.InvokeMethod,
                            null, null, new object[] { name }) as ICbValue;
            return result as ICbValue;
        }

        private static ICbValue CallGetCbFunc(ICbValue ret, string name, Type originalType)
        {
            object result = GetFuncType(originalType, ret.OriginalReturnType)
                        .InvokeMember("GetCbFunc", BindingFlags.InvokeMethod,
                            null, null, new object[] { name }) as ICbValue;
            return result as ICbValue;
        }

        /// <summary>
        /// Func 及び Action のノード接続の可否判定を行います。
        /// </summary>
        /// <param name="toName">接続先の型名</param>
        /// <param name="fromName">接続元の型名</param>
        /// <returns></returns>
        public static bool IsCanConnect(string toName, string fromName)
        {
            if ((toName == $"{CbSTUtils.FUNC_STR}<" && toName.EndsWith($", {CbSTUtils.OBJECT_STR}>"))
                || toName == $"{CbSTUtils.FUNC_STR}<{CbSTUtils.OBJECT_STR}>"
                || toName.StartsWith(CbSTUtils.ACTION_STR))
            {
                // イベント接続

                return true;
            }
            if (toName.StartsWith($"{CbSTUtils.FUNC_STR}<") && toName.EndsWith(fromName + ">"))
            {
                // イベント接続（返し値が一致するなら接続可）

                return true;
            }
            if (toName.StartsWith($"{CbSTUtils.FUNC_STR}<") && toName.EndsWith($"{CbSTUtils.OBJECT_STR}>"))
            {
                // イベント接続（返し値が一致するなら接続可）

                return true;
            }
            return false;
        }

        /// <summary>
        /// Func 及び Action ノードの通常接続の可否判定を行います。
        /// </summary>
        /// <param name="toName">接続先の型名</param>
        /// <param name="fromName">接続元の型名</param>
        /// <returns></returns>
        public static bool IsNormalConnect(string toName, string fromName)
        {
            if (toName.StartsWith(CbSTUtils.ACTION_STR))
            {
                // イベント接続

                return true;
            }
            if (toName.StartsWith($"{CbSTUtils.FUNC_STR}<") && toName.EndsWith(fromName + ">"))
            {
                // イベント接続（返し値が一致するなら接続可）

                return true;
            }
            if (toName.StartsWith($"{CbSTUtils.FUNC_STR}<") && toName.EndsWith($"{CbSTUtils.OBJECT_STR}>"))
            {
                // イベント接続（返し値がobjectなら接続可）

                return true;
            }
            return false;
        }

        /// <summary>
        /// Func ノードのキャスト接続の可否判定を行います。
        /// </summary>
        /// <param name="toName">接続先の型名</param>
        /// <param name="fromName">接続元の型名</param>
        /// <param name="toType"></param>
        /// <param name="fromType"></param>
        /// <returns></returns>
        public static bool IsCastConnect(string toName, string fromName, Type toType, Type fromType)
        {
            // Action はこの判定まで来ない
            Debug.Assert(!toName.StartsWith(CbSTUtils.ACTION_STR));

            return CbSTUtils.IsCastAssignment(
                CbSTUtils._GetTypeName(toType.GenericTypeArguments.Last()),
                CbSTUtils._GetTypeName(fromType.GenericTypeArguments.Last()));
        }

        public static bool IsEventType(Type type)
        {
            return IsActionType(type) || IsFuncType(type);
        }

        public static bool IsFuncType(Type type)
        {
            return FuncTypeArgCount(type) != -1;
        }

        public static int FuncTypeArgCount(Type type)
        {
            if (type.GetGenericTypeDefinition() == typeof(Func<>))
                return 1;
            if (type.GetGenericTypeDefinition() == typeof(Func<,>))
                return 2;
            if (type.GetGenericTypeDefinition() == typeof(Func<,,>))
                return 3;
            if (type.GetGenericTypeDefinition() == typeof(Func<,,,>))
                return 4;
            if (type.GetGenericTypeDefinition() == typeof(Func<,,,,>))
                return 5;
            if (type.GetGenericTypeDefinition() == typeof(Func<,,,,,>))
                return 6;
            if (type.GetGenericTypeDefinition() == typeof(Func<,,,,,,>))
                return 7;
            if (type.GetGenericTypeDefinition() == typeof(Func<,,,,,,,>))
                return 8;
            if (type.GetGenericTypeDefinition() == typeof(Func<,,,,,,,,>))
                return 9;
            if (type.GetGenericTypeDefinition() == typeof(Func<,,,,,,,,,>))
                return 10;
            if (type.GetGenericTypeDefinition() == typeof(Func<,,,,,,,,,,>))
                return 11;
            if (type.GetGenericTypeDefinition() == typeof(Func<,,,,,,,,,,,>))
                return 12;
            if (type.GetGenericTypeDefinition() == typeof(Func<,,,,,,,,,,,,>))
                return 13;
            if (type.GetGenericTypeDefinition() == typeof(Func<,,,,,,,,,,,,,>))
                return 14;
            if (type.GetGenericTypeDefinition() == typeof(Func<,,,,,,,,,,,,,,>))
                return 15;
            if (type.GetGenericTypeDefinition() == typeof(Func<,,,,,,,,,,,,,,,>))
                return 16;
            if (type.GetGenericTypeDefinition() == typeof(Func<,,,,,,,,,,,,,,,,>))
                return 17;
            return -1;
        }

        public static bool IsActionType(Type type)
        {
            return ActionTypeArgCount(type) != -1;
        }

        public static int ActionTypeArgCount(Type type)
        {
            if (type == typeof(Action))
                return 0;
            if (type.GetGenericTypeDefinition() == typeof(Action<>))
                return 1;
            if (type.GetGenericTypeDefinition() == typeof(Action<,>))
                return 2;
            if (type.GetGenericTypeDefinition() == typeof(Action<,,>))
                return 3;
            if (type.GetGenericTypeDefinition() == typeof(Action<,,,>))
                return 4;
            if (type.GetGenericTypeDefinition() == typeof(Action<,,,,>))
                return 5;
            if (type.GetGenericTypeDefinition() == typeof(Action<,,,,,>))
                return 6;
            if (type.GetGenericTypeDefinition() == typeof(Action<,,,,,,>))
                return 7;
            if (type.GetGenericTypeDefinition() == typeof(Action<,,,,,,,>))
                return 8;
            if (type.GetGenericTypeDefinition() == typeof(Action<,,,,,,,,>))
                return 9;
            if (type.GetGenericTypeDefinition() == typeof(Action<,,,,,,,,,>))
                return 10;
            if (type.GetGenericTypeDefinition() == typeof(Action<,,,,,,,,,,>))
                return 11;
            if (type.GetGenericTypeDefinition() == typeof(Action<,,,,,,,,,,,>))
                return 12;
            if (type.GetGenericTypeDefinition() == typeof(Action<,,,,,,,,,,,,>))
                return 13;
            if (type.GetGenericTypeDefinition() == typeof(Action<,,,,,,,,,,,,,>))
                return 14;
            if (type.GetGenericTypeDefinition() == typeof(Action<,,,,,,,,,,,,,,>))
                return 15;
            if (type.GetGenericTypeDefinition() == typeof(Action<,,,,,,,,,,,,,,,>))
                return 16;
            return -1;
        }

        /// <summary>
        /// Func<object, type> 型の CbFunc<type> 型の変数を返します。
        /// </summary>
        /// <param name="type">オリジナルの元々の型</param>
        /// <param name="retType">オリジナルの返し値の型</param>
        /// <param name="name">変数名</param>
        /// <returns>CbFunc<type> 型の変数</returns>
        public static ICbValue FuncValue(Type type, Type retType, string name)
        {
            if (type is null)
            {
                return null;
            }
            string typeName = type.FullName;
            if (type.IsByRef)
            {
                // リファレンス（スクリプト変数接続）

                typeName = typeName.Replace("&", "");
                type = CbST.GetTypeEx(typeName);
            }

            foreach (var arg in type.GenericTypeArguments)
            {
                Type cbType = CbST.ConvertCbType(arg);
                if (cbType is null)
                    return null;
            }

            return CbFunc.CreateFuncFromOriginalType(type, retType, name);
        }

        /// <summary>
        /// メソッド呼び出し用引数情報リストの中にイベント型を含んでいるかを判定します。
        /// </summary>
        /// <param name="arguments">メソッド呼び出し用引数情報リスト</param>
        /// <returns>イベント型を含んでいるなら true</returns>
        public static bool ContainsEvent(List<ArgumentInfoNode> arguments)
        {
            if (arguments is null)
                return false;
            foreach (var node in arguments)
            {
                if (node.CreateArgument() is ICbEvent)
                    return true;
            }
            return false;
        }
    }

    /// <summary>
    /// Func<> 型クラス
    /// </summary>
    /// <typeparam name="T">オリジナルの型</typeparam>
    /// <typeparam name="RT">CbXXX型の返し値の型</typeparam>
    public class CbFunc<T, RT> : BaseCbValueClass<ICbValue>, ICbValueClass<ICbValue>, ICbEvent
         where RT : class, ICbValue
    {
        public override Type MyType => typeof(CbFunc<T, RT>);

        public override Type OriginalReturnType => typeof(RT);

        public override Type OriginalType => typeof(T);

        public List<ICbValue> EventCallArgumentValueList
        {
            get
            {
                List<ICbValue> argVallist = new List<ICbValue>();
                foreach (var t in typeof(T).GenericTypeArguments)
                {
                    var dt = CbST.CbCreate(t);
                    Debug.Assert(dt != null);
                    argVallist.Add(dt);
                }
                return argVallist;
            }
        }

        public string ItemName => typeof(RT).FullName;

        /// <summary>
        /// Func<> もしくは Action<> を内包したコールバックを参照します。
        /// </summary>
        public Func<DummyArgumentsStack, object> CallBack { get; set; } = null;

        public CbFunc(ICbValue n, string name = "")
        {
            Value = n as ICbValue;
            Name = name;
        }

        public CbFunc(string name = "")
        {
            Value = null;
            Name = name;
        }

        public new void Set(ICbValue n)
        {
            try
            {
                if (n.IsError)
                    throw new Exception(n.ErrorMessage);

                if (n is CbObject)
                {
                    Data = n.Data;
                }
                else if (n is ICbEvent cbEvent)
                {
                    CallBack = cbEvent.CallBack;
                }
                if (IsError)
                {
                    // エラーからの復帰

                    IsError = false;
                    ErrorMessage = "";
                }
            }
            catch (Exception ex)
            {
                IsError = true;
                ErrorMessage = ex.Message;
                throw;
            }
        }

        public override string TypeName
        {
            get
            {
                string name = CbSTUtils.FUNC_STR;
                if (CbVoid.Is(typeof(RT)))
                {
                    name = CbSTUtils.ACTION_STR;
                }
                bool isFirst = true;
                foreach (var t in typeof(T).GenericTypeArguments)
                {
                    if (isFirst)
                    {
                        name += "<" + CbSTUtils._GetTypeName(t);
                    }
                    else
                    {
                        name += ", " + CbSTUtils._GetTypeName(t);
                    }
                    isFirst = false;
                }
                if (isFirst)
                    return name;
                return name + ">";
            }
        }

        public bool IsCallBack => CallBack != null;

        public void CallCallBack(DummyArgumentsStack cbPushList)
        {
            if (CbVoid.Is(typeof(RT)))
            {
                CallBack?.Invoke(cbPushList);
            }
            else if (typeof(RT) == typeof(CbObject))
            {
                Data = (ICbValue)CallBack?.Invoke(cbPushList);
            }
            else if (typeof(RT).IsGenericType && typeof(RT).GetGenericTypeDefinition() == typeof(CbFunc<,>))
            {
                ICbEvent cbEvent = (ICbEvent)CallBack?.Invoke(cbPushList);
                Data = cbEvent.Data;
                CallBack = cbEvent.CallBack;
            }
            else
            {
                Value = (RT)CallBack?.Invoke(cbPushList);
            }
        }

        public override ICbValue Value
        {
            get
            {
                if (CallBack is null)
                    return null;
                return _value;
            }
            set
            {
                _value = value;
            }
        }

        public override string ValueString
        {
            get
            {
                string baseName = "[" + TypeName + "()]";
                if (IsError)
                    return CbSTUtils.ERROR_STR;
                if (IsNull)
                    return baseName + CbSTUtils.NULL_STR;
                return baseName;
            }
            set => new NotImplementedException();
        }

        public override bool IsDelegate => true;

        public override bool IsReadOnlyValue { get; set; } = true;

        public override bool IsStringableValue => false;

        public object GetCallBackOriginalType(DummyArgumentsControl dummyArgumentsControl, DummyArgumentsStack cagt)
        {
            return ConvertOriginalType(dummyArgumentsControl, cagt, typeof(RT));
        }

        /// <summary>
        /// Cbクラスでラッピングしていない元々の型（T）の Func<> 及び Action<> を返す機能を取得します。
        /// </summary>
        /// <param name="dummyArgumentsControl"></param>
        /// <param name="cagt"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public object ConvertOriginalType(DummyArgumentsControl dummyArgumentsControl, DummyArgumentsStack cagt, Type type)
        {
            if (CbFunc.IsActionType(typeof(T)))
            {
                // Action 用

                if (typeof(T) == typeof(Action))
                {
                    return this.GetType()
                        .GetMethod(nameof(_ConvertOriginalActionType))
                        .Invoke(this, new object[] { dummyArgumentsControl, cagt });
                }

                // Action<> 用

                return this.GetType()
                    .GetMethod(nameof(_ConvertOriginalActionType) + typeof(T).GetGenericArguments().Length)
                    .MakeGenericMethod(typeof(T).GetGenericArguments())
                    .Invoke(this, new object[] { dummyArgumentsControl, cagt });
            }

            // Func<> 用

            return this.GetType()
                    .GetMethod("_ConvertOriginalFuncType" + typeof(T).GetGenericArguments().Length)
                    .MakeGenericMethod(typeof(T).GetGenericArguments())
                    .Invoke(this, new object[] { dummyArgumentsControl, cagt });
        }

        //-------------------------------------------------------------------------------------
        public object _ConvertOriginalActionType(DummyArgumentsControl dummyArgumentsControl, DummyArgumentsStack cagt)
        {
            return new Action(
                () =>
                {
                    CallCallBack(cagt);
                }
                );
        }

        public object _ConvertOriginalActionType1<A1>(DummyArgumentsControl dummyArgumentsControl, DummyArgumentsStack cagt)
        {
            return new Action<A1>(
                (arg) =>
                {
                    dummyArgumentsControl.Enable(cagt, arg);    // 仮引数に引数を登録
                    CallCallBack(cagt);
                    dummyArgumentsControl.Invalidated(cagt);    // 仮引数後処理
                }
                );
        }

        public object _ConvertOriginalActionType2<A2, A1>(DummyArgumentsControl dummyArgumentsControl, DummyArgumentsStack cagt)
        {
            return new Action<A2, A1>(
                (arg1, arg2) =>
                {
                    dummyArgumentsControl.Enable(cagt, arg1, arg2);    // 仮引数に引数を登録
                    CallCallBack(cagt);
                    dummyArgumentsControl.Invalidated(cagt);    // 仮引数後処理
                }
                );
        }

        public object _ConvertOriginalActionType3<A3, A2, A1>(DummyArgumentsControl dummyArgumentsControl, DummyArgumentsStack cagt)
        {
            return new Action<A3, A2, A1>(
                (arg1, arg2, arg3) =>
                {
                    dummyArgumentsControl.Enable(cagt, arg1, arg2, arg3);    // 仮引数に引数を登録
                    CallCallBack(cagt);
                    dummyArgumentsControl.Invalidated(cagt);    // 仮引数後処理
                }
                );
        }

        public object _ConvertOriginalActionType4<A4, A3, A2, A1>(DummyArgumentsControl dummyArgumentsControl, DummyArgumentsStack cagt)
        {
            return new Action<A4, A3, A2, A1>(
                (arg1, arg2, arg3, arg4) =>
                {
                    dummyArgumentsControl.Enable(cagt, arg1, arg2, arg3, arg4);    // 仮引数に引数を登録
                    CallCallBack(cagt);
                    dummyArgumentsControl.Invalidated(cagt);    // 仮引数後処理
                }
                );
        }

        public object _ConvertOriginalActionType5<A5, A4, A3, A2, A1>(DummyArgumentsControl dummyArgumentsControl, DummyArgumentsStack cagt)
        {
            return new Action<A5, A4, A3, A2, A1>(
                (arg1, arg2, arg3, arg4, arg5) =>
                {
                    dummyArgumentsControl.Enable(cagt, arg1, arg2, arg3, arg4, arg5);    // 仮引数に引数を登録
                    CallCallBack(cagt);
                    dummyArgumentsControl.Invalidated(cagt);    // 仮引数後処理
                }
                );
        }

        public object _ConvertOriginalActionType6<A6, A5, A4, A3, A2, A1>(DummyArgumentsControl dummyArgumentsControl, DummyArgumentsStack cagt)
        {
            return new Action<A6, A5, A4, A3, A2, A1>(
                (arg1, arg2, arg3, arg4, arg5, arg6) =>
                {
                    dummyArgumentsControl.Enable(cagt, arg1, arg2, arg3, arg4, arg5, arg6);    // 仮引数に引数を登録
                    CallCallBack(cagt);
                    dummyArgumentsControl.Invalidated(cagt);    // 仮引数後処理
                }
                );
        }

        public object _ConvertOriginalActionType7<A7, A6, A5, A4, A3, A2, A1>(DummyArgumentsControl dummyArgumentsControl, DummyArgumentsStack cagt)
        {
            return new Action<A7, A6, A5, A4, A3, A2, A1>(
                (arg1, arg2, arg3, arg4, arg5, arg6, arg7) =>
                {
                    dummyArgumentsControl.Enable(cagt, arg1, arg2, arg3, arg4, arg5, arg6, arg7);    // 仮引数に引数を登録
                    CallCallBack(cagt);
                    dummyArgumentsControl.Invalidated(cagt);    // 仮引数後処理
                }
                );
        }

        public object _ConvertOriginalActionType8<A8, A7, A6, A5, A4, A3, A2, A1>(DummyArgumentsControl dummyArgumentsControl, DummyArgumentsStack cagt)
        {
            return new Action<A8, A7, A6, A5, A4, A3, A2, A1>(
                (arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8) =>
                {
                    dummyArgumentsControl.Enable(cagt, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);    // 仮引数に引数を登録
                    CallCallBack(cagt);
                    dummyArgumentsControl.Invalidated(cagt);    // 仮引数後処理
                }
                );
        }

        public object _ConvertOriginalActionType9<A9, A8, A7, A6, A5, A4, A3, A2, A1>(DummyArgumentsControl dummyArgumentsControl, DummyArgumentsStack cagt)
        {
            return new Action<A9, A8, A7, A6, A5, A4, A3, A2, A1>(
                (arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9) =>
                {
                    dummyArgumentsControl.Enable(cagt, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9);    // 仮引数に引数を登録
                    CallCallBack(cagt);
                    dummyArgumentsControl.Invalidated(cagt);    // 仮引数後処理
                }
                );
        }

        public object _ConvertOriginalActionType10<A10, A9, A8, A7, A6, A5, A4, A3, A2, A1>(DummyArgumentsControl dummyArgumentsControl, DummyArgumentsStack cagt)
        {
            return new Action<A10, A9, A8, A7, A6, A5, A4, A3, A2, A1>(
                (arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10) =>
                {
                    dummyArgumentsControl.Enable(cagt, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10);    // 仮引数に引数を登録
                    CallCallBack(cagt);
                    dummyArgumentsControl.Invalidated(cagt);    // 仮引数後処理
                }
                );
        }

        public object _ConvertOriginalActionType11<A11, A10, A9, A8, A7, A6, A5, A4, A3, A2, A1>(DummyArgumentsControl dummyArgumentsControl, DummyArgumentsStack cagt)
        {
            return new Action<A11, A10, A9, A8, A7, A6, A5, A4, A3, A2, A1>(
                (arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11) =>
                {
                    dummyArgumentsControl.Enable(cagt, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11);    // 仮引数に引数を登録
                    CallCallBack(cagt);
                    dummyArgumentsControl.Invalidated(cagt);    // 仮引数後処理
                }
                );
        }

        public object _ConvertOriginalActionType12<A12, A11, A10, A9, A8, A7, A6, A5, A4, A3, A2, A1>(DummyArgumentsControl dummyArgumentsControl, DummyArgumentsStack cagt)
        {
            return new Action<A12, A11, A10, A9, A8, A7, A6, A5, A4, A3, A2, A1>(
                (arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12) =>
                {
                    dummyArgumentsControl.Enable(cagt, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12);    // 仮引数に引数を登録
                    CallCallBack(cagt);
                    dummyArgumentsControl.Invalidated(cagt);    // 仮引数後処理
                }
                );
        }

        public object _ConvertOriginalActionType13<A13, A12, A11, A10, A9, A8, A7, A6, A5, A4, A3, A2, A1>(DummyArgumentsControl dummyArgumentsControl, DummyArgumentsStack cagt)
        {
            return new Action<A13, A12, A11, A10, A9, A8, A7, A6, A5, A4, A3, A2, A1>(
                (arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13) =>
                {
                    dummyArgumentsControl.Enable(cagt, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13);    // 仮引数に引数を登録
                    CallCallBack(cagt);
                    dummyArgumentsControl.Invalidated(cagt);    // 仮引数後処理
                }
                );
        }

        public object _ConvertOriginalActionType14<A14, A13, A12, A11, A10, A9, A8, A7, A6, A5, A4, A3, A2, A1>(DummyArgumentsControl dummyArgumentsControl, DummyArgumentsStack cagt)
        {
            return new Action<A14, A13, A12, A11, A10, A9, A8, A7, A6, A5, A4, A3, A2, A1>(
                (arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14) =>
                {
                    dummyArgumentsControl.Enable(cagt, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14);    // 仮引数に引数を登録
                    CallCallBack(cagt);
                    dummyArgumentsControl.Invalidated(cagt);    // 仮引数後処理
                }
                );
        }

        public object _ConvertOriginalActionType15<A15, A14, A13, A12, A11, A10, A9, A8, A7, A6, A5, A4, A3, A2, A1>(DummyArgumentsControl dummyArgumentsControl, DummyArgumentsStack cagt)
        {
            return new Action<A15, A14, A13, A12, A11, A10, A9, A8, A7, A6, A5, A4, A3, A2, A1>(
                (arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15) =>
                {
                    dummyArgumentsControl.Enable(cagt, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15);    // 仮引数に引数を登録
                    CallCallBack(cagt);
                    dummyArgumentsControl.Invalidated(cagt);    // 仮引数後処理
                }
                );
        }

        public object _ConvertOriginalActionType16<A16, A15, A14, A13, A12, A11, A10, A9, A8, A7, A6, A5, A4, A3, A2, A1>(DummyArgumentsControl dummyArgumentsControl, DummyArgumentsStack cagt)
        {
            return new Action<A16, A15, A14, A13, A12, A11, A10, A9, A8, A7, A6, A5, A4, A3, A2, A1>(
                (arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15, arg16) =>
                {
                    dummyArgumentsControl.Enable(cagt, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15, arg16);    // 仮引数に引数を登録
                    CallCallBack(cagt);
                    dummyArgumentsControl.Invalidated(cagt);    // 仮引数後処理
                }
                );
        }

        //-------------------------------------------------------------------------------------
        public object _ConvertOriginalFuncType1<AR>(DummyArgumentsControl dummyArgumentsControl, DummyArgumentsStack cagt)
        {
            return new Func<AR>(
                () =>
                {
                    CallCallBack(cagt);
                    dummyArgumentsControl.Invalidated(cagt);    // 仮引数後処理
                    return (AR)Value.Data;
                }
                );
        }

        public object _ConvertOriginalFuncType2<A1, AR>(DummyArgumentsControl dummyArgumentsControl, DummyArgumentsStack cagt)
        {
            return new Func<A1, AR>(
                (arg) =>
                {
                    dummyArgumentsControl.Enable(cagt, arg);    // 仮引数に引数を登録
                    CallCallBack(cagt);
                    dummyArgumentsControl.Invalidated(cagt);    // 仮引数後処理
                    return (AR)Value.Data;
                }
                );
        }

        public object _ConvertOriginalFuncType3<A2, A1, AR>(DummyArgumentsControl dummyArgumentsControl, DummyArgumentsStack cagt)
        {
            return new Func<A2, A1, AR>(
                (arg1, arg2) =>
                {
                    dummyArgumentsControl.Enable(cagt, arg1, arg2);    // 仮引数に引数を登録
                    CallCallBack(cagt);
                    dummyArgumentsControl.Invalidated(cagt);    // 仮引数後処理
                    return (AR)Value.Data;
                }
                );
        }

        public object _ConvertOriginalFuncType4<A3, A2, A1, AR>(DummyArgumentsControl dummyArgumentsControl, DummyArgumentsStack cagt)
        {
            return new Func<A3, A2, A1, AR>(
                (arg1, arg2, arg3) =>
                {
                    dummyArgumentsControl.Enable(cagt, arg1, arg2, arg3);    // 仮引数に引数を登録
                    CallCallBack(cagt);
                    dummyArgumentsControl.Invalidated(cagt);    // 仮引数後処理
                    return (AR)Value.Data;
                }
                );
        }

        public object _ConvertOriginalFuncType5<A4, A3, A2, A1, AR>(DummyArgumentsControl dummyArgumentsControl, DummyArgumentsStack cagt)
        {
            return new Func<A4, A3, A2, A1, AR>(
                (arg1, arg2, arg3, arg4) =>
                {
                    dummyArgumentsControl.Enable(cagt, arg1, arg2, arg3, arg4);    // 仮引数に引数を登録
                    CallCallBack(cagt);
                    dummyArgumentsControl.Invalidated(cagt);    // 仮引数後処理
                    return (AR)Value.Data;
                }
                );
        }

        public object _ConvertOriginalFuncType6<A5, A4, A3, A2, A1, AR>(DummyArgumentsControl dummyArgumentsControl, DummyArgumentsStack cagt)
        {
            return new Func<A5, A4, A3, A2, A1, AR>(
                (arg1, arg2, arg3, arg4, arg5) =>
                {
                    dummyArgumentsControl.Enable(cagt, arg1, arg2, arg3, arg4, arg5);    // 仮引数に引数を登録
                    CallCallBack(cagt);
                    dummyArgumentsControl.Invalidated(cagt);    // 仮引数後処理
                    return (AR)Value.Data;
                }
                );
        }

        public object _ConvertOriginalFuncType7<A6, A5, A4, A3, A2, A1, AR>(DummyArgumentsControl dummyArgumentsControl, DummyArgumentsStack cagt)
        {
            return new Func<A6, A5, A4, A3, A2, A1, AR>(
                (arg1, arg2, arg3, arg4, arg5, arg6) =>
                {
                    dummyArgumentsControl.Enable(cagt, arg1, arg2, arg3, arg4, arg5, arg6);    // 仮引数に引数を登録
                    CallCallBack(cagt);
                    dummyArgumentsControl.Invalidated(cagt);    // 仮引数後処理
                    return (AR)Value.Data;
                }
                );
        }

        public object _ConvertOriginalFuncType8<A7, A6, A5, A4, A3, A2, A1, AR>(DummyArgumentsControl dummyArgumentsControl, DummyArgumentsStack cagt)
        {
            return new Func<A7, A6, A5, A4, A3, A2, A1, AR>(
                (arg1, arg2, arg3, arg4, arg5, arg6, arg7) =>
                {
                    dummyArgumentsControl.Enable(cagt, arg1, arg2, arg3, arg4, arg5, arg6, arg7);    // 仮引数に引数を登録
                    CallCallBack(cagt);
                    dummyArgumentsControl.Invalidated(cagt);    // 仮引数後処理
                    return (AR)Value.Data;
                }
                );
        }

        public object _ConvertOriginalFuncType9<A8, A7, A6, A5, A4, A3, A2, A1, AR>(DummyArgumentsControl dummyArgumentsControl, DummyArgumentsStack cagt)
        {
            return new Func<A8, A7, A6, A5, A4, A3, A2, A1, AR>(
                (arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8) =>
                {
                    dummyArgumentsControl.Enable(cagt, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);    // 仮引数に引数を登録
                    CallCallBack(cagt);
                    dummyArgumentsControl.Invalidated(cagt);    // 仮引数後処理
                    return (AR)Value.Data;
                }
                );
        }

        public object _ConvertOriginalFuncType10<A9, A8, A7, A6, A5, A4, A3, A2, A1, AR>(DummyArgumentsControl dummyArgumentsControl, DummyArgumentsStack cagt)
        {
            return new Func<A9, A8, A7, A6, A5, A4, A3, A2, A1, AR>(
                (arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9) =>
                {
                    dummyArgumentsControl.Enable(cagt, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9);    // 仮引数に引数を登録
                    CallCallBack(cagt);
                    dummyArgumentsControl.Invalidated(cagt);    // 仮引数後処理
                    return (AR)Value.Data;
                }
                );
        }

        public object _ConvertOriginalFuncType11<A10, A9, A8, A7, A6, A5, A4, A3, A2, A1, AR>(DummyArgumentsControl dummyArgumentsControl, DummyArgumentsStack cagt)
        {
            return new Func<A10, A9, A8, A7, A6, A5, A4, A3, A2, A1, AR>(
                (arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10) =>
                {
                    dummyArgumentsControl.Enable(cagt, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10);    // 仮引数に引数を登録
                    CallCallBack(cagt);
                    dummyArgumentsControl.Invalidated(cagt);    // 仮引数後処理
                    return (AR)Value.Data;
                }
                );
        }

        public object _ConvertOriginalFuncType12<A11, A10, A9, A8, A7, A6, A5, A4, A3, A2, A1, AR>(DummyArgumentsControl dummyArgumentsControl, DummyArgumentsStack cagt)
        {
            return new Func<A11, A10, A9, A8, A7, A6, A5, A4, A3, A2, A1, AR>(
                (arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11) =>
                {
                    dummyArgumentsControl.Enable(cagt, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11);    // 仮引数に引数を登録
                    CallCallBack(cagt);
                    dummyArgumentsControl.Invalidated(cagt);    // 仮引数後処理
                    return (AR)Value.Data;
                }
                );
        }

        public object _ConvertOriginalFuncType13<A12, A11, A10, A9, A8, A7, A6, A5, A4, A3, A2, A1, AR>(DummyArgumentsControl dummyArgumentsControl, DummyArgumentsStack cagt)
        {
            return new Func<A12, A11, A10, A9, A8, A7, A6, A5, A4, A3, A2, A1, AR>(
                (arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12) =>
                {
                    dummyArgumentsControl.Enable(cagt, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12);    // 仮引数に引数を登録
                    CallCallBack(cagt);
                    dummyArgumentsControl.Invalidated(cagt);    // 仮引数後処理
                    return (AR)Value.Data;
                }
                );
        }

        public object _ConvertOriginalFuncType14<A13, A12, A11, A10, A9, A8, A7, A6, A5, A4, A3, A2, A1, AR>(DummyArgumentsControl dummyArgumentsControl, DummyArgumentsStack cagt)
        {
            return new Func<A13, A12, A11, A10, A9, A8, A7, A6, A5, A4, A3, A2, A1, AR>(
                (arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13) =>
                {
                    dummyArgumentsControl.Enable(cagt, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13);    // 仮引数に引数を登録
                    CallCallBack(cagt);
                    dummyArgumentsControl.Invalidated(cagt);    // 仮引数後処理
                    return (AR)Value.Data;
                }
                );
        }

        public object _ConvertOriginalFuncType15<A14, A13, A12, A11, A10, A9, A8, A7, A6, A5, A4, A3, A2, A1, AR>(DummyArgumentsControl dummyArgumentsControl, DummyArgumentsStack cagt)
        {
            return new Func<A14, A13, A12, A11, A10, A9, A8, A7, A6, A5, A4, A3, A2, A1, AR>(
                (arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14) =>
                {
                    dummyArgumentsControl.Enable(cagt, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14);    // 仮引数に引数を登録
                    CallCallBack(cagt);
                    dummyArgumentsControl.Invalidated(cagt);    // 仮引数後処理
                    return (AR)Value.Data;
                }
                );
        }

        public object _ConvertOriginalFuncType16<A15, A14, A13, A12, A11, A10, A9, A8, A7, A6, A5, A4, A3, A2, A1, AR>(DummyArgumentsControl dummyArgumentsControl, DummyArgumentsStack cagt)
        {
            return new Func<A15, A14, A13, A12, A11, A10, A9, A8, A7, A6, A5, A4, A3, A2, A1, AR>(
                (arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15) =>
                {
                    dummyArgumentsControl.Enable(cagt, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15);    // 仮引数に引数を登録
                    CallCallBack(cagt);
                    dummyArgumentsControl.Invalidated(cagt);    // 仮引数後処理
                    return (AR)Value.Data;
                }
                );
        }

        public object _ConvertOriginalFuncType17<A16, A15, A14, A13, A12, A11, A10, A9, A8, A7, A6, A5, A4, A3, A2, A1, AR>(DummyArgumentsControl dummyArgumentsControl, DummyArgumentsStack cagt)
        {
            return new Func<A16, A15, A14, A13, A12, A11, A10, A9, A8, A7, A6, A5, A4, A3, A2, A1, AR>(
                (arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15, arg16) =>
                {
                    dummyArgumentsControl.Enable(cagt, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15, arg16);    // 仮引数に引数を登録
                    CallCallBack(cagt);
                    dummyArgumentsControl.Invalidated(cagt);    // 仮引数後処理
                    return (AR)Value.Data;
                }
                );
        }

        //-------------------------------------------------------------------------------------
        public static CbFunc<T, RT> Create(string name = "")
        {
            return new CbFunc<T, RT>(name);
        }

        public static CbFunc<T, RT> Create(RT n, string name = "")
        {
            return new CbFunc<T, RT>(n, name);
        }

        /// <summary>
        /// イベントの返り値が null か？
        /// </summary>
        public override bool IsNull => CallBack is null;

        public static Func<ICbValue> TF = () => CbFunc<T, RT>.Create();
        public static Func<string, ICbValue> NTF = (name) => CbFunc<T, RT>.Create(name);

        public static ICbValue GetCbFunc(string name)  // ※リフレクションから参照されている
        {
            return CbFunc<T, RT>.Create(name);
        }
    }
}
