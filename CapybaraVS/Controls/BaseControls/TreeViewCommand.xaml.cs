using CapybaraVS.Script;
using CapyCSS.Controls;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Serialization;

namespace CapybaraVS.Controls.BaseControls
{
    /// <summary>
    /// 
    /// </summary>
    public class TreeMenuNodeCommand : ICommand
    {
        //-----------------------------------------------------------------------------------
        /// <summary>
        /// 
        /// </summary>
        public Func<object, bool> CanExecuteEvent { get; set; } = null;

        //-----------------------------------------------------------------------------------
        /// <summary>
        /// 
        /// </summary>
        public Action<object> ExecuteEvent { get; set; } = null;

        //-----------------------------------------------------------------------------------
        /// <summary>
        /// 
        /// </summary>
        /// <param name="viewmodel"></param>
        /// <param name="executeEvent"></param>
        /// <param name="canExecuteEvent"></param>
        public TreeMenuNodeCommand(
            Action<object> executeEvent = null,
            Func<object, bool> canExecuteEvent = null)
        {
            if (executeEvent != null)
            {
                ExecuteEvent = executeEvent;
                CanExecuteEvent = canExecuteEvent;
            }
        }

        //-----------------------------------------------------------------------------------
        /// <summary>
        /// コマンドの実行可否を判定します。
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public bool CanExecute(object parameter)
        {
            if (CanExecuteEvent is null)
                return true;
            return CanExecuteEvent(parameter);
        }

        //-----------------------------------------------------------------------------------
        /// <summary>
        /// 
        /// </summary>
#pragma warning disable 67  // インターフェイスにより強制されるが現在未使用
        public event EventHandler CanExecuteChanged;

        //-----------------------------------------------------------------------------------
        /// <summary>
        /// コマンドを実行します。
        /// </summary>
        /// <param name="parameter"></param>
        public void Execute(object parameter)
        {
            ExecuteEvent?.Invoke(parameter);
        }
    }

    //-----------------------------------------------------------------------------------
    /// <summary>
    /// ツリーメニューを構成するノードクラスです。
    /// </summary>
    public sealed class TreeMenuNode : INotifyPropertyChanged
    {
        /// <summary>
        /// コンストラクタです。
        /// </summary>
        /// <param name="name">項目名</param>
        /// <param name="personCommand">実行コマンド</param>
        public TreeMenuNode(string name, TreeMenuNodeCommand personCommand = null)
        {
            Name = name;
            Path = name;
            if (personCommand is null)
            {
                LeftClickCommand = null;
            }
            else
            {
                LeftClickCommand = personCommand;
            }
        }

        /// <summary>
        /// コンストラクタです。
        /// </summary>
        /// <param name="name">項目名</param>
        /// <param name="personCommand">実行コマンド</param>
        public TreeMenuNode(string name, string hintText, TreeMenuNodeCommand personCommand = null)
        {
            Name = name;
            Path = name;
            if (hintText != null && hintText != "")
            {
                HintText = hintText;
            }
            if (personCommand is null)
            {
                LeftClickCommand = new TreeMenuNodeCommand();
            }
            else
            {
                LeftClickCommand = personCommand;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string name)
        {
            if (null == this.PropertyChanged) return;
            this.PropertyChanged(this, new PropertyChangedEventArgs(name));
        }

        //-----------------------------------------------------------------------------------
        /// <summary>
        /// 項目名を参照します。
        /// </summary>
        public string Name { get; set; }

        //-----------------------------------------------------------------------------------
        /// <summary>
        /// 正確な項目名を参照します。
        /// </summary>

        public string Path { get; set; } = null;

        //-----------------------------------------------------------------------------------
        /// <summary>
        /// 説明を参照します。
        /// </summary>
        public string HintText { get; set; } = null;

        //-----------------------------------------------------------------------------------
        /// <summary>
        /// 子メニューコレクションを参照します。
        /// ※ 直接 add しないでください（AddChild を使ってください）。
        /// </summary>
        public ObservableCollection<TreeMenuNode> Child { get; set; } = new ObservableCollection<TreeMenuNode>();

        //-----------------------------------------------------------------------------------
        /// <summary>
        /// 子を追加します。
        /// </summary>
        public void AddChild(TreeMenuNode node)
        {
            if (Path is null)
            {
                node.Path = "";
            }
            else
            {
                node.Path = Path + ".";
            }
            node.Path = node.Path + node.Name;
            Child.Add(node);
        }

        //-----------------------------------------------------------------------------------
        /// <summary>
        /// 左クリックイベントで呼ばれるイベントです。
        /// </summary>
        public ICommand LeftClickCommand { get; set; }

        public Brush Foreground
        {
            get
            {
                if (LeftClickCommand is null)
                {
                    return Brushes.Gray;
                }
                if (!LeftClickCommand.CanExecute(null))
                {
                    return Brushes.Silver;
                }
                return Brushes.Black;
            }
            set {
                OnPropertyChanged(nameof(Foreground));
                OnPropertyChanged(nameof(IsEnabled));
            }
        }

        //-----------------------------------------------------------------------------------
        /// <summary>
        /// 子メニューの無いノードの表示状態（ボタン側）を参照します。
        /// </summary>
        public Visibility MenuNodeView
        {
            get => (Child.Count == 0) ? Visibility.Visible : Visibility.Collapsed;
            set {}
        }

        //-----------------------------------------------------------------------------------
        /// <summary>
        /// 子メニューの有るノードの表示状態（テキストBOX側）を参照します。
        /// </summary>
        public Visibility GroupNodeView
        {
            get => (Child.Count != 0) ? Visibility.Visible : Visibility.Collapsed;
            set {}
        }

        //-----------------------------------------------------------------------------------
        /// <summary>
        /// 有効状態を参照します。
        /// </summary>
        public bool IsEnabled
        {
            get
            {
                if (LeftClickCommand is null)
                {
                    return true;
                }
                return LeftClickCommand.CanExecute(null) && (Child.Count == 0);
            }
            set {}
        }
    }

    /// <summary>
    /// TreeViewCommand.xaml の相互作用ロジック
    /// </summary>
    public partial class TreeViewCommand
        : UserControl
    {
        //-----------------------------------------------------------------------------------
        /// <summary>
        /// ツリーメニューコレクション
        /// </summary>
        #region AssetTreeData 添付プロパティ実装

        private static ImplementDependencyProperty<TreeViewCommand, ObservableCollection<TreeMenuNode>> impAssetTreeData =
            new ImplementDependencyProperty<TreeViewCommand, ObservableCollection<TreeMenuNode>>(
                nameof(AssetTreeData),
                (self, getValue) =>
                {
                    ObservableCollection<TreeMenuNode> value = getValue(self);
                    self.TreeView.ItemsSource = value;
                });

        public static readonly DependencyProperty AssetTreeDataProperty = impAssetTreeData.Regist(new ObservableCollection<TreeMenuNode>());

        public ObservableCollection<TreeMenuNode> AssetTreeData
        {
            get { return impAssetTreeData.GetValue(this); }
            set { impAssetTreeData.SetValue(this, value); }
        }

        #endregion

        #region OwnerCommandCanvas 添付プロパティ実装

        private static ImplementDependencyProperty<TreeViewCommand, CommandCanvas> impOwnerCommandCanvas =
            new ImplementDependencyProperty<TreeViewCommand, CommandCanvas>(
                nameof(OwnerCommandCanvas),
                (self, getValue) =>
                {
                    //Action value = getValue(self);
                });

        public static readonly DependencyProperty OwnerCommandCanvasProperty = impOwnerCommandCanvas.Regist(null);

        public CommandCanvas OwnerCommandCanvas
        {
            get { return impOwnerCommandCanvas.GetValue(this); }
            set { impOwnerCommandCanvas.SetValue(this, value); }
        }

        #endregion

        public ObservableCollection<TreeMenuNode> BackupTreeData = null;

        //-----------------------------------------------------------------------------------
        /// <summary>
        /// 
        /// </summary>
        public TreeViewCommand()
        {
            InitializeComponent();

            TreeView.ItemsSource = AssetTreeData;
            TreeView.PreviewMouseWheel += PreviewMouseWheel;
        }

        const string RecentName = "Recent";
        const int MaxRecent = 10;

        const int FilteringWaitSleep = 20;

        /// <summary>
        /// 名前を指定してコマンドを実行します。
        /// </summary>
        /// <param name="name"></param>
        public void ExecuteFindCommand(string name)
        {
            TreeMenuNode hit = FindMenuName(name, true);
            if (hit != null)
            {
                hit.LeftClickCommand.Execute(null);
            }
        }

        /// <summary>
        /// 最近使ったスクリプトノードのメニューを取得します。
        /// </summary>
        public TreeMenuNode GetRecent()
        {
            foreach (var node in AssetTreeData)
            {
                if (node.Path == RecentName)
                {
                    return node;
                }
            }
            var recentNode = new TreeMenuNode(RecentName);
            AssetTreeData.Add(recentNode);
            return recentNode;
        }

        /// <summary>
        /// 最近使ったスクリプトノードの数を規定数に調整します。
        /// </summary>
        public void AdjustNumberRecent()
        {
            TreeMenuNode recent = GetRecent();
            while (recent.Child.Count > MaxRecent)
            {
                recent.Child.RemoveAt(0);
            }
        }

        /// <summary>
        /// 名前からコマンドノードを参照します。
        /// </summary>
        /// <param name="name">メニュー名</param>
        /// <param name="execute">実行可能な対象か</param>
        /// <returns>存在しているならノードを返します</returns>
        public TreeMenuNode FindMenuName(string name, bool execute = false)
        {
            foreach (var node in AssetTreeData)
            {
                if (node.Path == RecentName)
                {
                    continue;
                }

                TreeMenuNode hit = FindMenuPath(node, name, execute);
                if (hit != null)
                {
                    return hit;
                }
            }
            return null;
        }

        /// <summary>
        /// 項目の状態を更新します。
        /// </summary>
        public void RefreshItem()
        {
            ObservableCollection<TreeMenuNode> treeView = TreeView.ItemsSource as ObservableCollection<TreeMenuNode>;
            _RefreshItem(TreeView.ItemsSource as ObservableCollection<TreeMenuNode>);
        }

        private void _RefreshItem(ObservableCollection<TreeMenuNode> treeView)
        {
            foreach (var node in treeView)
            {
                if (node.Child.Count != 0)
                {
                    _RefreshItem(node.Child);
                }
                else
                {
                    node.Foreground = Brushes.Black;    // ダミー（更新目的）
                }
            }
        }

        /// <summary>
        /// コマンドをフィルタリングします。
        /// </summary>
        /// <param name="name">メニュー名</param>
        public CancellationTokenSource SetFilter(TreeViewCommand viewer, string name)
        {
            ObservableCollection<TreeMenuNode> treeView = viewer.TreeView.ItemsSource as ObservableCollection<TreeMenuNode>;
            treeView.Clear();   // これを起因にバインド系のエラーが出るが…無視して良い...

            Dictionary<string, string> keyValuePairs = new Dictionary<string, string>()
            {
                { "STRING", "" },
            };

            if (keyValuePairs.ContainsKey(name.ToUpper()))
            {
                name = "System" + keyValuePairs[name.ToUpper()] + "." + name;
            }

            string searchName = name.ToUpper().Replace(" ", "");

            List<TreeMenuNode> treeMenuNodes = new List<TreeMenuNode>();
            foreach (var assetData in AssetTreeData)
            {
                treeMenuNodes.Add(assetData);
            }

            var tokenSource = new CancellationTokenSource();
            var token = tokenSource.Token;
            Task.Run(() => ActionFilter(treeMenuNodes, viewer, token, searchName));
            return tokenSource;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="token"></param>
        /// <param name="treeView"></param>
        /// <param name="searchName"></param>
        private void ActionFilter(List<TreeMenuNode> treeMenuNodes, TreeViewCommand viewer, CancellationToken token, string searchName)
        {
            foreach (var node in treeMenuNodes)
            {
                if (token.IsCancellationRequested)
                {
                    break;
                }

                if (node.Path == RecentName)
                {
                    continue;
                }

                SetFilter(viewer, token, node, searchName, CbSTUtils.StripParamater(searchName));
            }
        }

        /// <summary>
        /// コマンドをフィルタリングします。
        /// 「.」を見つけた場合、そこを起点に一致判定を「.」区切りの完全一致に切り替えます。
        /// </summary>
        /// <param name="viewer">結果登録用リスト</param>
        /// <param name="node">メニューノード</param>
        /// <param name="name">メニュー名</param>
        private void SetFilter(TreeViewCommand viewer, CancellationToken token, TreeMenuNode node, string name, string stripName, TreeMenuNode currentLock = null)
        {
            if (token.IsCancellationRequested)
            {
                return;
            }

            string nodeName = node.Name.ToUpper().Replace(" ", "");

            if (name.Contains(".") || currentLock != null)
            {
                // 「.」区切りの完全一致で下の階層すべてを対象とする

                string current = name;
                string next = name;
                if (name.Contains("."))
                {
                    current = name.Substring(0, name.IndexOf('.'));
                    next = name.Substring(name.IndexOf('.') + 1);
                }
                if (nodeName == current && !name.EndsWith('.'))
                {
                    foreach (var child in node.Child)
                    {
                        SetFilter(viewer, token, child, next, stripName, node);
                    }
                }
                if (currentLock != null)
                {
                    if (nodeName == name)
                    {
                        SetAll(viewer, token, node, currentLock.Path.Substring(0, currentLock.Path.LastIndexOf('.') + 1));
                    }
                    return;
                }
            }
            else if (nodeName == name)
            {
                // 名前が完全一致したら下の階層すべてを対象とする

                SetAll(viewer, token, node);
                return;
            }
            else
            {
                CurrentFilter(viewer, token, node, name, stripName);
            }
            foreach (var child in node.Child)
            {
                SetFilter(viewer, token, child, name, stripName);
            }
        }

        /// <summary>
        /// コマンドをフィルタリングします。
        /// </summary>
        /// <param name="token"></param>
        /// <param name="treeView"></param>
        /// <param name="node"></param>
        /// <param name="name"></param>
        /// <param name="stripName"></param>
        private void CurrentFilter(TreeViewCommand viewer, CancellationToken token, TreeMenuNode node, string name, string stripName)
        {
            ObservableCollection<TreeMenuNode> treeView = viewer.TreeView.ItemsSource as ObservableCollection<TreeMenuNode>;

            if (token.IsCancellationRequested)
            {
                return;
            }

            string nodeName = node.Name.ToUpper().Replace(" ", "");

            if (stripName.Contains("<") && stripName.Contains(">") &&
                nodeName.Contains("<") && nodeName.Contains(">") &&
                stripName == CbSTUtils.StripParamater(nodeName))
            {
                // パラメータを取り除いた名前が完全一致したら下の階層すべてを対象とする

                SetAll(viewer, token, node);
                return;
            }
            if (nodeName.Contains(name))
            {
                if (node.LeftClickCommand != null && node.LeftClickCommand.CanExecute(null))
                {
                    viewer.Dispatcher.Invoke(() =>
                    {
                        var title = node.Path;
                        title = StripDotNetStandardGroupTitle(title);
                        title = CbSTUtils.StartStrip(title, ApiImporter.MENU_TITLE_DOT_NET_STANDERD_FULL_PATH);
                        title = CbSTUtils.StartStrip(title, ApiImporter.MENU_TITLE_DOT_NET_FUNCTION_FULL_PATH);
                        treeView.Add(new TreeMenuNode(title, node.HintText, OwnerCommandCanvas.CreateImmediateExecutionCanvasCommand(() =>
                        {
                            ExecuteFindCommand(node.Path);
                        })));
                    });
                    Thread.Sleep(FilteringWaitSleep);
                }
                if (token.IsCancellationRequested)
                {
                    return;
                }
            }
        }

        /// <summary>
        /// コマンドをまとめて取り込みます。
        /// </summary>
        /// <param name="token"></param>
        /// <param name="treeView"></param>
        /// <param name="node"></param>
        /// <param name="frontCut"></param>
        private void SetAll(TreeViewCommand viewer, CancellationToken token, TreeMenuNode node, string frontCut = null)
        {
            ObservableCollection<TreeMenuNode> treeView = viewer.TreeView.ItemsSource as ObservableCollection<TreeMenuNode>;

            void _SetAll(TreeViewCommand viewer, CancellationToken token, TreeMenuNode node, string frontCut = null)
            {
                ObservableCollection<TreeMenuNode> treeView = viewer.TreeView.ItemsSource as ObservableCollection<TreeMenuNode>;

                if (token.IsCancellationRequested)
                {
                    return;
                }

                if (node.LeftClickCommand != null && node.LeftClickCommand.CanExecute(null))
                {
                    var title = node.Path;
                    title = StripDotNetStandardGroupTitle(title);
                    if (frontCut != null)
                    {
                        title = CbSTUtils.StartStrip(title, frontCut);
                    }
                    title = CbSTUtils.StartStrip(title, ApiImporter.MENU_TITLE_DOT_NET_STANDERD_FULL_PATH);
                    title = CbSTUtils.StartStrip(title, ApiImporter.MENU_TITLE_DOT_NET_FUNCTION_FULL_PATH);
                    viewer.Dispatcher.Invoke(() =>
                    {
                        treeView.Add(new TreeMenuNode(title, node.HintText, OwnerCommandCanvas.CreateImmediateExecutionCanvasCommand(() =>
                        {
                            ExecuteFindCommand(node.Path);
                        })));
                    });
                    Thread.Sleep(FilteringWaitSleep);
                }
                if (token.IsCancellationRequested)
                {
                    return;
                }

                foreach (var child in node.Child)
                {
                    _SetAll(viewer, token, child, frontCut);
                }
            }

            foreach (var child in node.Child)
            {
                _SetAll(viewer, token, child, frontCut);
            }
        }

        /// <summary>
        /// メソッドグループタイトルを削除します。
        /// </summary>
        /// <param name="title"></param>
        /// <returns></returns>
        private static string StripDotNetStandardGroupTitle(string title)
        {
            if (title.StartsWith(ApiImporter.MENU_TITLE_DOT_NET_STANDERD_FULL_PATH))
            {
                int pos1 = title.LastIndexOf('.');
                string temp = title.Substring(0, pos1);
                int pos2 = temp.LastIndexOf('.');
                title = title.Substring(0, pos2) + title.Substring(pos1);
            }

            return title;
        }

        /// <summary>
        /// 正式な名前からコマンドノードを参照します。
        /// </summary>
        /// <param name="node">メニューノード</param>
        /// <param name="name">正式なメニュー名</param>
        /// <param name="execute">実行可能な対象か</param>
        /// <returns>存在しているならノードを返します</returns>
        public TreeMenuNode FindMenuPath(TreeMenuNode node, string name, bool execute = false)
        {
            if (node.Path == name)
            {
                if (execute)
                {
                    if (node.LeftClickCommand.CanExecute(null))
                        return node;
                }
                else
                {
                    return node;
                }
            }
            foreach (var child in node.Child)
            {
                TreeMenuNode hit = FindMenuPath(child, name, execute);
                if (hit != null)
                {
                    return hit;
                }
            }
            return null;
        }

        /// <summary>
        /// 名前からコマンドノードを参照します。
        /// </summary>
        /// <param name="node">メニューノード</param>
        /// <param name="name">メニュー名</param>
        /// <param name="execute">実行可能な対象か</param>
        /// <returns>存在しているならノードを返します</returns>
        public TreeMenuNode FindMenuName(TreeMenuNode node, string name, bool execute = false)
        {
            if (node.Name == name)
            {
                if (execute)
                {
                    if (node.LeftClickCommand.CanExecute(null))
                        return node;
                }
                else
                {
                    return node;
                }
            }
            foreach (var child in node.Child)
            {
                TreeMenuNode hit = FindMenuName(child, name, execute);
                if (hit != null)
                {
                    return hit;
                }
            }
            return null;
        }

        private static new void PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (!e.Handled)
            {
                e.Handled = true;
                var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta);
                eventArg.RoutedEvent = UIElement.MouseWheelEvent;
                eventArg.Source = sender;
                var parent = ((System.Windows.Controls.Control)sender).Parent as UIElement;
                parent.RaiseEvent(eventArg);
            }
        }

        /// <summary>
        /// .区切りで階層を作成する
        /// </summary>
        /// <param name="group"></param>
        /// <param name="title"></param>
        /// <returns></returns>
        public static string MakeGroup(ref TreeMenuNode group, string title)
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

        public static void AddGroupedMenu(
            TreeMenuNode node,
            string name,
            string help = null,
            Action<object> executeEvent = null,
            Func<object, bool> canExecuteEvent = null)
        {
            TreeMenuNode group = node;
            string title = TreeViewCommand.MakeGroup(ref group, name);
            var command = new TreeMenuNodeCommand(executeEvent, canExecuteEvent);
            TreeMenuNode menu;
            if (help is null)
            {
                menu = new TreeMenuNode(title, "", command);
            }
            else
            {
                menu = new TreeMenuNode(title, help, command);
            }
            group.AddChild(menu);
        }
    }
}
