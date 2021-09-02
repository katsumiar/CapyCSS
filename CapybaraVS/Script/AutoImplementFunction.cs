using CapybaraVS.Controls;
using CapybaraVS.Controls.BaseControls;
using CbVS.Script;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using static CapybaraVS.Controls.BaseControls.CommandCanvas;
using static CapybaraVS.Script.ScriptImplement;

namespace CapybaraVS.Script
{
    /// <summary>
    /// リフレクションによる自動実装用ファンクションアセット定義クラス
    /// </summary>
    public class AutoImplementFunction : FuncAssetSub, IFuncAssetWithArgumentDef
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
                typeRequests = info.typeRequests
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

        public List<TypeRequest> typeRequests { get; set; } = null;

        public Type ClassType { get; set; } = null;

        public List<ArgumentInfoNode> ArgumentTypeList { get; set; } = null;

        public Func<ICbValue> ReturnType { get; set; } = null;

        /// <summary>
        /// モジュール（DLL）
        /// </summary>
        public Module DllModule = null;

        /// <summary>
        /// コンストラクターか？
        /// </summary>
        public bool IsConstructor = false;

        /// <summary>
        /// メソッド呼び出し処理を実装する
        /// </summary>
        /// <param name="col">スクリプトのルートノード</param>
        /// <param name="isReBuildMode">再構築か？（保存データからの復帰）</param>
        public virtual bool ImplAsset(MultiRootConnector col, bool isReBuildMode = false)
        {
            string exTitle = GetGenericArgumentsString(col, isReBuildMode);
            return ImplAsset(col, null, exTitle);
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

        private Type GetRequestType(MultiRootConnector col, string name)
        {
            for (int i = 0; i < typeRequests.Count; i++)
            {
                TypeRequest typeRequest = typeRequests[i];
                if (typeRequest.Name == name)
                {
                    // 対応する型が見つかった

                    return col.SelectedVariableType[i];
                }
            }
            return null;
        }

        /// <summary>
        /// メソッド呼び出し処理を実装する
        /// </summary>
        /// <param name="col">スクリプトのルートノード</param>
        /// <param name="dummyArgumentsControl">仮引数管理</param>
        protected bool ImplAsset(
            MultiRootConnector col, 
            DummyArgumentsControl dummyArgumentsControl,
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
                    List<Type> argTypes = new List<Type>();
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

            List<ICbValue> argumentTypeList = new List<ICbValue>();
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

            col.MakeFunction(
                funcTitle + exTitle,
                NodeHelpText,
                returnType,
                argumentTypeList,
                new Func<List<ICbValue>, DummyArgumentsStack, ICbValue>(
                    (argument, cagt) =>
                    {
                        var ret = returnType();
                        if (dummyArgumentsControl != null && dummyArgumentsControl.IsInvalid(cagt))
                            return ret; // 実行環境が有効でない

                        ImplCallMethod(col, classType, dummyArgumentsControl, argument, cagt, ret);
                        return ret;
                    }
                )
            );

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
            else if (replaceArgumentType.IsGenericTypeParameter)
            {
                replaceArgumentType = GetRequestType(col, replaceArgumentType.Name);
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
            List<Type> argTypes = new List<Type>();
            foreach (var gat in type.GetGenericArguments())
            {
                if (gat.IsGenericType)
                {
                    // パラメータがジェネリックだった

                    var ngt = MakeRequestGenericType(col, gat);
                    argTypes.Add(ngt);
                }
                else
                {
                    // ジェネリック引数を収集する

                    for (int i = 0; i < typeRequests.Count; i++)
                    {
                        TypeRequest typeRequest = typeRequests[i];
                        if (typeRequest.Name == gat.Name)
                        {
                            // 対応する型を登録する

                            argTypes.Add(col.SelectedVariableType[i]);
                        }
                    }
                }
            }
            // 確定した型を返す（repType を使って MakeGenericType しては駄目）
            Type nType = Type.GetType(type.Namespace + "." + type.Name);
            return nType.MakeGenericType(argTypes.ToArray());
        }

        /// <summary>
        /// メソッド呼び出しの実装です。
        /// </summary>
        /// <param name="col">スクリプトのルートノード</param>
        /// <param name="dummyArgumentsControl">仮引数管理オブジェクト</param>
        /// <param name="callArguments">引数リスト</param>
        /// <param name="dummyArgumentsStack">仮引数スタック</param>
        /// <param name="returnValue">返り値</param>
        private void ImplCallMethod(
            MultiRootConnector col, 
            Type classType,
            DummyArgumentsControl dummyArgumentsControl, 
            List<ICbValue> callArguments, 
            DummyArgumentsStack dummyArgumentsStack, 
            ICbValue returnValue
            )
        {
            try
            {
                bool isClassInstanceMethod = ArgumentTypeList != null && ArgumentTypeList[0].IsSelf && !IsConstructor;
                List<object> methodArguments = null;

                methodArguments = SetArguments(
                    col,
                    dummyArgumentsControl,
                    callArguments,
                    dummyArgumentsStack,
                    isClassInstanceMethod,
                    methodArguments
                    );

                object result = CallMethod(
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
        /// <param name="dummyArgumentsControl">仮引数管理オブジェクト</param>
        /// <param name="callArguments">引数リスト</param>
        /// <param name="dummyArgumentsStack">仮引数スタック</param>
        /// <param name="variableIds">スクリプト変数IDリスト</param>
        /// <param name="isClassInstanceMethod">インスタンスメソッドか？</param>
        /// <param name="methodArguments">メソッド呼び出し引数リスト</param>
        /// <returns>メソッド呼び出し引数リスト</returns>
        private List<object> SetArguments(
            MultiRootConnector col, 
            DummyArgumentsControl dummyArgumentsControl, 
            List<ICbValue> callArguments, 
            DummyArgumentsStack dummyArgumentsStack, 
            bool isClassInstanceMethod,
            List<object> methodArguments
            )
        {
            int argumentIndex = 0;
            for (int i = 0; i < callArguments.Count; ++i)
            {
                methodArguments ??= new List<object>();

                var node = callArguments[argumentIndex++];
                CheckArgument(node);

                if (i == 0 && isClassInstanceMethod)
                {
                    // クラスメソッドの第一引数は、self（this）を受け取るのでメソッド呼び出しの引数にしない

                    continue;
                }

                if (node.IsList)
                {
                    ICbList cbList = node.GetListValue;

                    // リストは、オリジナルの型のインスタンスに差し替える

                    if (dummyArgumentsControl is null)
                    {
                        methodArguments.Add(cbList.ConvertOriginalTypeList(col, dummyArgumentsStack));
                    }
                    else
                    {
                        methodArguments.Add(cbList.ConvertOriginalTypeList(dummyArgumentsControl, dummyArgumentsStack));
                    }
                }
                else if (dummyArgumentsControl != null && node is ICbEvent cbEvent)
                {
                    // Func<> 及び Action<> は、オリジナルの型のインスタンスに置き換える

                    if (cbEvent.CallBack is null)
                    {
                        methodArguments.Add(null);
                    }
                    else
                    {
                        Debug.Assert(dummyArgumentsControl != null);
                        methodArguments.Add(cbEvent.GetCallBackOriginalType(dummyArgumentsControl, dummyArgumentsStack));
                    }
                }
                else methodArguments.Add(node.Data);
            }

            return methodArguments;
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
            Type classType,
            List<ICbValue> callArguments, 
            bool isClassInstanceMethod, 
            List<object> methodArguments
            )
        {
            object classInstance = null;

            if (isClassInstanceMethod)
            {
                // クラスメソッドの第一引数は、self（this）を受け取るのでクラスインスタンスとして扱う

                classInstance = callArguments[0].Data;
                if (classInstance is null)
                {
                    throw new Exception($"self(this) is invalid.");
                }
            }

            object result;
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

                    ReturnArgumentsValue(callArguments, isClassInstanceMethod, args);
                }
            }
            else if (methodArguments is null)
            {
                // 引数のないメソッド

                result = classType.InvokeMember(
                                FuncCode,
                                BindingFlags.InvokeMethod,
                                null,
                                classInstance,
                                new object[] { }
                                );
            }
            else
            {
                // 引数ありのメソッド

                object[] args = methodArguments.ToArray();

                result = classType.InvokeMember(
                                FuncCode,
                                BindingFlags.InvokeMethod,
                                null,
                                classInstance,
                                args
                                );

                ReturnArgumentsValue(callArguments, isClassInstanceMethod, args);
            }
            if (isClassInstanceMethod)
            {
                var scrArg = callArguments[0];
                if (scrArg.IsDelegate)
                {
                    // デリゲートは無条件で更新値を捨てる
                    Debug.Assert(callArguments[0].ReturnAction is null);
                }
                else if (callArguments[0].IsLiteral)
                {
                    // リレラルは更新情報を捨てる
                }
                else
                {
                    // 変更後の値を戻す

                    callArguments[0].ReturnAction?.Invoke(classInstance);
                }
            }

            return result;
        }

        /// <summary>
        /// メソッド呼び出しの引数が参照渡しの場合、変更値を基の管理者に戻します。
        /// </summary>
        /// <param name="col">スクリプトのルートノード</param>
        /// <param name="callArguments">引数リスト</param>
        /// <param name="variableIds">スクリプト変数IDリスト</param>
        /// <param name="isClassInstanceMethod">インスタンスメソッドか？</param>
        /// <param name="args">実際にメソッド呼び出しで渡した引数配列</param>
        private static void ReturnArgumentsValue(
            List<ICbValue> callArguments,
            bool isClassInstanceMethod,
            object[] args)
        {
            for (int i = (isClassInstanceMethod ? 1 : 0); i < callArguments.Count; ++i)
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
                // Func<> or Action<> 型の返し値

                if (CbFunc.IsFuncType(result.GetType()))
                {
                    cbEvent.CallBack = (cagt) =>
                    {
                        ICbValue retTypeValue = null;
                        try
                        {
                            // イベントからの返り値を取得
                            object tempReturnValue = ((dynamic)result).Invoke(cagt.GetValue().Data);

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
                else if (CbFunc.IsActionType(result.GetType()))
                {
                    // Action 型だと考える

                    cbEvent.CallBack = (cagt) =>
                    {
                        try
                        {
                            var argType = cagt.GetValue();
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
                else
                    throw new NotImplementedException();
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
