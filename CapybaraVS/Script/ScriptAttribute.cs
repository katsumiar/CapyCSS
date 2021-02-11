using CapybaraVS.Controls.BaseControls;
using CapyCSS.Controls;
using CapyCSS.Controls.BaseControls;
using CapyCSS.Script;
using CbVS.Script;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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
        /// NuGetをでインストールしてメソッドを取り込み、ノード化します。
        /// </summary>
        /// <param name="OwnerCommandCanvas">オーナーキャンバス</param>
        /// <param name="node">登録先のノード</param>
        /// <param name="packageDir"></param>
        /// <param name="packageName">パッケージ名</param>
        /// <returns>インポートしたパッケージ名</returns>
        public static string ImportScriptMethodsFromNuGet(
            CommandCanvas OwnerCommandCanvas,
            TreeMenuNode node,
            string packageDir,
            string packageName)
        {
            string pkgId;
            string ver = "(" + packageName.Split("(")[1];
            CommandCanvasList.OutPut.OutLine(nameof(ScriptImplement), $"NuGet {packageName}.");
            try
            {
                List<NugetClient.PackageInfo> packageList = CapyCSS.Script.NugetClient.install(packageDir, packageName, out pkgId);
                if (packageList is null)
                {
                    return null;
                }
                foreach (var package in packageList)
                {
#if true
                    ScriptImplement.ImportScriptMethodsFromDllFile(OwnerCommandCanvas, node, package.Path, null, $"{package.Name}({package.Version})");
#else
                    if (package == packageList.Last())
                    {
                        ScriptImplement.ImportScriptMethodsFromDllFile(OwnerCommandCanvas, node, package.Path, null, $"{package.Name}({package.Version})");
                    }
                    else
                    {
                        Assembly.LoadFrom(package.Path);
                        CommandCanvasList.OutPut.OutLine(nameof(ScriptImplement), $"imported {package.Name}({package.Version}) package.");
                    }
#endif
                }
            }
            catch (WebException)
            {
                CommandCanvasList.OutPut.OutLine(nameof(ScriptImplement), $"NG.");
                return null;
            }
            return ModuleControler.HEADER_NUGET + pkgId + ver;
        }

        /// <summary>
        /// 本アプリ内のメソッドを取り込み、ノード化します。
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
        /// パッケージ名からメソッドをスクリプトで使えるように取り込みます。
        /// </summary>
        /// <param name="OwnerCommandCanvas">オーナーキャンバス</param>
        /// <param name="node">登録先のノード</param>
        /// <param name="name">パッケージ名</param>
        /// <param name="ignoreClassList">無視するクラスリスト</param>
        /// <returns>インポートしたパッケージ名</returns>
        public static string ImportScriptMethodsFromPackage(
            CommandCanvas OwnerCommandCanvas,
            TreeMenuNode node,
            string name,
            List<string> ignoreClassList)
        {
            var asm = Assembly.Load(name);
            string outputName = ModuleControler.HEADER_PACKAGE + name;
            var functionNode = ImplementAsset.CreateGroup(node, outputName);
            ImportScriptMethods(
                OwnerCommandCanvas, 
                functionNode, 
                asm, 
                null, 
                ignoreClassList,
                (t) => OwnerCommandCanvas.AddImportTypeMenu(t)
                );
            CommandCanvasList.OutPut.OutLine(nameof(ScriptImplement), $"imported {name} package.");
            return outputName;
        }

        /// <summary>
        /// クラス名からメソッドをスクリプトで使えるように取り込みます。
        /// </summary>
        /// <param name="OwnerCommandCanvas">オーナーキャンバス</param>
        /// <param name="node">登録先のノード</param>
        /// <param name="name">クラス名</param>
        /// <param name="ignoreClassList">無視するクラスリスト</param>
        /// <returns>インポートしたクラス名</returns>
        public static string ImportScriptMethodsFromClass(
            CommandCanvas OwnerCommandCanvas,
            TreeMenuNode node,
            string name)
        {
            Type classType = CbST.GetTypeEx(name);
            if (classType is null)
            {
                return null;
            }
            var asm = classType.GetTypeInfo().Assembly;
            string outputName = ModuleControler.HEADER_CLASS + name;
            var functionNode = ImplementAsset.CreateGroup(node, outputName);
            ImportScriptMethods(
                OwnerCommandCanvas, 
                functionNode, 
                asm, 
                null, 
                null,
                (t) => OwnerCommandCanvas.AddImportTypeMenu(t)
                );
            CommandCanvasList.OutPut.OutLine(nameof(ScriptImplement), $"imported {name} class.");
            return outputName;
        }

        /// <summary>
        /// DLLファイルを読み込んでメソッドをスクリプトで使えるように取り込みます。
        /// </summary>
        /// <param name="OwnerCommandCanvas">オーナーキャンバス</param>
        /// <param name="node">登録先のノード</param>
        /// <param name="path"></param>
        /// <param name="ignoreClassList">無視するクラスリスト</param>
        /// <returns>インポートしたモジュール名</returns>
        public static string ImportScriptMethodsFromDllFile(
            CommandCanvas OwnerCommandCanvas,
            TreeMenuNode node,
            string path,
            List<string> ignoreClassList,
            string version = null)
        {
            var asm = Assembly.LoadFrom(path);
            Module mod = asm.GetModule(path);
            string name = Path.GetFileName(path);
            ImportScriptMethods(
                OwnerCommandCanvas, 
                node, 
                asm, 
                mod, 
                ignoreClassList,
                (t) => OwnerCommandCanvas.AddImportTypeMenu(t)
                );
            if (version is null)
            {
                CommandCanvasList.OutPut.OutLine(nameof(ScriptImplement), $"imported {System.IO.Path.GetFileNameWithoutExtension(name)} package.");
            }
            else
            {
                CommandCanvasList.OutPut.OutLine(nameof(ScriptImplement), $"imported {version} package.");
            }
            return name;
        }

        /// <summary>
        /// 受け入れられるメソッドか判定します。
        /// </summary>
        /// <param name="classType">クラス情報</param>
        /// <param name="methodInfo">メソッド情報</param>
        /// <returns>true==受け入れられる</returns>
        private static bool IsAcceptMethod(Type classType, MethodBase methodInfo)
        {
            if (methodInfo.IsGenericMethod || methodInfo.IsGenericMethodDefinition)
                return false;   // ジェネリックメソッドは現在未対応

            if (!classType.IsInterface && methodInfo.IsAbstract)
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
            if (classType.IsGenericType || classType.IsGenericTypeDefinition)
                return false;   // ジェネリックなクラスには未対応

            if (classType.IsNestedPrivate)
                return false;   // 扱えない

            if (classType.IsNotPublic)
                return false;   // 扱えない

            if (classType.IsInterface)
                return true;    // インターフェイスは扱う

            if (!classType.IsClass)
                return false;   // クラス以外は、扱わない

            if (classType.IsAbstract)
                return false;   // 象徴クラスは、扱えない

            return true;
        }

        private static bool IsAcceptTypeMenuType(Type type)
        {
            if (type.IsNestedPrivate)
                return false;   // 扱えない

            if (type.IsNotPublic)
                return false;   // 扱えない

            if (type.IsPointer)
                return false;   // 扱えない

            if (type.IsByRefLike)
            {
                // ref-like型構造体は、ジェネリック型引数にできない

                return false;
            }

            if (type.IsEnum)
                return true;

            if (type.IsInterface)
                return true;

            if (CbStruct.IsStruct(type))
                return true;

            if (type.IsClass)
                return true;

            return false;
        }

        /// <summary>
        /// Assemblyからメソッドをスクリプトで使えるように取り込みます。
        /// </summary>
        /// <param name="OwnerCommandCanvas">オーナーキャンバス</param>
        /// <param name="node">登録先のノード</param>
        /// <param name="asm">対象Assembly</param>
        /// <param name="module">モジュール</param>
        /// <param name="ignoreClassList">無視するクラスリスト</param>
        public static void ImportScriptMethods(
            CommandCanvas OwnerCommandCanvas,
            TreeMenuNode node,
            Assembly asm,
            Module module,
            List<string> ignoreClassList,
            Action<Type> inportTypeMenu)
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

            Task<List<Type>> tcTask = null;
            if (inportTypeMenu != null)
            {
                // 型情報を収集する

                tcTask = Task.Run(() =>
                {
                    List<Type> resultTypes = new List<Type>();
                    foreach (Type type in types)
                    {
                        if (ignoreClassList != null && !ignoreClassList.Contains(type.Name))
                            continue;

                        if (!IsAcceptTypeMenuType(type))
                            continue;

                        resultTypes.Add(type);
                    }
                    return resultTypes;
                });
            }

#if false    // テスト用
            foreach (Type classType in types)
            {
                if (!IsAcceptClass(classType))
                    continue;   // 扱えない

                if (ignoreClassList != null && !ignoreClassList.Contains(classType.Name))
                    continue;

                if (!classType.IsAbstract)
                {
                    // コンストラクタをインポートする

                    CreateMakeInportConstructorInfoTasks(module, classType);
                }

                // メソッドをインポートする
                CreateMakeInportMethodInfoTasks(module, classType);
            }
#endif

            List<Task<List<AutoImplementFunctionInfo>>> tasks = CreateMakeInportFunctionInfoTasks(module, ignoreClassList, types);

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

            // 型情報をメニューに登録する
            if (tcTask != null)
            {
                var typeList = tcTask.Result;
                foreach (var type in typeList)
                {
                    inportTypeMenu(type);
                }
            }
        }

        /// <summary>
        /// メソッド取り込み用情報収集タスクリストを作成します。
        /// </summary>
        /// <param name="module">モジュール</param>
        /// <param name="ignoreClassList">無視するクラスリスト</param>
        /// <param name="types">モジュールの情報</param>
        /// <returns>タスクリスト</returns>
        private static List<Task<List<AutoImplementFunctionInfo>>> CreateMakeInportFunctionInfoTasks(Module module, List<string> ignoreClassList, Type[] types)
        {
            var tasks = new List<Task<List<AutoImplementFunctionInfo>>>();
            foreach (Type classType in types)
            {
                if (ignoreClassList != null && !ignoreClassList.Contains(classType.Name))
                    continue;

                if (!IsAcceptClass(classType))
                    continue;   // 扱えない

                if (!classType.IsAbstract)
                {
                    // コンストラクタをインポートする

                    Task<List<AutoImplementFunctionInfo>> importConstructorTask = Task.Run(() =>
                    {
                        return CreateMakeInportConstructorInfoTasks(module, classType);
                    });
                    tasks.Add(importConstructorTask);
                }

                // メソッドをインポートする
                Task<List<AutoImplementFunctionInfo>> importMethodTask = Task.Run(() =>
                {
                    return CreateMakeInportMethodInfoTasks(module, classType);
                });
                tasks.Add(importMethodTask);
            }

            return tasks;
        }

        private static List<AutoImplementFunctionInfo> CreateMakeInportConstructorInfoTasks(Module module, Type classType)
        {
            List<AutoImplementFunctionInfo> importFuncInfoList = new List<AutoImplementFunctionInfo>();
            foreach (ConstructorInfo constructorInfo in classType.GetConstructors())
            {
                if (!IsAcceptMethod(classType, constructorInfo))
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
        }

        private static List<AutoImplementFunctionInfo> CreateMakeInportMethodInfoTasks(Module module, Type classType)
        {
            List<AutoImplementFunctionInfo> importFuncInfoList = new List<AutoImplementFunctionInfo>();
            foreach (MethodInfo methodInfo in classType.GetMethods())
            {
                if (!IsAcceptMethod(classType, methodInfo))
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

            // 型情報を収集する
            var task = Task.Run(()=>
            {
                foreach (Type type in types)
                {
                    if (!IsAcceptTypeMenuType(type))
                        return;   // 扱えない

                    OwnerCommandCanvas.AddTypeMenu(type);
                }
            });

            foreach (Type classType in types)
            {
                if (!IsAcceptClass(classType))
                    continue;   // 扱えない

                if (!classType.IsAbstract)
                {
                    // コンストラクタをインポートする

                    foreach (ConstructorInfo constructorInfo in classType.GetConstructors())
                    {
                        try
                        {
                            if (!IsAcceptMethod(classType, constructorInfo))
                                continue;   // 未対応

                            importScriptMethodAttributeMethods(OwnerCommandCanvas, node, classType, constructorInfo, null);
                        }
                        catch (Exception ex)
                        {
                            CommandCanvasList.ErrorLog += nameof(ScriptImplement) + "." + nameof(ImportScriptMethods) + ": " + ex.Message + Environment.NewLine + Environment.NewLine;
                        }
                    }
                }

                // メソッドをインポートする
                foreach (MethodInfo methodInfo in classType.GetMethods())
                {
                    try
                    {
                        if (!IsAcceptMethod(classType, methodInfo))
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

                if (retType("") is null)
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
                        menuName = classType.Name + ".(new)." + classType.Name;
                    }
                    else
                    {
                        string group = "";
                        if (methodInfo.Name.Contains("_"))
                        {
                            // 最初に見つかった _ までの文字列でグループを分ける

                            group = methodInfo.Name.Split("_")[0] + ".";
                        }
                        if (methodInfo.IsStatic)
                        {
                            menuName = "[s] " + menuName;
                        }
                        if (methodInfo.IsVirtual)
                        {
                            menuName = "[v] " + menuName;
                        }
                        switch (group)
                        {
                            case "get.":
                                menuName = classType.Name + ".(getter)." + menuName;
                                break;
                            case "set.":
                                menuName = classType.Name + ".(setter)." + menuName;
                                break;
                            default:
                                {
                                    List<string> baseGroup = new List<string>()
                                    {
                                        "ToString",
                                        "GetType",
                                        "Equals",
                                        "GetHashCode"
                                    };
                                    if (baseGroup.Contains(methodInfo.Name))
                                    {
                                        menuName = classType.Name + ".(base)." + menuName;
                                    }
                                    else
                                    {
                                        menuName = classType.Name + "." + group + methodInfo.Name + "." + menuName;
                                    }
                                }
                                break;
                        }
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
