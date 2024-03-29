﻿using CapyCSS.Controls.BaseControls;
using CapyCSS.Script;
using CapyCSSattribute;
using CapyCSS.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using MahApps.Metro.Controls;

namespace CapyCSS
{
    [ScriptClass]
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        static private MainWindow instance;

        /// <summary>
        /// MainWindowのインスタンスを参照します。
        /// </summary>
        static public MainWindow Instance
        {
            get { return instance; }
        }

        public MainWindow()
        {
            InitializeComponent();
            instance = this;
            Closing += MainWindow_Closing;
            SourceInitialized += MainWindow_SourceInitialized;
        }

        private void MainWindow_SourceInitialized(object sender, EventArgs e)
        {
            CommandControl.Setup(
                this,
                App.CAPY_CSS_PATH,
                (filename) =>
                {
                    // タイトルをセットします。

                    var assm = Assembly.GetExecutingAssembly();
                    var name = assm.GetName();
                    Title = name.Name + " ver " + AppVersion + "-beta.6";
                    if (filename != null && filename != "")
                        Title += $" [{filename}]";
                }, CallClosing, App.EntryLoadFile, App.IsAutoExecute, App.IsAutoExit);
        }

        /// <summary>
        /// アプリ名を参照します。
        /// </summary>
        static public string AppName
        {
            get
            {
                var assm = Assembly.GetExecutingAssembly();
                var name = assm.GetName();
                return name.Name;
            }
        }

        /// <summary>
        /// アプリバージョンを参照します。
        /// </summary>
        static public string AppVersion
        {
            get
            {
                var assm = Assembly.GetExecutingAssembly();
                var name = assm.GetName();
                return name.Version.ToString();
            }
        }

        /// <summary>
        /// アプリの終了処理です。
        /// </summary>
        public void CallClosing()
        {
            Close();
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            CommandControl.SaveInfo();
            CommandControl.Dispose();
            App.Instance.SaveAppInfo();
        }
    }
}
