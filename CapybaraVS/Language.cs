using CapyCSS.Controls;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Resources;
using System.Text;
using System.Windows;

namespace CapybaraVS
{
    class Language
    {
        static Language language = null;
        private string languageType = "en-US";
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

        public string this[string index]
        {
            get 
            {
                string text = null;
                try
                {
                    if (!index.StartsWith("SYSTE_"))
                    {
                        // ひとまず null を返す
                        // TODO テキストファイルからの参照にする

                        return null;
                    }

                    text = rm.GetString(index, cultureInfo);
                    if (text is null)
                        return "(none)";
                    text = text.Replace(@"\n", Environment.NewLine);
                    text = text.Replace("</r>", @"\r");
                    text = text.Replace("</n>", @"\n");
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
