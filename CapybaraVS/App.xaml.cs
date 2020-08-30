/*
Copyright © 2020 Katsumi Aradono. All rights reserved.
*/

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

                    // 次回の為の初期化
                    self.AssetXML = new _AssetXML<App>(self);
                };
            }
            public _AssetXML(OwnerClass self)
            {
                WriteAction = () =>
                {
                    Language = CapybaraVS.Language.GetInstance.LanguageType;
                };
            }
            #region 固有定義
            public string Language { get; set; } = "ja-JP";
            #endregion
        }
        public _AssetXML<App> AssetXML { get; set; } = null;
        #endregion

        const string APP_INFO_NAME = "./app.xml";
        public static string ErrorLog = "";
        public static string EntryLoadFile = null;  // スクリプトの起動後読み込み
        public static bool IsAutoExecute = false;   // スクリプトの自動実行
        public static bool IsAutoExit = false;      // スクリプトの自動実行後自動終了

        private void Application_StartUp(object sender, StartupEventArgs e)
        {
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
