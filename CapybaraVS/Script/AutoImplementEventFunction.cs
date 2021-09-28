﻿using CapybaraVS.Controls;
using CbVS.Script;

namespace CapybaraVS.Script
{
    /// <summary>
    /// リフレクションによる自動実装用ファンクションアセット定義クラス
    /// </summary>
    public class AutoImplementEventFunction : AutoImplementFunction
    {
        static new public AutoImplementEventFunction Create(AutoImplementFunctionInfo info)
        {
            AutoImplementEventFunction ret = new AutoImplementEventFunction()
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
            };
            return ret;
        }

        /// <summary>
        /// メソッド呼び出し処理を実装する
        /// </summary>
        /// <param name="col">スクリプトのルートノード</param>
        /// <param name="isReBuildMode">再構築か？（保存データからの復帰）</param>
        public override bool ImplAsset(MultiRootConnector col, bool isReBuildMode = false)
        {
            DummyArgumentsControl dummyArgumentsControl = new DummyArgumentsControl(col);
            string exTitle = GetGenericArgumentsString(col, isReBuildMode);
            return ImplAsset(col, dummyArgumentsControl, exTitle);
        }
    }
}
