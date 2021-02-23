using CapybaraVS;
using CapyCSS.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CapyCSS
{
    /// <summary>
    /// HelpWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class HelpWindow : Window
    {
        public HelpWindow()
        {
            InitializeComponent();

            Title = MainWindow.AppName;
            Info.Text = MainWindow.AppName + Environment.NewLine;
            Info.Text += "Version " + MainWindow.AppVersion + Environment.NewLine;

            Info.Text += "© 2021 Aradono Katsumi" + Environment.NewLine;
            Info.Text += "All rights reserved." + Environment.NewLine;
            Info.Text += Environment.NewLine;
            Info.Text += "Included package:" + Environment.NewLine;
            Info.Text += "  MaterialDesignThemes 3.2.0" + Environment.NewLine;
            Info.Text += "  System.Drawing.Common 5.0.0";
        }

        public static HelpWindow Create()
        {
            HelpWindow helpWindow = new HelpWindow();
            helpWindow.Owner = CommandCanvasList.OwnerWindow;
            return helpWindow;
        }
    }
}
