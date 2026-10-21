using System;
using System.IO;
using MegaPixel.Menus;
using UnityEngine;

namespace Hotd2Remake_ArcadePlugin
{
    /// <summary>
    /// Plugin settings, read from Hotd2Remake_ArcadePlugin.ini in the BepInEx plugins folder (it can be edited with Hotd2Remake_ArcadeConfig.exe)
    /// Missing or invalid values keep their default value
    /// </summary>
    public class ConfManager
    {
        //Credits
        public bool Freeplay { get; private set; }
        public int CreditsToStart { get; private set; }
        public int CreditsToContinue { get; private set; }

        //Keys, as legacy Unity KeyCode values
        public int[] StartKeys { get; private set; }
        public int[] CoinKeys { get; private set; }
        public int ExitKey { get; private set; }

        //Game started with START from the title screen
        public CampaignMode Mode { get; private set; }
        public Difficulty GameDifficulty { get; private set; }
        public ScoreMode GameScoreMode { get; private set; }

        //Title screen : no access to the game's main menu, like an arcade cabinet
        public bool DisableMainMenu { get; private set; }

        //Separate save file in the game folder, so that the arcade games don't overwrite the home save
        public bool SaveInGameFolder { get; private set; }

        //Mouse cursor always hidden, like argonlefou's House of the Dead Remake Arcade Plugin
        public bool HideCursor { get; private set; }

        //No score and no weapon name during the game, like the original arcade
        public bool ArcadeHud { get; private set; }

        //Title screen with or without sound, like argonlefou's House of the Dead Remake Arcade Plugin
        public bool AttractSound { get; private set; }

        //Game settings, replacing the ones of the save (see ApplyGameSettings). Empty in the config file = the save's own value
        public int? Lives { get; private set; }
        public Language? GameLanguage { get; private set; }
        public bool? ClassicMusic { get; private set; }
        public int? ResolutionWidth { get; private set; }
        public int? ResolutionHeight { get; private set; }
        public FullScreenMode? FullscreenMode { get; private set; }
        public VSYNCMode? VSync { get; private set; }
        public SettingsAntiAliasing? AntiAliasing { get; private set; }
        public SettingsShadowQuality? ShadowQuality { get; private set; }
        public bool? Bloom { get; private set; }
        public bool? MotionBlur { get; private set; }
        public bool? AmbientOcclusion { get; private set; }
        public int? Brightness { get; private set; }
        public int? HudScale { get; private set; }
        public int? MasterVolume { get; private set; }
        public int? MusicVolume { get; private set; }
        public int? EffectsVolume { get; private set; }
        public int? UIVolume { get; private set; }
        public int? DialogsVolume { get; private set; }
        public int? ShotVolume { get; private set; }

        public ConfManager()
        {
            Freeplay = false;
            CreditsToStart = 1;
            CreditsToContinue = 1;
            StartKeys = new int[] { 49, 50 };   //1, 2
            CoinKeys = new int[] { 53, 54 };    //5, 6
            ExitKey = 27;                       //Escape, like argonlefou's House of the Dead Remake Arcade Plugin
            Mode = CampaignMode.Original;
            GameDifficulty = Difficulty.Normal;
            GameScoreMode = ScoreMode.Classic;
            DisableMainMenu = true;
            SaveInGameFolder = true;
            HideCursor = true;
            ArcadeHud = true;
            AttractSound = true;
        }

        public void ReadConf(string ConfigFilePath)
        {
            INIFile IniFile = new INIFile(ConfigFilePath);
            if (!File.Exists(IniFile.FInfo.FullName))
            {
                Hotd2r_ArcadePlugin.MyLogger.LogWarning("ConfManager.ReadConf() => " + ConfigFilePath + " not found, using default values");
                return;
            }

            Freeplay = ReadBool(IniFile, "CREDITS", "FREEPLAY", Freeplay);
            CreditsToStart = ReadInt(IniFile, "CREDITS", "CREDITS_TO_START", CreditsToStart, 1, 9);
            CreditsToContinue = ReadInt(IniFile, "CREDITS", "CREDITS_TO_CONTINUE", CreditsToContinue, 1, 9);

            for (int i = 0; i < Hotd2r_ArcadePlugin.MAX_PLAYERS; i++)
            {
                StartKeys[i] = ReadInt(IniFile, "INPUT_KEYS", "P" + (i + 1) + "_START", StartKeys[i], 0, 1000);
                CoinKeys[i] = ReadInt(IniFile, "INPUT_KEYS", "P" + (i + 1) + "_COIN", CoinKeys[i], 0, 1000);
            }
            ExitKey = ReadInt(IniFile, "INPUT_KEYS", "EXIT", ExitKey, 0, 1000);

            Mode = ReadEnum(IniFile, "GAME", "MODE", Mode);
            GameDifficulty = ReadEnum(IniFile, "GAME", "DIFFICULTY", GameDifficulty);
            GameScoreMode = ReadEnum(IniFile, "GAME", "SCORE_MODE", GameScoreMode);
            DisableMainMenu = ReadBool(IniFile, "GAME", "DISABLE_MAIN_MENU", DisableMainMenu);
            SaveInGameFolder = ReadBool(IniFile, "GAME", "SAVE_IN_GAME_FOLDER", SaveInGameFolder);
            HideCursor = ReadBool(IniFile, "GAME", "HIDE_CURSOR", HideCursor);
            ArcadeHud = ReadBool(IniFile, "GAME", "ARCADE_HUD", ArcadeHud);
            Lives = ReadOptionalInt(IniFile, "GAME", "LIVES", 1, 6);
            GameLanguage = ReadOptionalEnum<Language>(IniFile, "GAME", "LANGUAGE");

            string Resolution = IniFile.IniReadValue("VIDEO", "RESOLUTION").Trim();
            string[] ResolutionParts = Resolution.ToLower().Split('x');
            int Width, Height;
            if (ResolutionParts.Length == 2 && int.TryParse(ResolutionParts[0], out Width) && int.TryParse(ResolutionParts[1], out Height) && Width > 0 && Height > 0)
            {
                ResolutionWidth = Width;
                ResolutionHeight = Height;
            }
            else if (Resolution.Length > 0)
            {
                Hotd2r_ArcadePlugin.MyLogger.LogWarning("ConfManager.ReadConf() => Invalid value for RESOLUTION : " + Resolution);
            }
            FullscreenMode = ReadOptionalEnum<FullScreenMode>(IniFile, "VIDEO", "FULLSCREEN_MODE");
            VSync = ReadOptionalEnum<VSYNCMode>(IniFile, "VIDEO", "VSYNC");
            AntiAliasing = ReadOptionalEnum<SettingsAntiAliasing>(IniFile, "VIDEO", "ANTI_ALIASING");
            ShadowQuality = ReadOptionalEnum<SettingsShadowQuality>(IniFile, "VIDEO", "SHADOW_QUALITY");
            Bloom = ReadOptionalBool(IniFile, "VIDEO", "BLOOM");
            MotionBlur = ReadOptionalBool(IniFile, "VIDEO", "MOTION_BLUR");
            AmbientOcclusion = ReadOptionalBool(IniFile, "VIDEO", "AMBIENT_OCCLUSION");
            Brightness = ReadOptionalInt(IniFile, "VIDEO", "BRIGHTNESS", 0, 100);
            HudScale = ReadOptionalInt(IniFile, "VIDEO", "HUD_SCALE", 50, 150);

            MasterVolume = ReadOptionalInt(IniFile, "SOUND", "MASTER_VOLUME", 0, 100);
            MusicVolume = ReadOptionalInt(IniFile, "SOUND", "MUSIC_VOLUME", 0, 100);
            EffectsVolume = ReadOptionalInt(IniFile, "SOUND", "EFFECTS_VOLUME", 0, 100);
            UIVolume = ReadOptionalInt(IniFile, "SOUND", "UI_VOLUME", 0, 100);
            DialogsVolume = ReadOptionalInt(IniFile, "SOUND", "DIALOGS_VOLUME", 0, 100);
            ShotVolume = ReadOptionalInt(IniFile, "SOUND", "SHOT_VOLUME", 0, 100);
            ClassicMusic = ReadOptionalBool(IniFile, "SOUND", "CLASSIC_MUSIC");
            AttractSound = ReadBool(IniFile, "SOUND", "ATTRACT_SOUND", AttractSound);

            Hotd2r_ArcadePlugin.MyLogger.LogMessage("ConfManager.ReadConf() => Freeplay=" + Freeplay + ", CreditsToStart=" + CreditsToStart + ", CreditsToContinue=" + CreditsToContinue
                + ", Mode=" + Mode + ", Difficulty=" + GameDifficulty + ", ScoreMode=" + GameScoreMode + ", DisableMainMenu=" + DisableMainMenu + ", SaveInGameFolder=" + SaveInGameFolder + ", HideCursor=" + HideCursor + ", ArcadeHud=" + ArcadeHud
                + ", AttractSound=" + AttractSound + ", Lives=" + Lives + ", Language=" + GameLanguage);
        }

        /// <summary>
        /// Like argonlefou's House of the Dead Remake Arcade Plugin : the settings of the config file are replacing the ones of the save, before the game applies them (see mMP_SaveDataHandler)
        /// The game's own option menus are not available in the arcade mode
        /// </summary>
        public void ApplyGameSettings(MP_GameSettings Settings)
        {
            //The HUD is displaying one torch for 2 HP
            if (Lives.HasValue)
                Settings.Life = Lives.Value * 2f;
            if (GameLanguage.HasValue)
                Settings.CurrentLanguage = GameLanguage.Value;
            if (ClassicMusic.HasValue)
                Settings.UseClassicMusic = ClassicMusic.Value;

            if (ResolutionWidth.HasValue && ResolutionHeight.HasValue)
                Settings.CurrentResolution = new MP_ScreenResolution(ResolutionWidth.Value, ResolutionHeight.Value);
            if (FullscreenMode.HasValue)
                Settings.FullScreenMode = FullscreenMode.Value;
            if (VSync.HasValue)
                Settings.VSyncMode = VSync.Value;
            if (AntiAliasing.HasValue)
                Settings.AntiAliasing = AntiAliasing.Value;
            if (ShadowQuality.HasValue)
                Settings.ShadowQuality = ShadowQuality.Value;
            if (Bloom.HasValue)
                Settings.BloomEnabled = Bloom.Value;
            if (MotionBlur.HasValue)
                Settings.MotionBlurEnabled = MotionBlur.Value;
            if (AmbientOcclusion.HasValue)
                Settings.AmbientOcclusionEnabled = AmbientOcclusion.Value;
            //No brightness calibration screen on the first start either
            if (Brightness.HasValue)
            {
                Settings.Brightness = Brightness.Value;
                Settings.BrightnessInitialized = true;
            }
            if (HudScale.HasValue)
                Settings.HUDScale = HudScale.Value / 100f;

            //A volume set in the config file is not muted
            if (MasterVolume.HasValue)
            {
                Settings.MasterVolume = MasterVolume.Value;
                Settings.MuteMasterVolume = false;
            }
            if (MusicVolume.HasValue)
            {
                Settings.MusicVolume = MusicVolume.Value;
                Settings.MuteMusic = false;
            }
            if (EffectsVolume.HasValue)
            {
                Settings.EffectsVolume = EffectsVolume.Value;
                Settings.MuteEffects = false;
            }
            if (UIVolume.HasValue)
            {
                Settings.UIVolume = UIVolume.Value;
                Settings.MuteUI = false;
            }
            if (DialogsVolume.HasValue)
            {
                Settings.DialogsVolume = DialogsVolume.Value;
                Settings.MuteDialogs = false;
            }
            if (ShotVolume.HasValue)
            {
                Settings.ShotVolume = ShotVolume.Value;
                Settings.MuteShots = false;
            }

            Hotd2r_ArcadePlugin.MyLogger.LogMessage("ConfManager.ApplyGameSettings() => Life=" + Settings.Life + ", Language=" + Settings.CurrentLanguage + ", Resolution=" + Settings.CurrentResolution.Width + "x" + Settings.CurrentResolution.Height
                + ", FullScreenMode=" + Settings.FullScreenMode + ", VSync=" + Settings.VSyncMode + ", AntiAliasing=" + Settings.AntiAliasing + ", Shadows=" + Settings.ShadowQuality + ", Bloom=" + Settings.BloomEnabled
                + ", MotionBlur=" + Settings.MotionBlurEnabled + ", AmbientOcclusion=" + Settings.AmbientOcclusionEnabled + ", Brightness=" + Settings.Brightness + ", HUDScale=" + Settings.HUDScale
                + ", Volumes=" + Settings.MasterVolume + "/" + Settings.MusicVolume + "/" + Settings.EffectsVolume + "/" + Settings.UIVolume + "/" + Settings.DialogsVolume + "/" + Settings.ShotVolume + ", ClassicMusic=" + Settings.UseClassicMusic);
        }

        private static int? ReadOptionalInt(INIFile IniFile, string Section, string Key, int Min, int Max)
        {
            string s = IniFile.IniReadValue(Section, Key).Trim();
            int i;
            if (int.TryParse(s, out i) && i >= Min && i <= Max)
                return i;
            if (s.Length > 0)
                Hotd2r_ArcadePlugin.MyLogger.LogWarning("ConfManager.ReadConf() => Invalid value for " + Key + " : " + s);
            return null;
        }

        private static bool? ReadOptionalBool(INIFile IniFile, string Section, string Key)
        {
            string s = IniFile.IniReadValue(Section, Key).Trim();
            if (s.Equals("1"))
                return true;
            if (s.Equals("0"))
                return false;
            if (s.Length > 0)
                Hotd2r_ArcadePlugin.MyLogger.LogWarning("ConfManager.ReadConf() => Invalid value for " + Key + " : " + s);
            return null;
        }

        private static T? ReadOptionalEnum<T>(INIFile IniFile, string Section, string Key) where T : struct
        {
            string s = IniFile.IniReadValue(Section, Key).Trim();
            if (s.Length == 0)
                return null;
            try
            {
                object Value = Enum.Parse(typeof(T), s, true);
                if (Enum.IsDefined(typeof(T), Value))
                    return (T)Value;
            }
            catch
            {
            }
            Hotd2r_ArcadePlugin.MyLogger.LogWarning("ConfManager.ReadConf() => Invalid value for " + Key + " : " + s);
            return null;
        }

        private static bool ReadBool(INIFile IniFile, string Section, string Key, bool Default)
        {
            string s = IniFile.IniReadValue(Section, Key).Trim();
            if (s.Equals("1"))
                return true;
            if (s.Equals("0"))
                return false;
            return Default;
        }

        private static int ReadInt(INIFile IniFile, string Section, string Key, int Default, int Min, int Max)
        {
            int i;
            if (int.TryParse(IniFile.IniReadValue(Section, Key).Trim(), out i) && i >= Min && i <= Max)
                return i;
            return Default;
        }

        private static T ReadEnum<T>(INIFile IniFile, string Section, string Key, T Default)
        {
            string s = IniFile.IniReadValue(Section, Key).Trim();
            if (s.Length > 0)
            {
                try
                {
                    return (T)Enum.Parse(typeof(T), s, true);
                }
                catch
                {
                    Hotd2r_ArcadePlugin.MyLogger.LogWarning("ConfManager.ReadConf() => Invalid value for " + Key + " : " + s);
                }
            }
            return Default;
        }
    }
}
