using CapyCSS;
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
        public const string HELP = "Help:";

        public HelpWindow()
        {
            InitializeComponent();

            Title = MainWindow.AppName;
            Info.Text = MainWindow.AppName + Environment.NewLine;
            Info.Text += "Version " + MainWindow.AppVersion + Environment.NewLine;

            Info.Text += "© 2022 Aradono Katsumi" + Environment.NewLine;
            Info.Text += "All rights reserved." + Environment.NewLine;
            Info.Text += Environment.NewLine;
            Info.Text += "Copyright notice:" + Environment.NewLine;
            Info.Text += "  Microsoft.NET " + Environment.Version.ToString() + " (" + (Environment.Is64BitProcess ? "64bit" : "32bit") + ")" + Environment.NewLine;
            Info.Text += "  Material Design In XAML Toolkit 4.3.0" + Environment.NewLine;
            Info.Text += "  Microsoft.ML.NET 1.7.0" + Environment.NewLine;
            Info.Text += "  Microsoft.ML.ImageAnalytics 1.7.0" + Environment.NewLine;
            Info.Text += "  Microsoft.ML.OnnxRuntime 1.10.0" + Environment.NewLine;
            Info.Text += "  Microsoft.ML.OnnxTransformer 1.7.0" + Environment.NewLine;

            Info.Text += Environment.NewLine;
            Info.Text += "Shortcut Key:" + Environment.NewLine;
            Info.Text += CapyCSS.Language.Instance["Help:SCK"] + Environment.NewLine;
        }

        public static HelpWindow Create()
        {
            HelpWindow helpWindow = new HelpWindow();
            helpWindow.Owner = CommandCanvasList.OwnerWindow;
            return helpWindow;
        }
    }
}
