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
        protected static TreeMenuNode CreateGroup(string name)
        {
            var newNode = new TreeMenuNode(name);
            CommandCanvas.TreeViewCommand.AssetTreeData.Add(newNode);
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
            node.Child.Add(newNode);
            return newNode;
        }

        /// <summary>
        /// グループにアセットを登録する
        /// </summary>
        /// <param name="group">登録するグループ</param>
        /// <param name="funcAssetDef">アセット</param>
        protected static void CreateAssetMenu(TreeMenuNode group, IFuncCreateAssetDef funcAssetDef)
        {
            AddAsset(funcAssetDef);
            string title = TryMakeGroup(ref group, funcAssetDef.MenuTitle);
            group.Child.Add(new TreeMenuNode(title, funcAssetDef.HelpText, CommandCanvas.CreateEventCanvasCommand(() => CbScript.CreateFunction(funcAssetDef.AssetCode))));
        }

        /// <summary>
        /// グループにアセットを登録する
        /// </summary>
        /// <param name="group">登録するグループ</param>
        /// <param name="funcAssetDef">アセット</param>
        public static void CreateAssetMenu(TreeMenuNode group, IFuncCreateVariableAssetDef funcAssetDef)
        {
            AddAsset(funcAssetDef);
            string title = TryMakeGroup(ref group, funcAssetDef.MenuTitle);
            group.Child.Add(new TreeMenuNode(title, funcAssetDef.HelpText, CommandCanvas.CreateEventCanvasCommand(() => CbScript.CreateFreeTypeVariableFunction(funcAssetDef.AssetCode, funcAssetDef.TargetType, funcAssetDef.DeleteSelectItems))));
        }

        /// <summary>
        /// グループにアセットを登録する
        /// </summary>
        /// <param name="group">登録するグループ</param>
        /// <param name="funcAssetDef">アセット</param>
        public static void CreateAssetMenu(TreeMenuNode group, IFuncCreateVariableListAssetDef funcAssetDef)
        {
            AddAsset(funcAssetDef);
            string title = TryMakeGroup(ref group, funcAssetDef.MenuTitle);
            group.Child.Add(new TreeMenuNode(title, funcAssetDef.HelpText, CommandCanvas.CreateEventCanvasCommand(() => CbScript.CreateFreeTypeVariableFunction(funcAssetDef.AssetCode, funcAssetDef.TargetType, null, true))));
        }

        /// <summary>
        /// グループにアセットを登録する
        /// </summary>
        /// <param name="group">登録するグループ</param>
        /// <param name="funcAssetDef">アセット</param>
        public static void CreateAssetMenu(TreeMenuNode group, IFuncCreateClassVariableAssetDef funcAssetDef)
        {
            AddAsset(funcAssetDef);
            string title = TryMakeGroup(ref group, funcAssetDef.MenuTitle);
            group.Child.Add(new TreeMenuNode(title, funcAssetDef.HelpText, CommandCanvas.CreateEventCanvasCommand(() => CbScript.CreateFreeTypeVariableFunction(funcAssetDef.AssetCode, funcAssetDef.TargetType))));
        }

        /// <summary>
        /// グループにアセットを登録する
        /// </summary>
        /// <param name="group">登録するグループ</param>
        /// <param name="funcAssetDef">アセット</param>
        public static void CreateAssetMenu(TreeMenuNode group, IFuncAssetWithArgumentDef funcAssetDef)
        {
            AddAsset(funcAssetDef);
            string title = TryMakeGroup(ref group, funcAssetDef.MenuTitle);
            group.Child.Add(new TreeMenuNode(title, funcAssetDef.HelpText, CommandCanvas.CreateEventCanvasCommand(() => CbScript.CreateFreeTypeFunction(funcAssetDef.AssetCode, funcAssetDef.TargetType, funcAssetDef.DeleteSelectItems))));
        }

        /// <summary>
        /// グループにアセットを登録する
        /// </summary>
        /// <param name="group">登録するグループ</param>
        /// <param name="funcAssetDef">アセット</param>
        public static void CreateAssetMenu(TreeMenuNode group, IFuncAssetLiteralDef funcAssetDef)
        {
            //AddAsset(funcAssetDef); 不要
            string title = TryMakeGroup(ref group, funcAssetDef.MenuTitle);
            group.Child.Add(new TreeMenuNode(title, funcAssetDef.HelpText, CommandCanvas.CreateEventCanvasCommand(() => CbScript.SelectVariableType(funcAssetDef.TargetType, funcAssetDef.DeleteSelectItems))));
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
                    TreeMenuNode temp = group.Child.Find(m => m.Name == arr[i]);
                    if (temp == null)
                    {
                        temp = new TreeMenuNode(arr[i]);
                        group.Child.Add(temp);
                    }
                    group = temp;
                }
            }
            return title;
        }
    }
}
