using CapybaraVS.Controls;
using CapybaraVS.Controls.BaseControls;
using CbVS.Script;

namespace CapybaraVS.Script
{
    public class ImplementAsset
    {
        private static void AddAsset(IFuncAssetDef asset)
        {
            MultiRootConnector.AssetFunctionList.Add(asset);
        }

        /// <summary>
        /// グループを作成する
        /// </summary>
        /// <param name="name">グループ名</param>
        /// <returns>作成したグループ</returns>
        protected static TreeMenuNode CreateGroup(CommandCanvas OwnerCommandCanvas, string name)
        {
            var newNode = new TreeMenuNode(name);
            OwnerCommandCanvas.CommandMenu.AssetTreeData.Add(newNode);
            return newNode;
        }

        /// <summary>
        /// グループの下にグループを作成する
        /// </summary>
        /// <param name="node">親のグループ</param>
        /// <param name="name">グループ名</param>
        /// <returns>作成したグループ</returns>
        protected static TreeMenuNode CreateGroup(TreeMenuNode node, string name)
        {
            var newNode = new TreeMenuNode(name);
            node.AddChild(newNode);
            return newNode;
        }

        /// <summary>
        /// グループにアセットを登録する
        /// </summary>
        /// <param name="group">登録するグループ</param>
        /// <param name="funcAssetDef">アセット</param>
        protected static void CreateAssetMenu(CommandCanvas OwnerCommandCanvas, TreeMenuNode group, IFuncCreateAssetDef funcAssetDef)
        {
            AddAsset(funcAssetDef);
            string title = TryMakeGroup(ref group, funcAssetDef.MenuTitle);
            var menu = new TreeMenuNode(title, funcAssetDef.HelpText);
            group.AddChild(menu);
            menu.LeftClickCommand = OwnerCommandCanvas.CreateEventCanvasCommand(menu.Path, () => CbScript.CreateFunction(OwnerCommandCanvas, funcAssetDef.AssetCode));
        }

        /// <summary>
        /// グループにアセットを登録する
        /// </summary>
        /// <param name="group">登録するグループ</param>
        /// <param name="funcAssetDef">アセット</param>
        public static void CreateAssetMenu(CommandCanvas OwnerCommandCanvas, TreeMenuNode group, IFuncCreateVariableAssetDef funcAssetDef)
        {
            AddAsset(funcAssetDef);
            string title = TryMakeGroup(ref group, funcAssetDef.MenuTitle);
            var menu = new TreeMenuNode(title, funcAssetDef.HelpText);
            group.AddChild(menu);
            menu.LeftClickCommand = OwnerCommandCanvas.CreateEventCanvasCommand(menu.Path, () => CbScript.CreateFreeTypeVariableFunction(OwnerCommandCanvas, funcAssetDef.AssetCode, funcAssetDef.TargetType, funcAssetDef.DeleteSelectItems));
        }

        /// <summary>
        /// グループにアセットを登録する
        /// </summary>
        /// <param name="group">登録するグループ</param>
        /// <param name="funcAssetDef">アセット</param>
        public static void CreateAssetMenu(CommandCanvas OwnerCommandCanvas, TreeMenuNode group, IFuncCreateVariableListAssetDef funcAssetDef)
        {
            AddAsset(funcAssetDef);
            string title = TryMakeGroup(ref group, funcAssetDef.MenuTitle);
            var menu = new TreeMenuNode(title, funcAssetDef.HelpText);
            group.AddChild(menu);
            menu.LeftClickCommand = OwnerCommandCanvas.CreateEventCanvasCommand(menu.Path, () => CbScript.CreateFreeTypeVariableFunction(OwnerCommandCanvas, funcAssetDef.AssetCode, funcAssetDef.TargetType, null, true));
        }

        /// <summary>
        /// グループにアセットを登録する
        /// </summary>
        /// <param name="group">登録するグループ</param>
        /// <param name="funcAssetDef">アセット</param>
        public static void CreateAssetMenu(CommandCanvas OwnerCommandCanvas, TreeMenuNode group, IFuncCreateClassVariableAssetDef funcAssetDef)
        {
            AddAsset(funcAssetDef);
            string title = TryMakeGroup(ref group, funcAssetDef.MenuTitle);
            var menu = new TreeMenuNode(title, funcAssetDef.HelpText);
            group.AddChild(menu);
            menu.LeftClickCommand = OwnerCommandCanvas.CreateEventCanvasCommand(menu.Path, () => CbScript.CreateFreeTypeVariableFunction(OwnerCommandCanvas, funcAssetDef.AssetCode, funcAssetDef.TargetType));
        }

        /// <summary>
        /// グループにアセットを登録する
        /// </summary>
        /// <param name="group">登録するグループ</param>
        /// <param name="funcAssetDef">アセット</param>
        public static void CreateAssetMenu(CommandCanvas OwnerCommandCanvas, TreeMenuNode group, IFuncAssetWithArgumentDef funcAssetDef)
        {
            AddAsset(funcAssetDef);
            string title = TryMakeGroup(ref group, funcAssetDef.MenuTitle);
            var menu = new TreeMenuNode(title, funcAssetDef.HelpText);
            group.AddChild(menu);
            menu.LeftClickCommand = OwnerCommandCanvas.CreateEventCanvasCommand(menu.Path, () => CbScript.CreateFreeTypeFunction(OwnerCommandCanvas, funcAssetDef.AssetCode, funcAssetDef.TargetType, funcAssetDef.DeleteSelectItems));
        }

        /// <summary>
        /// グループにアセットを登録する
        /// </summary>
        /// <param name="group">登録するグループ</param>
        /// <param name="funcAssetDef">アセット</param>
        public static void CreateAssetMenu(CommandCanvas OwnerCommandCanvas, TreeMenuNode group, IFuncAssetLiteralDef funcAssetDef)
        {
            //AddAsset(funcAssetDef); 不要
            string title = TryMakeGroup(ref group, funcAssetDef.MenuTitle);
            var menu = new TreeMenuNode(title, funcAssetDef.HelpText);
            group.AddChild(menu);
            menu.LeftClickCommand = OwnerCommandCanvas.CreateEventCanvasCommand(menu.Path, () => CbScript.SelectVariableType(OwnerCommandCanvas, funcAssetDef.TargetType, funcAssetDef.DeleteSelectItems));
        }

        /// <summary>
        /// .区切りができるなら階層を作成する
        /// </summary>
        /// <param name="group"></param>
        /// <param name="title"></param>
        /// <returns></returns>
        private static string TryMakeGroup(ref TreeMenuNode group, string title)
        {
            string[] arr = title.Split('.');
            if (arr.Length > 1)
            {
                title = arr[arr.Length - 1];
                for (int i = 0; i < arr.Length - 1; ++i)
                {
                    TreeMenuNode temp = null;
                    foreach (var child in group.Child)
                    {
                        if (child.Name == arr[i])
                        {
                            temp = child;
                            break;
                        }
                    }
                    if (temp == null)
                    {
                        temp = new TreeMenuNode(arr[i]);
                        group.AddChild(temp);
                    }
                    group = temp;
                }
            }
            return title;
        }
    }
}
