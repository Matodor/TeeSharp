using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Teecsharp
{
    public struct TranslatedString
    {
        public string Or;
        public string Tr;
    }

    public class Language
    {
        private readonly Dictionary<string, TranslatedString> _translatedStrings;
        private readonly string _currentLanguage;

        public Language(string language)
        {
            _translatedStrings = new Dictionary<string, TranslatedString>();
            _currentLanguage = language;

            Load();
        }

        public string GetTranslatedString(string original)
        {
            if (_translatedStrings.ContainsKey(original))
                return _translatedStrings[original].Tr;
            return original;
        }

        private void Load()
        {
            if (File.Exists(GetPath()))
            {
                var textReader = (TextReader) File.OpenText(GetPath());
                var jsonReader = new JsonTextReader(textReader);
                dynamic obj = JsonSerializer.Create().Deserialize(jsonReader);

                for (int i = 0; i < obj["translated strings"].Count; i++)
                {
                    string tr = obj["translated strings"][i].tr;
                    string or = obj["translated strings"][i].or;

                    _translatedStrings.Add(or, new TranslatedString {Or = or, Tr = tr});
                }
            }
        }

        private string GetPath()
        {
            return Path.Combine(Environment.CurrentDirectory, Languages.LangDir, _currentLanguage + ".json");
        }
    }

    public class Languages
    {
        public static string LangDir = "langs";

        private readonly Dictionary<string, Language> _languages;

        public Languages()
        {
            _languages = new Dictionary<string, Language>();
        }

        public Language GetLanguage(string lang)
        {
            if (_languages.ContainsKey(lang))
                return _languages[lang];
            return null;
        }

        public void LoadLanguages()
        {
            if (Directory.Exists(Path.Combine(Environment.CurrentDirectory, LangDir)))
            {
                var langsFiles = Directory.GetFiles(Path.Combine(Environment.CurrentDirectory, LangDir), "*.json");
                if (langsFiles.Length > 0)
                {
                    foreach (var langPath in langsFiles)
                    {
                        var lang = Path.GetFileNameWithoutExtension(langPath);
                        if (!string.IsNullOrEmpty(lang))
                        {
                            var language = new Language(lang);
                            _languages.Add(lang, language);
                        }
                    }
                    
                    CSystem.dbg_msg_clr("languages", "found {0} language/s", ConsoleColor.Green, langsFiles.Length);
                }
                else
                    CSystem.dbg_msg_clr("languages", "languages not found", ConsoleColor.Red);
            }
            else
                CSystem.dbg_msg_clr("languages", "languages directory not found", ConsoleColor.Red);
        }
    }

    public class Localization
    {
        public string CurrentLang { get; set; }

        private readonly Languages _languages;

        public Localization(string defaultLang, Languages languages)
        {
            CurrentLang = defaultLang;

            _languages = languages;
        }

        public string Localize(string str)
        {
            if (_languages.GetLanguage(CurrentLang) != null)
                return _languages.GetLanguage(CurrentLang).GetTranslatedString(str);
            return str;
        }
    }
}
