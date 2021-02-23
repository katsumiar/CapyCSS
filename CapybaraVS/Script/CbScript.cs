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

namespace CbVS.Script
{
    public class CbScript
    {
        /// <summary>
        /// object 型を除く
        /// </summary>
        public static Func<Type, bool> AcceptAll => (t) => true;

        /// <summary>
        /// Enum 型
        /// </summary>
        public static Func<Type, bool> IsEnum => (t) => t.IsEnum;

        /// <summary>
        /// object 型を除く
        /// </summary>
        public static Func<Type, bool> IsNotObject => (t) => t != typeof(object);

        /// <summary>
        // 演算可能な型
        /// </summary>
        public static Func<Type, bool> IsValueType => (t) => t.IsValueType && !t.IsEnum && t.IsPrimitive || t == typeof(decimal);

        /// <summary>
        /// signed型
        /// </summary>
        public static Func<Type, bool> IsSigned => (t) => IsValueType(t) && (t == typeof(short) || t == typeof(int) || t == typeof(long) || t == typeof(float) || t == typeof(double) || t == typeof(sbyte) || t == typeof(decimal));

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
        /// 変数の型を選択します。
        /// </summary>
        /// <param name="typeName">選択された型の格納先</param>
        /// <param name="isAccept">受け付ける型を判定する処理</param>
        /// <returns>true = 有効</returns>
        private static bool _SelectVariableType(CommandCanvas OwnerCommandCanvas, ref string typeName, Func<Type, bool> isAccept)
        {
            if (typeName == CbSTUtils.FREE_ENUM_TYPE_STR)
            {
                typeName = OwnerCommandCanvas.RequestTypeString(isAccept);
            }
            if (typeName == CbSTUtils.FREE_TYPE_STR)
            {
                typeName = OwnerCommandCanvas.RequestTypeString(isAccept);
            }
            else
            {
                typeName = OwnerCommandCanvas.RequestGenericTypeName(typeName, isAccept);
            }
            if (typeName is null)
            {
                return false;
            }
            if (isAccept != null && !isAccept(CbST.GetTypeEx(typeName)))
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 指定されたアセットコードに対してウインドウを表示して変数の型を選択しノードを作成します。
        /// </summary>
        /// <param name="assetCode">アセットコード</param>
        /// <param name="multiRootConnector">対象のMultiRootConnector</param>
        /// <param name="stackNode">変数の登録領域</param>
        /// <param name="forcedListTypeSelect">リスト型を選択するか？</param>
        /// <returns>ノード</returns>
        private static MultiRootConnector _CreateFreeTypeVariableFunction(CommandCanvas OwnerCommandCanvas, string assetCode, string valueType, Func<Type, bool> isAccept, MultiRootConnector multiRootConnector, StackNode stackNode, bool forcedListTypeSelect)
        {
            if (multiRootConnector is null)
            {
                if (isAccept == null && OwnerCommandCanvas.ScriptWorkStack.StackData.Count != 0)
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

                    valueType = stackNode.ValueData.OriginalType.FullName;
                }
                else
                {
                    // 変数を新規作成する

                    if (!_SelectVariableType(OwnerCommandCanvas, ref valueType, isAccept))
                        return null;

                    int nameIndex = 1;
                    string name;
                    do
                    {
                        name = "variable" + nameIndex++;
                    }
                    while (OwnerCommandCanvas.ScriptWorkStack.NameContains(name));

                    ListSelectWindow.DefaultValue = CbST.CbCreate(CbST.GetTypeEx(valueType), name);
                    stackNode = OwnerCommandCanvas.ScriptWorkStack.Append(ListSelectWindow.DefaultValue).stackNode;
                    ListSelectWindow.DefaultValue = null;
                }

                multiRootConnector = new MultiRootConnector();
                multiRootConnector.OwnerCommandCanvas = OwnerCommandCanvas;
                multiRootConnector.AssetValueType = valueType;
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
        public static MultiRootConnector CreateFreeTypeVariableFunction(CommandCanvas OwnerCommandCanvas, string assetCode, string valueType, Func<Type, bool> isAccept = null, bool forcedListTypeSelect = false)
        {
            MultiRootConnector multiRootConnector = null;
            StackNode stackNode = null;

            return _CreateFreeTypeVariableFunction(OwnerCommandCanvas, assetCode, valueType, isAccept, multiRootConnector, stackNode, forcedListTypeSelect);
        }

        /// <summary>
        /// 指定されたアセットコードに対して引数の型を選択しノードを作成します。
        /// </summary>
        /// <param name="assetCode">アセットコード</param>
        /// <param name="cbType">選択された型の格納先</param>
        /// <param name="ignoreTypes">選択除外の型を指定</param>
        /// <returns>ノード</returns>
        public static MultiRootConnector CreateFreeTypeFunction(CommandCanvas OwnerCommandCanvas, string assetCode, string valueType, Func<Type, bool> isAccept = null)
        {
            if (!_SelectVariableType(OwnerCommandCanvas, ref valueType, isAccept))
                return null;

            var ret = new MultiRootConnector();
            ret.OwnerCommandCanvas = OwnerCommandCanvas;
            ret.AssetValueType = valueType;
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
        public static MultiRootConnector SelectVariableType(CommandCanvas OwnerCommandCanvas, string valueType, Func<Type, bool> isAccept = null)
        {
            if (!_SelectVariableType(OwnerCommandCanvas, ref valueType, isAccept))
                return null;

            var ret = new MultiRootConnector();
            ret.OwnerCommandCanvas = OwnerCommandCanvas;
            ret.AssetValueType = valueType;
            ret.AssetType = FunctionType.LiteralType;
            return ret;
        }

        /// <summary>
        /// （削除検討）
        /// </summary>
        /// <param name="OwnerCommandCanvas"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static SingleRootConnector MakeSingleRootConnector(CommandCanvas OwnerCommandCanvas, string name = "Root")
        {
            var ret = new SingleRootConnector();
            ret.LinkConnectorControl.Caption = nameof(SingleRootConnector);
            ret.OwnerCommandCanvas = OwnerCommandCanvas;
            return ret;
        }

        /// <summary>
        /// （削除検討）
        /// </summary>
        /// <param name="OwnerCommandCanvas"></param>
        /// <returns></returns>
        public static MultiRootConnector MakeMultiRootConnector(CommandCanvas OwnerCommandCanvas)
        {
            var ret = new MultiRootConnector();
            ret.OwnerCommandCanvas = OwnerCommandCanvas;
            ret.AssetType = FunctionType.ConnectorType;
            return ret;
        }

        /// <summary>
        /// （削除検討）
        /// </summary>
        /// <param name="OwnerCommandCanvas"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static SingleLinkConnector MakeSingleLinkConnector(CommandCanvas OwnerCommandCanvas, string name = "Link")
        {
            // ※新形式に未対応

            var ret = new SingleLinkConnector();
            ret.LinkConnectorControl.ValueData = new ParamNameOnly(name);
            ret.OwnerCommandCanvas = OwnerCommandCanvas;
            return ret;
        }

        /// <summary>
        /// （削除検討）
        /// </summary>
        /// <param name="OwnerCommandCanvas"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static MultiLinkConnector MakeMultiLinkConnector(CommandCanvas OwnerCommandCanvas, string name = "Link")
        {
            // ※新形式に未対応

            var ret = new MultiLinkConnector();
            ret.LinkConnectorControl.ValueData = new ParamNameOnly(name);
            ret.OwnerCommandCanvas = OwnerCommandCanvas;
            return ret;
        }

        public static object GetValue<T>(List<object> list, int index)
            where T : class, ICbValueClass<T>
        {
            if ((list[index] as T) is null)
                new NotImplementedException();
            return (list[index] as T).Value;
        }
        public static bool TryEnumParse<T>(string s, out T d) where T : struct
        {
            return Enum.TryParse(s, out d) && Enum.IsDefined(typeof(T), d);
        }
    }
}
