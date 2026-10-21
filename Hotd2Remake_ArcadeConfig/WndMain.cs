using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Hotd2Remake_ArcadeConfig
{
    /// <summary>
    /// Settings of the House of the Dead 2 Remake Arcade Plugin, like argonlefou's ArcadeMod_Config for the first game.
    /// To be copied in the game folder, next to the game's exe : it is editing BepInEx\plugins\Hotd2Remake_ArcadePlugin.ini
    /// </summary>
    public class WndMain : Form
    {
        #region WIN32

        [DllImport("user32.dll")]
        private static extern bool EnumDisplaySettings(string deviceName, int modeNum, ref DEVMODE devMode);

        [StructLayout(LayoutKind.Sequential)]
        private struct DEVMODE
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x20)]
            public string dmDeviceName;
            public short dmSpecVersion;
            public short dmDriverVersion;
            public short dmSize;
            public short dmDriverExtra;
            public int dmFields;
            public int dmPositionX;
            public int dmPositionY;
            public int dmDisplayOrientation;
            public int dmDisplayFixedOutput;
            public short dmColor;
            public short dmDuplex;
            public short dmYResolution;
            public short dmTTOption;
            public short dmCollate;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x20)]
            public string dmFormName;
            public short dmLogPixels;
            public int dmBitsPerPel;
            public int dmPelsWidth;
            public int dmPelsHeight;
            public int dmDisplayFlags;
            public int dmDisplayFrequency;
            public int dmICMMethod;
            public int dmICMIntent;
            public int dmMediaType;
            public int dmDitherType;
            public int dmReserved1;
            public int dmReserved2;
            public int dmPanningWidth;
            public int dmPanningHeight;
        }

        #endregion

        private const string INI_FILE_NAME = "Hotd2Remake_ArcadePlugin.ini";
        private const string TAB_ARCADE = "Coins and keys";
        private const string TAB_GAME = "Game";
        private const string TAB_VIDEO = "Video";
        private const string TAB_SOUND = "Sound";

        private IniFile _Ini;
        private List<Setting> _Settings = new List<Setting>();

        public WndMain()
        {
            //Sizes are given for 96 DPI and scaled with S(), the font size in points is already right
            AutoScaleMode = AutoScaleMode.None;
            using (Graphics g = CreateGraphics())
                _Scale = g.DpiX / 96f;
            Text = "House of the Dead 2 Remake - Arcade Plugin settings";
            Font = new Font("Segoe UI", 9f);
            ClientSize = new Size(S(560), S(560));
            FormBorderStyle = FormBorderStyle.Sizable;
            StartPosition = FormStartPosition.CenterScreen;

            _Ini = new IniFile(findIniFile());
            createSettings();
            createControls();
            readSettings();
            //Can be made bigger, not smaller than the space needed for all the settings
            MinimumSize = Size;
        }

        private float _Scale = 1f;

        private int S(int Size)
        {
            return (int)Math.Round(Size * _Scale);
        }

        /// <summary>
        /// Next to the game's exe : BepInEx\plugins\Hotd2Remake_ArcadePlugin.ini. Or in the same folder, if copied in the plugins folder
        /// </summary>
        private static string findIniFile()
        {
            string Folder = AppDomain.CurrentDomain.BaseDirectory;
            string InPluginsFolder = Path.Combine(Path.Combine(Path.Combine(Folder, "BepInEx"), "plugins"), INI_FILE_NAME);
            string InSameFolder = Path.Combine(Folder, INI_FILE_NAME);
            if (!File.Exists(InPluginsFolder) && File.Exists(InSameFolder))
                return InSameFolder;
            return InPluginsFolder;
        }

        private void createSettings()
        {
            string[] Credits = new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9" };
            _Settings.Add(Setting.YesNo(TAB_ARCADE, "CREDITS", "FREEPLAY", "Free play (no coins)", false));
            _Settings.Add(Setting.List(TAB_ARCADE, "CREDITS", "CREDITS_TO_START", "Credits to start or join", "1", Credits));
            _Settings.Add(Setting.List(TAB_ARCADE, "CREDITS", "CREDITS_TO_CONTINUE", "Credits to continue", "1", Credits));
            _Settings.Add(Setting.Text(TAB_ARCADE, "INPUT_KEYS", "P1_START", "P1 START key", "49"));
            _Settings.Add(Setting.Text(TAB_ARCADE, "INPUT_KEYS", "P2_START", "P2 START key", "50"));
            _Settings.Add(Setting.Text(TAB_ARCADE, "INPUT_KEYS", "P1_COIN", "P1 COIN key", "53"));
            _Settings.Add(Setting.Text(TAB_ARCADE, "INPUT_KEYS", "P2_COIN", "P2 COIN key", "54"));
            _Settings.Add(Setting.Text(TAB_ARCADE, "INPUT_KEYS", "EXIT", "Quit key (0 = none)", "27"));

            _Settings.Add(Setting.List(TAB_GAME, "GAME", "MODE", "Game mode", "Original", "Original", "Arcade", "Horde"));
            _Settings.Add(Setting.List(TAB_GAME, "GAME", "DIFFICULTY", "Difficulty", "Normal", "VeryEasy", "Easy", "Normal", "Hard", "VeryHard"));
            _Settings.Add(Setting.List(TAB_GAME, "GAME", "SCORE_MODE", "Score mode", "Classic", "Classic", "Modern"));
            _Settings.Add(Setting.OptionalRange(TAB_GAME, "GAME", "LIVES", "Life torches", 1, 6, 1));
            _Settings.Add(Setting.Optional(TAB_GAME, "GAME", "LANGUAGE", "Language",
                "English", "English", "French", "French", "German", "German", "Italian", "Italian", "Spanish", "Spanish",
                "Portuguese", "Portuguese", "Polish", "Polish", "Japanese", "Japanese", "Chinese", "Chinese"));
            _Settings.Add(Setting.YesNo(TAB_GAME, "GAME", "ARCADE_HUD", "No score and weapon name in the HUD", true));
            _Settings.Add(Setting.YesNo(TAB_GAME, "GAME", "DISABLE_MAIN_MENU", "Title screen only reacts to START", true));
            _Settings.Add(Setting.YesNo(TAB_GAME, "GAME", "HIDE_CURSOR", "Hide the mouse cursor", true));
            _Settings.Add(Setting.YesNo(TAB_GAME, "GAME", "SAVE_IN_GAME_FOLDER", "Separate save (NVRAM folder)", true));

            Setting Resolution = Setting.Optional(TAB_VIDEO, "VIDEO", "RESOLUTION", "Resolution");
            foreach (string r in getScreenResolutions())
                Resolution.Choices.Add(new KeyValuePair<string, string>(r, r));
            _Settings.Add(Resolution);
            _Settings.Add(Setting.Optional(TAB_VIDEO, "VIDEO", "FULLSCREEN_MODE", "Display mode",
                "Exclusive fullscreen", "ExclusiveFullScreen", "Fullscreen window", "FullScreenWindow", "Maximized window", "MaximizedWindow", "Windowed", "Windowed"));
            _Settings.Add(Setting.Optional(TAB_VIDEO, "VIDEO", "VSYNC", "V-Sync", "Off", "Off", "Every V-Blank", "EveryVBlank", "Every second V-Blank", "EverySecondVBlank"));
            _Settings.Add(Setting.Optional(TAB_VIDEO, "VIDEO", "ANTI_ALIASING", "Anti-aliasing", "Off", "Off", "SMAA", "SMAA", "TAA", "TAA"));
            _Settings.Add(Setting.Optional(TAB_VIDEO, "VIDEO", "SHADOW_QUALITY", "Shadows", "Low", "Low", "Medium", "Medium", "High", "High"));
            _Settings.Add(Setting.Optional(TAB_VIDEO, "VIDEO", "BLOOM", "Bloom", "On", "1", "Off", "0"));
            _Settings.Add(Setting.Optional(TAB_VIDEO, "VIDEO", "MOTION_BLUR", "Motion blur", "On", "1", "Off", "0"));
            _Settings.Add(Setting.Optional(TAB_VIDEO, "VIDEO", "AMBIENT_OCCLUSION", "Ambient occlusion", "On", "1", "Off", "0"));
            _Settings.Add(Setting.OptionalRange(TAB_VIDEO, "VIDEO", "BRIGHTNESS", "Brightness (game : 50)", 0, 100, 5));
            _Settings.Add(Setting.OptionalRange(TAB_VIDEO, "VIDEO", "HUD_SCALE", "HUD scale", 50, 150, 10, " %"));

            _Settings.Add(Setting.OptionalRange(TAB_SOUND, "SOUND", "MASTER_VOLUME", "Master volume", 0, 100, 10, " %"));
            _Settings.Add(Setting.OptionalRange(TAB_SOUND, "SOUND", "MUSIC_VOLUME", "Music volume", 0, 100, 10, " %"));
            _Settings.Add(Setting.OptionalRange(TAB_SOUND, "SOUND", "EFFECTS_VOLUME", "Effects volume", 0, 100, 10, " %"));
            _Settings.Add(Setting.OptionalRange(TAB_SOUND, "SOUND", "UI_VOLUME", "Menus volume", 0, 100, 10, " %"));
            _Settings.Add(Setting.OptionalRange(TAB_SOUND, "SOUND", "DIALOGS_VOLUME", "Dialogs volume", 0, 100, 10, " %"));
            _Settings.Add(Setting.OptionalRange(TAB_SOUND, "SOUND", "SHOT_VOLUME", "Gun shots volume", 0, 100, 10, " %"));
            _Settings.Add(Setting.Optional(TAB_SOUND, "SOUND", "CLASSIC_MUSIC", "Music", "Original House of the Dead 2", "1", "Remake", "0"));
            _Settings.Add(Setting.YesNo(TAB_SOUND, "SOUND", "ATTRACT_SOUND", "Sound on the title screen", true));
        }

        private void createControls()
        {
            TabControl Tabs = new TabControl();
            Tabs.Dock = DockStyle.Fill;
            foreach (string TabName in new string[] { TAB_ARCADE, TAB_GAME, TAB_VIDEO, TAB_SOUND })
            {
                TabPage Page = new TabPage(TabName);
                Page.Padding = new Padding(S(12));
                TableLayoutPanel Table = new TableLayoutPanel();
                Table.Dock = DockStyle.Fill;
                Table.ColumnCount = 2;
                Table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, S(230)));
                Table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));

                foreach (Setting s in _Settings)
                {
                    if (s.Tab != TabName)
                        continue;
                    Label SettingLabel = new Label();
                    SettingLabel.Text = s.Label;
                    SettingLabel.AutoSize = true;
                    SettingLabel.Anchor = AnchorStyles.Left;
                    if (s.IsText)
                    {
                        TextBox Box = new TextBox();
                        Box.Width = S(80);
                        s.Control = Box;
                    }
                    else
                    {
                        ComboBox Box = new ComboBox();
                        Box.DropDownStyle = ComboBoxStyle.DropDownList;
                        Box.Width = S(260);
                        //Wider with the window
                        Box.Anchor = AnchorStyles.Left | AnchorStyles.Right;
                        foreach (KeyValuePair<string, string> Choice in s.Choices)
                            Box.Items.Add(Choice.Key);
                        s.Control = Box;
                    }
                    Table.Controls.Add(SettingLabel);
                    Table.Controls.Add(s.Control);
                }

                Label Help = new Label();
                Help.AutoSize = true;
                Help.MaximumSize = new Size(S(480), 0);
                Help.Margin = new Padding(S(3), S(12), S(3), S(3));
                if (TabName == TAB_ARCADE)
                    Help.Text = "Keys are Unity KeyCode numbers : 49 = 1, 50 = 2, 53 = 5, 54 = 6, 27 = Esc.\r\nThe guns, their recoil and the crosshairs are handled by DemulShooter.";
                else
                    Help.Text = Setting.GAME_SETTING + " : the game keeps its own setting, from its save file.";
                Table.Controls.Add(Help);
                Table.SetColumnSpan(Help, 2);

                Page.Controls.Add(Table);
                Tabs.TabPages.Add(Page);
            }

            Panel Bottom = new Panel();
            Bottom.Dock = DockStyle.Bottom;
            //Final width before adding the anchored controls, their distances to the borders are computed when they are added
            Bottom.Size = new Size(ClientSize.Width, S(64));
            Label FileLabel = new Label();
            FileLabel.Text = _Ini.FilePath;
            FileLabel.AutoEllipsis = true;
            FileLabel.SetBounds(S(12), S(6), S(536), S(20));
            FileLabel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            Button SaveButton = new Button();
            SaveButton.Text = "Save";
            SaveButton.SetBounds(S(448), S(30), S(100), S(28));
            SaveButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            SaveButton.Click += SaveButton_Click;
            Bottom.Controls.Add(FileLabel);
            Bottom.Controls.Add(SaveButton);

            Controls.Add(Bottom);
            Controls.Add(Tabs);
            Tabs.BringToFront();
        }

        private void readSettings()
        {
            bool IsFileFound = File.Exists(_Ini.FilePath);
            if (!IsFileFound)
                MessageBox.Show(_Ini.FilePath + " not found.\r\nIt will be created when saving, copy this tool in the game folder (next to the game's exe) to use the plugin's one.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);

            foreach (Setting s in _Settings)
            {
                string Value = IsFileFound ? _Ini.Read(s.Section, s.Key) : string.Empty;
                if (Value.Length == 0)
                    Value = s.DefaultValue;

                if (s.IsText)
                {
                    ((TextBox)s.Control).Text = Value;
                    continue;
                }

                ComboBox Box = (ComboBox)s.Control;
                int Index = s.Choices.FindIndex(c => string.Equals(c.Value, Value, StringComparison.OrdinalIgnoreCase));
                if (Index < 0)
                {
                    //Value written by hand, not in the list
                    s.Choices.Add(new KeyValuePair<string, string>(Value, Value));
                    Box.Items.Add(Value);
                    Index = s.Choices.Count - 1;
                }
                Box.SelectedIndex = Index;
            }
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            foreach (Setting s in _Settings)
            {
                int KeyCode;
                if (s.IsText && (!int.TryParse(((TextBox)s.Control).Text.Trim(), out KeyCode) || KeyCode < 0 || KeyCode > 1000))
                {
                    MessageBox.Show(s.Label + " : invalid key code (0-1000)", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            try
            {
                string Folder = Path.GetDirectoryName(_Ini.FilePath);
                if (!Directory.Exists(Folder))
                    Directory.CreateDirectory(Folder);
                foreach (Setting s in _Settings)
                {
                    string Value = s.IsText ? ((TextBox)s.Control).Text.Trim() : s.Choices[((ComboBox)s.Control).SelectedIndex].Value;
                    _Ini.Write(s.Section, s.Key, Value);
                }
                MessageBox.Show("Settings saved. They are used the next time the game is started.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception Ex)
            {
                MessageBox.Show("Error writing " + _Ini.FilePath + " :\r\n" + Ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Resolutions of the main screen, biggest first
        /// </summary>
        private static List<string> getScreenResolutions()
        {
            List<Size> Sizes = new List<Size>();
            DEVMODE Mode = new DEVMODE();
            Mode.dmSize = (short)Marshal.SizeOf(typeof(DEVMODE));
            for (int i = 0; EnumDisplaySettings(null, i, ref Mode); i++)
            {
                Size s = new Size(Mode.dmPelsWidth, Mode.dmPelsHeight);
                if (!Sizes.Contains(s))
                    Sizes.Add(s);
            }
            Sizes.Sort((a, b) => (b.Width * b.Height).CompareTo(a.Width * a.Height));
            return Sizes.ConvertAll(s => s.Width + "x" + s.Height);
        }
    }
}
