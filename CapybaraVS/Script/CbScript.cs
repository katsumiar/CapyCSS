using CapybaraVS.Controls;
using CapybaraVS.Controls.BaseControls;
using System;
using System.Collections.Generic;
using System.Windows.Input;
using System.Windows;
using System.Linq;
using CapybaraVS.Script;
using CapybaraVS;
using System.Diagnostics;
using static CapybaraVS.Controls.BaseControls.CommandCanvas;

namespace CbVS.Script
{
    public class CbScript
    {
        /// <summary>
        /// object 型を除く
        /// </summary>
        public static Func<Type, bool> AcceptAll => t => true;

        /// <summary>
        /// Enum 型
        /// </summary>
        public static Func<Type, bool> IsEnum => t => t.IsEnum;

        /// <summary>
        /// object 型を除く
        /// </summary>
        public static Func<Type, bool> IsNotObject => t => t != typeof(object);

        /// <summary>
        // 演算可能な型
        /// </summary>
        public static Func<Type, bool> IsValueType => t => t == typeof(decimal) || (t.IsValueType && !t.IsEnum && t.IsPrimitive && t != typeof(bool));

        /// <summary>
        /// signed型
        /// </summary>
        public static Func<Type, bool> IsSigned => t => IsValueType(t) && (t == typeof(short) || t == typeof(int) || t == typeof(long) || t == typeof(float) || t == typeof(double) || t == typeof(sbyte) || t == typeof(decimal));

        /// <summary>
        /// アセットコードでノードを作成します。
        /// </summary>
        /// <param name="assetCode">アセットコード</param>
        /// <returns>ノード</returns>
        public static MultiRootConnector CreateFunction(CommandCanvas OwnerCommandCanvas, string assetCode)
        {
            var ret = new MultiRootConnector();
            ret.AssetFuncType = assetCode;
            ret.AssetType = FunctionType.FuncType;
            ret.OwnerCommandCanvas = OwnerCommandCanvas;
            return ret;
        }

        /// <summary>
        /// 指定されたアセットコードに対してウインドウを表示して変数の型を選択しノードを作成します。
        /// </summary>
        /// <param name="assetCode">アセットコード</param>
        /// <param name="multiRootConnector">対象のMultiRootConnector</param>
        /// <param name="stackNode">変数の登録領域</param>
        /// <param name="forcedListTypeSelect">リスト型を選択するか？</param>
        /// <returns>ノード</returns>
        private static MultiRootConnector _CreateFreeTypeVariableFunction(
            CommandCanvas OwnerCommandCanvas, 
            string assetCode,
            List<TypeRequest> typeRequests, 
            MultiRootConnector multiRootConnector, 
            StackNode stackNode, 
            bool forcedListTypeSelect)
        {
            if (multiRootConnector is null)
            {
                List<string> typeNames;
                if (typeRequests == null && OwnerCommandCanvas.ScriptWorkStack.StackData.Count != 0)
                {
                    // 既存の変数から選択する

                    stackNode = ListSelectWindow.Create(
                        OwnerCommandCanvas,
                        "Variable",
                        OwnerCommandCanvas.ScriptWorkStack.StackData,
                        forcedListTypeSelect,
                        new Point(Mouse.GetPosition(null).X, Mouse.GetPosition(null).Y));

                    if (stackNode is null)
                        return null;

                    typeNames = new List<string>();
                    typeNames.Add(stackNode.ValueData.OriginalType.FullName);
                }
                else
                {
                    // 変数を新規作成する

                    typeNames = OwnerCommandCanvas.RequestTypeName(typeRequests);

                    if (typeNames is null)
                        return null;

                    int nameIndex = 1;
                    string name;
                    do
                    {
                        name = "variable" + nameIndex++;
                    }
                    while (OwnerCommandCanvas.ScriptWorkStack.NameContains(name));

                    ListSelectWindow.DefaultValue = CbST.CbCreate(CbST.GetTypeEx(typeNames[0]), name);
                    stackNode = OwnerCommandCanvas.ScriptWorkStack.Append(ListSelectWindow.DefaultValue).stackNode;
                    ListSelectWindow.DefaultValue = null;
                }

                multiRootConnector = new MultiRootConnector();
                multiRootConnector.OwnerCommandCanvas = OwnerCommandCanvas;
                multiRootConnector.SelectedVariableTypes = typeNames.ToArray();

                multiRootConnector.AssetFuncType = assetCode;
            }

            multiRootConnector.AttachParam = new MultiRootConnector.AttachVariableId(stackNode.Id);
            multiRootConnector.AssetType = FunctionType.FuncType;

            return multiRootConnector;
        }

        /// <summary>
        /// 指定されたアセットコードに対して変数の型を選択しノードを作成します。
        /// </summary>
        /// <param name="assetCode">アセットコード</param>
        /// <param name="cbType">選択された型の格納先</param>
        /// <param name="ignoreTypes">選択除外の型を指定</param>
        /// <param name="forcedListTypeSelect">リスト型を選択するか？</param>
        /// <returns>ノード</returns>
        public static MultiRootConnector CreateFreeTypeVariableFunction(
            CommandCanvas OwnerCommandCanvas, 
            string assetCode,
            List<TypeRequest> typeRequests, 
            bool forcedListTypeSelect = false)
        {
            MultiRootConnector multiRootConnector = null;
            StackNode stackNode = null;
            return _CreateFreeTypeVariableFunction(OwnerCommandCanvas, assetCode, typeRequests, multiRootConnector, stackNode, forcedListTypeSelect);
        }

        /// <summary>
        /// 指定されたアセットコードに対して引数の型を選択しノードを作成します。
        /// </summary>
        /// <param name="assetCode">アセットコード</param>
        /// <param name="cbType">選択された型の格納先</param>
        /// <param name="ignoreTypes">選択除外の型を指定</param>
        /// <returns>ノード</returns>
        public static MultiRootConnector CreateFreeTypeFunction(
            CommandCanvas OwnerCommandCanvas, 
            string assetCode,
            List<TypeRequest> typeRequests = null)
        {
            var ret = new MultiRootConnector();
            ret.OwnerCommandCanvas = OwnerCommandCanvas;
            if (typeRequests != null)
            {
                // 型選択要求用のタイトルを作成する

                string methodName = assetCode;
                string className = null;
                if (methodName.Contains("`"))
                {
                    className = methodName.Substring(0, methodName.IndexOf("`"));
                }
                if (methodName.Contains(".."))
                {
                    methodName = methodName.Substring(methodName.LastIndexOf("..") + 1);
                }
                else
                {
                    methodName = methodName.Substring(methodName.LastIndexOf(".") + 1);
                }
                if (methodName.Contains("#"))
                {
                    methodName = methodName.Substring(0, methodName.IndexOf('#'));
                    string args = null;
                    foreach (var typeRequest in typeRequests)
                    {
                        if (args is null)
                            args = "<" + typeRequest.Name;
                        else
                            args += "," + typeRequest.Name;
                    }
                    methodName += args + ">";
                }
                else
                    methodName += "<>";  // 本来ジェネリックメソッドには引数がある筈だが、システムの提供するメソッドでそうでない場合がある
                if (className != null)
                {
                    methodName = className + "." + methodName;
                }
                List<string> typeNames = OwnerCommandCanvas.RequestTypeName(typeRequests, $"[{methodName}] ");
                if (typeNames is null)
                    return null;
                ret.SelectedVariableTypes = typeNames.ToArray();
            }
            ret.AssetFuncType = assetCode;
            ret.AssetType = FunctionType.FuncType;
            return ret;
        }

        /// <summary>
        /// 引数の型を選択しノードを作成します。
        /// </summary>
        /// <param name="cbType">選択された型の格納先</param>
        /// <param name="ignoreTypes">選択除外の型を指定</param>
        /// <returns>ノード</returns>
        public static MultiRootConnector SelectVariableType(
            CommandCanvas OwnerCommandCanvas,
            List<TypeRequest> typeRequests = null)
        {
            var ret = new MultiRootConnector();
            ret.OwnerCommandCanvas = OwnerCommandCanvas;
            if (typeRequests != null)
            {
                List<string> typeNames = OwnerCommandCanvas.RequestTypeName(typeRequests);
                if (typeNames is null)
                    return null;
                ret.SelectedVariableTypes = typeNames.ToArray();
            }
            ret.AssetType = FunctionType.LiteralType;
            return ret;
        }
    }
}
