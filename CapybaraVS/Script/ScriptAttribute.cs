using CapybaraVS.Controls.BaseControls;
using CapyCSS.Controls;
using CbVS.Script;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace CapybaraVS.Script
{
    /// <summary>
    /// リフレクションによるメソッドのスクリプトノード化用のメソッド用属性です。
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
    public class ScriptMethodAttribute : Attribute
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
    public class ScriptParamAttribute : Attribute
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
        /// リフレクションで本アプリ内のメソッドを取り込み、ノード化します。
        /// </summary>
        /// <param name="OwnerCommandCanvas">オーナーキャンバス</param>
        /// <param name="node">登録先のノード</param>
        public static void ImportScriptMethods(
            CommandCanvas OwnerCommandCanvas,
            TreeMenuNode node)
        {
            // 現在のコードを実行しているアセンブリを取得する
            Assembly asm = Assembly.GetExecutingAssembly();
            GetApiFromAssemblyForScriptMethodAttribute(OwnerCommandCanvas, node, asm);
        }

        /// <summary>
        /// DLLからメソッドをスクリプトで使えるように取り込みます。
        /// </summary>
        /// <param name="OwnerCommandCanvas">オーナーキャンバス</param>
        /// <param name="node">登録先のノード</param>
        /// <param name="name">DLL名</param>
        /// <param name="classList">取り込み対象クラスリスト</param>
        /// <returns>インポートしたモジュール名</returns>
        public static string ImportScriptMethodsFromModule(
            CommandCanvas OwnerCommandCanvas,
            TreeMenuNode node,
            string name,
            List<string> classList)
        {
            var asm = Assembly.Load(name);
            var functionNode = ImplementAsset.CreateGroup(node, name);
            ImportScriptMethods(OwnerCommandCanvas, functionNode, asm, null, classList);
            return name;
        }

        /// <summary>
        /// DLLファイルを読み込んでメソッドをスクリプトで使えるように取り込みます。
        /// </summary>
        /// <param name="OwnerCommandCanvas">オーナーキャンバス</param>
        /// <param name="node">登録先のノード</param>
        /// <param name="path"></param>
        /// <param name="classList">取り込み対象クラスリスト</param>
        /// <returns>インポートしたモジュール名</returns>
        public static string ImportScriptMethodsFromDllFile(
            CommandCanvas OwnerCommandCanvas,
            TreeMenuNode node,
            string path,
            List<string> classList)
        {
            var asm = Assembly.LoadFrom(path);
            Module mod = asm.GetModule(path);
            string name = Path.GetFileName(path);
            ImportScriptMethods(OwnerCommandCanvas, node, asm, mod, classList);
            return name;
        }

        /// <summary>
        /// 受け入れられるメソッドか判定します。
        /// </summary>
        /// <param name="methodInfo">メソッド情報</param>
        /// <returns>true==受け入れられる</returns>
        private static bool IsAcceptMethod(MethodBase methodInfo)
        {
            if (methodInfo.IsGenericMethod || methodInfo.IsGenericMethodDefinition)
                return false;   // ジェネリックメソッドは現在未対応

            if (methodInfo.IsAbstract)
                return false;   // 象徴メソッドは呼べない

            return true;
        }

        /// <summary>
        /// 受け入れられるクラスか判定します。
        /// </summary>
        /// <param name="classType">クラスの型</param>
        /// <returns>true==受け入れられる</returns>
        private static bool IsAcceptClass(Type classType)
        {
            if (!classType.IsClass)
                return false;   // クラス以外は、扱わない

            if (classType.IsAbstract)
                return false;   // 象徴クラスは、扱えない

            if (classType.IsGenericType || classType.IsGenericTypeDefinition)
                return false;   // ジェネリックなクラスには未対応

            if (classType.IsNestedPrivate)
                return false;   // 扱えない

            if (classType.IsNotPublic)
                return false;   // 扱えない

            return true;
        }

        /// <summary>
        /// Assemblyからメソッドをスクリプトで使えるように取り込みます。
        /// </summary>
        /// <param name="OwnerCommandCanvas">オーナーキャンバス</param>
        /// <param name="node">登録先のノード</param>
        /// <param name="asm">対象Assembly</param>
        /// <param name="module">モジュール</param>
        /// <param name="classList">取り込み対象クラスリスト</param>
        public static void ImportScriptMethods(
            CommandCanvas OwnerCommandCanvas,
            TreeMenuNode node,
            Assembly asm,
            Module module,
            List<string> classList)
        {
            Type[] types = null;
            if (module is null)
            {
                types = asm.GetTypes();
            }
            else
            {
                types = module.GetTypes();
                CbST.AddModule(module);
            }

            List<Task<List<AutoImplementFunctionInfo>>> tasks = CreateMakeInportFunctionInfoTasks(module, classList, types);

            // ノード化
            foreach (var task in tasks)
            {
                var classInfos = task.Result;
                if (classInfos is null)
                    continue;

                foreach (var info in classInfos)
                {
                    if (info is null)
                        continue;

                    CreateMethodNode(OwnerCommandCanvas, node, info);
                }
            }
        }

        /// <summary>
        /// メソッド取り込み用情報収集タスクリストを作成します。
        /// </summary>
        /// <param name="module">モジュール</param>
        /// <param name="classList">取り込み対象クラスリスト</param>
        /// <param name="types">モジュールの情報</param>
        /// <returns>タスクリスト</returns>
        private static List<Task<List<AutoImplementFunctionInfo>>> CreateMakeInportFunctionInfoTasks(Module module, List<string> classList, Type[] types)
        {
            var tasks = new List<Task<List<AutoImplementFunctionInfo>>>();
            foreach (Type classType in types)
            {
                if (!IsAcceptClass(classType))
                    continue;   // 扱えない

                if (classList != null && !classList.Contains(classType.Name))
                    continue;

                // コンストラクタをインポートする
                Task<List<AutoImplementFunctionInfo>> importConstructorTask = Task.Run(() =>
                {
                    List<AutoImplementFunctionInfo> importFuncInfoList = new List<AutoImplementFunctionInfo>();

                    foreach (ConstructorInfo constructorInfo in classType.GetConstructors())
                    {
                        if (!IsAcceptMethod(constructorInfo))
                            continue;   // 未対応

                        try
                        {
                            var functionInfo = MakeInportFunctionInfo(classType, constructorInfo, null, null, module);
                            if (functionInfo != null)
                            {
                                importFuncInfoList.Add(functionInfo);
                            }
                        }
                        catch (Exception ex)
                        {
                            CommandCanvasList.ErrorLog += nameof(ScriptImplement) + "." + nameof(ImportScriptMethods) + ": " + ex.Message + Environment.NewLine + Environment.NewLine;
                        }
                    }
                    return importFuncInfoList;
                });
                tasks.Add(importConstructorTask);

                // メソッドをインポートする
                Task<List<AutoImplementFunctionInfo>> importMethodTask = Task.Run(() =>
                {
                    List<AutoImplementFunctionInfo> importFuncInfoList = new List<AutoImplementFunctionInfo>();
                    foreach (MethodInfo methodInfo in classType.GetMethods())
                    {
                        if (!IsAcceptMethod(methodInfo))
                            continue;   // 未対応

                        try
                        {
                            var functionInfo = MakeInportFunctionInfo(classType, methodInfo, methodInfo.ReturnType, null, module);
                            if (functionInfo != null)
                            {
                                importFuncInfoList.Add(functionInfo);
                            }
                        }
                        catch (Exception ex)
                        {
                            CommandCanvasList.ErrorLog += nameof(ScriptImplement) + "." + nameof(ImportScriptMethods) + ": " + ex.Message + Environment.NewLine + Environment.NewLine;
                        }
                    }
                    return importFuncInfoList;
                });
                tasks.Add(importMethodTask);
            }

            return tasks;
        }

        /// <summary>
        /// リフレクションでAssemblyからScriptMethodAttribute属性を持つメソッドをスクリプトで使えるように取り込みます。
        /// </summary>
        /// <param name="OwnerCommandCanvas">オーナーキャンバス</param>
        /// <param name="node">登録先のノード</param>
        /// <param name="asm">対象Assembly</param>
        public static void GetApiFromAssemblyForScriptMethodAttribute(
            CommandCanvas OwnerCommandCanvas,
            TreeMenuNode node,
            Assembly asm)
        {
            Type[] types = asm.GetTypes();
            foreach (Type classType in types)
            {
                if (!IsAcceptClass(classType))
                    continue;   // 扱えない

                // コンストラクタをインポートする
                foreach (ConstructorInfo constructorInfo in classType.GetConstructors())
                {
                    try
                    {
                        if (!IsAcceptMethod(constructorInfo))
                            continue;   // 未対応

                        importScriptMethodAttributeMethods(OwnerCommandCanvas, node, classType, constructorInfo, null);
                    }
                    catch (Exception ex)
                    {
                        CommandCanvasList.ErrorLog += nameof(ScriptImplement) + "." + nameof(ImportScriptMethods) + ": " + ex.Message + Environment.NewLine + Environment.NewLine;
                    }
                }

                // メソッドをインポートする
                foreach (MethodInfo methodInfo in classType.GetMethods())
                {
                    try
                    {
                        if (!IsAcceptMethod(methodInfo))
                            continue;   // 未対応

                        importScriptMethodAttributeMethods(OwnerCommandCanvas, node, classType, methodInfo, methodInfo.ReturnType);
                    }
                    catch (Exception ex)
                    {
                        CommandCanvasList.ErrorLog += nameof(ScriptImplement) + "." + nameof(ImportScriptMethods) + ": " + ex.Message + Environment.NewLine + Environment.NewLine;
                    }
                }
            }
        }

        /// <summary>
        /// クラスとメソッド情報からScriptMethodAttribute属性を持つメソッドをスクリプトで使えるように取り込みます。
        /// </summary>
        /// <param name="OwnerCommandCanvas">オーナーキャンバス</param>
        /// <param name="node">登録先のノード</param>
        /// <param name="classType">クラスの型情報</param>
        /// <param name="methodInfo">メソッド情報</param>
        /// <param name="returnType">メソッドの返り値の型情報</param>
        private static void importScriptMethodAttributeMethods(
            CommandCanvas OwnerCommandCanvas, 
            TreeMenuNode node, 
            Type classType,
            MethodBase methodInfo,
            Type returnType)
        {
            List<Task<AutoImplementFunctionInfo>> tasks = CreateMakeInportFunctionInfoTasks(classType, methodInfo, returnType);

            // ノード化
            foreach (var info in tasks)
            {
                if (info is null)
                    continue;

                CreateMethodNode(OwnerCommandCanvas, node, info.Result);
            }
        }

        /// <summary>
        /// メソッド取り込み用情報収集タスクリストを作成します。
        /// </summary>
        /// <param name="classType">クラスの型情報</param>
        /// <param name="methodInfo">メソッド情報</param>
        /// <param name="returnType">メソッドの返り値の型情報</param>
        /// <returns>タスクリスト</returns>
        private static List<Task<AutoImplementFunctionInfo>> CreateMakeInportFunctionInfoTasks(Type classType, MethodBase methodInfo, Type returnType)
        {
            var tasks = new List<Task<AutoImplementFunctionInfo>>();
            var methods = methodInfo.GetCustomAttributes(typeof(ScriptMethodAttribute));
            foreach (Attribute att in methods)
            {
                ScriptMethodAttribute methodAttr = att as ScriptMethodAttribute;
                if (methodAttr != null)
                {
                    Task<AutoImplementFunctionInfo> task = Task.Run(() =>
                    {
                        return MakeInportFunctionInfo(classType, methodInfo, returnType, methodAttr);
                    });
                    tasks.Add(task);
                }
            }
            return tasks;
        }

        /// <summary>
        /// クラスとメソッド情報からスクリプトで使えるように取り込みます。
        /// </summary>
        /// <param name="OwnerCommandCanvas">オーナーキャンバス</param>
        /// <param name="node">登録先のノード</param>
        /// <param name="classType">クラスの型情報</param>
        /// <param name="methodInfo">メソッド情報</param>
        /// <param name="returnType">メソッドの返り値の型情報</param>
        /// <param name="methodAttr">ScriptMethodAttribute情報(or null)</param>
        /// <param name="module">DLLモジュール(or null)</param>
        /// <returns>スクリプト用メソッド情報</returns>
        private static AutoImplementFunctionInfo MakeInportFunctionInfo(
            Type classType,
            MethodBase methodInfo, 
            Type returnType,
            ScriptMethodAttribute methodAttr = null,
            Module module = null)
        {
            List<ArgumentInfoNode> argumentList = null;

            if (!methodInfo.IsStatic && !methodInfo.IsConstructor)
            {
                // 静的メソッドでは無いので所属するクラスの情報を取得

                var selfType = TryGetCbType(methodInfo.ReflectedType);
                if (selfType is null)
                    return null;
                argumentList = MakeSelfTypeForClassMethod(methodInfo, argumentList, selfType);
            }

            if (GetArgumentList(methodInfo, ref argumentList))
            {
                Func<string, ICbValue> retType;

                if (methodInfo.IsConstructor)
                {
                    // コンストラクタ

                    retType = TryGetCbType(classType);
                }
                else
                {
                    // 返り値の型を準備
                    if (returnType.FullName == "System.Void")
                    {
                        // void 型は専用のクラスを利用する

                        retType = TryGetCbType(CbVoid.T);
                    }
                    else
                    {
                        retType = TryGetCbType(returnType);
                    }
                }

                if (retType is null)
                    return null; // 返り値の型が対象外

                // メニュー用の名前を作成
                string menuName = methodInfo.Name;
                if (methodAttr != null && methodAttr.MenuName != "")
                {
                    // 指定の名前を採用する

                    menuName = methodAttr.MenuName;
                }
                else if (methodAttr is null)
                {
                    // 名前を作成する

                    if (methodInfo.IsConstructor)
                    {
                        menuName = classType.Name + ".new." + classType.Name;
                    }
                    else
                    {
                        if (methodInfo.IsStatic)
                        {
                            menuName = "[s] " + menuName;
                        }
                        if (methodInfo.IsVirtual)
                        {
                            menuName = "[v] " + menuName;
                        }

                        menuName = classType.Name + "." + menuName + "." + menuName;
                    }
                    if (classType.Namespace != null)
                    {
                        // ネームスペースを付ける

                        menuName = classType.Namespace + "." + menuName;
                    }

                    // 引数情報を追加する
                    menuName += MakeParamsString(methodInfo);

                    // 返り値の型名を追加する
                    menuName += " : " + retType("").TypeName;
                }

                // ノード用の名前を作成
                string nodeName = MakeScriptNodeName(menuName);
                if (methodAttr != null && methodAttr.FuncName != "")
                {
                    // 指定の名前を採用する

                    nodeName = methodAttr.FuncName;
                }
                else if (methodAttr is null)
                {
                    if (methodInfo.IsConstructor)
                    {
                        nodeName = "new " + classType.Name;
                    }
                    else
                    {
                        nodeName = methodInfo.Name;
                    }
                }

                // スクリプトノード用のヒント
                string nodeHint = "";
                string nodeHintTitle = menuName;
                if (methodAttr != null)
                {
                    nodeHint = methodAttr.NodeHint.Trim();
                    if (nodeHint.StartsWith("RS=>"))
                    {
                        // ノード用ヒントをリソースから取得

                        string id = nodeHint.Split("=>")[1];
                        nodeHint = Language.GetInstance[id];
                    }
                }
                nodeHint = $"【{nodeHintTitle}】" + (nodeHint != "" ? Environment.NewLine : "") + nodeHint;

                // メニュー用のヒント
                string hint = "";
                if (methodAttr != null)
                {
                    hint = methodAttr.Hint.Trim();
                    if (hint.StartsWith("RS=>"))
                    {
                        // メニュー用ヒントをリソースから取得

                        string id = hint.Split("=>")[1];
                        hint = Language.GetInstance[id];
                    }
                }

                // オーバーロード用の名前保管情報を作成（同名にならないようにする）
                string addArg = "#";
                ParameterInfo[] paramsinfo = methodInfo.GetParameters();
                foreach (ParameterInfo para in paramsinfo)
                {
                    addArg += "_" + CbSTUtils._GetTypeName(para.ParameterType);
                }

                // ノード化依頼用の情報をセット
                AutoImplementFunctionInfo autoImplementFunctionInfo = new AutoImplementFunctionInfo()
                {
                    assetCode = methodInfo.ReflectedType.Namespace + "." + methodInfo.ReflectedType.Name + "." + methodInfo.Name + addArg,
                    menuTitle = menuName,
                    funcTitle = nodeName,
                    hint = hint,
                    nodeHint = nodeHint,
                    classType = classType,
                    returnType = () => retType(""),
                    argumentTypeList = argumentList,
                    dllModule = module,
                    isConstructor = methodInfo.IsConstructor,
                };

                return autoImplementFunctionInfo;
            }
            return null;
        }

        /// <summary>
        /// スクリプト用メソッド情報を基にメソッドをノード化してメニューに紐付けます。
        /// </summary>
        /// <param name="OwnerCommandCanvas">オーナーキャンバス</param>
        /// <param name="node">登録先のノード</param>
        /// <param name="autoImplementFunctionInfo">スクリプト用メソッド情報</param>
        private static void CreateMethodNode(CommandCanvas OwnerCommandCanvas, TreeMenuNode node, AutoImplementFunctionInfo autoImplementFunctionInfo)
        {
            if (CbFunc.ContainsEvent(autoImplementFunctionInfo.argumentTypeList))
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

        /// <summary>
        /// 引数情報を文字列化します。
        /// </summary>
        /// <param name="methodInfo">メソッド情報</param>
        /// <returns>文字列化した引数情報</returns>
        private static string MakeParamsString(MethodBase methodInfo)
        {
            string paramsStr = "()";
            ParameterInfo[] paramss = methodInfo.GetParameters();
            if (paramss.Length != 0)
            {
                paramsStr = "(";
                bool isFirst = true;
                foreach (ParameterInfo para in paramss)
                {
                    if (!isFirst)
                    {
                        paramsStr += ",";
                    }

                    paramsStr += CbSTUtils._GetTypeName(para.ParameterType);

                    isFirst = false;
                }
                paramsStr += ")";
            }
            return paramsStr;
        }

        /// <summary>
        /// スクリプトノード名を作成します。
        /// </summary>
        /// <param name="name">ネームスペース付きの名前</param>
        /// <returns>スクリプトノード名</returns>
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
        /// <param name="methodInfo">メソッド情報</param>
        /// <param name="argumentList">引数リスト</param>
        /// <param name="selfType"></param>
        /// <returns></returns>
        private static List<ArgumentInfoNode> MakeSelfTypeForClassMethod(
            MethodBase methodInfo,
            List<ArgumentInfoNode> argumentList,
            Func<string, ICbValue> selfType)
        {
            ArgumentInfoNode argNode = new ArgumentInfoNode();
            string name = "self";// info.ReflectedType.Name;
            foreach (var attrNode in methodInfo.ReflectedType.GetCustomAttributes())
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
            MethodBase info,
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
