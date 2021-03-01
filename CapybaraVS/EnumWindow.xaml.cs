using CapybaraVS.Controls.BaseControls;
using CapybaraVS.Script;
using CapyCSS.Controls;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace CapybaraVS
{
    /// <summary>
    /// EnumWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class EnumWindow : Window
    {
        #region Caption 添付プロパティ実装

        private static ImplementWindowDependencyProperty<EnumWindow, string> impCaption =
            new ImplementWindowDependencyProperty<EnumWindow, string>(
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

        #region EnumItem 添付プロパティ実装

        private static ImplementWindowDependencyProperty<EnumWindow, ICbValueEnumClass<Enum>> impEnumItem =
            new ImplementWindowDependencyProperty<EnumWindow, ICbValueEnumClass<Enum>>(
                nameof(EnumItem),
                (self, getValue) =>
                {
                    ICbValueEnumClass<Enum> selectValue = getValue(self);

                    if (selectValue != null)
                    {
                        int selectIndex = 0;
                        int count = 0;

                        foreach (var node in selectValue.ElementList)
                        {
                            self.Select.Items.Add(node);
                            if (selectValue.TypeName + "." + node == selectValue.ValueUIString)
                                selectIndex = count;
                            count++;
                        }
                        self.Select.SelectedIndex = selectIndex;
                    }
                });

        public static readonly DependencyProperty EnumItemProperty = impEnumItem.Regist(null);

        public ICbValueEnumClass<Enum> EnumItem
        {
            get { return impEnumItem.GetValue(this); }
            set { impEnumItem.SetValue(this, value); }
        }

        #endregion

        public static EnumWindow Create(ICbValueEnumClass<Enum> enumClass, Point? pos = null)
        {
            EnumWindow enumWindow = new EnumWindow();
            enumWindow.EnumItem = enumClass;
            enumWindow.Caption = enumWindow.EnumItem.TypeName;
            enumWindow.Owner = CommandCanvasList.OwnerWindow;
            ControlTools.SetWindowPos(enumWindow, pos);
            return enumWindow;
        }

        public EnumWindow()
        {
            InitializeComponent();
        }

        public void RemoveItem(string name)
        {
            if (Select.Items.Contains(name))
            {
                Select.Items.Remove(name);
                Select.SelectedIndex = 0;
            }
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            EnumItem.ValueString = (string)Select.SelectedItem;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            EnumItem = null;
            Close();
        }
    }
}
