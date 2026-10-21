using System;
using System.IO;

namespace Hotd2Remake_ArcadePlugin
{
    /// <summary>
    /// Plugin settings, read from Hotd2Remake_ArcadePlugin.ini in the BepInEx plugins folder
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

            Hotd2r_ArcadePlugin.MyLogger.LogMessage("ConfManager.ReadConf() => Freeplay=" + Freeplay + ", CreditsToStart=" + CreditsToStart + ", CreditsToContinue=" + CreditsToContinue
                + ", Mode=" + Mode + ", Difficulty=" + GameDifficulty + ", ScoreMode=" + GameScoreMode + ", DisableMainMenu=" + DisableMainMenu + ", SaveInGameFolder=" + SaveInGameFolder + ", HideCursor=" + HideCursor + ", ArcadeHud=" + ArcadeHud);
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
