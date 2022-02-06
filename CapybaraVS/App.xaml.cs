/*
Copyright © 2020 Katsumi Aradono. All rights reserved.
*/

using CapyCSS.Controls.BaseControls;
using CapyCSS.Controls;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;
using System.Xml.Serialization;

namespace CapyCSS
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        #region XML定義
        [XmlRoot(nameof(App))]
        public class _AssetXML<OwnerClass> : IDisposable
            where OwnerClass : App
        {
            [XmlIgnore]
            public Action WriteAction = null;
            [XmlIgnore]
            public Action<OwnerClass> ReadAction = null;
            private bool disposedValue;

            public _AssetXML()
            {
                ReadAction = (self) =>
                {

                    // 次回の為の初期化
                    self.AssetXML = new _AssetXML<App>(self);
                };
            }
            public _AssetXML(OwnerClass self)
            {
                WriteAction = () =>
                {

                };
            }

            protected virtual void Dispose(bool disposing)
            {
                if (!disposedValue)
                {
                    if (disposing)
                    {
                        WriteAction = null;
                        ReadAction = null;

                        // 以下、固有定義開放
                    }
                    disposedValue = true;
                }
            }

            public void Dispose()
            {
                Dispose(disposing: true);
                GC.SuppressFinalize(this);
            }
            #region 固有定義
            #endregion
        }
        public _AssetXML<App> AssetXML { get; set; } = null;
        #endregion

        public static string CAPY_CSS_PATH;
        public static string APP_INFO_PATH = @"app.xml";
        public static string EntryLoadFile = null;  // スクリプトの起動後読み込み
        public static bool IsAutoExecute = false;   // スクリプトの自動実行
        public static bool IsAutoExit = false;      // スクリプトの自動実行後自動終了
        public static App Instance = null;

        [System.Runtime.InteropServices.DllImport("Kernel32.dll")]
        public static extern bool AttachConsole(int processId);

        static App()
        {
            // コンソールにアタッチする
            AttachConsole(-1);
        }

        private void Application_StartUp(object sender, StartupEventArgs e)
        {
            Instance = this;

            // カレントディレクトリに作成する
            {
                CAPY_CSS_PATH = Path.Combine(
                    System.Environment.GetFolderPath(Environment.SpecialFolder.Personal),
                    Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().GetName().Name));
                if (!Directory.Exists(CAPY_CSS_PATH))
                {
                    Directory.CreateDirectory(CAPY_CSS_PATH);
                }
                APP_INFO_PATH = Path.Combine(CAPY_CSS_PATH, APP_INFO_PATH);
            }

            // ツールチップの表示時間を設定
            System.Windows.Controls.ToolTipService.ShowDurationProperty.OverrideMetadata(
                typeof(DependencyObject),
                new FrameworkPropertyMetadata(4000));

            AssetXML = new _AssetXML<App>(this);
            if (!File.Exists(APP_INFO_PATH))
            {
                SaveAppInfo();
            }
            LoadAppInfo();

            string[] cmds = System.Environment.GetCommandLineArgs();
            if (cmds.Length > 1)
            {
                int index = 0;
                foreach (var arg in cmds)
                {
                    if (arg.StartsWith("-"))
                    {
                        switch (arg)
                        {
                            case "-as":
                                IsAutoExecute = true;
                                break;

                            case "-ae":
                                IsAutoExit = true;
                                break;

                            case "-ase":
                                IsAutoExecute = true;
                                IsAutoExit = true;
                                break;
                        }
                        continue;
                    }
                    if (index == 1)
                    {
                        EntryLoadFile = arg;
                    }
                    index++;
                }
            }
        }

        /// <summary>
        /// アプリケーションのカレントディレクトリを参照します。
        /// </summary>
        public static string CurrentAppDir => System.Environment.CurrentDirectory;

        public void SaveAppInfo()
        {
            try
            {
                var writer = new StringWriter();
                var serializer = new XmlSerializer(AssetXML.GetType());
                var namespaces = new XmlSerializerNamespaces();
                namespaces.Add(string.Empty, string.Empty);
                AssetXML.WriteAction();
                serializer.Serialize(writer, AssetXML, namespaces);
                StreamWriter swriter = new StreamWriter(APP_INFO_PATH, false);
                swriter.WriteLine(writer.ToString());
                swriter.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(CommandCanvasList.OwnerWindow, ex.Message);
            }
        }

        public void LoadAppInfo()
        {
            StreamReader reader = new StreamReader(APP_INFO_PATH);
            XmlSerializer serializer = new XmlSerializer(AssetXML.GetType());

            XmlDocument doc = new XmlDocument();
            doc.PreserveWhitespace = true;
            doc.Load(reader);
            XmlNodeReader nodeReader = new XmlNodeReader(doc.DocumentElement);

            object data = (App._AssetXML<App>)serializer.Deserialize(nodeReader);
            AssetXML = (App._AssetXML<App>)data;
            reader.Close();
            AssetXML.ReadAction(this);
        }
    }
}
