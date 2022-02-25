using CapyCSS.Controls;
using CapyCSS.Controls.BaseControls;
using CbVS.Script;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using static CapyCSS.Controls.BaseControls.CommandCanvas;
using static CapyCSS.Script.ScriptImplement;

namespace CapyCSS.Script
{
    /// <summary>
    /// リフレクションによる自動実装用ファンクションアセット定義クラス
    /// </summary>
    public class AutoImplementFunction 
        : FuncAssetSub
        , IFuncAssetWithArgumentDef
        , IBuildScriptInfo
    {
        static public AutoImplementFunction Create(AutoImplementFunctionInfo info)
        {
            AutoImplementFunction ret = new AutoImplementFunction()
            {
                AssetCode = info.assetCode,
                FuncCode = funcCodeFilter(info),
                hint = info.hint,
                nodeHint = info.nodeHint,
                MenuTitle = info.menuTitle,
                FuncTitle = info.funcTitle,
                ClassType = info.classType,
                ReturnType = info.returnType,
                ArgumentTypeList = info.argumentTypeList,
                DllModule = info.dllModule,
                IsConstructor = info.isConstructor,
                typeRequests = info.typeRequests,
                GenericMethodParameters = info.genericMethodParameters,
                oldSpecification = info.oldSpecification,
                isRunable = info.isRunable,
                IsProperty = info.IsProperty,
            };
            return ret;
        }

        public static string funcCodeFilter(AutoImplementFunctionInfo info)
        {
            string funcCode = info.assetCode;
            if (funcCode.Contains("#"))
            {
                string[] sp = funcCode.Split('#');
                funcCode = sp[0];
            }
            if (funcCode.Contains("."))
            {
                string[] sp = funcCode.Split('.');
                funcCode = sp[sp.Length - 1];
            }

            return funcCode;
        }

        public string AssetCode { get; set; } = "";

        public string FuncCode { get; set; } = "";

        protected string hint = "";

        public string HelpText => hint;

        protected string nodeHint = "";

        public string NodeHelpText => nodeHint;

        public string MenuTitle { get; set; } = "";

        public string FuncTitle { get; set; } = "";

        public IList<TypeRequest> typeRequests { get; set; } = null;

        public Type ClassType { get; set; } = null;

        public IList<ArgumentInfoNode> ArgumentTypeList { get; set; } = null;

        public Func<ICbValue> ReturnType { get; set; } = null;

        /// <summary>
        /// モジュール（DLL）
        /// </summary>
        public Module DllModule = null;

        /// <summary>
        /// コンストラクターか？
        /// </summary>
        public bool IsConstructor { get; set; } = false;

        /// <summary>
        /// ジェネリックメソッドのパラメータ
        /// ※ジェネリックメソッドでないなら null
        /// </summary>
        public Type[] GenericMethodParameters { get; set; } = null;

        /// <summary>
        /// 古い仕様のノードか？
        /// </summary>
        public bool oldSpecification;

        /// <summary>
        /// 任意実行可能ノードか？（RUNボタンが追加される）
        /// </summary>
        public bool isRunable;

        /// <summary>
        /// プロパティのゲッターもしくはセッターか？
        /// </summary>
        public bool IsProperty { get; set; }

        /// <summary>
        /// インスタンスメソッドか？
        /// </summary>
        public bool IsClassInstanceMethod => ArgumentTypeList != null && ArgumentTypeList[0].IsSelf && !IsConstructor;

        /// <summary>
        /// メソッド呼び出し処理を実装する
        /// </summary>
        /// <param name="col">スクリプトのルートノード</param>
        /// <param name="isReBuildMode">再構築か？（保存データからの復帰）</param>
        public virtual bool ImplAsset(MultiRootConnector col, bool isReBuildMode = false)
        {
            string exTitle = GetGenericArgumentsString(col, isReBuildMode);
            return ImplAsset(col, exTitle);
        }

        public string GetGenericArgumentsString(MultiRootConnector col, bool isReBuildMode)
        {
            if (!isReBuildMode && typeRequests != null)
            {
                string exTitle = "";
                foreach (var geneArg in col.SelectedVariableType)
                {
                    if (geneArg != null)
                    {
                        if (exTitle == "")
                            exTitle += "<" + CbSTUtils.GetTypeName(geneArg);
                        else
                            exTitle += ", " + CbSTUtils.GetTypeName(geneArg);
                    }
                }
                return exTitle + ">";
            }
            return null;
        }

        /// <summary>
        /// メソッド呼び出し処理を実装する
        /// </summary>
        /// <param name="col">スクリプトのルートノード</param>
        protected bool ImplAsset(
            MultiRootConnector col, 
            string exTitle)
        {
            Type classType = ClassType;
            string funcTitle = FuncTitle;
            Func<ICbValue> returnType = ReturnType;
            if (classType.IsGenericType)
            {
                // ジェネリッククラスの型を確定する

                var data = ReturnType().Data;
                if (data is CbGeneMethArg gmaType)
                {
                    ICollection<Type> argTypes = new List<Type>();
                    for (int i = 0; i < typeRequests.Count; ++i)
                    {
                        argTypes.Add(col.SelectedVariableType[i]);
                    }

                    //クラスの型を確定した型で差し替える
                    classType = classType.MakeGenericType(argTypes.ToArray());

                    if (IsConstructor)
                    {
                        returnType = CbST.CbCreateTF(classType);
                        if (funcTitle.Contains("<"))
                        {
                            // 確定した型情報に差し替える

                            funcTitle = funcTitle.Substring(0, funcTitle.IndexOf("<"));
                        }
                    }
                }
                else if (CbSTUtils.HaveGenericParamater(classType))
                {
                    // ユーザー選択ではなく直接 self からコピーされた型の場合は、そのまま内容を登録する

                    ICollection<Type> argTypes = new List<Type>();
                    for (int i = 0; i < typeRequests.Count; ++i)
                    {
                        argTypes.Add(col.SelectedVariableType[i]);
                    }

                    //クラスの型を確定した型で差し替える
                    classType = CbST.GetTypeEx(classType.FullName).MakeGenericType(argTypes.ToArray());
                }
            }

            {// 返し値の型を差し替える

                var methodReturnType = returnType();
                if (methodReturnType.MyType == typeof(CbClass<CbGeneMethArg>))
                {
                    // 未確定なジェネリック型を確定した型で差し替える

                    // 引数の型を差し替える
                    Type repType = GetConfirmedType(col, (CbGeneMethArg)methodReturnType.Data);
                    returnType = CbST.CbCreateTF(repType);
                }
            }

            ICollection<ICbValue> argumentTypeList = new List<ICbValue>();
            if (ArgumentTypeList != null)
            {
                // 引数用の変数を用意する

                foreach (var node in ArgumentTypeList)
                {
                    var argumentType = node.CreateArgument();
                    if (argumentType.MyType == typeof(CbClass<CbGeneMethArg>))
                    {
                        // 未確定なジェネリック型を確定した型で差し替える

                        // 引数の型を差し替える
                        Type replaceArgumentType = GetConfirmedType(col, (CbGeneMethArg)argumentType.Data);
                        argumentType = CbST.CbCreate(replaceArgumentType, argumentType.Name);
                    }
                    argumentTypeList.Add(argumentType);
                }
            }

            col.OldSpecification = oldSpecification;
            col.IsRunable = isRunable;
            col.FunctionInfo = this;
            col.MakeFunction(
                funcTitle + exTitle,
                NodeHelpText,
                returnType,
                argumentTypeList,
                new Func<IList<ICbValue>, DummyArgumentsMemento, ICbValue>(
                    (arguments, dummyArguments) =>
                    {
                        var ret = returnType();

                        ImplCallMethod(col, classType, arguments, ret);
                        return ret;
                    }
                )
            );

            argumentTypeList.Clear();

            return true;
        }

        /// <summary>
        /// ジェネリックなパラメータを持つジェネリック型を確定した型に変換します。
        /// </summary>
        /// <param name="col"></param>
        /// <param name="gmaType">CbGeneMethArgのインスタンス</param>
        /// <returns>確定した型</returns>
        private Type GetConfirmedType(MultiRootConnector col, CbGeneMethArg gmaType)
        {
            Type replaceArgumentType = gmaType.ArgumentType;

            if (replaceArgumentType.IsGenericType)
            {
                replaceArgumentType = MakeRequestGenericType(col, replaceArgumentType);
            }
            else if (replaceArgumentType.IsGenericParameter)
            {
                replaceArgumentType = col.GetRequestType(typeRequests, replaceArgumentType.Name);
            }
            else if (replaceArgumentType.ContainsGenericParameters)
            {
                if (replaceArgumentType.IsArray)
                {
                    replaceArgumentType = col.GetRequestType(typeRequests, replaceArgumentType.GetElementType().Name).MakeArrayType();
                }
            }
            else
            {
                Debug.Assert(false);
            }

            return replaceArgumentType;
        }

        /// <summary>
        /// リクエストされた型でジェネリックなパラメータを持つジェネリック型の型を確定します。
        /// </summary>
        /// <param name="col"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        private Type MakeRequestGenericType(MultiRootConnector col, Type type)
        {
            ICollection<Type> argTypes = new List<Type>();
            foreach (var gat in type.GetGenericArguments())
            {
                if (gat.IsGenericType)
                {
                    // パラメータがジェネリックだった

                    var ngt = MakeRequestGenericType(col, gat);
                    argTypes.Add(ngt);
                }
                else if (gat.IsGenericParameter)
                {
                    argTypes.Add(col.GetRequestType(typeRequests, gat.Name));
                }
                else
                {
                    // ジェネリックでないパラメータはそのまま使う

                    argTypes.Add(gat);
                }
            }
            // 確定した型を返す（repType を使って MakeGenericType しては駄目）
            Type nType = CbST.GetTypeEx(type.Namespace + "." + type.Name);
            return nType.MakeGenericType(argTypes.ToArray());
        }

        /// <summary>
        /// メソッド呼び出しの実装です。
        /// </summary>
        /// <param name="col">スクリプトのルートノード</param>
        /// <param name="callArguments">引数リスト</param>
        /// <param name="returnValue">返り値</param>
        private void ImplCallMethod(
            MultiRootConnector col, 
            Type classType,
            IList<ICbValue> callArguments, 
            ICbValue returnValue
            )
        {
            try
            {
                bool isClassInstanceMethod = IsClassInstanceMethod;
                ICollection<object> methodArguments = null;

                methodArguments = SetArguments(
                    col,
                    callArguments,
                    isClassInstanceMethod,
                    methodArguments
                    );

                object classInstance = null;
                if (isClassInstanceMethod)
                {
                    // クラスメソッドの第一引数は、self（this）を受け取るのでクラスインスタンスとして扱う

                    classInstance = getBindObject(callArguments.First(), col);
                    if (classInstance is null)
                    {
                        throw new Exception($"self(this) is invalid.");
                    }
                    // 返却方法が入っていないならセットしておく
                    var cbVSValue = callArguments.First();
                    if (!cbVSValue.IsDelegate && !cbVSValue.IsLiteral && cbVSValue.ReturnAction is null)
                    {
                        if (cbVSValue.IsList)
                        {
                            ICbList cbList = cbVSValue.GetListValue;
                            cbVSValue.ReturnAction = (value) =>
                            {
                                cbList.CopyFrom(value);
                            };
                        }
                        else if (!(cbVSValue is ICbClass))
                        {
                            cbVSValue.ReturnAction = (value) =>
                            {
                                cbVSValue.Data = value;
                            };
                        }
                    }
                }

                object result = CallMethod(
                    classInstance,
                    col,
                    classType,
                    callArguments,
                    isClassInstanceMethod, 
                    methodArguments);

                ProcReturnValue(col, returnValue, result);
            }
            catch (Exception ex)
            {
                col.ExceptionFunc(returnValue, ex);
            }
        }

        /// <summary>
        /// 引数をメソッドに合わせて調整しリストアップします。
        /// </summary>
        /// <param name="col">スクリプトのルートノード</param>
        /// <param name="callArguments">引数リスト</param>
        /// <param name="isClassInstanceMethod">インスタンスメソッドか？</param>
        /// <param name="methodArguments">メソッド呼び出し引数リスト</param>
        /// <returns>メソッド呼び出し引数リスト</returns>
        private ICollection<object> SetArguments(
            MultiRootConnector col, 
            IList<ICbValue> callArguments, 
            bool isClassInstanceMethod,
            ICollection<object> methodArguments
            )
        {
            int argumentIndex = 0;
            if (callArguments.Count > 0)
            {
                methodArguments ??= new List<object>();
            }

            bool isTaskRequest = callArguments.Count(n => n.IsList || n is ICbEvent) > 2;   // タスクで処理する条件

            if (isTaskRequest)
            {
                // タスクを使った処理

                List<Task<object>> tasks = new List<Task<object>>();
                for (int i = 0; i < callArguments.Count; ++i)
                {
                    var node = callArguments[argumentIndex++];
                    CheckArgument(node);

                    if (i == 0 && isClassInstanceMethod)
                    {
                        // クラスメソッドの第一引数は、self（this）を受け取るのでメソッド呼び出しの引数にしない

                        continue;
                    }

                    tasks.Add(Task.Run(() =>
                    {
                        return getBindObject(node, col);
                    }));
                }
                foreach (var task in tasks)
                {
                    methodArguments.Add(task.Result);
                }
            }
            else
            {
                // 通常処理

                for (int i = 0; i < callArguments.Count; ++i)
                {
                    var node = callArguments[argumentIndex++];
                    CheckArgument(node);

                    if (i == 0 && isClassInstanceMethod)
                    {
                        // クラスメソッドの第一引数は、self（this）を受け取るのでメソッド呼び出しの引数にしない

                        continue;
                    }

                    methodArguments.Add(getBindObject(node, col));
                }
            }

            return methodArguments;
        }

        /// <summary>
        /// オリジナル型の値に変換します。
        /// </summary>
        /// <param name="value"></param>
        /// <param name="col"></param>
        /// <param name="dummyArgumentsStack"></param>
        /// <returns></returns>
        private object getBindObject(
            ICbValue value,
            MultiRootConnector col
            )
        {
            if (value.IsList)
            {
                ICbList cbList = value.GetListValue;

                // リストは、オリジナルの型のインスタンスを用意する

                return cbList.ConvertOriginalTypeList();
            }
            else if (value is ICbEvent cbEvent)
            {
                // Func<> 及び Action<> は、オリジナルの型のインスタンスを用意する

                if (cbEvent.Callback is null)
                {
                    return null;
                }
                else
                {
                    return cbEvent.GetCallbackOriginalType();
                }
            }
            if (value.IsNull)
            {
                return null;
            }
            return value.Data;
        }

        /// <summary>
        /// メソッドを実行します。
        /// </summary>
        /// <param name="col">スクリプトのルートノード</param>
        /// <param name="callArguments">引数リスト</param>
        /// <param name="variableIds">スクリプト変数IDリスト</param>
        /// <param name="isClassInstanceMethod">インスタンスメソッドか？</param>
        /// <param name="methodArguments">メソッド呼び出し引数リスト</param>
        /// <returns>メソッドの返り値</returns>
        private object CallMethod(
            object classInstance,
            MultiRootConnector col,
            Type classType,
            IList<ICbValue> callArguments, 
            bool isClassInstanceMethod, 
            IEnumerable<object> methodArguments
            )
        {
            object result;

            if (classInstance != null && callArguments.Count == 1)
            {
                if (FuncCode == "Dispose")
                {
                    // Disposeメソッドを実行するとUI上で破棄された値を表示しようとするので Dispose は無視する
                    // ※値は Data のリファレンスで繋がっているので、対策として状態を残すのは簡単では無い

                    Console.WriteLine(CapyCSS.Language.Instance["Help:Dispose"]);
                    return null;
                }
            }

            if (IsConstructor)
            {
                // new されたコンストラクタとして振る舞う

                if (methodArguments is null)
                {
                    result = Activator.CreateInstance(classType);
                }
                else
                {
                    object[] args = methodArguments.ToArray();
                    result = Activator.CreateInstance(classType, args);

                    ReturnArgumentsValue(classInstance, callArguments, args);
                }
            }
            else if (GenericMethodParameters != null)
            {
                // ジェネリックメソッド

                IEnumerable<Type> genericParams = GenericMethodParameters.Select(n => col.GetRequestType(typeRequests, n.Name));
                if (methodArguments is null)
                {
                    // 引数のないメソッドを型で補完して呼ぶ

                    result = InvokeGenericMethod(classType, genericParams, new object[] { });
                }
                else
                {
                    // 引数ありのメソッド

                    object[] args = methodArguments.ToArray();

                    // 引数の型リストを作成
                    IEnumerable<Type> argTypes = callArguments.Select(n => n.OriginalType);
                    if (classType.GetMethod(FuncCode, argTypes.ToArray()) != null)
                    {
                        // 同じ型のメソッドが既に定義されているのでそちらを呼ぶ

                        result = InvokeMethodWithArguments(classType, classInstance, args);
                    }
                    else
                    {
                        // ジェネリックメソッドを型で補完して呼ぶ

                        result = InvokeGenericMethod(classType, genericParams, args);
                    }
                    ReturnArgumentsValue(classInstance, callArguments, args);
                }
            }
            else if (methodArguments is null)
            {
                // 引数のないメソッド

                result = InvokeMethod(classType, classInstance);
            }
            else
            {
                // 引数ありのメソッド

                object[] args = methodArguments.ToArray();
                result = InvokeMethodWithArguments(classType, classInstance, args);
                ReturnArgumentsValue(classInstance, callArguments, args);
            }

            return result;
        }

        /// <summary>
        /// ジェネリックメソッドを作成して呼びます。
        /// </summary>
        /// <param name="classType">所属するクラスの型</param>
        /// <param name="genericParams">ジェネリックパラメータ</param>
        /// <param name="args">呼び出す時の引数リスト</param>
        /// <returns>呼び出したメソッドの返し値</returns>
        private object InvokeGenericMethod(Type classType, IEnumerable<Type> genericParams, object[] args)
        {
            var attr = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public;

            // ジェネリックメソッドを抽出
            var methods = classType.GetMethods(attr).Where(n => n.IsGenericMethod && n.IsGenericMethodDefinition && n.ContainsGenericParameters);

            // メソッド名と引数の数でフィルタリング
            methods = methods.Where(n => n.Name == FuncCode && n.GetParameters().Length == args.Length);

            if (methods.Count() == 0)
            {
                throw new Exception($"{FuncCode}{CbSTUtils.GetGenericParamatersString(args.Select(n => n.GetType()).ToArray(), "(", ")")} method not found.");
            }

            if (methods.Count() == 1)
            {
                // 該当が一件だけ有った

                return methods.First()
                    .MakeGenericMethod(genericParams.ToArray())
                    .Invoke(classType, args);
            }

            // 引数の一致で探す
            var sample = methods.Where(n =>
            {
                var parameters = n.GetParameters();
                for (int i = 0; i < args.Length; ++i)
                {
                    if (args[i].GetType() != parameters[i].ParameterType)
                        return false;
                }
                return true;
            });
            if (sample.Count() == 1)
            {
                return sample.First()
                    .MakeGenericMethod(genericParams.ToArray())
                    .Invoke(classType, args);
            }

            // ジェネリックの比較で探す（手抜き...）
            methods = methods.Where(n =>
            {
                var parameters = n.GetParameters();
                for (int i = 0; i < args.Length; ++i)
                {
                    if (!args[i].GetType().IsGenericType)
                    {
                        if (parameters[i].ParameterType.IsGenericParameter)
                            continue;   // ジェネリックパラメータを一律一つのワイルドカードとして扱う（手抜き...）
                                        // ※ジェネリックパラメータはいくつか違うタイプが設定されている可能性がある
                    }
                    if (args[i].GetType().IsGenericType && parameters[i].ParameterType.IsGenericType)
                    {
                        // 両方ともジェネリックタイプなら型の名前の一致だけ見る（手抜き...）
                        // ※ジェネリックパラメータを全く無視している

                        if (args[i].GetType().Name == parameters[i].ParameterType.Name)
                            continue;
                    }
                    if (args[i].GetType() != parameters[i].ParameterType)
                        return false;   // 型が一致しないなら不一致と確定する
                }
                return true;
            });
            if (methods.Count() != 1)
            {
                throw new Exception($"{FuncCode}{CbSTUtils.GetGenericParamatersString(args.Select(n => n.GetType()).ToArray(), "(", ")")} method not found.");
            }

            // 該当が一件だけ有った
            return methods.First()
                .MakeGenericMethod(genericParams.ToArray())
                .Invoke(classType, args);
        }

        /// <summary>
        /// 引数有りでメソッドを呼びます。
        /// </summary>
        /// <param name="classType">所属するクラスの型</param>
        /// <param name="classInstance">クラスインスタンス</param>
        /// <param name="args">呼び出す時の引数リスト</param>
        /// <returns>呼び出したメソッドの返し値</returns>
        private object InvokeMethodWithArguments(Type classType, object classInstance, object[] args)
        {
            return classType.InvokeMember(
                            FuncCode,
                            BindingFlags.InvokeMethod,
                            null,
                            classInstance,
                            args,
                            Language.Culture
                            );
        }

        /// <summary>
        /// 引数無しでメソッドを呼びます。
        /// </summary>
        /// <param name="classType">所属するクラスの型</param>
        /// <param name="classInstance">クラスインスタンス</param>
        /// <returns>呼び出したメソッドの返し値</returns>
        private object InvokeMethod(Type classType, object classInstance)
        {
            return classType.InvokeMember(
                            FuncCode,
                            BindingFlags.InvokeMethod,
                            null,
                            classInstance,
                            new object[] { },
                            Language.Culture
                            );
        }

        /// <summary>
        /// メソッド呼び出しの引数が参照渡しの場合、変更値を基の管理者に戻します。
        /// </summary>
        /// <param name="classInstance">呼び出し時のクラスインスタンス（変更が入っている）</param>
        /// <param name="callArguments">呼び出し用に参照した引数リスト</param>
        /// <param name="args">呼び出し時の引数リスト（変更が入っている）</param>
        private void ReturnArgumentsValue(
            object classInstance,
            IList<ICbValue> callArguments,
            object[] args)
        {
            int argStartIndex = 0;
            if (classInstance != null)
            {
                // 参照渡しのため変更後の値を戻す（直接リファレンスが渡されていないケース）

                var scrArg = callArguments[0];
                if (scrArg.IsDelegate)
                {
                    // デリゲートは無条件で更新値を捨てる（ローカル変数の破棄に該当）

                    Debug.Assert(callArguments[0].ReturnAction is null);
                }
                else if (callArguments[0].IsLiteral)
                {
                    // リレラルは更新情報を捨てる（ローカル変数の破棄に該当）
                }
                else
                {
                    // 変更後の値を戻す

                    scrArg.ReturnAction?.Invoke(classInstance);
                }
                argStartIndex++;
            }
            for (int i = argStartIndex; i < callArguments.Count; ++i)
            {
                var scrArg = callArguments[i];

                if (scrArg.IsDelegate)
                {
                    // デリゲートは更新情報を捨てる

                    Debug.Assert(callArguments[i].ReturnAction is null);
                    continue;
                }
                else if (scrArg.IsLiteral)
                {
                    // リレラルは更新情報を捨てる

                    continue;
                }

                // 参照渡しのため変更後の値を戻す
                scrArg.ReturnAction?.Invoke(args[i]);
            }
        }

        /// <summary>
        /// 返り値を処理します。
        /// </summary>
        /// <param name="col">スクリプトのルートノード</param>
        /// <param name="returnValue">返り値を格納する変数</param>
        /// <param name="result">メソッド呼び出しの返り値</param>
        private static void ProcReturnValue(MultiRootConnector col, ICbValue returnValue, object result)
        {
            if (returnValue.IsList)
            {
                ICbList retCbList = returnValue.GetListValue;
                retCbList.CopyFrom(result);
                col.LinkConnectorControl.UpdateValueData();
            }
            else if (result != null && returnValue is ICbEvent cbEvent)
            {
                // デリゲート型の返し値

                Type resultType = CbSTUtils.GetDelegateReturnType(result.GetType());

                if (!CbSTUtils.IsVoid(resultType))
                {
                    // 返し値のあるデリゲート型

                    cbEvent.Callback = (dummyArguments) =>
                    {
                        ICbValue retTypeValue = null;
                        try
                        {
                            // イベントからの返り値を取得
                            object tempReturnValue = ((dynamic)result).Invoke(dummyArguments.GetValue().Data);

                            // 直接メソッドを呼ぶため帰ってくるのは通常の値なので Cb タイプに変換する
                            retTypeValue = CbST.CbCreate(tempReturnValue.GetType());
                            retTypeValue.Data = tempReturnValue;
                        }
                        catch (Exception ex)
                        {
                            col.ExceptionFunc(null, ex);
                        }
                        return retTypeValue;
                    };
                }
                else
                {
                    // 返し値の無いデリゲート型

                    cbEvent.Callback = (dummyArguments) =>
                    {
                        try
                        {
                            var argType = dummyArguments.GetValue();
                            if (argType is null)
                                ((dynamic)result).Invoke(null);
                            else
                                ((dynamic)result).Invoke(argType.Data);
                        }
                        catch (Exception ex)
                        {
                            col.ExceptionFunc(null, ex);
                        }
                        return new CbVoid();
                    };
                }
            }
            else
                returnValue.Data = result;
        }

        public static VariableGetter CreateVariableGetter(MultiRootConnector col, string funcTitle, int index)
        {
            return new VariableGetter(
                    col,
                    (name) =>
                    {
                        string title = "";
                        try
                        {
                            title = string.Format(funcTitle, name);
                        }
                        catch (Exception)
                        {
                            title = "** Name Error **";
                        }
                        return title;
                    },
                    index
                    );
        }
    }
}
