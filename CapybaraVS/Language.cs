using CapybaraVS.Script;
using CapyCSS.Controls;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading;
using System.Windows;
using System.Diagnostics;
using CapyCSS;

namespace CapybaraVS
{
    class Language : IDisposable
    {
        static Language language = null;
        private const string ext = ".htxt";
        private const string us = "en-US";
        private string languageType = us;

        /// <summary>
        /// ヘルプファイルクラスです。
        /// </summary>
        class HText
        {
            private const char sepalater = '=';
            private ReaderWriterLock rwLock = new ReaderWriterLock();
            private bool isCreate = false;
            public bool IsCreate
            {
                get => isCreate;
                set
                {
                    isCreate = isCreate || value;
                }
            }
            private string path = null;
            public string Path => path;
            private StringBuilder data;
            private IDictionary<string, string> keyValuePairs = new Dictionary<string, string>();
            public string Data => data.ToString();
            public HText(string path, bool create)
            {
                try
                {
                    rwLock.AcquireWriterLock(Timeout.Infinite);
                    IsCreate = create;
                    this.path = path;
                    if (File.Exists(Path))
                    {
                        data = new StringBuilder(File.ReadAllText(Path, Encoding.Unicode));
                        foreach (var line in Data.Split(Environment.NewLine))
                        {
                            if (!line.Contains(sepalater))
                                continue;

                            int pos = line.IndexOf(sepalater);
                            if (pos == 0)
                                continue;
                            var key = line.Substring(0, pos).Trim();
                            if (keyValuePairs.ContainsKey(key))
                            {
                                continue;
                            }
                            if (pos + 1 == line.Length)
                            {
                                keyValuePairs.Add(key, "");
                            }
                            else
                            {
                                int startPos = pos + 1;
                                keyValuePairs.Add(key, line.Substring(startPos, line.Length - startPos).Trim());
                            }
                        }
                    }
                    else
                    {
                        data = new StringBuilder();
                    }
                }
                finally
                {
                    rwLock.ReleaseWriterLock();
                }
            }
            public string Search(string tag, bool append)
            {
                try
                {
                    append = append && IsCreate;
                    if (append)
                        rwLock.AcquireWriterLock(Timeout.Infinite);
                    else
                        rwLock.AcquireReaderLock(Timeout.Infinite);

                    if (keyValuePairs.TryGetValue(tag, out string value))
                    {
                        return value;
                    }
                    if (append)
                    {
                        keyValuePairs.Add(tag, "");
                        data.Append($"{tag}{sepalater}" + Environment.NewLine);
                    }
                }
                finally
                {
                    if (append)
                        rwLock.ReleaseWriterLock();
                    else
                        rwLock.ReleaseReaderLock();
                }
                return null;
            }
            public void Save()
            {
                if (!IsCreate)
                    return;
                
                try
                {
                    rwLock.AcquireWriterLock(Timeout.Infinite);
                    File.WriteAllText(Path, Data, Encoding.Unicode);
                }
                finally
                {
                    rwLock.ReleaseWriterLock();
                }
            }
        }

        /// <summary>
        /// ヘルプファイルリストです。
        /// </summary>
        private static ICollection<HText> hTexts = new List<HText>();
        private static ReaderWriterLock hTextsRwLock = new ReaderWriterLock();

        /// <summary>
        /// ヘルプファイルを参照します。
        /// </summary>
        /// <param name="path">ヘルプファイルのパス</param>
        /// <returns>ヘルプファイル</returns>
        private static HText GetHText(string path, bool create)
        {
            HText htext;
            try
            {
                hTextsRwLock.AcquireWriterLock(Timeout.Infinite);

                if (hTexts.Any(n => n.Path == path))
                {
                    htext = hTexts.First(n => n.Path == path);
                    htext.IsCreate = create;
                }
                else
                {
                    htext = new HText(path, create);
                    hTexts.Add(htext);
                }
            }
            finally
            {
                hTextsRwLock.ReleaseWriterLock();
            }
            return htext;
        }
        
        public string LanguageType
        {
            get => languageType;
            set
            {
                languageType = value;
                cultureInfo = new CultureInfo(value);
            }
        }

        public static Language Instance
        {
            get
            {
                language ??= new Language();
                return language;
            }
        }

        static ResourceManager rm = null;
        static CultureInfo cultureInfo = null;
        public static CultureInfo Culture
        {
            get => cultureInfo;
            set
            {
                cultureInfo = value;
            }
        }

        static string hTextPath = null;
        static string exHTextPath = null;
        private bool disposedValue;

        public Language()
        {
            const string dir = "hText";
            hTextPath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), dir);
            exHTextPath = Path.Combine(CommandCanvasList.CAPYCSS_WORK_PATH, dir);
            if (!Directory.Exists(exHTextPath))
            {
                Directory.CreateDirectory(exHTextPath);
            }
            Assembly asm = Assembly.GetExecutingAssembly();
            string name = $"{asm.GetName().Name}.Resource";
            rm = new ResourceManager(name, asm);
        }

        ~Language()
        {
            Save();
        }

        /// <summary>
        /// ヘルプファイルを保存します。
        /// </summary>
        private static void Save()
        {
            foreach (var node in hTexts)
            {
                node.Save();
            }
            hTexts = new List<HText>();
        }

        private string _GetHelpText(string path, string tag, bool append = false)
        {
            HText hText = GetHText(path, append);
            return hText.Search(tag, append);
        }

        private string GetHelpText(string path, string tag, bool append = false)
        {
            string result = _GetHelpText($"{path}-{languageType}{ext}", tag, append);
            if (languageType != us)
            {
                _GetHelpText($"{path}-{languageType}{ext}", tag, append);
            }
            return result;
        }

        public string this[string index]
        {
            get 
            {
                string text = null;
                try
                {
                    string tag = index.Contains(':') ? index.Split(':')[1] : index;
                    tag = tag.Replace("=", "&#61;");
                    if (index.StartsWith(ApiImporter.BASE_LIB_TAG_PRE))
                    {
                        // 基本のヘルプファイルを参照する

                        string path = Path.Combine(hTextPath, ApiImporter.BASE_LIB_TAG_PRE.Substring(0, ApiImporter.BASE_LIB_TAG_PRE.Length - 1));
                        text = GetHelpText(path, tag, true);
                    }
                    else if (index.StartsWith(HelpWindow.HELP))
                    {
                        // 基本のヘルプファイルを参照する

                        string path = Path.Combine(hTextPath, HelpWindow.HELP.Substring(0, HelpWindow.HELP.Length - 1));
                        text = GetHelpText(path, tag, true);
                    }
                    else if (!index.StartsWith("SYSTEM_"))
                    {
                        string fileName = index.Split('.')[1].Split(':')[0].Replace('.', '_');
                        string path = Path.Combine(hTextPath, fileName);

                        // 基本のヘルプファイルを参照する
                        text = GetHelpText(path, tag);
                        if (text is null && exHTextPath != null)
                        {
                            path = Path.Combine(exHTextPath, fileName);

                            // 拡張のヘルプファイルを参照する
                            text = GetHelpText(path, tag);
                        }
                        if (text is null)
                        {
                            // 拡張にヘルプファイルを作成する

                            path = Path.Combine(exHTextPath, fileName);
                            GetHelpText(path, tag, true);
                        }
                    }
                    else
                    {
                        // リソースからヘルプテキストを参照する

                        text = rm.GetString(index, cultureInfo);
                    }
                    if (text is null || text == "")
                        return null;

                    text = text.Replace(@"<br>", Environment.NewLine);
                }
                catch (Exception ex)
                {
                    CommandCanvasList.ErrorLog += nameof(Language) + $".this[\"{index}\"]: " + ex.Message + Environment.NewLine + Environment.NewLine;
                }
                return text;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Save();
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
