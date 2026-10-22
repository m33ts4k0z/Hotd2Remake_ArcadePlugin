using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Hotd2Remake_ArcadeConfig
{
    /// <summary>
    /// Settings of the House of the Dead 2 Remake Arcade Plugin, looking like argonlefou's ArcadeMod_Config for the first game.
    /// To be copied in the game folder, next to the game's exe : it is editing BepInEx\plugins\Hotd2Remake_ArcadePlugin.ini
    /// </summary>
    public class WndMain : Form
    {
        #region WIN32

        [DllImport("user32.dll")]
        private static extern bool EnumDisplaySettings(string deviceName, int modeNum, ref DEVMODE devMode);
        private const int ENUM_CURRENT_SETTINGS = -1;

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
        private const string GAME_EXE_NAME = "THE HOUSE OF THE DEAD 2 Remake.exe";
        private const string TAB_ARCADE = "Arcade Menu";
        private const string TAB_DISPLAY = "Display Settings";
        private const string TAB_SOUND = "Sound Option";
        private const string TAB_CONTROLS = "Controls";

        private IniFile _Ini;
        private List<Setting> _Settings = new List<Setting>();
        //Mod Options boxes and their check boxes, sized once the fonts are known
        private Dictionary<GroupBox, TableLayoutPanel> _GroupBoxes = new Dictionary<GroupBox, TableLayoutPanel>();

        public WndMain()
        {
            //Sizes are given for 96 DPI and scaled with S(), the font size in points is already right
            AutoScaleMode = AutoScaleMode.None;
            using (Graphics g = CreateGraphics())
                _Scale = g.DpiX / 96f;
            Text = "House Of The Dead 2 Remake - Arcade Configurator";
            Font = new Font("Segoe UI", 9f);
            ClientSize = new Size(S(640), S(480));
            Padding = new Padding(S(12), 0, S(12), S(12));
            FormBorderStyle = FormBorderStyle.Sizable;
            StartPosition = FormStartPosition.CenterScreen;
            setGameIcon();

            _Ini = new IniFile(findIniFile());
            createSettings();
            createControls();
            readSettings();
            //Can be made bigger, not smaller than the space needed for all the settings
            MinimumSize = Size;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            foreach (KeyValuePair<GroupBox, TableLayoutPanel> Box in _GroupBoxes)
            {
                //Title and borders, then the check boxes
                int Content = Box.Value.GetPreferredSize(new Size(Box.Key.DisplayRectangle.Width, 0)).Height;
                Box.Key.Height += Content - Box.Key.DisplayRectangle.Height;
            }
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

        /// <summary>
        /// The game's icon, when the tool is in the game folder
        /// </summary>
        private void setGameIcon()
        {
            string GameExe = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, GAME_EXE_NAME);
            try
            {
                if (File.Exists(GameExe))
                    Icon = Icon.ExtractAssociatedIcon(GameExe);
            }
            catch (Exception)
            {
                //Default icon
            }
        }

        private void createSettings()
        {
            _Settings.Add(Setting.OnOff(TAB_ARCADE, "CREDITS", "FREEPLAY", "Freeplay", false));
            _Settings.Add(Setting.Range(TAB_ARCADE, "CREDITS", "CREDITS_TO_START", "Credits To Start", 1, 9, false, "1"));
            _Settings.Add(Setting.Range(TAB_ARCADE, "CREDITS", "CREDITS_TO_CONTINUE", "Credits To Continue", 1, 9, false, "1"));
            _Settings.Add(Setting.List(TAB_ARCADE, "GAME", "MODE", "Game Mode", "Original", "ORIGINAL", "Original", "ARCADE", "Arcade"));
            _Settings.Add(Setting.List(TAB_ARCADE, "GAME", "DIFFICULTY", "Difficulty", "Normal",
                "VERY EASY", "VeryEasy", "EASY", "Easy", "NORMAL", "Normal", "HARD", "Hard", "VERY HARD", "VeryHard"));
            _Settings.Add(Setting.List(TAB_ARCADE, "GAME", "SCORE_MODE", "Score Mode", "Classic", "CLASSIC", "Classic", "MODERN", "Modern"));
            _Settings.Add(Setting.Range(TAB_ARCADE, "GAME", "LIVES", "Life Torches", 1, 6, true));
            _Settings.Add(Setting.Optional(TAB_ARCADE, "GAME", "LANGUAGE", "Language",
                "ENGLISH", "English", "FRENCH", "French", "GERMAN", "German", "ITALIAN", "Italian", "SPANISH", "Spanish",
                "PORTUGUESE", "Portuguese", "POLISH", "Polish", "JAPANESE", "Japanese", "CHINESE", "Chinese"));
            _Settings.Add(Setting.OnOff(TAB_ARCADE, "SOUND", "ATTRACT_SOUND", "Advertise Sound", true));
            _Settings.Add(Setting.Check(TAB_ARCADE, "GAME", "HIDE_CURSOR", "Hide Mouse Cursor", true));
            _Settings.Add(Setting.Check(TAB_ARCADE, "GAME", "ARCADE_HUD", "Arcade HUD (no score, no weapon)", true));
            _Settings.Add(Setting.Check(TAB_ARCADE, "GAME", "DISABLE_MAIN_MENU", "Disable Main Menu (START only)", true));
            _Settings.Add(Setting.Check(TAB_ARCADE, "GAME", "SAVE_IN_GAME_FOLDER", "Separate Save (NVRAM folder)", true));

            Setting Resolution = Setting.Optional(TAB_DISPLAY, "VIDEO", "RESOLUTION", "Resolution");
            Resolution.Choices.AddRange(getScreenResolutions());
            _Settings.Add(Resolution);
            _Settings.Add(Setting.Optional(TAB_DISPLAY, "VIDEO", "FULLSCREEN_MODE", "Screen Mode",
                "EXCLUSIVE FULLSCREEN", "ExclusiveFullScreen", "FULLSCREEN WINDOW", "FullScreenWindow", "MAXIMIZED WINDOW", "MaximizedWindow", "WINDOWED", "Windowed"));
            _Settings.Add(Setting.Optional(TAB_DISPLAY, "VIDEO", "SHADOW_QUALITY", "Shadow Quality", "LOW", "Low", "MEDIUM", "Medium", "HIGH", "High"));
            _Settings.Add(Setting.Optional(TAB_DISPLAY, "VIDEO", "ANTI_ALIASING", "Anti-aliasing", "OFF", "Off", "SMAA", "SMAA", "TAA", "TAA"));
            _Settings.Add(Setting.Optional(TAB_DISPLAY, "VIDEO", "BLOOM", "Bloom", "OFF", "0", "ON", "1"));
            _Settings.Add(Setting.Optional(TAB_DISPLAY, "VIDEO", "MOTION_BLUR", "Motion Blur", "OFF", "0", "ON", "1"));
            _Settings.Add(Setting.Optional(TAB_DISPLAY, "VIDEO", "AMBIENT_OCCLUSION", "Ambient Occlusion", "OFF", "0", "ON", "1"));
            _Settings.Add(Setting.Optional(TAB_DISPLAY, "VIDEO", "VSYNC", "VSync", "OFF", "Off", "EVERY V-BLANK", "EveryVBlank", "EVERY 2ND V-BLANK", "EverySecondVBlank"));
            _Settings.Add(Setting.OptionalSlider(TAB_DISPLAY, "VIDEO", "HUD_SCALE", "HUD Scale", 50, 150, 10, 100, " %"));
            _Settings.Add(Setting.OptionalSlider(TAB_DISPLAY, "VIDEO", "BRIGHTNESS", "Brightness", 0, 100, 10, 50, string.Empty));

            _Settings.Add(Setting.OptionalSlider(TAB_SOUND, "SOUND", "MASTER_VOLUME", "Master Volume", 0, 100, 10, 100, " %").InArea(SettingArea.Left));
            _Settings.Add(Setting.OptionalSlider(TAB_SOUND, "SOUND", "MUSIC_VOLUME", "Music Volume", 0, 100, 10, 100, " %").InArea(SettingArea.Left));
            _Settings.Add(Setting.OptionalSlider(TAB_SOUND, "SOUND", "EFFECTS_VOLUME", "Effects Volume", 0, 100, 10, 100, " %").InArea(SettingArea.Left));
            _Settings.Add(Setting.Optional(TAB_SOUND, "SOUND", "CLASSIC_MUSIC", "Soundtrack", "REMAKE", "0", "CLASSIC", "1"));
            _Settings.Add(Setting.OptionalSlider(TAB_SOUND, "SOUND", "UI_VOLUME", "UI Volume", 0, 100, 10, 100, " %"));
            _Settings.Add(Setting.OptionalSlider(TAB_SOUND, "SOUND", "DIALOGS_VOLUME", "Dialogs Volume", 0, 100, 10, 100, " %"));
            _Settings.Add(Setting.OptionalSlider(TAB_SOUND, "SOUND", "SHOT_VOLUME", "Shot Volume", 0, 100, 10, 100, " %"));

            _Settings.Add(Setting.Text(TAB_CONTROLS, "INPUT_KEYS", "P1_START", "P1 Start Key", "49"));
            _Settings.Add(Setting.Text(TAB_CONTROLS, "INPUT_KEYS", "P2_START", "P2 Start Key", "50"));
            _Settings.Add(Setting.Text(TAB_CONTROLS, "INPUT_KEYS", "P1_COIN", "P1 Coin Key", "53"));
            _Settings.Add(Setting.Text(TAB_CONTROLS, "INPUT_KEYS", "P2_COIN", "P2 Coin Key", "54"));
            _Settings.Add(Setting.Text(TAB_CONTROLS, "INPUT_KEYS", "EXIT", "Quit Key (0 = none)", "27"));
        }

        #region Layout

        private void createControls()
        {
            ToolTip Tips = new ToolTip();

            //Logo on the left, Save button on the right, like ArcadeMod_Config
            TableLayoutPanel Top = new TableLayoutPanel();
            Top.Dock = DockStyle.Top;
            Top.Height = S(146);
            Top.Padding = new Padding(0, S(12), 0, S(8));
            Top.ColumnCount = 2;
            Top.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, S(270)));
            Top.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            Top.RowCount = 2;
            Top.RowStyles.Add(new RowStyle(SizeType.Percent, 60f));
            Top.RowStyles.Add(new RowStyle(SizeType.Percent, 40f));

            PictureBox Logo = new PictureBox();
            Logo.Dock = DockStyle.Fill;
            Logo.Margin = new Padding(0);
            Logo.BorderStyle = BorderStyle.Fixed3D;
            Logo.BackColor = Color.Black;
            Logo.SizeMode = PictureBoxSizeMode.Zoom;
            Logo.Image = loadLogo();
            Top.Controls.Add(Logo, 0, 0);
            Top.SetRowSpan(Logo, 2);

            Button SaveButton = new Button();
            SaveButton.Text = "Save Config";
            SaveButton.Font = new Font(Font.FontFamily, 11f);
            SaveButton.Height = S(44);
            SaveButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            SaveButton.Margin = new Padding(S(18), 0, 0, S(4));
            SaveButton.Click += SaveButton_Click;
            Top.Controls.Add(SaveButton, 1, 0);

            Label FileLabel = new Label();
            FileLabel.Text = _Ini.FilePath;
            FileLabel.ForeColor = SystemColors.GrayText;
            FileLabel.Font = new Font(Font.FontFamily, 7.5f);
            FileLabel.AutoEllipsis = true;
            FileLabel.Height = S(34);
            FileLabel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            FileLabel.Margin = new Padding(S(18), S(4), 0, 0);
            Tips.SetToolTip(FileLabel, _Ini.FilePath);
            Top.Controls.Add(FileLabel, 1, 1);

            TabControl Tabs = new TabControl();
            Tabs.Dock = DockStyle.Fill;
            foreach (string TabName in new string[] { TAB_ARCADE, TAB_DISPLAY, TAB_SOUND, TAB_CONTROLS })
                Tabs.TabPages.Add(createTab(TabName));

            //Docked in the reverse order : the logo first, then the tabs in the remaining space
            Controls.Add(Tabs);
            Controls.Add(Top);
        }

        private Image loadLogo()
        {
            using (Stream Res = typeof(WndMain).Assembly.GetManifestResourceStream("Hotd2Remake_ArcadeConfig.logo.jpg"))
            {
                if (Res == null)
                    return null;
                //The stream must stay open with Image.FromStream, a copy doesn't need it
                using (Image Img = Image.FromStream(Res))
                    return new Bitmap(Img);
            }
        }

        /// <summary>
        /// Lists on the left, mod options and sliders on the right, help at the bottom
        /// </summary>
        private TabPage createTab(string TabName)
        {
            TabPage Page = new TabPage(TabName);
            //Same color as the sliders
            Page.BackColor = SystemColors.Control;
            Page.Padding = new Padding(S(10));

            TableLayoutPanel Columns = new TableLayoutPanel();
            Columns.Dock = DockStyle.Fill;
            Columns.ColumnCount = 2;
            Columns.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 56f));
            Columns.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 44f));
            Columns.RowCount = 2;
            Columns.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            Columns.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            TableLayoutPanel Left = createColumn(2);
            Left.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, S(125)));
            Left.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            Left.Margin = new Padding(0, 0, S(16), 0);
            TableLayoutPanel Right = createColumn(1);
            Right.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            Right.Margin = new Padding(0);

            GroupBox ModOptions = null;
            TableLayoutPanel Options = null;
            bool HasOptionalSetting = false;
            foreach (Setting s in _Settings)
            {
                if (s.Tab != TabName)
                    continue;
                HasOptionalSetting |= s.Kind == SettingKind.Slider || s.Choices.Exists(c => c.Key == Setting.GAME_SETTING);
                TableLayoutPanel Column = s.Area == SettingArea.Left ? Left : Right;

                if (s.Kind == SettingKind.Check)
                {
                    if (ModOptions == null)
                    {
                        ModOptions = new GroupBox();
                        ModOptions.Text = "Mod Options";
                        ModOptions.Anchor = AnchorStyles.Left | AnchorStyles.Right;
                        ModOptions.Padding = new Padding(S(10), S(6), S(6), S(6));
                        ModOptions.Margin = new Padding(0, 0, 0, S(8));
                        Options = createColumn(1);
                        _GroupBoxes.Add(ModOptions, Options);
                        Options.Dock = DockStyle.Fill;
                        Options.Margin = new Padding(0);
                        ModOptions.Controls.Add(Options);
                        addRow(Column, ModOptions);
                    }
                    CheckBox Box = new CheckBox();
                    Box.Text = s.Label;
                    Box.AutoSize = true;
                    s.Control = Box;
                    addRow(Options, Box);
                }
                else if (s.Kind == SettingKind.Slider)
                {
                    addRow(Column, createSlider(s));
                }
                else
                {
                    Label SettingLabel = new Label();
                    SettingLabel.Text = s.Label;
                    SettingLabel.AutoSize = true;
                    SettingLabel.Anchor = AnchorStyles.Left;
                    if (s.Kind == SettingKind.Text)
                    {
                        TextBox Box = new TextBox();
                        Box.Width = S(80);
                        Box.Anchor = AnchorStyles.Left;
                        s.Control = Box;
                    }
                    else
                    {
                        ComboBox Box = new ComboBox();
                        Box.DropDownStyle = ComboBoxStyle.DropDownList;
                        //Wider with the window
                        Box.Anchor = AnchorStyles.Left | AnchorStyles.Right;
                        foreach (KeyValuePair<string, string> Choice in s.Choices)
                        {
                            Box.Items.Add(Choice.Key);
                            //Whole texts in the list, even when the window is small
                            Box.DropDownWidth = Math.Max(Box.DropDownWidth, TextRenderer.MeasureText(Choice.Key, Font).Width + S(24));
                        }
                        s.Control = Box;
                    }
                    addRow(Column, SettingLabel, s.Control);
                }
            }
            //Remaining space at the bottom, the rows keep their size
            addRow(Left, createFiller(), null, true);
            addRow(Right, createFiller(), null, true);

            Label Help = new Label();
            Help.AutoSize = true;
            Help.ForeColor = SystemColors.GrayText;
            Help.Margin = new Padding(S(3), S(6), S(3), 0);
            if (TabName == TAB_CONTROLS)
                Help.Text = "Keys are Unity KeyCode numbers, keyboard keys only : 49 = 1, 50 = 2, 53 = 5, 54 = 6, 27 = Esc.\r\nThe guns, their recoil and the crosshairs are handled by DemulShooter.";
            else if (HasOptionalSetting)
                Help.Text = Setting.GAME_SETTING + " or unchecked slider : the game keeps its own setting.";

            Columns.Controls.Add(Left, 0, 0);
            Columns.Controls.Add(Right, 1, 0);
            Columns.Controls.Add(Help, 0, 1);
            Columns.SetColumnSpan(Help, 2);
            Page.Controls.Add(Columns);
            return Page;
        }

        private static Control createFiller()
        {
            Panel Filler = new Panel();
            Filler.Size = Size.Empty;
            Filler.Margin = new Padding(0);
            return Filler;
        }

        private TableLayoutPanel createColumn(int ColumnCount)
        {
            TableLayoutPanel Column = new TableLayoutPanel();
            Column.Dock = DockStyle.Fill;
            Column.ColumnCount = ColumnCount;
            Column.RowCount = 0;
            return Column;
        }

        /// <summary>
        /// Label and value, or one control on the whole row
        /// </summary>
        private void addRow(TableLayoutPanel Column, Control First, Control Second = null, bool IsLast = false)
        {
            int Row = Column.RowCount;
            Column.RowCount++;
            Column.RowStyles.Add(IsLast ? new RowStyle(SizeType.Percent, 100f) : new RowStyle(SizeType.AutoSize));
            Column.Controls.Add(First, 0, Row);
            if (Second != null)
                Column.Controls.Add(Second, 1, Row);
            else if (Column.ColumnCount > 1)
                Column.SetColumnSpan(First, Column.ColumnCount);
        }

        /// <summary>
        /// "Label :" check box (unchecked = game setting) and value, slider under them
        /// </summary>
        private Control createSlider(Setting s)
        {
            TableLayoutPanel Block = new TableLayoutPanel();
            Block.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            Block.Height = S(72);
            Block.Margin = new Padding(0, 0, 0, S(4));
            Block.ColumnCount = 2;
            Block.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            Block.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            Block.RowCount = 2;
            Block.RowStyles.Add(new RowStyle(SizeType.Absolute, S(24)));
            Block.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            s.OverrideBox = new CheckBox();
            s.OverrideBox.Text = s.Label + " :";
            s.OverrideBox.AutoSize = true;
            s.OverrideBox.Anchor = AnchorStyles.Left;
            s.ValueLabel = new Label();
            s.ValueLabel.AutoSize = true;
            s.ValueLabel.Anchor = AnchorStyles.Right;
            s.Slider = new TrackBar();
            s.Slider.AutoSize = false;
            s.Slider.Height = S(45);
            s.Slider.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            s.Slider.Minimum = s.Min;
            s.Slider.Maximum = s.Max;
            s.Slider.TickFrequency = s.Step;
            s.Slider.TickStyle = TickStyle.Both;
            s.Slider.SmallChange = 1;
            s.Slider.LargeChange = s.Step;
            s.Slider.Value = s.GameDefault;
            s.Control = s.Slider;

            s.OverrideBox.CheckedChanged += (sender, e) => updateSlider(s);
            s.Slider.ValueChanged += (sender, e) => updateSlider(s);

            Block.Controls.Add(s.OverrideBox, 0, 0);
            Block.Controls.Add(s.ValueLabel, 1, 0);
            Block.Controls.Add(s.Slider, 0, 1);
            Block.SetColumnSpan(s.Slider, 2);
            updateSlider(s);
            return Block;
        }

        private void updateSlider(Setting s)
        {
            s.Slider.Enabled = s.OverrideBox.Checked;
            s.ValueLabel.Text = s.OverrideBox.Checked ? s.Slider.Value + s.Unit : Setting.GAME_SETTING;
            s.ValueLabel.ForeColor = s.OverrideBox.Checked ? SystemColors.ControlText : SystemColors.GrayText;
        }

        #endregion

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

                switch (s.Kind)
                {
                    case SettingKind.Text:
                        ((TextBox)s.Control).Text = Value;
                        break;

                    case SettingKind.Check:
                        ((CheckBox)s.Control).Checked = Value == "1" || Value.Equals("true", StringComparison.OrdinalIgnoreCase);
                        break;

                    case SettingKind.Slider:
                        int Number;
                        bool IsSet = int.TryParse(Value, out Number);
                        s.Slider.Value = IsSet ? Math.Max(s.Min, Math.Min(s.Max, Number)) : s.GameDefault;
                        s.OverrideBox.Checked = IsSet;
                        updateSlider(s);
                        break;

                    default:
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
                        break;
                }
            }
        }

        private string getValue(Setting s)
        {
            switch (s.Kind)
            {
                case SettingKind.Text:
                    return ((TextBox)s.Control).Text.Trim();
                case SettingKind.Check:
                    return ((CheckBox)s.Control).Checked ? "1" : "0";
                case SettingKind.Slider:
                    return s.OverrideBox.Checked ? s.Slider.Value.ToString() : string.Empty;
                default:
                    return s.Choices[((ComboBox)s.Control).SelectedIndex].Value;
            }
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            foreach (Setting s in _Settings)
            {
                int KeyCode;
                if (s.Kind == SettingKind.Text && (!int.TryParse(getValue(s), out KeyCode) || KeyCode < 0 || KeyCode > 1000))
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
                    _Ini.Write(s.Section, s.Key, getValue(s));
                MessageBox.Show("Settings saved. They are used the next time the game is started.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception Ex)
            {
                MessageBox.Show("Error writing " + _Ini.FilePath + " :\r\n" + Ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Resolutions of the main screen, biggest first : "1920x1080 (16:9)" displayed, "1920x1080" written in the INI file
        /// The screen's own resolution is marked : a resolution with another aspect ratio has black bars in a fullscreen window
        /// </summary>
        private static List<KeyValuePair<string, string>> getScreenResolutions()
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

            Size ScreenSize = Size.Empty;
            if (EnumDisplaySettings(null, ENUM_CURRENT_SETTINGS, ref Mode))
                ScreenSize = new Size(Mode.dmPelsWidth, Mode.dmPelsHeight);

            return Sizes.ConvertAll(s => new KeyValuePair<string, string>(
                s.Width + "x" + s.Height + " (" + getAspectRatio(s) + (s == ScreenSize ? ", SCREEN" : "") + ")",
                s.Width + "x" + s.Height));
        }

        /// <summary>
        /// Usual names (1366x768 is 16:9 too), or the exact ratio
        /// </summary>
        private static string getAspectRatio(Size Resolution)
        {
            float Ratio = (float)Resolution.Width / Resolution.Height;
            string[] Names = new string[] { "16:9", "16:10", "4:3", "5:4", "3:2", "5:3", "17:9", "21:9", "32:9" };
            float[] Ratios = new float[] { 16f / 9f, 16f / 10f, 4f / 3f, 5f / 4f, 3f / 2f, 5f / 3f, 256f / 135f, 64f / 27f, 32f / 9f };
            for (int i = 0; i < Names.Length; i++)
            {
                if (Math.Abs(Ratio - Ratios[i]) < 0.02f)
                    return Names[i];
            }
            int a = Resolution.Width, b = Resolution.Height;
            while (b != 0)
            {
                int t = a % b;
                a = b;
                b = t;
            }
            return (Resolution.Width / a) + ":" + (Resolution.Height / a);
        }
    }
}
