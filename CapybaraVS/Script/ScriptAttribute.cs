﻿//#define DEBUG_IMPORT    // インポート機能のデバッグモード

using CapyCSS.Controls.BaseControls;
using CapyCSS.Controls;
using CapyCSS.Script;
using CapyCSS.Script.Lib;
using CbVS.Script;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using static CapyCSS.Controls.BaseControls.CommandCanvas;
using CapyCSSattribute;

namespace CapyCSS.Script
{
    public class ScriptImplement
    {
        public static string ImportingName = "";

        /// <summary>
        /// 引数情報です。
        /// </summary>
        public struct ArgumentInfoNode
        {
            public Func<ICbValue> CreateArgument = null;
            public bool IsByRef = false;
            public bool IsOut = false;
            public bool IsIn = false;
            public bool IsSelf = false;
            public ArgumentInfoNode()
            {

            }
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

            // 基本DLLを追加する
            foreach (var dllName in CbSTUtils.BaseDllList)
            {
                Assembly assembly = Assembly.Load(dllName);
                GetApiFromAssemblyForScriptMethodAttribute(OwnerCommandCanvas, node, assembly);
            }
        }

        /// <summary>
        /// ネームスペース名からメソッドをスクリプトで使えるように取り込みます。
        /// </summary>
        /// <param name="OwnerCommandCanvas">オーナーキャンバス</param>
        /// <param name="node">登録先のノード</param>
        /// <param name="name">ネームスペース名</param>
        /// <returns>インポートしたネームスペース名</returns>
        public static string ImportScriptMethodsFromNameSpace(
            CommandCanvas OwnerCommandCanvas,
            TreeMenuNode node,
            string name)
        {
            string outputName = ModuleControler.HEADER_NAMESPACE + name;
            ImportingName = outputName;
            ICollection<Type> types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(assembly => assembly.GetTypes()).Where(ct => ct.Namespace == name).ToList();
            ICollection<AutoImplementFunctionInfo> addMethodInfos = new List<AutoImplementFunctionInfo>();
            ICollection<Type> addTypeInfos = new List<Type>();
            ImportScriptMethods(
                OwnerCommandCanvas,
                (info) => addMethodInfos.Add(info),
                null,
                null,
                (t) => addTypeInfos.Add(t),
                types.ToArray()
                );
            TreeMenuNode methodGroup = ImplementAsset.CreateGroup(node, outputName);
            foreach (var methodInfo in addMethodInfos)
            {
                // スレッド上で処理できないのでここで登録する

                CreateMethodNode(OwnerCommandCanvas, methodGroup, methodInfo);
            }
            foreach (var typeInfo in addTypeInfos)
            {
                // スレッド上で処理できないのでここで登録する

                OwnerCommandCanvas.AddImportTypeMenu(typeInfo);
            }
            Console.WriteLine($"imported namespace {name}");
            return outputName;
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
                return false;   // 抽象メソッドは呼べない

            if (methodInfo is MethodInfo method)
            {
                // Task や async を返すものは今のところ使えないので取り込まない

                if (CheckTaskType(method.ReturnParameter.ParameterType))
                {
                    return false;
                }
                foreach (var arg in method.GetParameters())
                {
                    if (CheckTaskType(arg.ParameterType))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private static bool CheckTaskType(Type type)
        {
            if (typeof(IAsyncResult).IsAssignableFrom(type))
            {
                return true;
            }
            if (typeof(ValueTask).IsAssignableFrom(type))
            {
                return true;
            }
            if (type.IsGenericType)
            {
                if (type.GetGenericTypeDefinition() == typeof(ValueTask<>))
                {
                    return true;
                }
                if (type.GetGenericTypeDefinition() == typeof(IAsyncEnumerator<>))
                {
                    return true;
                }
            }
            return false;
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

            if (classType.IsValueType)
                return true;    // 値型は扱う

            if (!classType.IsClass)
                return false;   // クラス以外は、扱わない

            if (typeof(Exception).IsAssignableFrom(classType) || typeof(IAsyncResult).IsAssignableFrom(classType))
            {
                // 例外クラスは扱わない

                return false;
            }

            return true;
        }

        /// <summary>
        /// 受け入れる型を判定します。
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
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

            if (type.IsValueType)
                return true;

            return false;
        }

        /// <summary>
        /// Assemblyからメソッドをスクリプトで使えるように取り込みます。
        /// </summary>
        /// <param name="OwnerCommandCanvas">オーナーキャンバス</param>
        /// <param name="importMethod">メソッド取り込み処理</param>
        /// <param name="asm">対象Assembly</param>
        /// <param name="module">モジュール</param>
        /// <param name="importNames">型取り込み処理</param>
        public static void ImportScriptMethods(
            CommandCanvas OwnerCommandCanvas,
            Action<AutoImplementFunctionInfo> importMethod,
            Assembly asm,
            Module module,
            ICollection<string> importNames,
            Action<Type> importType)
        {
            Type[] types;
            if (module is null)
            {
                types = asm.GetTypes();
            }
            else
            {
                types = module.GetTypes();
            }
            ImportScriptMethods(OwnerCommandCanvas, importMethod, module, importNames, importType, types);
        }

        private static void ImportScriptMethods(
            CommandCanvas OwnerCommandCanvas,
            Action<AutoImplementFunctionInfo> importMethod, 
            Module module, 
            ICollection<string> importNames,
            Action<Type> importType,
            Type[] types)
        {
            Task<IEnumerable<Type>> tcTask = null;
            if (importType != null)
            {
                // 型情報を収集する

                tcTask = Task.Run(() =>
                {
                    Predicate<Type> ignorePredicater;
                    if (importNames != null)
                    {
                        ignorePredicater = (type) => !importNames.Any(n => type.FullName.StartsWith(n)) || !IsAcceptTypeMenuType(type);
                    }
                    else
                    {
                        ignorePredicater = (type) => !IsAcceptTypeMenuType(type);
                    }
                    IEnumerable<Type> resultTypes = types.Where(t => !ignorePredicater(t)).ToList();
                    return resultTypes;
                });
#if DEBUG_IMPORT
                tcTask.Wait();
#endif
            }

            IEnumerable<Task<IEnumerable<AutoImplementFunctionInfo>>> tasks = CreateMakeImportFunctionInfoTasks(module, importNames, types);

            Task[] addMethodsAndTypesTasks = new Task[] {
                // メソッドをメニューに登録する
                Task.Run(() =>
                {
                    IEnumerable<AutoImplementFunctionInfo> autoImplementFunctionInfos =
                        tasks.Where(t => t.Result != null).SelectMany(classInfos => classInfos.Result).Where(info => info != null).ToList();
                    foreach (AutoImplementFunctionInfo classInfo in autoImplementFunctionInfos)
                    {
                        importMethod(classInfo);
                    }
                }),
                // 型をメニューに登録する
                Task.Run(() =>
                {
                    if (tcTask != null)
                    {
                        var typeList = tcTask.Result;
                        foreach (var type in typeList)
                        {
                            importType(type);
                        }
                    }
                })
            };

            Task.WaitAll(addMethodsAndTypesTasks);
        }

        /// <summary>
        /// メソッド取り込み用情報収集タスクリストを作成します。
        /// </summary>
        /// <param name="module">モジュール</param>
        /// <param name="importNameList">取り込む名前リスト</param>
        /// <param name="types">モジュールの情報</param>
        /// <returns>タスクリスト</returns>
        private static IEnumerable<Task<IEnumerable<AutoImplementFunctionInfo>>> CreateMakeImportFunctionInfoTasks(
            Module module, 
            ICollection<string> importNameList, 
            Type[] types)
        {
            var tasks = new List<Task<IEnumerable<AutoImplementFunctionInfo>>>();
            Type actionType = typeof(Action);
            foreach (Type classType in types)
            {
                if (importNameList != null && importNameList.Count > 0 && !importNameList.Any(n => classType.FullName.StartsWith(n)))
                    continue;

                if (!IsAcceptClass(classType))
                    continue;   // 扱えない

                if (!classType.IsAbstract)
                {
                    // コンストラクタをインポートする

                    Task<IEnumerable<AutoImplementFunctionInfo>> importConstructorTask = Task.Run(() =>
                    {
                        return CreateMakeImportConstructorInfoTasks(module, classType);
                    });
                    tasks.Add(importConstructorTask);
#if DEBUG_IMPORT
                    importConstructorTask.Wait();
#endif
                }

                // メソッドをインポートする
                Task<IEnumerable<AutoImplementFunctionInfo>> importMethodTask = Task.Run(() =>
                {
                    return CreateMakeImportMethodInfoTasks(module, classType);
                });
                tasks.Add(importMethodTask);
#if DEBUG_IMPORT
                importMethodTask.Wait();
#endif
            }

            return tasks;
        }

        private static IEnumerable<AutoImplementFunctionInfo> CreateMakeImportConstructorInfoTasks(Module module, Type classType)
        {
            ICollection<AutoImplementFunctionInfo> importFuncInfoList = new List<AutoImplementFunctionInfo>();
            foreach (ConstructorInfo constructorInfo in classType.GetConstructors())
            {
                if (!IsAcceptMethod(classType, constructorInfo))
                    continue;   // 未対応

#if !DEBUG_IMPORT
                try
#endif
                {
                    var functionInfo = MakeImportFunctionInfo(classType, constructorInfo, null, null, module);
                    if (functionInfo != null)
                    {
                        importFuncInfoList.Add(functionInfo);
                    }
                }
#if !DEBUG_IMPORT
                catch (Exception ex)
                {
                    CommandCanvasList.ErrorLog += nameof(ScriptImplement) + "." + nameof(CreateMakeImportConstructorInfoTasks) + ": " + ex.Message + Environment.NewLine + Environment.NewLine;
                }
#endif
            }
            return importFuncInfoList;
        }

        private static IEnumerable<AutoImplementFunctionInfo> CreateMakeImportMethodInfoTasks(Module module, Type classType)
        {
            ICollection<AutoImplementFunctionInfo> importFuncInfoList = new List<AutoImplementFunctionInfo>();
            foreach (MethodInfo methodInfo in classType.GetMethods())
            {
                if (!IsAcceptMethod(classType, methodInfo))
                    continue;   // 未対応

#if !DEBUG_IMPORT
                try
#endif
                {
                    var functionInfo = MakeImportFunctionInfo(classType, methodInfo, methodInfo.ReturnType, null, module);
                    if (functionInfo != null)
                    {
                        importFuncInfoList.Add(functionInfo);
                    }
                }
#if !DEBUG_IMPORT
                catch (Exception ex)
                {
                    CommandCanvasList.ErrorLog += nameof(ScriptImplement) + "." + nameof(CreateMakeImportMethodInfoTasks) + ": " + ex.Message + Environment.NewLine + Environment.NewLine;
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

                    //Debug.Assert(false);
                    // AddImportTypeMenu の間違い？
                    OwnerCommandCanvas.AddTypeMenu(type);
                }
            });

            foreach (Type classType in types)
            {
                var classAttr = classType.GetCustomAttribute(typeof(ScriptClassAttribute)) as ScriptClassAttribute;
                if (classAttr is null)
                {
                    continue;
                }

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

                            importScriptMethodAttributeMethods(OwnerCommandCanvas, node, classType, constructorInfo, classAttr, null);
                        }
#if !DEBUG_IMPORT
                        catch (Exception ex)
                        {
                            CommandCanvasList.ErrorLog += nameof(ScriptImplement) + "." + nameof(GetApiFromAssemblyForScriptMethodAttribute) + ": " + ex.Message + Environment.NewLine + Environment.NewLine;
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

                        importScriptMethodAttributeMethods(OwnerCommandCanvas, node, classType, methodInfo, classAttr, methodInfo.ReturnType);
                    }
#if !DEBUG_IMPORT
                    catch (Exception ex)
                    {
                        CommandCanvasList.ErrorLog += nameof(ScriptImplement) + "." + nameof(GetApiFromAssemblyForScriptMethodAttribute) + ": " + ex.Message + Environment.NewLine + Environment.NewLine;
                    }
#endif
                }

                task.Wait();
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
            IScriptArribute methodAttr,
            Type returnType)
        {
            List<Task<AutoImplementFunctionInfo>> tasks = CreateMakeImportFunctionInfoTasks(classType, methodInfo, methodAttr, returnType);

            if (tasks is null)
            {
                return;
            }

            // ノード化
            foreach (var info in tasks)
            {
                if (info is null || info.Result is null)
                    continue;

                CreateMethodNode(OwnerCommandCanvas, node, info.Result);
            }
        }

        public class ScriptAttributeInfo : IScriptArribute
        {
            public string Path { get; set; } = null;
            public string MethodName { get; set; } = null;
            public bool OldSpecification { get; set; } = false;
            public bool DefaultHide { get; set; } = false;
            public bool IsRunable { get; set; } = false;
        }

        /// <summary>
        /// メソッド取り込み用情報収集タスクリストを作成します。
        /// </summary>
        /// <param name="classType">クラスの型情報</param>
        /// <param name="methodInfo">メソッド情報</param>
        /// <param name="returnType">メソッドの返り値の型情報</param>
        /// <returns>タスクリスト</returns>
        private static List<Task<AutoImplementFunctionInfo>> CreateMakeImportFunctionInfoTasks(
            Type classType, 
            MethodBase methodInfo,
            IScriptArribute methodAttr,
            Type returnType)
        {
            var tasks = new List<Task<AutoImplementFunctionInfo>>();

            var scriptAttributeInfo = new ScriptAttributeInfo()
            {
                Path = methodAttr.Path,
                MethodName = methodAttr.MethodName,
                OldSpecification = methodAttr.OldSpecification,
                DefaultHide = methodAttr.DefaultHide,
                IsRunable = methodAttr.IsRunable,
            };

            var ovrMethodAttr = methodInfo.GetCustomAttribute(typeof(ScriptMethodAttribute)) as ScriptMethodAttribute;
            if (ovrMethodAttr != null)
            {
                if (!String.IsNullOrEmpty(ovrMethodAttr.Path))
                {
                    scriptAttributeInfo.Path = ovrMethodAttr.Path;
                }
                if (!String.IsNullOrEmpty(ovrMethodAttr.MethodName))
                {
                    scriptAttributeInfo.MethodName = ovrMethodAttr.MethodName;
                }
                if (ovrMethodAttr.OldSpecification)
                {
                    scriptAttributeInfo.OldSpecification = ovrMethodAttr.OldSpecification;
                }
                if (ovrMethodAttr.IsRunable)
                {
                    scriptAttributeInfo.IsRunable = ovrMethodAttr.IsRunable;
                }
            }
            else if (scriptAttributeInfo.DefaultHide)
            {
                return null;
            }

            if (methodAttr != null)
            {
                Task<AutoImplementFunctionInfo> task = Task.Run(() =>
                {
                    return MakeImportFunctionInfo(classType, methodInfo, returnType, scriptAttributeInfo);
                });
                tasks.Add(task);
#if DEBUG_IMPORT
                task.Wait();
#endif
            }
            return tasks;
        }

        static Type _VoidType = null;

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
        private static AutoImplementFunctionInfo MakeImportFunctionInfo(
            Type classType,
            MethodBase methodInfo, 
            Type returnType,
            IScriptArribute methodAttr = null,
            Module module = null)
        {
            if (returnType != null && returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>))
                return null;
            if (returnType == typeof(Task))
                return null;

            IList<ArgumentInfoNode> argumentList = null;

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

                var refType = methodInfo.ReflectedType;
                if ((_VoidType != null && refType == _VoidType) ||
                    (_VoidType == null
                        && refType.IsValueType   // 絞り込み
                        && refType.Name == "Void"   // 絞り込み
                        && refType.FullName == "System.Void"))
                {
                    // System.Void に所属するものには対応しない
                    // System.Void は、ジェネリック引数に使えないので CbStruct で管理できない…

                    _VoidType = refType;
                    return null;
                }
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
                    if ((_VoidType != null && returnType == _VoidType) ||
                        (_VoidType == null
                            && returnType.IsValueType   // 絞り込み
                            && returnType.Name == "Void"   // 絞り込み
                            && returnType.FullName == "System.Void"))
                    {
                        // void 型は専用のクラスを利用する

                        _VoidType = returnType;
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
                    addArg += "_" + CbSTUtils.GetTypeName(para.ParameterType);
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
                string nodeTitle;
                if (methodAttr != null)
                {
                    // メソッド属性情報を持っている

                    if (methodAttr.MethodName != null)
                    {
                        // 指定の名前を採用する

                        nodeTitle = nodeName = methodAttr.MethodName;

                    }
                    else
                    {
                        nodeTitle = nodeName = methodInfo.Name;
                    }
                }
                else
                {
                    if (methodInfo.IsConstructor)
                    {
                        if (methodInfo.IsAbstract)
                        {
                            // 抽象クラスは new できない

                            return null;
                        }
                        nodeName = CbSTUtils.NEW_STR + " " + className;
                        nodeTitle = className;
                    }
                    else
                    {
                        nodeTitle = nodeName = methodInfo.Name;
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
                else
                {
                    // 名前を作成する

                    if (methodInfo.IsConstructor)
                    {
                        menuName = className + CbSTUtils.MENU_CONSTRUCTOR + className;
                    }
                    else
                    {
                        string group = "";
                        if (methodInfo.Name.Contains("_") && methodInfo.IsSpecialName)
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
                                menuName = menuName.Replace("get_","");
                                nodeTitle = nodeTitle.Replace("get_","");
                                menuName = className + CbSTUtils.MENU_GETTER + menuName;
                                break;
                            case "set.":
                                menuName = menuName.Replace("set_", "");
                                nodeTitle = nodeTitle.Replace("set_", "");
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

                bool isProperty = methodInfo.IsSpecialName && (methodInfo.Name.StartsWith("get_") || methodInfo.Name.StartsWith("set_"));

                // 引数情報を追加する
                string paramString = MakeParamsString(methodInfo);
                if (!isProperty)
                {
                    menuName += paramString;
                }
                string nodeHintTitle = $"{classType.Namespace}.{className}.{nodeTitle}";
                if (!isProperty)
                {
                    nodeHintTitle += paramString;
                }

                // 返り値の型名を追加する
                if (methodInfo.IsConstructor && classType.IsGenericType)
                {
                    menuName += " : " + className;
                }
                else
                {
                    var type = retType("");
                    string returnTypeString;
                    if (type.MyType == typeof(CbClass<CbGeneMethArg>))
                        returnTypeString = CbSTUtils.GetGenericTypeName((type.Data as CbGeneMethArg).ArgumentType);
                    else
                        returnTypeString = type.TypeName;
                    menuName += " : " + returnTypeString;
                    nodeHintTitle += " : " + returnTypeString;
                }

                string helpCode = $"{classType.Namespace}:{className}." + nodeName.Replace(" ", "_") + " " + addState;

                bool _oldSpecification = false;
                bool isRunable = false;
                if (methodAttr != null)
                {
                    // 古い仕様指定されているか？
                    _oldSpecification = methodAttr.OldSpecification;
                    isRunable = methodAttr.IsRunable;
                }

                Func<string> getNodeHint = () =>
                {
                    // スクリプトノード用のヒント
                    string nodeHint = Language.Instance[$"Assembly.{helpCode}/node"];
                    return nodeHint = $"【{nodeHintTitle}】" + (nodeHint is null ? "" : Environment.NewLine + nodeHint);
                };

                Func<string> getHint = () =>
                {
                    // メニュー用ヒントをリソースから取得
                    string hint = Language.Instance[$"Assembly.{helpCode}/menu"];
                    if (hint is null)
                    {
                        hint = getNodeHint();
                    }
                    if (hint is null)
                    {
                        hint = "";
                    }
                    return hint;
                };

                Func<string> getMenuTitle = () =>
                {
                    // メニュー項目名
                    string addTitleMessage = "";
                    if (methodAttr != null)
                    {
                        if (methodAttr.OldSpecification)
                        {
                            addTitleMessage += CbSTUtils.MENU_OLD_SPECIFICATION;
                        }
                        else if (methodAttr.IsRunable)
                        {
                            addTitleMessage += CbSTUtils.MENU_RUNABLE;
                        }
                    }
                    return menuName + " " + addState + addTitleMessage;
                };

                // ノード化依頼用の情報をセット
                AutoImplementFunctionInfo autoImplementFunctionInfo = new AutoImplementFunctionInfo()
                {
                    assetCode = nodeCode,
                    menuTitle = getMenuTitle,
                    funcTitle = nodeTitle,
                    hint = getHint,
                    nodeHint = getNodeHint,
                    classType = classType,
                    returnType = () => retType(""),
                    argumentTypeList = argumentList,
                    dllModule = module,
                    isConstructor = methodInfo.IsConstructor,
                    typeRequests = genericTypeRequests,
                    genericMethodParameters = (methodInfo.IsGenericMethod) ? methodInfo.GetGenericArguments() : null,
                    oldSpecification = _oldSpecification,
                    isRunable = isRunable,
                    IsProperty = isProperty,
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
        /// パラメータ型の型制約フィルターを登録します。
        /// </summary>
        /// <param name="genericTypeRequests">型リクエストリスト</param>
        /// <param name="geneArg">パラメータ型</param>
        private static void SetParamaterTypeFilter(List<TypeRequest> genericTypeRequests, Type geneArg)
        {
            if (!geneArg.IsGenericParameter)
                return;

            if (genericTypeRequests.Any(n => n.Name == geneArg.Name))
                return; // 二重登録拒否

            Func<Type, bool> isAccept = MakeParameterConstraintAccepter(geneArg);
            genericTypeRequests.Add(new TypeRequest(isAccept, geneArg.Name));
        }

        /// <summary>
        /// パラメータ型の型制約フィルターを作成します。
        /// </summary>
        /// <param name="geneArg"></param>
        /// <returns></returns>
        public static Func<Type, bool> MakeParameterConstraintAccepter(Type geneArg)
        {
            Func<Type, bool> isAccept = (t) =>
            {
                // t には、判定対象の型が入ります。

                if (!CbScript.AcceptAll(t))
                {
                    return false;
                }

                return IsConstraintSatisfied(geneArg, t);
            };
            return isAccept;
        }

        /// <summary>
        /// IsConstraintSatisfiedの複数ジェネリックパラメータ対応版です。
        /// </summary>
        /// <param name="genericTypeDefinition">ジェネリック型</param>
        /// <param name="targetTypes">ジェネリック引数配列</param>
        /// <returns></returns>
        public static bool AreConstraintsSatisfied(Type genericTypeDefinition, Type[] targetTypes)
        {
            Type[] genericTypes = genericTypeDefinition.GetGenericArguments();

            if (targetTypes is null)
            {
                return genericTypes.Length == 0;
            }

            // Check if the number of generic parameters matches the number of target types
            if (genericTypes.Length != targetTypes.Length)
            {
                // The number of generic parameters and target types must be the same.
                return false;
            }

            return genericTypes.Zip(targetTypes, IsConstraintSatisfied).All(result => result);
        }

        /// <summary>
        /// genericType型のジェネリック引数target型がパラメータ制約を満たすかどうかを判断します。
        /// ※要検証
        /// </summary>
        /// <param name="genericType">ジェネリック引数を持つ型</param>
        /// <param name="target">パラメータ制約を満たすかどうかを判断する型</param>
        /// <returns>true==制約を満たす</returns>
        public static bool IsConstraintSatisfied(Type genericType, Type targetType)
        {
            var sConstraints = genericType.GenericParameterAttributes & GenericParameterAttributes.SpecialConstraintMask;

            if (sConstraints != GenericParameterAttributes.None)
            {
                if (targetType.IsClass && (sConstraints & GenericParameterAttributes.DefaultConstructorConstraint) != GenericParameterAttributes.None)
                {
                    var constructors = targetType.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
                    bool haveDefaultConstructor = constructors.Any(constructor => constructor.GetParameters().Length == 0);
                    if (!haveDefaultConstructor)
                        return false;
                }

                if ((sConstraints & GenericParameterAttributes.ReferenceTypeConstraint) != GenericParameterAttributes.None)
                {
                    if (targetType.IsValueType || targetType.IsPointer)
                        return false;
                }

                if ((sConstraints & GenericParameterAttributes.NotNullableValueTypeConstraint) != GenericParameterAttributes.None)
                {
                    if (!targetType.IsValueType || (targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(Nullable<>)))
                        return false;// C# 8.0以降のNullable Reference Typesは、後々考える
                }
            }

            if (genericType.GetInterfaces().Any(interfaceType => !targetType.GetInterfaces().Contains(interfaceType)))
                return false;

            if (genericType.IsGenericParameter)
                return true;

            if (genericType != typeof(object) && genericType.BaseType.IsAssignableFrom(targetType))
                return true;

            if (genericType.IsAssignableFrom(targetType))
                return true;

            return false;
        }

        /// <summary>
        /// スクリプト用メソッド情報を基にメソッドをノード化してメニューに紐付けます。
        /// </summary>
        /// <param name="OwnerCommandCanvas">オーナーキャンバス</param>
        /// <param name="node">登録先のノード</param>
        /// <param name="autoImplementFunctionInfo">スクリプト用メソッド情報</param>
        private static void CreateMethodNode(CommandCanvas OwnerCommandCanvas, TreeMenuNode node, AutoImplementFunctionInfo autoImplementFunctionInfo)
        {
            ImplementAsset.CreateAssetMenu(
                OwnerCommandCanvas,
                node,
                AutoImplementFunction.Create(autoImplementFunctionInfo)
            );
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

                    string argStr = CbSTUtils.GetTypeName(para.ParameterType);
                    argStr = ReplaceModifier(para, argStr);
                    paramsStr += argStr;

                    isFirst = false;
                }
                paramsStr += ")";
            }
            return paramsStr;
        }

        /// <summary>
        /// 修飾子を置き換えます。
        /// </summary>
        /// <param name="para">パラメータ情報</param>
        /// <param name="argStr">置き換える前の名前</param>
        /// <returns>置き換えた文字列</returns>
        private static string ReplaceModifier(ParameterInfo para, string argStr)
        {
            argStr = argStr.Replace("&", "");
            if (para.IsIn)
            {
                argStr = $"{CbSTUtils.MENU_IN_STR} " + argStr;
            }
            else if (para.IsOut)
            {
                argStr = $"{CbSTUtils.MENU_OUT_STR} " + argStr;
            }
            else if (para.ParameterType.IsByRef)
            {
                argStr = $"{CbSTUtils.MENU_REF_STR} " + argStr;
            }
            return argStr;
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
        private static IList<ArgumentInfoNode> MakeSelfTypeForClassMethod(
            MethodBase methodInfo,
            IList<ArgumentInfoNode> argumentList,
            Func<string, ICbValue> selfType)
        {
            ArgumentInfoNode argNode = new ArgumentInfoNode();
            string name = "self";// info.ReflectedType.Name;
            foreach (var attrNode in methodInfo.ReflectedType.GetCustomAttributes())
            {
                if (attrNode is ScriptParamAttribute scriptParam)
                {
                    name = scriptParam.ParamName;
                    break;
                }
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
            ref IList<ArgumentInfoNode> argumentList)
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

                // 引数の修飾をチェック
                if (para.IsIn)
                {
                    argNode.IsIn = true;
                }
                else if (para.IsOut)
                {
                    argNode.IsOut = true;
                }
                else if (para.ParameterType.IsByRef)
                {
                    // IsIn と IsOut が false

                    argNode.IsByRef = true;
                }

                // 引数名を取得
                string name = para.Name;
                foreach (var attrNode in para.GetCustomAttributes())
                {
                    if (attrNode is ScriptParamAttribute scriptParam)
                    {
                        name = scriptParam.ParamName;
                        break;
                    }
                }

                if (typeInfo(name) is null)
                    return false;   // 型情報の作成に失敗

                // 引数作成用の処理を作成
                argumentList ??= new List<ArgumentInfoNode>();
                argNode.CreateArgument = () =>
                {
                    ICbValue addParam = typeInfo(name);
                    addParam.IsByRef = argNode.IsByRef;
                    addParam.IsOut = argNode.IsOut;
                    addParam.IsIn = argNode.IsIn;
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
