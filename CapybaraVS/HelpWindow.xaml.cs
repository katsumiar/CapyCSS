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
using MahApps.Metro.Controls;

namespace CapyCSS
{
    /// <summary>
    /// HelpWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class HelpWindow : MetroWindow
    {
        public const string HELP = "Help:";

        public HelpWindow()
        {
            InitializeComponent();

            Title = MainWindow.AppName;
            Info.Text = MainWindow.AppName;
            Info.Text += " Version " + MainWindow.AppVersion + Environment.NewLine;

            Info.Text += "© 2022 Aradono Katsumi" + Environment.NewLine;
            Info.Text += "All rights reserved." + Environment.NewLine;
            Info.Text += Environment.NewLine;
            Info.Text += "Copyright notice:" + Environment.NewLine;
            Info.Text += "  Microsoft.NET " + Environment.Version.ToString() + " (" + (Environment.Is64BitProcess ? "64bit" : "32bit") + ")" + Environment.NewLine;
            Info.Text += "  MahApps.Metro 2.4.10" + Environment.NewLine;
            Info.Text += "  MahApps.Metro.IconPacks 4.11.0" + Environment.NewLine;
            Info.Text += "  MaterialDesignThemes 4.9.0" + Environment.NewLine;
            Info.Text += "  System.Drawing.Common 7.0.0" + Environment.NewLine;
            Info.Text += "  Microsoft-WindowsAPICodePack-Shell 1.1.5" + Environment.NewLine;

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
