using CapybaraVS.Script;
using CapyCSS.Controls;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Windows;

namespace CapybaraVS
{
    class Language
    {
        static Language language = null;
        private const string ext = ".htxt";
        private const string us = "en-US";
        private string languageType = us;
        
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

        static System.Resources.ResourceManager rm = null;
        static CultureInfo cultureInfo = null;
        public static CultureInfo Culture
        {
            get => cultureInfo;
            set
            {
                cultureInfo = value;
            }
        }

        public Language()
        {
            System.Reflection.Assembly asm = System.Reflection.Assembly.GetExecutingAssembly();
            string name = $"{asm.GetName().Name}.Resource";
            rm = new System.Resources.ResourceManager(name, asm);
        }

        private string _GetHelpText(string path, string tag, bool append = false)
        {
            const char sepalater = '=';
            if (!File.Exists(path))
            {
                // ヘルプファイルが存在しない

                if (append)
                    File.AppendAllText(path, $"{tag}{sepalater}" + Environment.NewLine);
                return null;
            }
            using (StreamReader sr = new StreamReader(path))
            {
                string text = sr.ReadToEnd();
                var lines = text.Split(Environment.NewLine);
                foreach (var line in lines)
                {
                    if (!line.Contains(sepalater))
                        continue;

                    int pos = line.IndexOf(sepalater);
                    if (pos == 0)
                        continue;
                    var key = line.Substring(0, pos);
                    if (key.Trim() == tag)
                    {
                        if (pos + 1 == line.Length)
                            return "";

                        return line.Substring(pos + 1, line.Length).Trim();
                    }
                }
            }
            if (append)
                File.AppendAllText(path, $"{tag}{sepalater}" + Environment.NewLine);
            return null;
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

        private bool ExistsHelpText(string path)
        {
            return File.Exists($"{path}-{languageType}{ext}");
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

                        string path = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "BaseLib");
                        text = GetHelpText(path, tag);
                    }
                    else if (!index.StartsWith("SYSTEM_"))
                    {
                        string fileName = index.Split('.')[1].Split(':')[0].Replace('.', '_');
                        string path = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), fileName);

                        if (ExistsHelpText(path))
                        {
                            // 基本のヘルプファイルを参照する

                            text = GetHelpText(path, tag);
                        }
                        if (text is null && CommandCanvasList.CAPYCSS_WORK_PATH != null)
                        {
                            path = Path.Combine(CommandCanvasList.CAPYCSS_WORK_PATH, fileName);
                            if (ExistsHelpText(path))
                            {
                                // 拡張のヘルプファイルを参照する

                                text = GetHelpText(path, tag);
                            }
                        }
                        if (text is null)
                        {
                            // 拡張にヘルプファイルを作成する

                            GetHelpText(CommandCanvasList.CAPYCSS_WORK_PATH, tag, true);
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
    }
}
