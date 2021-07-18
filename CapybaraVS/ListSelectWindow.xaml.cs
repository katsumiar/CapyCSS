using CapybaraVS.Controls;
using CapybaraVS.Controls.BaseControls;
using CapybaraVS.Script;
using CapyCSS.Controls;
using CbVS.Script;
using System;
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
using System.Windows.Shapes;

namespace CapybaraVS
{
    /// <summary>
    /// ListSelectWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class ListSelectWindow : Window
    {
        #region Caption 添付プロパティ実装

        private static ImplementWindowDependencyProperty<ListSelectWindow, string> impCaption =
            new ImplementWindowDependencyProperty<ListSelectWindow, string>(
                nameof(Caption),
                (self, getValue) =>
                {
                    string text = getValue(self);
                    self.TypeName.Content = text;
                });

        public static readonly DependencyProperty CaptionProperty = impCaption.Regist("");

        public string Caption
        {
            get { return impCaption.GetValue(this); }
            set { impCaption.SetValue(this, value); }
        }

        #endregion

        /// <summary>
        /// 指定付き選択指定
        /// ※参照指定で強制的に変数を作成もしくは選択するときに設定されます。
        /// </summary>
        public static ICbValue DefaultValue { get; set; } = null;

        public static StackNode Create(
            CommandCanvas OwnerCommandCanvas,
            string caption,
            ObservableCollection<StackGroup> stackList,
            bool forcedListTypeSelect,
            Point? pos = null)
        {
            ListSelectWindow selectWindow = new ListSelectWindow();
            selectWindow.Caption = caption;
            selectWindow.Owner = CommandCanvasList.OwnerWindow;
            ControlTools.SetWindowPos(selectWindow, pos);

            if (DefaultValue != null)
                selectWindow.Add("[ New ]");
            List<string> teble = new List<string>();
            foreach (var node in stackList)
            {
                teble.Add(node.stackNode.ValueData.Name);

                if (DefaultValue != null)
                {
                    if (DefaultValue.TypeName != node.stackNode.ValueData.TypeName)
                        continue;
                }

                if (forcedListTypeSelect)
                {
                    if (!node.stackNode.ValueData.IsList)
                        continue;
                }

                selectWindow.Add(node.stackNode.ValueData.Name);
            }
            selectWindow.ShowDialog();
            if (selectWindow.SelectedIndex == -1)
            {
                return null;
            }

            if (selectWindow.SelectedName == "[ New ]")
            {
                if (DefaultValue is null)
                    return null;

                // 新規作成する
                DefaultValue.Name = "variable" + (OwnerCommandCanvas.ScriptWorkStack.StackData.Count + 1);
                var ret = OwnerCommandCanvas.ScriptWorkStack.Append(DefaultValue).stackNode;
                DefaultValue = null;
                return ret;
            }
            DefaultValue = null;

            // 選ばれたアイテムの名前からインデックスを求める
            int index = teble.FindIndex((x) => x == selectWindow.SelectedName);
            return stackList[index].stackNode;
        }

        public int SelectedIndex { get; set; } = 0;

        public string SelectedName
        {
            get
            {
                return (string)Select.Items[SelectedIndex]; 
            } 
        }

        public ListSelectWindow()
        {
            InitializeComponent();
        }

        public void Add(string name)
        {
            Select.Items.Add(name);
            Select.SelectedIndex = 0;
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            SelectedIndex = Select.SelectedIndex;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            SelectedIndex = -1;
            Close();
        }
    }
}
