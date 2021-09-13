//#define DEBUG_IMPORT    // インポート機能のデバッグモード

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
using static CapybaraVS.Controls.BaseControls.CommandCanvas;

namespace CapybaraVS.Script
{
    /// <summary>
    /// リフレクションによるメソッドのスクリプトノード化用のメソッド用属性です。
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
    public class ScriptMethodAttribute : Attribute
    {
        private string path;        // メニュー用のパス
        private string methodName;    // メソッド名
        public string Path => path;
        public string MethodName => methodName;
        public ScriptMethodAttribute(string path = "", string methodName = null)
        {
            if (path != "" && !path.EndsWith("."))
            {
                path += ".";
            }
            this.path = path;
            this.methodName = methodName;
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
#if !DEBUG_IMPORT
            try
#endif
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
#if !DEBUG_IMPORT
            catch (WebException)
            {
                CommandCanvasList.OutPut.OutLine(nameof(ScriptImplement), $"NG.");
                return null;
            }
#endif
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
        /// <param name="importNameList">取り込む名前リスト</param>
        /// <returns>インポートしたパッケージ名</returns>
        public static string ImportScriptMethodsFromPackage(
            CommandCanvas OwnerCommandCanvas,
            TreeMenuNode node,
            string name,
            List<string> importNameList)
        {
            string outputName = ModuleControler.HEADER_PACKAGE + name;
            var functionNode = ImplementAsset.CreateGroup(node, outputName);
#if !DEBUG_IMPORT
            try
#endif
            {
                ImportScriptMethods(
                    OwnerCommandCanvas,
                    functionNode,
                    Assembly.Load(name),
                    null,
                    importNameList,
                    (t) => OwnerCommandCanvas.AddImportTypeMenu(t)
                    );
            }
#if !DEBUG_IMPORT
            catch (Exception ex)
            {
                ControlTools.ShowErrorMessage(ex.Message, "Import Error.");
            }
#endif
            CommandCanvasList.OutPut.OutLine(nameof(ScriptImplement), $"imported {name} package.");
            return outputName;
        }

        /// <summary>
        /// スクリプトに基本のクラスをインポートします。
        /// </summary>
        /// <param name="OwnerCommandCanvas">オーナーキャンバス</param>
        /// <param name="node">登録先のノード</param>
        /// <param name="name">クラス名</param>
        public static void ImportScriptMethodsForBase(
            CommandCanvas OwnerCommandCanvas,
            TreeMenuNode node)
        {
            ImportScriptMethods(
                OwnerCommandCanvas,
                node,
                typeof(System.Object).GetTypeInfo().Assembly,
                null,
                new List<string>() {
                    "System."
                },
                (t) => OwnerCommandCanvas.AddImportTypeMenu(t)
                );
        }

        /// <summary>
        /// クラス名からメソッドをスクリプトで使えるように取り込みます。
        /// </summary>
        /// <param name="OwnerCommandCanvas">オーナーキャンバス</param>
        /// <param name="node">登録先のノード</param>
        /// <param name="name">クラス名</param>
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
        /// <param name="importNameList">取り込む名前リスト</param>
        /// <returns>インポートしたモジュール名</returns>
        public static string ImportScriptMethodsFromDllFile(
            CommandCanvas OwnerCommandCanvas,
            TreeMenuNode node,
            string path,
            List<string> importNameList,
            string version = null)
        {
#if !DEBUG_IMPORT
            try
#endif
            {
                var asm = Assembly.LoadFrom(path);
                Module mod = asm.GetModule(path);
                string name = Path.GetFileName(path);
                ImportScriptMethods(
                    OwnerCommandCanvas,
                    node,
                    asm,
                    mod,
                    importNameList,
                    (t) => OwnerCommandCanvas.AddImportTypeMenu(t)
                    );
                if (version is null)
                {
                    CommandCanvasList.OutPut.OutLine(nameof(ScriptImplement), $"imported {Path.GetFileNameWithoutExtension(name)} package.");
                    CommandCanvasList.OutPut.Flush();
                }
                else
                {
                    CommandCanvasList.OutPut.OutLine(nameof(ScriptImplement), $"imported {version} package.");
                    CommandCanvasList.OutPut.Flush();
                }
                return name;
            }
#if !DEBUG_IMPORT
            catch (Exception ex)
            {
                CommandCanvasList.OutPut.OutLine(nameof(ScriptImplement), $"Import Dll Error: {ex.Message}.");
            }
            return null;
#endif
        }

        /// <summary>
        /// 受け入れられるメソッドか判定します。
        /// </summary>
        /// <param name="classType">クラス情報</param>
        /// <param name="methodInfo">メソッド情報</param>
        /// <returns>true==受け入れられる</returns>
        private static bool IsAcceptMethod(Type classType, MethodBase methodInfo)
        {
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
        /// <param name="importNameList">取り込む名前リスト</param>
        public static void ImportScriptMethods(
            CommandCanvas OwnerCommandCanvas,
            TreeMenuNode node,
            Assembly asm,
            Module module,
            List<string> importNameList,
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
                        if (importNameList != null && !importNameList.Any(n => type.FullName.StartsWith(n)))
                            continue;

                        if (!IsAcceptTypeMenuType(type))
                            continue;

                        resultTypes.Add(type);
                    }
                    return resultTypes;
                });
#if DEBUG_IMPORT
                tcTask.Wait();
#endif
            }

            List<Task<List<AutoImplementFunctionInfo>>> tasks = CreateMakeInportFunctionInfoTasks(module, importNameList, types);

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
        /// <param name="importNameList">取り込む名前リスト</param>
        /// <param name="types">モジュールの情報</param>
        /// <returns>タスクリスト</returns>
        private static List<Task<List<AutoImplementFunctionInfo>>> CreateMakeInportFunctionInfoTasks(Module module, List<string> importNameList, Type[] types)
        {
            var tasks = new List<Task<List<AutoImplementFunctionInfo>>>();
            foreach (Type classType in types)
            {
                if (importNameList != null && !importNameList.Any(n => classType.FullName.StartsWith(n)))
                    continue;

                if (!IsAcceptClass(classType))
                    continue;   // 扱えない

                if (classType == typeof(Action))
                    continue;   // 本システムで特殊な扱いをしているので扱わない
                if (classType.Name.StartsWith("Action`"))
                    continue;   // 本システムで特殊な扱いをしているので扱わない
                if (classType.Name.StartsWith("Func`"))
                    continue;   // 本システムで特殊な扱いをしているので扱わない

                if (!classType.IsAbstract)
                {
                    // コンストラクタをインポートする

                    Task<List<AutoImplementFunctionInfo>> importConstructorTask = Task.Run(() =>
                    {
                        return CreateMakeInportConstructorInfoTasks(module, classType);
                    });
                    tasks.Add(importConstructorTask);
#if DEBUG_IMPORT
                    importConstructorTask.Wait();
#endif
                }

                // メソッドをインポートする
                Task<List<AutoImplementFunctionInfo>> importMethodTask = Task.Run(() =>
                {
                    return CreateMakeInportMethodInfoTasks(module, classType);
                });
                tasks.Add(importMethodTask);
#if DEBUG_IMPORT
                importMethodTask.Wait();
#endif
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

#if !DEBUG_IMPORT
                try
#endif
                {
                    var functionInfo = MakeInportFunctionInfo(classType, constructorInfo, null, null, module);
                    if (functionInfo != null)
                    {
                        importFuncInfoList.Add(functionInfo);
                    }
                }
#if !DEBUG_IMPORT
                catch (Exception ex)
                {
                    CommandCanvasList.ErrorLog += nameof(ScriptImplement) + "." + nameof(ImportScriptMethods) + ": " + ex.Message + Environment.NewLine + Environment.NewLine;
                }
#endif
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

#if !DEBUG_IMPORT
                try
#endif
                {
                    var functionInfo = MakeInportFunctionInfo(classType, methodInfo, methodInfo.ReturnType, null, module);
                    if (functionInfo != null)
                    {
                        importFuncInfoList.Add(functionInfo);
                    }
                }
#if !DEBUG_IMPORT
                catch (Exception ex)
                {
                    CommandCanvasList.ErrorLog += nameof(ScriptImplement) + "." + nameof(ImportScriptMethods) + ": " + ex.Message + Environment.NewLine + Environment.NewLine;
                }
#endif
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
#if !DEBUG_IMPORT
                        try
#endif
                        {
                            if (!IsAcceptMethod(classType, constructorInfo))
                                continue;   // 未対応

                            importScriptMethodAttributeMethods(OwnerCommandCanvas, node, classType, constructorInfo, null);
                        }
#if !DEBUG_IMPORT
                        catch (Exception ex)
                        {
                            CommandCanvasList.ErrorLog += nameof(ScriptImplement) + "." + nameof(ImportScriptMethods) + ": " + ex.Message + Environment.NewLine + Environment.NewLine;
                        }
#endif
                    }
                }

                // メソッドをインポートする
                foreach (MethodInfo methodInfo in classType.GetMethods())
                {
#if !DEBUG_IMPORT
                    try
#endif
                    {
                        if (!IsAcceptMethod(classType, methodInfo))
                            continue;   // 未対応

                        importScriptMethodAttributeMethods(OwnerCommandCanvas, node, classType, methodInfo, methodInfo.ReturnType);
                    }
#if !DEBUG_IMPORT
                    catch (Exception ex)
                    {
                        CommandCanvasList.ErrorLog += nameof(ScriptImplement) + "." + nameof(ImportScriptMethods) + ": " + ex.Message + Environment.NewLine + Environment.NewLine;
                    }
#endif
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
                if (info is null || info.Result is null)
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
#if DEBUG_IMPORT
                    task.Wait();
#endif
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
            if (returnType != null && returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>))
                return null;
            if (returnType == typeof(Task))
                return null;

            List<ArgumentInfoNode> argumentList = null;

            string genericArgs = "";
            List<TypeRequest> genericTypeRequests = null;
            if (classType.IsGenericType)
            {
                // ジェネリッククラスのメニュー名に追加するジェネリック情報を作成
                // 型確定時に選択を要求する型リクエスト情報リストを作成

                genericTypeRequests ??= new List<TypeRequest>();
                MakeGenericParamaterTypeRequestList(classType, genericTypeRequests);
            }
            if (methodInfo.IsGenericMethod)
            {
                // ジェネリックメソッドのメニュー名に追加するジェネリック情報を作成
                // 型確定時に選択を要求する型リクエスト情報リストを作成

                genericTypeRequests ??= new List<TypeRequest>();
                MakeGenericParamaterTypeRequestList(methodInfo, genericTypeRequests);
                genericArgs = CbSTUtils.GetGenericParamatersString(methodInfo);
            }

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

                // オーバーロード用の名前保管情報を作成（同名にならないようにする）
                string addArg = "#";
                ParameterInfo[] paramsinfo = methodInfo.GetParameters();
                foreach (ParameterInfo para in paramsinfo)
                {
                    addArg += "_" + CbSTUtils._GetTypeName(para.ParameterType);
                }

                string nodeCode = methodInfo.ReflectedType.Namespace + "." + methodInfo.ReflectedType.Name + "." + methodInfo.Name + addArg;

                string addState = "";

                //----------------------------------
                // クラス名を作成
                string className;
                if (classType.IsGenericType)
                {
                    className = CbSTUtils.GetGenericTypeName(classType);
                }
                else
                {
                    className = classType.Name;
                }

                //----------------------------------
                // メソッド名を作成
                string nodeName;
                if (methodAttr != null)
                {
                    // メソッド属性情報を持っている

                    if (methodAttr.MethodName != null)
                    {
                        // 指定の名前を採用する

                        nodeName = methodAttr.MethodName;
                    }
                    else
                    {
                        nodeName = methodInfo.Name;
                    }
                }
                else
                {
                    if (methodInfo.IsConstructor)
                    {
                        nodeName = "new " + className;
                    }
                    else
                    {
                        nodeName = methodInfo.Name;
                    }
                }

                //----------------------------------
                // メニュー項目名を作成
                string menuName = methodInfo.Name;
                if (methodAttr != null)
                {
                    // 指定の名前を採用する

                    menuName = methodAttr.Path + nodeName;
                }
                else if (methodAttr is null)
                {
                    // 名前を作成する

                    if (methodInfo.IsConstructor)
                    {
                        menuName = className + CbSTUtils.MENU_CONSTRUCTOR + className;
                    }
                    else
                    {
                        string group = "";
                        if (methodInfo.Name.Contains("_"))
                        {
                            // 最初に見つかった _ までの文字列でグループを分ける

                            group = methodInfo.Name.Split("_")[0] + ".";
                        }
                        menuName += genericArgs;
                        if (methodInfo.IsStatic)
                        {
                            addState = CbSTUtils.MENU_STATIC;
                        }
                        if (methodInfo.IsVirtual)
                        {
                            addState = CbSTUtils.MENU_VIRTUAL;
                        }
                        switch (group)
                        {
                            case "get.":
                                menuName = className + CbSTUtils.MENU_GETTER + menuName;
                                break;
                            case "set.":
                                menuName = className + CbSTUtils.MENU_SETTER + menuName;
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
                                        menuName = className + ".(base)." + menuName;
                                    }
                                    else
                                    {
                                        menuName = className + "." + group + methodInfo.Name + "." + menuName;
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
                }

                // 引数情報を追加する
                menuName += MakeParamsString(methodInfo);

                // 返り値の型名を追加する
                if (methodInfo.IsConstructor && classType.IsGenericType)
                {
                    menuName += " : " + className;
                }
                else
                {
                    var type = retType("");
                    if (type.MyType == typeof(CbClass<CbGeneMethArg>))
                        menuName += " : " + CbSTUtils.GetGenericTypeName((type.Data as CbGeneMethArg).ArgumentType);
                    else
                        menuName += " : " + type.TypeName;
                }

                string helpCode = $"{classType.Namespace}:{className}." + nodeName.Replace(" ", "_");
                menuName = menuName + " " + addState;
                helpCode = helpCode + " " + addState;

                // スクリプトノード用のヒント
                string nodeHint = null;
                string nodeHintTitle = menuName;
                // ノード用ヒントを取得
                nodeHint = Language.Instance[$"Assembly.{helpCode}/node"];
                nodeHint = $"【{nodeHintTitle}】" + (nodeHint is null ? "" : Environment.NewLine + nodeHint);

                // メニュー用のヒント
                string hint = null;
                if (methodAttr != null)
                {
                    // メニュー用ヒントをリソースから取得

                    hint = Language.Instance[$"Assembly.{helpCode}/menu"];
                }

                if (hint is null)
                    hint = nodeHint;
                if (hint is null)
                    hint = "";

                // ノード化依頼用の情報をセット
                AutoImplementFunctionInfo autoImplementFunctionInfo = new AutoImplementFunctionInfo()
                {
                    assetCode = nodeCode,
                    menuTitle = menuName,
                    funcTitle = nodeName,
                    hint = hint,
                    nodeHint = nodeHint,
                    classType = classType,
                    returnType = () => retType(""),
                    argumentTypeList = argumentList,
                    dllModule = module,
                    isConstructor = methodInfo.IsConstructor,
                    typeRequests = genericTypeRequests,
                    genericMethodParameters = (methodInfo.IsGenericMethod) ? methodInfo.GetGenericArguments() : null,
                };

                return autoImplementFunctionInfo;
            }
            return null;
        }

        private static void MakeGenericParamaterTypeRequestList(Type classType, List<TypeRequest> genericTypeRequests)
        {
            foreach (var geneArg in classType.GetGenericArguments())
            {
                SetParamaterTypeFilter(genericTypeRequests, geneArg);
            }
        }

        private static void MakeGenericParamaterTypeRequestList(MethodBase method, List<TypeRequest> genericTypeRequests)
        {
            foreach (var geneArg in method.GetGenericArguments())
            {
                SetParamaterTypeFilter(genericTypeRequests, geneArg);
            }
        }

        /// <summary>
        /// パラメータ型の制約判定をフィルターに登録します。
        /// </summary>
        /// <param name="genericTypeRequests">型リクエストリスト</param>
        /// <param name="geneArg">パラメータ型</param>
        private static void SetParamaterTypeFilter(List<TypeRequest> genericTypeRequests, Type geneArg)
        {
            if (!geneArg.IsGenericParameter)
                return;

            if (genericTypeRequests.Any(n => n.Name == geneArg.Name))
                return; // 二重登録拒否

            // t には、判定対象の型が入ります。
            Func<Type, bool> isAccept = (t) =>
            {
                if (geneArg.GetGenericParameterConstraints().Length > 0)
                {
                    foreach (var constraint in geneArg.GetGenericParameterConstraints())
                    {
                        if (t.IsAssignableFrom(constraint))
                            return true;
                    }
                    return false;
                }

                GenericParameterAttributes sConstraints =
                            geneArg.GenericParameterAttributes &
                            GenericParameterAttributes.SpecialConstraintMask;

                if (sConstraints != GenericParameterAttributes.None)
                {
                    if (GenericParameterAttributes.None != (sConstraints &
                        GenericParameterAttributes.DefaultConstructorConstraint))
                    {
                        var query = t.GetMethods(BindingFlags.Public).Where(n => n.IsConstructor);
                        bool haveDefaultConstructer = query.Any(n => n.GetParameters().Length == 0);
                        if (!haveDefaultConstructer)
                           return false;    // デフォルトコンストラクタを持っていなければ拒否
                    }
                    if (GenericParameterAttributes.None != (sConstraints &
                        GenericParameterAttributes.ReferenceTypeConstraint))
                    {
                        if (!t.IsClass)
                            return false;   // リファレンス型でなければ拒否
                    }
                    if (GenericParameterAttributes.None != (sConstraints &
                        GenericParameterAttributes.NotNullableValueTypeConstraint))
                    {
                        if (t.GetGenericTypeDefinition() != typeof(Nullable<>))
                            return false;   // null許容型でなければ拒否
                    }
                }

                return true;
            };

            genericTypeRequests.Add(new TypeRequest(isAccept, geneArg.Name));
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
                    addParam.IsByRef = argNode.IsByRef;
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
