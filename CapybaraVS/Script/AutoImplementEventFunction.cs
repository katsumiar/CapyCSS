using CapybaraVS.Controls;
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
                IsConstructor = info.isConstructor
            };
            return ret;
        }

        public override bool ImplAsset(MultiRootConnector col, bool noThreadMode = false)
        {
            DummyArgumentsControl dummyArgumentsControl = new DummyArgumentsControl(col);
            return ImplAsset(col, noThreadMode, dummyArgumentsControl);
        }
    }
}
