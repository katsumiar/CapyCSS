using CapyCSS.Script;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using static CapyCSS.Script.ScriptImplement;

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
        public CbFuncArguments()
        {
            Values = new List<ICbValue>();
            for (int i = 0; i < Enum.GetNames(typeof(CbFuncArguments.INDEX)).Length; ++i)
            {
                Values.Add(null);
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
    }

    public interface ICbDelegate
    {
        Type OriginalType { get; }
    }

    public interface ICbEvent 
        : ICbValue
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
        bool IsCallback { get; }
        /// <summary>
        /// Func<> もしくは Action<> の内包したコールバックを参照します。
        /// </summary>
        Func<DummyArgumentsMemento, object> Callback { get; set; }
        /// <summary>
        /// コールバックの返し値を参照します。
        /// </summary>
        ICbValue Value { get; set; }
        /// <summary>
        /// コールバックを呼び出します。
        /// </summary>
        /// <param name="dummyArguments"></param>
        void InvokeCallback(DummyArgumentsMemento dummyArguments);
        /// <summary>
        /// オリジナルの型でコールバックを取得します。
        /// </summary>
        /// <returns></returns>
        object GetCallbackOriginalType();
        /// <summary>
        /// デリゲートの引数の数を参照します。
        /// </summary>
        int ArgumentsNum { get; }
        /// <summary>
        /// デリゲートの返し値の型名を参照します。
        /// </summary>
        string ReturnTypeName { get; }
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

            object result = cbFuncType.InvokeMember(
                        nameof(CbFunc<int, CbInt>.GetCbFunc),
                        BindingFlags.InvokeMethod,
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

            object result = cbFuncType.InvokeMember(
                        nameof(CbFunc<int, CbInt>.GetCbFunc),
                        BindingFlags.InvokeMethod,
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

            object result = cbFuncType.InvokeMember(
                        nameof(CbFunc<int, CbInt>.GetCbFunc),
                        BindingFlags.InvokeMethod,
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

            object result = cbFuncType.InvokeMember(
                        nameof(CbFunc<int, CbInt>.GetCbFunc),
                        BindingFlags.InvokeMethod,
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
                        .InvokeMember(
                            nameof(CbFunc<int, CbInt>.GetCbFunc),
                            BindingFlags.InvokeMethod,
                            null, null, new object[] { name }) as ICbValue;
            return result as ICbValue;
        }

        private static ICbValue CallGetCbFunc(ICbValue ret, string name, Type originalType)
        {
            object result = GetFuncType(originalType, ret.OriginalReturnType)
                        .InvokeMember(
                            nameof(CbFunc<int, CbInt>.GetCbFunc),
                            BindingFlags.InvokeMethod,
                            null, null, new object[] { name }) as ICbValue;
            return result as ICbValue;
        }

        public static bool IsEventType(Type type)
        {
            return CbSTUtils.IsDelegate(type);
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
                var arg = node.CreateArgument();
                if (arg is ICbEvent)
                {
                    return true;
                }

                if (arg.MyType == typeof(CbClass<CbGeneMethArg>))
                {
                    var gma = (CbGeneMethArg)arg.Data;
                    if (gma.IsEvent)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }

    /// <summary>
    /// Func<> 型クラス
    /// </summary>
    /// <typeparam name="T">オリジナルの型</typeparam>
    /// <typeparam name="RT">CbXXX型の返し値の型</typeparam>
    public class CbFunc<T, RT>
        : BaseCbValueClass<ICbValue>
        , ICbValueClass<ICbValue>
        , ICbEvent
         where RT : class, ICbValue
    {
        public override Type MyType => typeof(CbFunc<T, RT>);

        public override Type OriginalReturnType => typeof(RT);

        public override Type OriginalType => typeof(T);

        public int ArgumentsNum
        {
            get
            {
                MethodInfo signature = OriginalType.GetMethod("Invoke");
                return signature.GetParameters().Length;
            }
        }

        public string ReturnTypeName 
        { 
            get
            {
                MethodInfo signature = OriginalType.GetMethod("Invoke");
                return signature.ReturnType.Name;
            }
        }

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
        public Func<DummyArgumentsMemento, object> Callback { get; set; } = null;

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
                    Callback = cbEvent.Callback;
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
                return CbSTUtils.GetTypeName(OriginalType);
            }
        }

        public bool IsCallback => Callback != null;

        public void InvokeCallback(DummyArgumentsMemento dummyArguments)
        {
            if (CbVoid.Is(typeof(RT)))
            {
                // Action用

                Callback?.Invoke(dummyArguments);
            }
            else if (typeof(RT) == typeof(CbObject))
            {
                Debug.Assert(false);
            }
            else if (typeof(RT).IsGenericType && typeof(RT).GetGenericTypeDefinition() == typeof(CbFunc<,>))
            {
                Debug.Assert(false);
            }
            else
            {
                // Func用

                ICbValue result = Callback?.Invoke(dummyArguments) as ICbValue;
                if (!result.IsNull)
                {
                    Value = CbST.CbCreate(result.Data.GetType());
                    Value.Set(result);
                }
                else
                {
                    Value = result;
                }
            }
        }

        public override ICbValue Value
        {
            get
            {
                if (Callback is null)
                    return null;
                return _value;
            }
            set
            {
                _value = value;
            }
        }

        /// <summary>
        /// 値のUI上の文字列表現
        /// </summary>
        public override string ValueUIString
        {
            get
            {
                string baseName = "[" + TypeName + "()]";
                if (IsError)
                    return CbSTUtils.ERROR_STR;
                if (IsNull)
                    return baseName + CbSTUtils.UI_NULL_STR;
                return baseName;
            }
        }

        /// <summary>
        /// 変数の持つ値は null か？
        /// </summary>
        public override bool IsNull => Callback is null;

        /// <summary>
        /// 値の文字列表現
        /// </summary>
        public override string ValueString
        {
            get
            {
                if (IsNull)
                {
                    return CbSTUtils.NULL_STR;
                }
                else
                {
                    return CbSTUtils.DELEGATE_STR;
                }
            }
            set => new NotImplementedException();
        }

        public override bool IsDelegate => true;

        public override bool IsReadOnlyValue { get; set; } = true;

        public override bool IsStringableValue => false;

        public object GetCallbackOriginalType()
        {
            return ConvertOriginalType();
        }

        /// <summary>
        /// Cbクラスでラッピングしていない元々の型（T）の Func<> 及び Action<> を返す機能を取得します。
        /// </summary>
        /// <returns></returns>
        public object ConvertOriginalType()
        {
            // デリゲート型の情報を取得
            var targetType = typeof(T);

            // パラメータ情報にこのクラス（CbFunc<T, RT>）のジェネリックパラメータの型を入れる
            List<Type> selfParams = new List<Type>();
            selfParams.Add(targetType);
            selfParams.Add(typeof(RT));

            // デリゲート型の返し値の型を取得
            var paramTypes = CbSTUtils.GetDelegateParameterTypes(targetType);

            // デリゲート型の引数の型を取得
            var retType = CbSTUtils.GetDelegateReturnType(targetType);

            // パラメータ情報にデリゲート型のパラメータの型を入れる
            int index = 0;
            if (paramTypes.Length != 0)
            {
                selfParams.AddRange(paramTypes);
                index += paramTypes.Length;
            }
            if (!CbSTUtils.IsVoid(retType))
            {
                // 返し値が Void 以外ならパラメータ情報にデリゲート型の返し値の型を入れる

                selfParams.Add(retType);
                index++;
            }
            else
            {
                retType = null;
            }
            index--;

            if (retType is null)
            {
                // 返し値の無いデリゲート

                if (paramTypes.Length == 0)
                {
                    // Action を作成し、デリゲートにバインドする

                    var instanceType1 = typeof(DelegateAction);
                    var instance1 = Activator.CreateInstance(instanceType1, new object[] { this });
                    MethodInfo method1 = instanceType1.GetMethod("Method");
                    return Delegate.CreateDelegate(OriginalType, instance1, method1);
                }

                // Action<...> を作成し、デリゲートにバインドする
                Type[] actionTypes = new Type[]
                {
                    typeof(DelegateAction<>),
                    typeof(DelegateAction<,>),
                    typeof(DelegateAction<,,>),
                    typeof(DelegateAction<,,,>),
                    typeof(DelegateAction<,,,,>),
                    typeof(DelegateAction<,,,,,>),
                    typeof(DelegateAction<,,,,,,>),
                    typeof(DelegateAction<,,,,,,,>),
                    typeof(DelegateAction<,,,,,,,,>),
                    typeof(DelegateAction<,,,,,,,,,>),
                    typeof(DelegateAction<,,,,,,,,,,>),
                    typeof(DelegateAction<,,,,,,,,,,,>),
                    typeof(DelegateAction<,,,,,,,,,,,,>),
                    typeof(DelegateAction<,,,,,,,,,,,,,>),
                    typeof(DelegateAction<,,,,,,,,,,,,,,>),
                    typeof(DelegateAction<,,,,,,,,,,,,,,,>),
                };

                var instanceType2 = actionTypes[index].MakeGenericType(selfParams.ToArray());
                var instance2 = Activator.CreateInstance(instanceType2, new object[] { this });
                MethodInfo method2 = instanceType2.GetMethod("Method");
                return Delegate.CreateDelegate(OriginalType, instance2, method2);
            }

            // Func<...> を作成し、デリゲートにバインドする
            Type[] funcTypes = new Type[]
            {
                typeof(DelegateFunction<>),
                typeof(DelegateFunction<,>),
                typeof(DelegateFunction<,,>),
                typeof(DelegateFunction<,,,>),
                typeof(DelegateFunction<,,,,>),
                typeof(DelegateFunction<,,,,,>),
                typeof(DelegateFunction<,,,,,,>),
                typeof(DelegateFunction<,,,,,,,>),
                typeof(DelegateFunction<,,,,,,,,>),
                typeof(DelegateFunction<,,,,,,,,,>),
                typeof(DelegateFunction<,,,,,,,,,,>),
                typeof(DelegateFunction<,,,,,,,,,,,>),
                typeof(DelegateFunction<,,,,,,,,,,,,>),
                typeof(DelegateFunction<,,,,,,,,,,,,,>),
                typeof(DelegateFunction<,,,,,,,,,,,,,,>),
                typeof(DelegateFunction<,,,,,,,,,,,,,,,>),
                typeof(DelegateFunction<,,,,,,,,,,,,,,,,>),
            };

            var instanceType = funcTypes[index].MakeGenericType(selfParams.ToArray());
            var instance = Activator.CreateInstance(instanceType, new object[] { this });
            MethodInfo method = instanceType.GetMethod("Method");
            return Delegate.CreateDelegate(OriginalType, instance, method);
        }
        //=====================================================================================
        public class DelegateActionBase
            : ICbDelegate
        {
            public Type OriginalType => self.OriginalType;
            private CbFunc<T, RT> self;
            protected DummyArgumentsMemento dummyArguments;
            protected DelegateActionBase(CbFunc<T, RT> self)
            {
                this.self = self;
                this.dummyArguments = new DummyArgumentsMemento();
            }
            protected void Callback()
            {
                self.InvokeCallback(dummyArguments);
                dummyArguments.Unregist();
            }
            protected void ArgumentlessCallback()
            {
                self.InvokeCallback(dummyArguments);
            }
        }
        //=====================================================================================
        public class DelegateFunctionBase<TResult>
        {
            private CbFunc<T, RT> self;
            protected DummyArgumentsMemento dummyArguments;
            protected DelegateFunctionBase(CbFunc<T, RT> self)
            {
                this.self = self;
                this.dummyArguments = new DummyArgumentsMemento();
            }
            protected TResult Callback()
            {
                self.InvokeCallback(dummyArguments);
                dummyArguments.Unregist();
                return (TResult)self.Value.Data;
            }
        }
        //=====================================================================================
        public class DelegateAction
            : DelegateActionBase
        {
            public DelegateAction(CbFunc<T, RT> self) : base(self) { }
            public void Method()
            {
                ArgumentlessCallback();
            }
        }
        //=====================================================================================
        public class DelegateAction<T1>
            : DelegateActionBase
        {
            public DelegateAction(CbFunc<T, RT> self) : base(self) { }
            public void Method(T1 arg)
            {
                dummyArguments.Regist(arg);    // 仮引数に引数を登録
                Callback();
            }
        }
        //------------------------------------------------------
        public class DelegateAction<T1, T2>
            : DelegateActionBase
        {
            public DelegateAction(CbFunc<T, RT> self) : base(self) { }
            public void Method(T1 arg1, T2 arg2)
            {
                dummyArguments.Regist(arg1, arg2);    // 仮引数に引数を登録
                Callback();
            }
        }
        //------------------------------------------------------
        public class DelegateAction<T1, T2, T3>
            : DelegateActionBase
        {
            public DelegateAction(CbFunc<T, RT> self) : base(self) { }
            public void Method(T1 arg1, T2 arg2, T3 arg3)
            {
                dummyArguments.Regist(arg1, arg2, arg3);    // 仮引数に引数を登録
                Callback();
            }
        }
        //------------------------------------------------------
        public class DelegateAction<T1, T2, T3, T4>
            : DelegateActionBase
        {
            public DelegateAction(CbFunc<T, RT> self) : base(self) { }
            public void Method(T1 arg1, T2 arg2, T3 arg3, T4 arg4)
            {
                dummyArguments.Regist(arg1, arg2, arg3, arg4);    // 仮引数に引数を登録
                Callback();
            }
        }
        //------------------------------------------------------
        public class DelegateAction<T1, T2, T3, T4, T5>
            : DelegateActionBase
        {
            public DelegateAction(CbFunc<T, RT> self) : base(self) { }
            public void Method(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
            {
                dummyArguments.Regist(arg1, arg2, arg3, arg4, arg5);    // 仮引数に引数を登録
                Callback();
            }
        }
        //------------------------------------------------------
        public class DelegateAction<T1, T2, T3, T4, T5, T6>
            : DelegateActionBase
        {
            public DelegateAction(CbFunc<T, RT> self) : base(self) { }
            public void Method(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
            {
                dummyArguments.Regist(arg1, arg2, arg3, arg4, arg5, arg6);    // 仮引数に引数を登録
                Callback();
            }
        }
        //------------------------------------------------------
        public class DelegateAction<T1, T2, T3, T4, T5, T6, T7>
            : DelegateActionBase
        {
            public DelegateAction(CbFunc<T, RT> self) : base(self) { }
            public void Method(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7)
            {
                dummyArguments.Regist(arg1, arg2, arg3, arg4, arg5, arg6, arg7);    // 仮引数に引数を登録
                Callback();
            }
        }
        //------------------------------------------------------
        public class DelegateAction<T1, T2, T3, T4, T5, T6, T7, T8>
            : DelegateActionBase
        {
            public DelegateAction(CbFunc<T, RT> self) : base(self) { }
            public void Method(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8)
            {
                dummyArguments.Regist(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);    // 仮引数に引数を登録
                Callback();
            }
        }
        //------------------------------------------------------
        public class DelegateAction<T1, T2, T3, T4, T5, T6, T7, T8, T9>
            : DelegateActionBase
        {
            public DelegateAction(CbFunc<T, RT> self) : base(self) { }
            public void Method(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9)
            {
                dummyArguments.Regist(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9);    // 仮引数に引数を登録
                Callback();
            }
        }
        //------------------------------------------------------
        public class DelegateAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>
            : DelegateActionBase
        {
            public DelegateAction(CbFunc<T, RT> self) : base(self) { }
            public void Method(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10)
            {
                dummyArguments.Regist(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10);    // 仮引数に引数を登録
                Callback();
            }
        }
        //------------------------------------------------------
        public class DelegateAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>
            : DelegateActionBase
        {
            public DelegateAction(CbFunc<T, RT> self) : base(self) { }
            public void Method(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11)
            {
                dummyArguments.Regist(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11);    // 仮引数に引数を登録
                Callback();
            }
        }
        //------------------------------------------------------
        public class DelegateAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>
            : DelegateActionBase
        {
            public DelegateAction(CbFunc<T, RT> self) : base(self) { }
            public void Method(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12)
            {
                dummyArguments.Regist(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12);    // 仮引数に引数を登録
                Callback();
            }
        }
        //------------------------------------------------------
        public class DelegateAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>
            : DelegateActionBase
        {
            public DelegateAction(CbFunc<T, RT> self) : base(self) { }
            public void Method(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13)
            {
                dummyArguments.Regist(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13);    // 仮引数に引数を登録
                Callback();
            }
        }
        //------------------------------------------------------
        public class DelegateAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>
            : DelegateActionBase
        {
            public DelegateAction(CbFunc<T, RT> self) : base(self) { }
            public void Method(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14)
            {
                dummyArguments.Regist(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14);    // 仮引数に引数を登録
                Callback();
            }
        }
        //------------------------------------------------------
        public class DelegateAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>
            : DelegateActionBase
        {
            public DelegateAction(CbFunc<T, RT> self) : base(self) { }
            public void Method(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15)
            {
                dummyArguments.Regist(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15);    // 仮引数に引数を登録
                Callback();
            }
        }
        //------------------------------------------------------
        public class DelegateAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>
            : DelegateActionBase
        {
            public DelegateAction(CbFunc<T, RT> self) : base(self) { }
            public void Method(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15, T16 arg16)
            {
                dummyArguments.Regist(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15, arg16);    // 仮引数に引数を登録
                Callback();
            }
        }


        //=====================================================================================
        public class DelegateFunction<TResult>
           : DelegateFunctionBase<TResult>
        {
            public DelegateFunction(CbFunc<T, RT> self) : base(self) { }
            public TResult Method()
            {
                return Callback();
            }
        }
        //------------------------------------------------------
        public class DelegateFunction<T1, TResult>
            : DelegateFunctionBase<TResult>
        {
            public DelegateFunction(CbFunc<T, RT> self) : base(self) { }
            public TResult Method(T1 arg)
            {
                dummyArguments.Regist(arg);    // 仮引数に引数を登録
                return Callback();
            }
        }
        //------------------------------------------------------
        public class DelegateFunction<T1, T2, TResult>
            : DelegateFunctionBase<TResult>
        {
            public DelegateFunction(CbFunc<T, RT> self) : base(self) { }
            public TResult Method(T1 arg1, T2 arg2)
            {
                dummyArguments.Regist(arg1, arg2);    // 仮引数に引数を登録
                return Callback();
            }
        }
        //------------------------------------------------------
        public class DelegateFunction<T1, T2, T3, TResult>
            : DelegateFunctionBase<TResult>
        {
            public DelegateFunction(CbFunc<T, RT> self) : base(self) { }
            public TResult Method(T1 arg1, T2 arg2, T3 arg3)
            {
                dummyArguments.Regist(arg1, arg2, arg3);    // 仮引数に引数を登録
                return Callback();
            }
        }
        //------------------------------------------------------
        public class DelegateFunction<T1, T2, T3, T4, TResult>
            : DelegateFunctionBase<TResult>
        {
            public DelegateFunction(CbFunc<T, RT> self) : base(self) { }
            public TResult Method(T1 arg1, T2 arg2, T3 arg3, T4 arg4)
            {
                dummyArguments.Regist(arg1, arg2, arg3, arg4);    // 仮引数に引数を登録
                return Callback();
            }
        }
        //------------------------------------------------------
        public class DelegateFunction<T1, T2, T3, T4, T5, TResult>
            : DelegateFunctionBase<TResult>
        {
            public DelegateFunction(CbFunc<T, RT> self) : base(self) { }
            public TResult Method(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
            {
                dummyArguments.Regist(arg1, arg2, arg3, arg4, arg5);    // 仮引数に引数を登録
                return Callback();
            }
        }
        //------------------------------------------------------
        public class DelegateFunction<T1, T2, T3, T4, T5, T6, TResult>
            : DelegateFunctionBase<TResult>
        {
            public DelegateFunction(CbFunc<T, RT> self) : base(self) { }
            public TResult Method(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
            {
                dummyArguments.Regist(arg1, arg2, arg3, arg4, arg5, arg6);    // 仮引数に引数を登録
                return Callback();
            }
        }
        //------------------------------------------------------
        public class DelegateFunction<T1, T2, T3, T4, T5, T6, T7, TResult>
            : DelegateFunctionBase<TResult>
        {
            public DelegateFunction(CbFunc<T, RT> self) : base(self) { }
            public TResult Method(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7)
            {
                dummyArguments.Regist(arg1, arg2, arg3, arg4, arg5, arg6, arg7);    // 仮引数に引数を登録
                return Callback();
            }
        }
        //------------------------------------------------------
        public class DelegateFunction<T1, T2, T3, T4, T5, T6, T7, T8, TResult>
            : DelegateFunctionBase<TResult>
        {
            public DelegateFunction(CbFunc<T, RT> self) : base(self) { }
            public TResult Method(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8)
            {
                dummyArguments.Regist(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);    // 仮引数に引数を登録
                return Callback();
            }
        }
        //------------------------------------------------------
        public class DelegateFunction<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>
            : DelegateFunctionBase<TResult>
        {
            public DelegateFunction(CbFunc<T, RT> self) : base(self) { }
            public TResult Method(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9)
            {
                dummyArguments.Regist(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9);    // 仮引数に引数を登録
                return Callback();
            }
        }
        //------------------------------------------------------
        public class DelegateFunction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult>
            : DelegateFunctionBase<TResult>
        {
            public DelegateFunction(CbFunc<T, RT> self) : base(self) { }
            public TResult Method(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10)
            {
                dummyArguments.Regist(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10);    // 仮引数に引数を登録
                return Callback();
            }
        }
        //------------------------------------------------------
        public class DelegateFunction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult>
            : DelegateFunctionBase<TResult>
        {
            public DelegateFunction(CbFunc<T, RT> self) : base(self) { }
            public TResult Method(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11)
            {
                dummyArguments.Regist(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11);    // 仮引数に引数を登録
                return Callback();
            }
        }
        //------------------------------------------------------
        public class DelegateFunction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult>
            : DelegateFunctionBase<TResult>
        {
            public DelegateFunction(CbFunc<T, RT> self) : base(self) { }
            public TResult Method(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12)
            {
                dummyArguments.Regist(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12);    // 仮引数に引数を登録
                return Callback();
            }
        }
        //------------------------------------------------------
        public class DelegateFunction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult>
            : DelegateFunctionBase<TResult>
        {
            public DelegateFunction(CbFunc<T, RT> self) : base(self) { }
            public TResult Method(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13)
            {
                dummyArguments.Regist(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13);    // 仮引数に引数を登録
                return Callback();
            }
        }
        //------------------------------------------------------
        public class DelegateFunction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult>
            : DelegateFunctionBase<TResult>
        {
            public DelegateFunction(CbFunc<T, RT> self) : base(self) { }
            public TResult Method(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14)
            {
                dummyArguments.Regist(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14);    // 仮引数に引数を登録
                return Callback();
            }
        }
        //------------------------------------------------------
        public class DelegateFunction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult>
            : DelegateFunctionBase<TResult>
        {
            public DelegateFunction(CbFunc<T, RT> self) : base(self) { }
            public TResult Method(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15)
            {
                dummyArguments.Regist(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15);    // 仮引数に引数を登録
                return Callback();
            }
        }
        //------------------------------------------------------
        public class DelegateFunction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult>
            : DelegateFunctionBase<TResult>
        {
            public DelegateFunction(CbFunc<T, RT> self) : base(self) { }
            public TResult Method(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15, T16 arg16)
            {
                dummyArguments.Regist(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15, arg16);    // 仮引数に引数を登録
                return Callback();
            }
        }

        //=====================================================================================
        public static CbFunc<T, RT> Create(string name = "") => new CbFunc<T, RT>(name);

        public static CbFunc<T, RT> Create(RT n, string name = "") => new CbFunc<T, RT>(n, name);

        public static Func<ICbValue> TF = () => Create();
        public static Func<string, ICbValue> NTF = (name) => Create(name);

        public static ICbValue GetCbFunc(string name)  // ※リフレクションから参照されている
        {
            return Create(name);
        }

        private bool disposedValue;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    ClearWork();
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
