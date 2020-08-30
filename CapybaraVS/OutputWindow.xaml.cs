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

namespace CapybaraVS
{
    /// <summary>
    /// OutputWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class OutputWindow : Window
    {
        public OutputWindow()
        {
            InitializeComponent();
            DataContext = this;
            BindText = "";
        }

        //-----------------------------------------------------------------------------------
        /// <summary>
        /// 
        /// </summary>
        public static readonly DependencyProperty BindTextProperty =
            DependencyProperty.Register("BindText", typeof(string), typeof(OutputWindow), new PropertyMetadata(""));

        //-----------------------------------------------------------------------------------
        /// <summary>
        /// 
        /// </summary>
        public string BindText
        {
            get
            {
                return (string)GetValue(BindTextProperty);
            }
            set
            {
                SetValue(BindTextProperty, value);

                // スクロールを最終行へ移動
                Log.Focus();
                Log.CaretIndex = Log.Text.Length;
                Log.ScrollToEnd();

                Console.WriteLine(Title + " : " + value);

                if (!IsVisible)
                {
                    // 表示されていないので表示

                    Show();
                }
            }
        }

        public string AddBindText
        {
            set => BindText = BindText + value + Environment.NewLine;
        }
    }
}
