using System.Collections.Generic;
using System.Windows.Forms;

namespace Hotd2Remake_ArcadeConfig
{
    /// <summary>
    /// One value of Hotd2Remake_ArcadePlugin.ini, displayed as a list of choices (or a text box for the key codes)
    /// </summary>
    public class Setting
    {
        //Empty value in the INI file : the game's own setting (from the save) is kept
        public const string GAME_SETTING = "(game setting)";

        public string Tab { get; private set; }
        public string Section { get; private set; }
        public string Key { get; private set; }
        public string Label { get; private set; }
        //Text displayed, value written in the INI file
        public List<KeyValuePair<string, string>> Choices { get; private set; }
        //Used when the value is missing from the INI file
        public string DefaultValue { get; private set; }
        public bool IsText { get; private set; }
        public Control Control { get; set; }

        private Setting(string Tab, string Section, string Key, string Label, string DefaultValue)
        {
            this.Tab = Tab;
            this.Section = Section;
            this.Key = Key;
            this.Label = Label;
            this.DefaultValue = DefaultValue;
            Choices = new List<KeyValuePair<string, string>>();
        }

        public static Setting Text(string Tab, string Section, string Key, string Label, string DefaultValue)
        {
            Setting s = new Setting(Tab, Section, Key, Label, DefaultValue);
            s.IsText = true;
            return s;
        }

        public static Setting YesNo(string Tab, string Section, string Key, string Label, bool DefaultValue)
        {
            Setting s = new Setting(Tab, Section, Key, Label, DefaultValue ? "1" : "0");
            s.Choices.Add(new KeyValuePair<string, string>("Yes", "1"));
            s.Choices.Add(new KeyValuePair<string, string>("No", "0"));
            return s;
        }

        /// <summary>
        /// Choices with the same text and value
        /// </summary>
        public static Setting List(string Tab, string Section, string Key, string Label, string DefaultValue, params string[] Values)
        {
            Setting s = new Setting(Tab, Section, Key, Label, DefaultValue);
            foreach (string Value in Values)
                s.Choices.Add(new KeyValuePair<string, string>(Value, Value));
            return s;
        }

        /// <summary>
        /// Game setting replaced by the plugin : the first choice keeps the game's own one
        /// </summary>
        public static Setting Optional(string Tab, string Section, string Key, string Label, params string[] TextValuePairs)
        {
            Setting s = new Setting(Tab, Section, Key, Label, string.Empty);
            s.Choices.Add(new KeyValuePair<string, string>(GAME_SETTING, string.Empty));
            for (int i = 0; i + 1 < TextValuePairs.Length; i += 2)
                s.Choices.Add(new KeyValuePair<string, string>(TextValuePairs[i], TextValuePairs[i + 1]));
            return s;
        }

        /// <summary>
        /// Optional numeric setting, from Min to Max
        /// </summary>
        public static Setting OptionalRange(string Tab, string Section, string Key, string Label, int Min, int Max, int Step, string Unit = "")
        {
            Setting s = Optional(Tab, Section, Key, Label);
            for (int i = Min; i <= Max; i += Step)
                s.Choices.Add(new KeyValuePair<string, string>(i.ToString() + Unit, i.ToString()));
            return s;
        }
    }
}
