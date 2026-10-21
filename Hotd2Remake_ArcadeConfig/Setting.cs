using System.Collections.Generic;
using System.Windows.Forms;

namespace Hotd2Remake_ArcadeConfig
{
    public enum SettingKind
    {
        Combo,
        Text,
        Check,
        Slider
    }

    //Left column (list) or right column (options, sliders) of a tab
    public enum SettingArea
    {
        Left,
        Right
    }

    /// <summary>
    /// One value of Hotd2Remake_ArcadePlugin.ini : list of choices, text box (key codes), check box or slider
    /// </summary>
    public class Setting
    {
        //Empty value in the INI file : the game's own setting (from the save) is kept
        public const string GAME_SETTING = "(GAME SETTING)";

        public string Tab { get; private set; }
        public string Section { get; private set; }
        public string Key { get; private set; }
        public string Label { get; private set; }
        public SettingKind Kind { get; private set; }
        public SettingArea Area { get; private set; }
        //Text displayed, value written in the INI file
        public List<KeyValuePair<string, string>> Choices { get; private set; }
        //Used when the value is missing from the INI file
        public string DefaultValue { get; private set; }

        //Sliders : unchecked = the game's own setting
        public int Min { get; private set; }
        public int Max { get; private set; }
        public int Step { get; private set; }
        public int GameDefault { get; private set; }
        public string Unit { get; private set; }

        public Control Control { get; set; }
        public CheckBox OverrideBox { get; set; }
        public TrackBar Slider { get; set; }
        public Label ValueLabel { get; set; }

        private Setting(string Tab, string Section, string Key, string Label, SettingKind Kind, string DefaultValue)
        {
            this.Tab = Tab;
            this.Section = Section;
            this.Key = Key;
            this.Label = Label;
            this.Kind = Kind;
            this.DefaultValue = DefaultValue;
            Area = SettingArea.Left;
            Choices = new List<KeyValuePair<string, string>>();
        }

        public Setting InArea(SettingArea Area)
        {
            this.Area = Area;
            return this;
        }

        public static Setting Text(string Tab, string Section, string Key, string Label, string DefaultValue)
        {
            return new Setting(Tab, Section, Key, Label, SettingKind.Text, DefaultValue);
        }

        public static Setting Check(string Tab, string Section, string Key, string Label, bool DefaultValue)
        {
            return new Setting(Tab, Section, Key, Label, SettingKind.Check, DefaultValue ? "1" : "0").InArea(SettingArea.Right);
        }

        public static Setting OnOff(string Tab, string Section, string Key, string Label, bool DefaultValue)
        {
            Setting s = new Setting(Tab, Section, Key, Label, SettingKind.Combo, DefaultValue ? "1" : "0");
            s.Choices.Add(new KeyValuePair<string, string>("OFF", "0"));
            s.Choices.Add(new KeyValuePair<string, string>("ON", "1"));
            return s;
        }

        /// <summary>
        /// Choices as text / value pairs
        /// </summary>
        public static Setting List(string Tab, string Section, string Key, string Label, string DefaultValue, params string[] TextValuePairs)
        {
            Setting s = new Setting(Tab, Section, Key, Label, SettingKind.Combo, DefaultValue);
            for (int i = 0; i + 1 < TextValuePairs.Length; i += 2)
                s.Choices.Add(new KeyValuePair<string, string>(TextValuePairs[i], TextValuePairs[i + 1]));
            return s;
        }

        /// <summary>
        /// Game setting replaced by the plugin : the first choice keeps the game's own one
        /// </summary>
        public static Setting Optional(string Tab, string Section, string Key, string Label, params string[] TextValuePairs)
        {
            Setting s = List(Tab, Section, Key, Label, string.Empty, TextValuePairs);
            s.Choices.Insert(0, new KeyValuePair<string, string>(GAME_SETTING, string.Empty));
            return s;
        }

        /// <summary>
        /// Numeric choices from Min to Max, optional or not
        /// </summary>
        public static Setting Range(string Tab, string Section, string Key, string Label, int Min, int Max, bool IsOptional, string DefaultValue = "")
        {
            Setting s = IsOptional ? Optional(Tab, Section, Key, Label) : List(Tab, Section, Key, Label, DefaultValue);
            for (int i = Min; i <= Max; i++)
                s.Choices.Add(new KeyValuePair<string, string>(i.ToString(), i.ToString()));
            return s;
        }

        /// <summary>
        /// Game setting replaced by the plugin, if checked
        /// </summary>
        public static Setting OptionalSlider(string Tab, string Section, string Key, string Label, int Min, int Max, int Step, int GameDefault, string Unit)
        {
            Setting s = new Setting(Tab, Section, Key, Label, SettingKind.Slider, string.Empty);
            s.Min = Min;
            s.Max = Max;
            s.Step = Step;
            s.GameDefault = GameDefault;
            s.Unit = Unit;
            return s.InArea(SettingArea.Right);
        }
    }
}
