﻿using CapyCSS.Script;
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
using System.Windows.Threading;
using System.Xml.Serialization;
using System.Linq;

namespace CapyCSS.Controls.BaseControls
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
        /// 実行可能かの変化を通知します。
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

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

    public interface IMenuCommand
        : ICommand
    {
        TreeMenuNode.NodeType NodeType { get; }
        string Name { get; }
        Func<string> HintText { get; }
    }

    //-----------------------------------------------------------------------------------
    /// <summary>
    /// ツリーメニューを構成するノードクラスです。
    /// </summary>
    public sealed class TreeMenuNode : INotifyPropertyChanged
    {
        public enum NodeType
        {
            NORMAL,
            GROUP,
            DEFULT_COMMAND,
            RECENT_COMMAND,
            FILTERING_COMMAND,
        }

        /// <summary>
        /// コンストラクタです。
        /// </summary>
        /// <param name="name">項目名</param>
        /// <param name="personCommand">実行コマンド</param>
        public TreeMenuNode(IMenuCommand personCommand = null, ICommand deleteClickCommand = null)
        {
            makeMenuNode(personCommand.NodeType, personCommand.Name, personCommand.HintText, personCommand, null);
        }

        /// <summary>
        /// コンストラクタです。
        /// </summary>
        /// <param name="name">項目名</param>
        /// <param name="personCommand">実行コマンド</param>
        public TreeMenuNode(NodeType nodeType, string name, ICommand personCommand = null, ICommand deleteClickCommand = null)
        {
            this.nodeType = nodeType;
            Name = name;
            Path = name;
            GroupClickCommand = new TreeMenuNodeCommand()
            {
                ExecuteEvent = (o) => { IsExpanded = !IsExpanded; }
            };
            if (personCommand is null)
            {
                LeftClickCommand = null;
            }
            else
            {
                LeftClickCommand = personCommand;
            }
            DeleteClickCommand = deleteClickCommand;
        }

        /// <summary>
        /// コンストラクタです。
        /// </summary>
        /// <param name="name">項目名</param>
        /// <param name="personCommand">実行コマンド</param>
        public TreeMenuNode(NodeType nodeType, string name, Func<string> hintText, ICommand personCommand = null, ICommand deleteClickCommand = null)
        {
            makeMenuNode(nodeType, name, hintText, personCommand, deleteClickCommand);
        }

        private void makeMenuNode(NodeType nodeType, string name, Func<string> hintText, ICommand personCommand, ICommand deleteClickCommand)
        {
            this.nodeType = nodeType;
            Name = name;
            Path = name;
            GroupClickCommand = null;
            if (hintText != null)
            {
                _hintTextFunc = hintText;
            }
            if (personCommand is null)
            {
                LeftClickCommand = new TreeMenuNodeCommand();
            }
            else
            {
                LeftClickCommand = personCommand;
            }
            DeleteClickCommand = deleteClickCommand;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string name)
        {
            if (null == this.PropertyChanged) return;
            this.PropertyChanged(this, new PropertyChangedEventArgs(name));
        }

        //-----------------------------------------------------------------------------------
        private NodeType nodeType { get; set; }

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
        public string HintText
        {
            get
            {
                if (_hintText is null && _hintTextFunc != null)
                {
                    _hintText = _hintTextFunc();
                }
                return _hintText;
            }
            //set
            //{
            //    _hintText = value;
            //}
        }
        public string _hintText = null;
        public Func<string> _hintTextFunc = null;

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

            if (node.Name.Contains(CbSTUtils.MENU_OLD_SPECIFICATION))
            {
                // 古い仕様は、登録しない

                return;
            }

            Child.Add(node);

            nodeType = TreeMenuNode.NodeType.GROUP;
            OnPropertyChanged(nameof(MenuNodeView));
            OnPropertyChanged(nameof(GroupNodeView));
            OnPropertyChanged(nameof(GroupForeground));
            OnPropertyChanged(nameof(IsGroupEnabled));
        }

        /// <summary>
        /// すべての子をクリアします。
        /// </summary>
        public void ClearChild()
        {
            Child.Clear();
            OnPropertyChanged(nameof(GroupForeground));
            OnPropertyChanged(nameof(IsGroupEnabled));
        }

        /// <summary>
        /// グループ用文字色です。
        /// </summary>
        public Brush GroupForeground
        {
            get
            {
                if (!IsGroupEnabled)
                {
                    return (Brush)Application.Current.FindResource("EmptyCommandBrush");
                }
                return (Brush)Application.Current.FindResource("CommandGroupBrush");
            }
            set {}
        }

        //-----------------------------------------------------------------------------------
        /// <summary>
        /// コマンドノードの左クリックイベントで呼ばれるイベントです。
        /// </summary>
        public ICommand LeftClickCommand
        {
            get => leftClickCommand;
            set
            {
                leftClickCommand = value;
                OnPropertyChanged(nameof(Foreground));
                OnPropertyChanged(nameof(IsEnabled));
            }
        }
        private ICommand leftClickCommand = null;

        //-----------------------------------------------------------------------------------
        /// <summary>
        /// グループノードの左クリックイベントで呼ばれるイベントです。
        /// </summary>
        public ICommand GroupClickCommand
        {
            get => groupClickCommand;
            set
            {
                groupClickCommand = value;
                OnPropertyChanged(nameof(Foreground));
                OnPropertyChanged(nameof(IsEnabled));
            }
        }
        private ICommand groupClickCommand = null;

        //-----------------------------------------------------------------------------------
        /// <summary>
        /// コマンド用文字色です。
        /// </summary>
        public Brush Foreground
        {
            get
            {
                if (LeftClickCommand is null)
                {
                    return (Brush)Application.Current.FindResource("EmptyCommandBrush");
                }
                if (!IsEnabled)
                {
                    return (Brush)Application.Current.FindResource("UnenableCommandBrush");
                }
                switch (nodeType)
                {
                    case NodeType.DEFULT_COMMAND:
                        return (Brush)Application.Current.FindResource("DefaultCommandBrush");
                    case NodeType.RECENT_COMMAND:
                        return (Brush)Application.Current.FindResource("RecentCommandBrush");
                    case NodeType.FILTERING_COMMAND:
                        return (Brush)Application.Current.FindResource("FilteringCommandBrush");
                }
                return (Brush)Application.Current.FindResource("CommandBrush");
            }
            set {}
        }

        //-----------------------------------------------------------------------------------
        /// <summary>
        /// 削除アイコン左クリックイベントで呼ばれるイベントです。
        /// </summary>
        public ICommand DeleteClickCommand 
        {
            get => deleteClickCommand;
            set
            {
                deleteClickCommand = value;
                OnPropertyChanged(nameof(DeleteNodeView));
            }
        }
        private ICommand deleteClickCommand = null;

        /// <summary>
        /// 削除アイコンの表示を制御します。
        /// </summary>
        public Visibility DeleteNodeView
        {
            get
            {
                if (DeleteClickCommand is null)
                {
                    return Visibility.Collapsed;
                }
                if (!DeleteClickCommand.CanExecute(null))
                {
                    return Visibility.Collapsed;
                }
                return Visibility.Visible;
            }
            set {}
        }

        //-----------------------------------------------------------------------------------
        /// <summary>
        /// グループか否かを判定します。
        /// </summary>
        public bool IsGroup => nodeType == NodeType.GROUP || Child.Count != 0;

        //-----------------------------------------------------------------------------------
        /// <summary>
        /// 子メニューの無いノードの表示状態（ボタン側）を参照します。
        /// </summary>
        public Visibility MenuNodeView
        {
            get => !IsGroup ? Visibility.Visible : Visibility.Collapsed;
            set {}
        }

        //-----------------------------------------------------------------------------------
        /// <summary>
        /// 子メニューの有るノードの表示状態（テキストBOX側）を参照します。
        /// </summary>
        public Visibility GroupNodeView
        {
            get => IsGroup ? Visibility.Visible : Visibility.Collapsed;
            set {}
        }

        //-----------------------------------------------------------------------------------
        /// <summary>
        /// グループを閉じるか判定します。
        /// </summary>
        private bool isExpanded = false;
        public bool IsExpanded
        {
            get => isExpanded;
            set 
            {
#if false
                if (!IsGroupEnabled)
                {
                    isExpanded = false;
                    OnPropertyChanged("IsGroupEnabled");
                    OnPropertyChanged("GroupForeground");
                    OnPropertyChanged("IsExpanded");
                    return;
                }
#endif
                isExpanded = value;
                OnPropertyChanged("IsExpanded");
            }
        }

        //-----------------------------------------------------------------------------------
        /// <summary>
        /// コマンドの有効状態を参照します。
        /// </summary>
        public bool IsEnabled
        {
            get
            {
                if (LeftClickCommand is null || !LeftClickCommand.CanExecute(null))
                {
                    return false;
                }
                return isEnabled;
            }
            set 
            {
                isEnabled = value;
                OnPropertyChanged("IsEnabled");
                OnPropertyChanged("Foreground");
            }
        }
        private bool isEnabled = true;

        //-----------------------------------------------------------------------------------
        /// <summary>
        /// グループ用の有効状態を参照します。
        /// </summary>
        public bool IsGroupEnabled
        {
            get
            {
#if true
                return Child.Count > 0;
#else
                if (IsGroup)
                {
                    foreach (var child in Child)
                    {
                        if (IsGroup)
                        {
                            if (child.IsGroupEnabled)
                                return true;
                        }
                        else
                        {
                            if (child.IsEnabled)
                                return true;
                        }
                    }
                    return false;
                }
                return IsEnabled;
#endif
            }
            set {}
        }

        /// <summary>
        /// コマンドの状態を更新します。
        /// </summary>
        public void UpdateCommand()
        {
            OnPropertyChanged("IsEnabled");
            OnPropertyChanged("Foreground");
            foreach (var node in Child)
            {
                node.UpdateCommand();
            }
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
        const int FilteringMax = 5000;

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
            var recentNode = new TreeMenuNode(TreeMenuNode.NodeType.RECENT_COMMAND, RecentName);
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
                node.UpdateCommand();
            }
        }

        /// <summary>
        /// コマンドをフィルタリングします。
        /// </summary>
        /// <param name="name">メニュー名</param>
        public CancellationTokenSource SetFilter(TreeViewCommand viewer, string name)
        {
            ObservableCollection<TreeMenuNode> treeView = viewer.TreeView.ItemsSource as ObservableCollection<TreeMenuNode>;

            IDictionary<string, string> primitiveDic = new Dictionary<string, string>()
            {
                { "STRING", "system.string" },
                { "BOOL", "system.boolean" },
                { "BYTE", "system.byte" },
                { "SBYTE", "system.sbyte" },
                { "CHAR", "system.char" },
                { "SHORT", "system.int16" },
                { "INT", "system.int32" },
                { "LONG", "system.int64" },
                { "FLOAT", "system.single" },
                { "DOUBLE", "system.double" },
                { "USHORT", "system.uint16" },
                { "UINT", "system.uint32" },
                { "ULONG", "system.uint64" },
                { "DECIMAL", "system.decimal" },
            };

            if (primitiveDic.ContainsKey(name.ToUpper()))
            {
                name = primitiveDic[name.ToUpper()];
            }
            else if (name.EndsWith("[]"))
            {
                name = "system.array";
            }

            string searchName = name.ToUpper().Replace(" ", "");

            ICollection<TreeMenuNode> treeMenuNodes = new List<TreeMenuNode>();
            foreach (var assetData in AssetTreeData)
            {
                treeMenuNodes.Add(assetData);
            }

            var tokenSource = new CancellationTokenSource();
            var token = tokenSource.Token;
            string[] option = null;
            if (searchName.Contains('&'))
            {
                string opt = searchName.Substring(searchName.IndexOf('&') + 1);
                if (opt.Contains('&'))
                {
                    option = opt.Split('&');
                }
                else
                {
                    option = new string[] { opt };
                }
                searchName = searchName.Substring(0, searchName.IndexOf('&'));
            }
            Task.Run(() => ActionFilter(treeMenuNodes, viewer, token, searchName, option));
            return tokenSource;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="token"></param>
        /// <param name="treeView"></param>
        /// <param name="searchName"></param>
        private void ActionFilter(IEnumerable<TreeMenuNode> treeMenuNodes, TreeViewCommand viewer, CancellationToken token, string searchName, string[] option)
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

                int counter = 0;
                SetFilter(viewer, token, node, searchName, CbSTUtils.StripParamater(searchName), option, ref counter);
            }
        }

        /// <summary>
        /// コマンドをフィルタリングします。
        /// 「.」を見つけた場合、そこを起点に一致判定を「.」区切りの完全一致に切り替えます。
        /// </summary>
        /// <param name="viewer">結果登録用リスト</param>
        /// <param name="node">メニューノード</param>
        /// <param name="name">メニュー名</param>
        private void SetFilter(TreeViewCommand viewer, CancellationToken token, TreeMenuNode node, string name, string stripName, string[] option, ref int counter, TreeMenuNode currentLock = null)
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
                        SetFilter(viewer, token, child, next, stripName, option, ref counter, node);
                    }
                }
                if (currentLock != null)
                {
                    //「<」以降を含まずに一致判定

                    string target = nodeName;
                    string paramStr = "";
                    int pos = target.IndexOf('<');
                    if (pos != -1)
                    {
                        paramStr = CbSTUtils.GetParamater(target);
                        if (paramStr is null)
                        {
                            paramStr = "";
                        }
                        target = target.Substring(0, pos);
                    }
                    if (target == name)
                    {
                        SetAll(viewer, token, node, option, currentLock.Path.Substring(0, currentLock.Path.LastIndexOf('.') + 1) + stripName + paramStr + ".");
                    }
                    return;
                }
            }
            else if (nodeName == name)
            {
                // 名前が完全一致したら下の階層すべてを対象とする

                SetAll(viewer, token, node, option);
                return;
            }
            else
            {
                // 通常の一致判定

                CurrentFilter(viewer, token, node, name, stripName, option, ref counter);
            }
            foreach (var child in node.Child)
            {
                // 子の一致判定

                SetFilter(viewer, token, child, name, stripName, option, ref counter);
                if (counter % 100 == 99)
                    Thread.Sleep(FilteringWaitSleep);
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
        private void CurrentFilter(TreeViewCommand viewer, CancellationToken token, TreeMenuNode node, string name, string stripName, string[] option, ref int counter)
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

                SetAll(viewer, token, node, option);
                return;
            }
            if (CheckOption(option, nodeName, nodeName.Contains(name)))
            {
                if (node.LeftClickCommand != null && node.LeftClickCommand.CanExecute(null))
                {
                    var title = node.Path;
                    if (title.Contains(CbSTUtils.MENU_OLD_SPECIFICATION))
                    {
                        return;
                    }
                    title = StripDotNetStandardGroupTitle(title);
                    title = CbSTUtils.StripNameSpace(title, ApiImporter.MENU_TITLE_IMPORT_FUNCTION_FULL_PATH);
                    title = CbSTUtils.StartStrip(title, ApiImporter.MENU_TITLE_DOT_NET_FUNCTION_FULL_PATH);
                    viewer.Dispatcher.Invoke(() =>
                    {
                        if (treeView.Count < FilteringMax)
                        {
                            treeView.Add(new TreeMenuNode(TreeMenuNode.NodeType.FILTERING_COMMAND, title, node._hintTextFunc, OwnerCommandCanvas.CreateImmediateExecutionCanvasCommand(() =>
                            {
                                ExecuteFindCommand(node.Path);
                            })));
                        }
                    });
                    Thread.Sleep(FilteringWaitSleep);
                    counter = 0;
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
        private void SetAll(TreeViewCommand viewer, CancellationToken token, TreeMenuNode node, string[] option, string frontCut = null)
        {
            ObservableCollection<TreeMenuNode> treeView = viewer.TreeView.ItemsSource as ObservableCollection<TreeMenuNode>;

            void _SetAll(Collection<TreeMenuNode> treeView, CancellationToken token, TreeMenuNode node, string[] option, string frontCut = null)
            {
                if (token.IsCancellationRequested)
                {
                    return;
                }

                if (node.LeftClickCommand != null && node.LeftClickCommand.CanExecute(null))
                {
                    var title = node.Path;
                    if (title.Contains(CbSTUtils.MENU_OLD_SPECIFICATION))
                    {
                        return;
                    }
                    title = StripDotNetStandardGroupTitle(title);
                    if (frontCut != null)
                    {
                        title = CbSTUtils.StartStrip(title, frontCut, true);
                    }
                    title = CbSTUtils.StripNameSpace(title, ApiImporter.MENU_TITLE_IMPORT_FUNCTION_FULL_PATH);
                    title = CbSTUtils.StartStrip(title, ApiImporter.MENU_TITLE_DOT_NET_FUNCTION_FULL_PATH);
                    string menuTitle = null;
                    if (frontCut != null)
                    {
                        // 子メニューを用意する

                        menuTitle = frontCut.Substring(0, frontCut.Length - 1);
                        menuTitle = menuTitle.Substring(menuTitle.LastIndexOf('.') + 1);
                    }
                    viewer.Dispatcher.Invoke(() =>
                    {
                        if (treeView.Count < FilteringMax)
                        {
                            var addNode = new TreeMenuNode(TreeMenuNode.NodeType.RECENT_COMMAND, title, node._hintTextFunc, OwnerCommandCanvas.CreateImmediateExecutionCanvasCommand(() =>
                            {
                                ExecuteFindCommand(node.Path);
                            }));

                            TreeMenuNode menuNode = null;
                            if (menuTitle != null)
                            {
                                // 子メニューの中に追加する

                                bool existMenu = false;
                                foreach (var node in treeView)
                                {
                                    if (node.Name == menuTitle)
                                    {
                                        // 既存の子メニューが存在した

                                        existMenu = true;
                                        menuNode = node;
                                        break;
                                    }
                                }
                                if (!existMenu)
                                {
                                    // 新規の子メニューを追加

                                    menuNode = new TreeMenuNode(TreeMenuNode.NodeType.NORMAL, menuTitle);
                                    treeView.Add(menuNode);
                                }
                                menuNode.Child.Add(addNode);
                            }
                            else
                            {
                                treeView.Add(addNode);
                            }
                        }
                    });
                    Thread.Sleep(FilteringWaitSleep);
                }
                if (token.IsCancellationRequested)
                {
                    return;
                }

                foreach (var child in node.Child)
                {
                    if (CheckOption(option, child.Name.ToUpper().Replace(" ", ""), true))
                    {
                        _SetAll(treeView, token, child, option, frontCut);
                    }
                }
            }

            foreach (var child in node.Child)
            {
                _SetAll(treeView, token, child, option, frontCut);
            }
        }

        /// <summary>
        /// 対象文字列に指定のワードがすべて含まれているか調べます。
        /// </summary>
        /// <param name="words">アンド条件ワード</param>
        /// <param name="title">対象ワード</param>
        /// <param name="isHit">初期条件</param>
        /// <returns>条件を満たした==true</returns>
        private static bool CheckOption(string[] words, string title, bool isHit)
        {
            if (words != null)
            {
                foreach (var ck in words)
                {
                    isHit = isHit && title.Contains(ck);
                    if (!isHit)
                        break;
                }
            }
            return isHit;
        }

        /// <summary>
        /// メソッドグループタイトルを削除します。
        /// </summary>
        /// <param name="title"></param>
        /// <returns></returns>
        private static string StripDotNetStandardGroupTitle(string title)
        {
            if (title.StartsWith(ApiImporter.MENU_TITLE_IMPORT_FUNCTION_FULL_PATH))
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
                        temp = new TreeMenuNode(TreeMenuNode.NodeType.NORMAL, arr[i]);
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
            Func<string> help = null,
            Action<object> executeEvent = null,
            Func<object, bool> canExecuteEvent = null)
        {
            TreeMenuNode group = node;
            string title = TreeViewCommand.MakeGroup(ref group, name);
            var command = new TreeMenuNodeCommand(executeEvent, canExecuteEvent);
            TreeMenuNode menu;
            if (help is null)
            {
                menu = new TreeMenuNode(TreeMenuNode.NodeType.NORMAL, title, command);
            }
            else
            {
                menu = new TreeMenuNode(TreeMenuNode.NodeType.NORMAL, title, help, command);
            }
            group.AddChild(menu);
        }
    }
}
