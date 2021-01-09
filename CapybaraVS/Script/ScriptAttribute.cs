using CapybaraVS.Controls.BaseControls;
using CbVS.Script;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Controls;

namespace CapybaraVS.Script
{
    /// <summary>
    /// リフレクションによるメソッドのスクリプトノード化用のメソッド用属性です。
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
    class ScriptMethodAttribute : Attribute
    {
        private string menuName;    // メニュー用のメソッド名
        private string funcName;    // ノード用のメソッド名
        private string hint;        // メニュー用のヒントメッセージ
        private string nodeHint;    // ノード用のヒントメッセージ
        public string MenuName => menuName;
        public string FuncName => funcName;
        public string Hint => hint;
        public string NodeHint => nodeHint;
        public ScriptMethodAttribute(string menuName = "", string funcName = "", string hint = "", string nodeHint = "")
        {
            this.menuName = menuName;
            this.funcName = funcName;
            this.hint = hint;
            if (nodeHint == "(none)")
                this.nodeHint = "";
            else if (nodeHint.Trim() == "")
                this.nodeHint = hint;
            else
                this.nodeHint = nodeHint;
        }
    }

    /// <summary>
    /// リフレクションによるメソッドのスクリプトノード化用の引数用属性です。
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter)]
    class ScriptParamAttribute : Attribute
    {
        private string name;    // 引数名
        public string ParamName => name;
        public ScriptParamAttribute(string name)
        {
            this.name = name;
        }
    }

    public class ScriptImplement
    {
        /// <summary>
        /// 引数情報です。
        /// </summary>
        public class ArgumentInfoNode
        {
            public Func<ICbValue> CreateArgument;
            public bool IsByRef = false;
            public bool IsSelf = false;
            public bool IsFunc = false;
            public Func<ICbValue> CreateFuncReturn;
        }

        /// <summary>
        /// リフレクションでメソッドを取り込み、ノード化します。
        /// </summary>
        /// <param name="node"></param>
        static public void ImplemantScriptMethods(CommandCanvas OwnerCommandCanvas, TreeMenuNode node)
        {
            // 現在のコードを実行しているアセンブリを取得する
            Assembly asm = Assembly.GetExecutingAssembly();

            // アセンブリで定義されている型をすべて取得する
            Type[] ts = asm.GetTypes();
            foreach (Type classType in ts)
            {
                foreach (MethodInfo info in classType.GetMethods())
                {
                    try 
                    {
                        _implementScriptMethods(OwnerCommandCanvas, node, classType, info);
                    }
                    catch (Exception ex) 
                    {
                        App.ErrorLog += nameof(ScriptImplement) + "." + nameof(ImplemantScriptMethods) + ": " + ex.Message + Environment.NewLine + Environment.NewLine;
                    }
                }
            }
        }

        private static void _implementScriptMethods(CommandCanvas OwnerCommandCanvas, TreeMenuNode node, Type classType, MethodInfo info)
        {
            var methods = info.GetCustomAttributes(typeof(ScriptMethodAttribute));
            foreach (Attribute att in methods)
            {
                ScriptMethodAttribute method = att as ScriptMethodAttribute;
                if (method != null)
                {
                    List<ArgumentInfoNode> argumentList = null;

                    if (!info.IsStatic)
                    {
                        // 静的メソッドでは無いので所属するクラスの情報を取得

                        var selfType = TryGetCbType(info.ReflectedType);
                        if (selfType is null)
                            return;
                        argumentList = MakeSelfTypeForClassMethod(info, argumentList, selfType);
                    }

                    if (GetArgumentList(info, ref argumentList))
                    {
                        Func<string, ICbValue> retType;

                        // 返り値の型を準備
                        if (info.ReturnType.FullName == "System.Void")
                        {
                            // void 型は専用のクラスを利用する

                            retType = TryGetCbType(CbVoid.T);
                        }
                        else
                        {
                            retType = TryGetCbType(info.ReturnType);
                        }

                        if (retType is null)
                            return; // 返り値の型が対象外
                        
                        // メソッド名を取得
                        string funcName = method.MenuName;
                        if (funcName == "")
                            funcName = info.Name;

                        // オーバーロード用の名前保管情報を作成（同名にならないようにする）
                        string addArg = "#";
                        ParameterInfo[] paramsinfo = info.GetParameters();
                        foreach (ParameterInfo para in paramsinfo)
                        {
                            addArg = addArg + "_" + CbSTUtils._GetTypeName(para.ParameterType);
                        }

                        // スクリプトノード用のヒント
                        string nodeHint = method.NodeHint.Trim();
                        if (nodeHint.StartsWith("RS=>"))
                        {
                            // ノード用ヒントをリソースから取得

                            string id = nodeHint.Split("=>")[1];
                            nodeHint = Language.GetInstance[id];
                        }
                        // メニュー用のヒント
                        string hint = method.Hint.Trim();
                        if (hint.StartsWith("RS=>"))
                        {
                            // メニュー用ヒントをリソースから取得

                            string id = hint.Split("=>")[1];
                            hint = Language.GetInstance[id];
                        }

                        // ノード化依頼用の情報をセット
                        AutoImplementFunctionInfo autoImplementFunctionInfo = new AutoImplementFunctionInfo()
                        {
                            assetCode = info.ReflectedType.Namespace + "." + info.ReflectedType.Name + "." + info.Name + addArg,
                            menuTitle = funcName,
                            funcTitle = MakeScriptNodeName(method.FuncName != "" ? method.FuncName : funcName),
                            hint = hint,
                            nodeHint = $"【{(method.MenuName != "" ? method.MenuName : funcName)}】"
                                + (nodeHint != "" ? Environment.NewLine : "") + nodeHint,
                            classType = classType,
                            returnType = () => retType(""),
                            argumentTypeList = argumentList,
                        };

                        // ノード化を依頼
                        if (CbFunc.ContainsEvent(argumentList))
                        {
                            // Func<> 引数を持つノードを作成

                            ImplementAsset.CreateAssetMenu(
                                OwnerCommandCanvas,
                                node,
                                AutoImplementEventFunction.Create(autoImplementFunctionInfo)
                            );
                        }
                        else
                        {
                            // 通常のノードを作成

                            ImplementAsset.CreateAssetMenu(
                                OwnerCommandCanvas,
                                node,
                                AutoImplementFunction.Create(autoImplementFunctionInfo)
                            );
                        }
                    }
                }
            }
        }

        /// <summary>
        /// スクリプトノード名を作成します。
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private static string MakeScriptNodeName(string name)
        {
            if (name.Contains("."))
            {
                string[] tokens = name.Split(".");
                if (tokens.Length > 1)
                    name = tokens[tokens.Length - 1];
            }
            return name;
        }

        /// <summary>
        /// クラスメソッド用に第一引数用のクラス型の self 引数を作成します。
        /// </summary>
        /// <param name="info"></param>
        /// <param name="argumentList"></param>
        /// <param name="selfType"></param>
        /// <returns></returns>
        private static List<ArgumentInfoNode> MakeSelfTypeForClassMethod(
            MethodInfo info,
            List<ArgumentInfoNode> argumentList,
            Func<string, ICbValue> selfType)
        {
            ArgumentInfoNode argNode = new ArgumentInfoNode();
            string name = "self";// info.ReflectedType.Name;
            foreach (var attrNode in info.ReflectedType.GetCustomAttributes())
            {
                if (attrNode is ScriptParamAttribute scriptParam)
                    name = scriptParam.ParamName;
            }
            if (selfType(name) != null)
            {
                argumentList ??= new List<ArgumentInfoNode>();
                argNode.CreateArgument = () =>
                {
                    ICbValue addParam = selfType(name);
                    return addParam;
                };
                argNode.IsSelf = true;
                argumentList.Add(argNode);
            }
            return argumentList;
        }

        /// <summary>
        /// 引数情報リストを作成します。
        /// </summary>
        /// <param name="info"></param>
        /// <param name="argumentList"></param>
        /// <returns></returns>
        private static bool GetArgumentList(
            MethodInfo info,
            ref List<ArgumentInfoNode> argumentList)
        {
            if (info is null)
                return false;

            ParameterInfo[] paramsinfo = info.GetParameters();

            foreach (ParameterInfo para in paramsinfo)
            {
                var typeInfo = TryGetCbType(para.ParameterType);
                if (typeInfo is null)
                    return false;

                ArgumentInfoNode argNode = new ArgumentInfoNode();

                // リファレンス型をチェック
                if (para.ParameterType.IsByRef)
                    argNode.IsByRef = true;

                // イベント引数をチェック
                if (para.ParameterType.IsGenericParameter)
                {
                    if (para.ParameterType.GetGenericTypeDefinition() == typeof(CbFunc<,>))
                    {
                        argNode.IsFunc = true;
                        argNode.CreateFuncReturn = typeInfo("").NodeTF;
                    }
                }

                // 引数名を取得
                string name = para.Name;
                foreach (var attrNode in para.GetCustomAttributes())
                {
                    if (attrNode is ScriptParamAttribute scriptParam)
                        name = scriptParam.ParamName;
                }

                if (typeInfo(name) is null)
                    return false;   // 型情報の作成に失敗

                // 引数作成用の処理を作成
                argumentList ??= new List<ArgumentInfoNode>();
                argNode.CreateArgument = () =>
                {
                    ICbValue addParam = typeInfo(name);
                    if (para.HasDefaultValue)
                    {
                        if (para.DefaultValue != null)
                        {
                            // デフォルト引数を設定

                            addParam.Data = para.DefaultValue;
                        }
                    }
                    return addParam;
                };

                argumentList.Add(argNode);
            }
            return true;
        }

        /// <summary>
        /// 型をチェックし対応する型であれば対応する CbXXX クラスの型情報を取得します。
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static Func<string, ICbValue> TryGetCbType(Type type)
        {
            if (type is null)
                return null;

            return CbST.CbCreateNTF(type);
        }
    }
}
