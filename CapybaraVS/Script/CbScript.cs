using CapybaraVS.Controls;
using CapybaraVS.Controls.BaseControls;
using System;
using System.Collections.Generic;
using System.Windows.Input;
using System.Windows;
using System.Linq;
using CapybaraVS.Script;
using CapybaraVS;

namespace CbVS.Script
{
    public class CbScript
    {
        /// <summary>
        /// 型選択時に選択すべきでない最低限の型を除く
        /// </summary>
        public static CbType[] BaseDeleteCbTypes => new CbType[] { CbType.none, CbType.Func, CbType.Class, CbType.Text };

        /// <summary>
        /// 型選択時に選択すべきでない最低限の型を除く
        /// </summary>
        public static CbType[] BaseDeleteCbTypes2 => new CbType[] { CbType.none, CbType.Func, CbType.Class };

        /// <summary>
        /// 型選択時に object 型を除く
        /// </summary>
        public static CbType[] ObjectDeleteCbTypes => new CbType[] { CbType.none, CbType.Func, CbType.Class, CbType.Object, CbType.Text };

        /// <summary>
        /// 型選択時に演算不可能な型を除く
        /// </summary>
        public static CbType[] MathDeleteNotCbCalcableTypes => new CbType[] { CbType.none, CbType.Func, CbType.Class, CbType.Object, CbType.String, CbType.Text, CbType.Bool };

        /// <summary>
        /// 型選択時に加算不可能な型を除く
        /// </summary>
        public static CbType[] MathDeleteNotCbAddableTypes => new CbType[] { CbType.none, CbType.Func, CbType.Class, CbType.Text, CbType.Object, CbType.Bool };

        /// <summary>
        /// 型選択時にマイナス表現の無い型を除く
        /// </summary>
        public static CbType[] MathDeleteNotCbSignedNumberTypes => new CbType[] { CbType.none, CbType.Func, CbType.Class, CbType.Object, CbType.String, CbType.Text, CbType.Bool, CbType.Char, CbType.UInt, CbType.ULong, CbType.UShort };

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
        /// <param name="cbType">選択された型の格納先</param>
        /// <param name="ignoreTypes">>選択除外の型を指定</param>
        /// <returns>true = 有効</returns>
        private static bool _SelectVariableType(CbST cbType, CbType[] ignoreTypes)
        {
            if (cbType.LiteralType == CbType.none)
            {
                EnumWindow enumWindow = EnumWindow.Create(CbEnum<CbType>.Create(),
                    new Point(Mouse.GetPosition(null).X, Mouse.GetPosition(null).Y));

                if (ignoreTypes != null)
                {
                    foreach (var item in ignoreTypes)
                    {
                        enumWindow.RemoveItem(CbSTUtils.EnumCbTypeToString(item));
                    }
                }

                enumWindow.ShowDialog();
                ICbValueEnumClass<Enum> selectItem = enumWindow.EnumItem;

                if (selectItem != null)
                {
                    cbType.LiteralType = (CbType)selectItem.Value;
                }
                else
                {
                    return false;
                }
            }
            if (cbType.LiteralType == CbType.none)
                return false;
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
        private static MultiRootConnector _CreateFreeTypeVariableFunction(CommandCanvas OwnerCommandCanvas, string assetCode, MultiRootConnector multiRootConnector, StackNode stackNode, bool forcedListTypeSelect)
        {
            if (multiRootConnector is null)
            {
                stackNode = ListSelectWindow.Create(
                    OwnerCommandCanvas,
                    "Variable",
                    OwnerCommandCanvas.ScriptWorkStack.StackData,
                    forcedListTypeSelect,
                    new Point(Mouse.GetPosition(null).X, Mouse.GetPosition(null).Y));

                if (stackNode is null)
                    return null;

                multiRootConnector = new MultiRootConnector();
                multiRootConnector.OwnerCommandCanvas = OwnerCommandCanvas;
                multiRootConnector.AssetLiteralType = stackNode.ValueData.CbType;
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
        public static MultiRootConnector CreateFreeTypeVariableFunction(CommandCanvas OwnerCommandCanvas, string assetCode, CbST cbType, CbType[] ignoreTypes = null, bool forcedListTypeSelect = false)
        {
            MultiRootConnector multiRootConnector = null;
            StackNode stackNode = null;

            if (ignoreTypes != null)
            {
                if (!_SelectVariableType(cbType, ignoreTypes))
                    return null;

                stackNode = OwnerCommandCanvas.ScriptWorkStack.Append(CbST.Create(cbType, "variable" + (OwnerCommandCanvas.ScriptWorkStack.StackData.Count + 1))).stackNode;
                multiRootConnector = new MultiRootConnector();
                multiRootConnector.OwnerCommandCanvas = OwnerCommandCanvas;
                multiRootConnector.AssetLiteralType = cbType;
                multiRootConnector.AssetFuncType = assetCode;
            }

            return _CreateFreeTypeVariableFunction(OwnerCommandCanvas, assetCode, multiRootConnector, stackNode, forcedListTypeSelect);
        }

        /// <summary>
        /// 指定されたアセットコードに対して変数を選択しノードを作成します。
        /// </summary>
        /// <param name="assetCode">アセットコード</param>
        /// <param name="cbVSValue">選択された変数の格納先</param>
        /// <returns>ノード</returns>
        public static MultiRootConnector CreateFreeTypeVariableFunction(CommandCanvas OwnerCommandCanvas, string assetCode, ICbValue cbVSValue)
        {
            cbVSValue.Name = "variable" + (OwnerCommandCanvas.ScriptWorkStack.StackData.Count + 1);
            StackNode stackNode = OwnerCommandCanvas.ScriptWorkStack.Append(cbVSValue).stackNode;
            MultiRootConnector multiRootConnector = new MultiRootConnector();
            multiRootConnector.OwnerCommandCanvas = OwnerCommandCanvas;
            multiRootConnector.AssetLiteralType = cbVSValue.CbType;
            multiRootConnector.AssetFuncType = assetCode;

            return _CreateFreeTypeVariableFunction(OwnerCommandCanvas, assetCode, multiRootConnector, stackNode, false);
        }

        /// <summary>
        /// 指定されたアセットコードに対して引数の型を選択しノードを作成します。
        /// </summary>
        /// <param name="assetCode">アセットコード</param>
        /// <param name="cbType">選択された型の格納先</param>
        /// <param name="ignoreTypes">選択除外の型を指定</param>
        /// <returns>ノード</returns>
        public static MultiRootConnector CreateFreeTypeFunction(CommandCanvas OwnerCommandCanvas, string assetCode, CbST cbType, CbType[] ignoreTypes = null)
        {
            if (!_SelectVariableType(cbType, ignoreTypes))
                return null;

            var ret = new MultiRootConnector();
            ret.OwnerCommandCanvas = OwnerCommandCanvas;
            ret.AssetLiteralType = cbType;
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
        public static MultiRootConnector SelectVariableType(CommandCanvas OwnerCommandCanvas, CbST cbType, CbType[] ignoreTypes = null)
        {
            if (!_SelectVariableType(cbType, ignoreTypes))
                return null;

            var ret = new MultiRootConnector();
            ret.OwnerCommandCanvas = OwnerCommandCanvas;
            ret.AssetLiteralType = cbType;
            if (cbType.ObjectType == CbCType.List) // この切替は無くす予定
                ret.AssetType = FunctionType.ListType;
            else
                ret.AssetType = FunctionType.LiteralType;
            if (ret.AssetLiteralType.LiteralType == CbType.none)
                return null;    // 型が確定しなかったら異常
            return ret;
        }

        public static SingleRootConnector MakeSingleRootConnector(CommandCanvas OwnerCommandCanvas, string name = "Root")
        {
            var ret = new SingleRootConnector();
            ret.LinkConnectorControl.Caption = nameof(SingleRootConnector);
            ret.OwnerCommandCanvas = OwnerCommandCanvas;
            return ret;
        }

        public static MultiRootConnector MakeMultiRootConnector(CommandCanvas OwnerCommandCanvas)
        {
            var ret = new MultiRootConnector();
            ret.OwnerCommandCanvas = OwnerCommandCanvas;
            ret.AssetType = FunctionType.ConnectorType;
            return ret;
        }

        public static SingleLinkConnector MakeSingleLinkConnector(CommandCanvas OwnerCommandCanvas, string name = "Link")
        {
            // ※新形式に未対応

            var ret = new SingleLinkConnector();
            ret.LinkConnectorControl.ValueData = new ParamNameOnly(name);
            ret.OwnerCommandCanvas = OwnerCommandCanvas;
            return ret;
        }

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
