using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        private TreeMenuNode vm;

        //-----------------------------------------------------------------------------------
        /// <summary>
        /// 
        /// </summary>
        public Func<object, bool> canExecuteEvent { get; set; } = null;

        //-----------------------------------------------------------------------------------
        /// <summary>
        /// 
        /// </summary>
        public Action<object> executeEvent { get; set; } = null;

        //-----------------------------------------------------------------------------------
        /// <summary>
        /// 
        /// </summary>
        /// <param name="viewmodel"></param>
        /// <param name="executeEvent"></param>
        /// <param name="canExecuteEvent"></param>
        public TreeMenuNodeCommand(
            TreeMenuNode viewmodel,
            Action<object> executeEvent = null,
            Func<object, bool> canExecuteEvent = null)
        {
            vm = viewmodel;
            if (executeEvent != null)
            {
                this.executeEvent = executeEvent;
            }
            if (canExecuteEvent is null)
            {
                if (this.executeEvent != null)
                {
                    this.canExecuteEvent = (a) => { return true; };
                }
            }
            else
            {
                this.canExecuteEvent = canExecuteEvent;
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
            if (canExecuteEvent is null)
                return false;
            return canExecuteEvent(parameter);
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
            executeEvent?.Invoke(parameter);
        }
    }

    //-----------------------------------------------------------------------------------
    /// <summary>
    /// ツリーメニューを構成するノードクラスです。
    /// </summary>
    public sealed class TreeMenuNode
    {
        /// <summary>
        /// コンストラクタです。
        /// </summary>
        /// <param name="name">項目名</param>
        /// <param name="personCommand">実行コマンド</param>
        public TreeMenuNode(string name, TreeMenuNodeCommand personCommand = null)
        {
            Name = name;
            if (personCommand is null)
            {
                LeftClickCommand = new TreeMenuNodeCommand(this);
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
            if (hintText != "")
                HintText = hintText;
            if (personCommand is null)
            {
                LeftClickCommand = new TreeMenuNodeCommand(this);
            }
            else
            {
                LeftClickCommand = personCommand;
            }
        }

        //-----------------------------------------------------------------------------------
        /// <summary>
        /// 項目名を参照します。
        /// </summary>
        public string Name { get; set; }

        //-----------------------------------------------------------------------------------
        /// <summary>
        /// 説明を参照します。
        /// </summary>
        public string HintText { get; set; } = null;

        //-----------------------------------------------------------------------------------
        /// <summary>
        /// 子メニューコレクションを参照します。
        /// </summary>
        public List<TreeMenuNode> Child { get; set; } = new List<TreeMenuNode>();

        //-----------------------------------------------------------------------------------
        /// <summary>
        /// 左クリックイベントで呼ばれるイベントです。
        /// </summary>
        public ICommand LeftClickCommand { get; set; }

        //-----------------------------------------------------------------------------------
        /// <summary>
        /// 子メニューの有るノードの表示状態（ボタン側）を参照します。
        /// </summary>
        public Visibility GroupNodeView
        {
            get { return (Child.Count == 0) ? Visibility.Visible : Visibility.Collapsed; }
            set { }
        }

        //-----------------------------------------------------------------------------------
        /// <summary>
        /// 子メニューの無いノードの表示状態（テキストBOX側）を参照します。
        /// </summary>
        public Visibility MenuNodeView
        {
            get { return (Child.Count != 0) ? Visibility.Visible : Visibility.Collapsed; }
            set { }
        }

        //-----------------------------------------------------------------------------------
        /// <summary>
        /// 有効状態を参照します。
        /// </summary>
        public bool IsEnabled
        {
            get => LeftClickCommand.CanExecute(null) && (Child.Count == 0);
            set { }
        }
    }

    /// <summary>
    /// TreeViewCommand.xaml の相互作用ロジック
    /// </summary>
    public partial class TreeViewCommand : UserControl
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

        //-----------------------------------------------------------------------------------
        /// <summary>
        /// 
        /// </summary>
        public TreeViewCommand()
        {
            InitializeComponent();

            TreeView.ItemsSource = AssetTreeData;
        }
    }
}
