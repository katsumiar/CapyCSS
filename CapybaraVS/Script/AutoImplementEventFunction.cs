using CapyCSS.Controls;
using CbVS.Script;

namespace CapyCSS.Script
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
                oldSpecification = info.oldSpecification,
                isRunable = info.isRunable,
                IsProperty = info.IsProperty,
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
            string exTitle = GetGenericArgumentsString(col, isReBuildMode);
            return ImplAsset(col, exTitle);
        }
    }
}
