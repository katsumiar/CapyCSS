/*
Copyright © 2020 Katsumi Aradono. All rights reserved.
*/

using CapybaraVS.Control.BaseControls;
using CapybaraVS.Controls.BaseControls;
using CapyCSS.Controls;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;
using System.Xml.Serialization;

namespace CapybaraVS
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        #region XML定義
        [XmlRoot(nameof(App))]
        public class _AssetXML<OwnerClass>
            where OwnerClass : App
        {
            [XmlIgnore]
            public Action WriteAction = null;
            [XmlIgnore]
            public Action<OwnerClass> ReadAction = null;
            public _AssetXML()
            {
                ReadAction = (self) =>
                {
                    CapybaraVS.Language.GetInstance.LanguageType = Language;

                    if (Recent != null)
                    {
                        // 最近使ったスクリプトノードを復元する

                        var recentNode = CommandWindow.TreeViewCommand.GetRecent();
                        foreach (var node in Recent)
                        {
                            recentNode.AddChild(new TreeMenuNode(node, CommandCanvas.CreateImmediateExecutionCanvasCommand(() =>
                            {
                                CommandWindow.TreeViewCommand.ExecuteFindCommand(node);
                            })));
                        }
                    }

                    if (BackGroundImagePath != null)
                    {
                        // 作業領域の背景イメージのパスをセットする

                        BaseWorkCanvas.BackGrountImagePath = BackGroundImagePath;
                    }

                    // 次回の為の初期化
                    self.AssetXML = new _AssetXML<App>(self);
                };
            }
            public _AssetXML(OwnerClass self)
            {
                WriteAction = () =>
                {
                    Language = CapybaraVS.Language.GetInstance.LanguageType;

                    var recentNode = CommandWindow.TreeViewCommand.GetRecent();
                    if (recentNode.Child.Count != 0)
                    {
                        // 最近使ったスクリプトノードを記録する

                        Recent = new List<string>();
                        foreach (var node in recentNode.Child)
                        {
                            Recent.Add(node.Name);
                        }
                    }

                    BackGroundImagePath = BaseWorkCanvas.BackGrountImagePath;
                };
            }
            #region 固有定義
            public string Language { get; set; } = "ja-JP";
            public List<string> Recent { get; set; } = null;
            public string BackGroundImagePath { get; set; } = null;
            #endregion
        }
        public _AssetXML<App> AssetXML { get; set; } = null;
        #endregion

        string APP_INFO_NAME = @"\app.xml";
        public static string ErrorLog = "";
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
            APP_INFO_NAME = CurrentAppDir + APP_INFO_NAME;

            // ツールチップの表示時間を設定
            System.Windows.Controls.ToolTipService.ShowDurationProperty.OverrideMetadata(
                typeof(DependencyObject),
                new FrameworkPropertyMetadata(4000));

            AssetXML = new _AssetXML<App>(this);
            if (!File.Exists(APP_INFO_NAME))
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
        public static string CurrentAppDir => System.AppDomain.CurrentDomain.BaseDirectory;

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
                StreamWriter swriter = new StreamWriter(APP_INFO_NAME, false);
                swriter.WriteLine(writer.ToString());
                swriter.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public void LoadAppInfo()
        {
            StreamReader reader = new StreamReader(APP_INFO_NAME);
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
