using CapyCSS.Controls;
using CapyCSS.Controls.BaseControls;
using CbVS.Script;

namespace CapyCSS.Script
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
            var newNode = new TreeMenuNode(TreeMenuNode.NodeType.GROUP, name);
            OwnerCommandCanvas.CommandMenu.AssetTreeData.Add(newNode);
            return newNode;
        }

        /// <summary>
        /// グループの下にグループを作成する
        /// </summary>
        /// <param name="node">親のグループ</param>
        /// <param name="name">グループ名</param>
        /// <returns>作成したグループ</returns>
        public static TreeMenuNode CreateGroup(TreeMenuNode node, string name)
        {
            var newNode = new TreeMenuNode(TreeMenuNode.NodeType.GROUP, name);
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
            string title = TreeViewCommand.MakeGroup(ref group, funcAssetDef.MenuTitle);
            var menu = new TreeMenuNode(TreeMenuNode.NodeType.NORMAL, title, funcAssetDef.HelpText);
            group.AddChild(menu);
            menu.LeftClickCommand = OwnerCommandCanvas.CreateEventCanvasCommand(
                menu.Path, 
                () => {
                    var result = CbScript.CreateFunction(OwnerCommandCanvas, funcAssetDef.AssetCode);
                    if (funcAssetDef.MenuTitle.Contains(CbSTUtils.MENU_OLD_SPECIFICATION))
                    {
                        // メニューで非表示にしたのでここには来なくなった

                        result.OldSpecification = true;
                    }
                    return result;
                }
                );
        }

        /// <summary>
        /// グループにアセットを登録する（変数作成用）
        /// </summary>
        /// <param name="group">登録するグループ</param>
        /// <param name="funcAssetDef">アセット</param>
        public static void CreateAssetMenu(CommandCanvas OwnerCommandCanvas, TreeMenuNode group, IFuncCreateVariableAssetDef funcAssetDef)
        {
            AddAsset(funcAssetDef);
            string title = TreeViewCommand.MakeGroup(ref group, funcAssetDef.MenuTitle);
            var menu = new TreeMenuNode(TreeMenuNode.NodeType.NORMAL, title, funcAssetDef.HelpText);
            group.AddChild(menu);
            menu.LeftClickCommand = OwnerCommandCanvas.CreateEventCanvasCommand(
                menu.Path,
                () => 
                { 
                    var result = CbScript.CreateFreeTypeVariableFunction(OwnerCommandCanvas, funcAssetDef.AssetCode, funcAssetDef.typeRequests);
                    if (funcAssetDef.MenuTitle.Contains(CbSTUtils.MENU_OLD_SPECIFICATION))
                    {
                        // メニューで非表示にしたのでここには来なくなった

                        result.OldSpecification = true;
                    }
                    return result;
                }
                );
        }

        /// <summary>
        /// グループにアセットを登録する
        /// </summary>
        /// <param name="group">登録するグループ</param>
        /// <param name="funcAssetDef">アセット</param>
        public static void CreateAssetMenu(CommandCanvas OwnerCommandCanvas, TreeMenuNode group, IFuncCreateVariableListAssetDef funcAssetDef)
        {
            AddAsset(funcAssetDef);
            string title = TreeViewCommand.MakeGroup(ref group, funcAssetDef.MenuTitle);
            var menu = new TreeMenuNode(TreeMenuNode.NodeType.NORMAL, title, funcAssetDef.HelpText);
            group.AddChild(menu);
            menu.LeftClickCommand = OwnerCommandCanvas.CreateEventCanvasCommand(
                menu.Path,
                () =>
                { 
                    var result = CbScript.CreateFreeTypeVariableFunction(OwnerCommandCanvas, funcAssetDef.AssetCode, null, true);
                    if (funcAssetDef.MenuTitle.Contains(CbSTUtils.MENU_OLD_SPECIFICATION))
                    {
                        // メニューで非表示にしたのでここには来なくなった

                        result.OldSpecification = true;
                    }
                    return result;
                }
                );
        }

        /// <summary>
        /// グループにアセットを登録する
        /// </summary>
        /// <param name="group">登録するグループ</param>
        /// <param name="funcAssetDef">アセット</param>
        public static void CreateAssetMenu(CommandCanvas OwnerCommandCanvas, TreeMenuNode group, IFuncAssetWithArgumentDef funcAssetDef)
        {
            AddAsset(funcAssetDef);
            string title = TreeViewCommand.MakeGroup(ref group, funcAssetDef.MenuTitle);
            var menu = new TreeMenuNode(TreeMenuNode.NodeType.NORMAL, title, funcAssetDef.HelpText);
            group.AddChild(menu);
            menu.LeftClickCommand = OwnerCommandCanvas.CreateEventCanvasCommand(
                menu.Path, 
                () =>
                {
                    var result = CbScript.CreateFreeTypeFunction(OwnerCommandCanvas, funcAssetDef.AssetCode, funcAssetDef.typeRequests);
                    if (funcAssetDef.MenuTitle.Contains(CbSTUtils.MENU_OLD_SPECIFICATION))
                    {
                        // メニューで非表示にしたのでここには来なくなった

                        result.OldSpecification = true;
                    }
                    return result;
                }
                );
        }

        /// <summary>
        /// グループにアセットを登録する
        /// </summary>
        /// <param name="group">登録するグループ</param>
        /// <param name="funcAssetDef">アセット</param>
        public static void CreateAssetMenu(CommandCanvas OwnerCommandCanvas, TreeMenuNode group, IFuncAssetLiteralDef funcAssetDef)
        {
            //AddAsset(funcAssetDef); 不要
            string title = TreeViewCommand.MakeGroup(ref group, funcAssetDef.MenuTitle);
            var menu = new TreeMenuNode(TreeMenuNode.NodeType.NORMAL, title, funcAssetDef.HelpText);
            group.AddChild(menu);
            menu.LeftClickCommand = OwnerCommandCanvas.CreateEventCanvasCommand(
                menu.Path,
                () => { 
                    var result = CbScript.SelectVariableType(OwnerCommandCanvas, funcAssetDef.typeRequests);
                    if (funcAssetDef.MenuTitle.Contains(CbSTUtils.MENU_OLD_SPECIFICATION))
                    {
                        // メニューで非表示にしたのでここには来なくなった

                        result.OldSpecification = true;
                    }
                    return result;
                }
                );
        }
    }
}
