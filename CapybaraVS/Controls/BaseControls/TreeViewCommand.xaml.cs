using CapyCSS.Controls;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
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
        public void SetFilter(TreeViewCommand viewer, string name)
        {
            ObservableCollection<TreeMenuNode> treeView = viewer.TreeView.ItemsSource as ObservableCollection<TreeMenuNode>;
            treeView.Clear();   // これを起因にバインド系のエラーが出るが…無視して良い...
            int limit = 50;
            foreach (var node in AssetTreeData)
            {
                if (node.Path == RecentName)
                {
                    continue;
                }
                SetFilter(treeView, node, name.ToUpper(), ref limit);
                if (limit <= 0)
                    return;
            }
        }

        /// <summary>
        /// コマンドをフィルタリングします。
        /// </summary>
        /// <param name="viewer">結果登録用リスト</param>
        /// <param name="node">メニューノード</param>
        /// <param name="name">メニュー名</param>
        private void SetFilter(ObservableCollection<TreeMenuNode> treeView, TreeMenuNode node, string name, ref int limit)
        {
            if (limit <= 0)
                return;

            if (node.Name.ToUpper().Contains(name))
            {
                if (node.LeftClickCommand != null && node.LeftClickCommand.CanExecute(null))
                {
                    treeView.Add(new TreeMenuNode(node.Path, OwnerCommandCanvas.CreateImmediateExecutionCanvasCommand(() =>
                    {
                        ExecuteFindCommand(node.Path);
                    })));
                }
                if (--limit == 0)
                    return;
            }
            foreach (var child in node.Child)
            {
                SetFilter(treeView, child, name, ref limit);
            }
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
