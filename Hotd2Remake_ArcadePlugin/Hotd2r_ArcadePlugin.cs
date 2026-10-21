using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using HarmonyLib;
using I2.Loc;
using MegaPixel.Input;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Hotd2Remake_ArcadePlugin
{
    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    [BepInDependency(DEMULSHOOTER_PLUGIN_GUID, BepInDependency.DependencyFlags.SoftDependency)]
    public class Hotd2r_ArcadePlugin : BaseUnityPlugin
    {
        public const String pluginGuid = "m33ts4k0z.hotd2r.arcade";
        public const String pluginName = "House Of The Dead 2 Remake - Arcade Plugin";
        public const String pluginVersion = "0.1.0.0";
        public const String pluginConfigFile = "Hotd2Remake_ArcadePlugin.ini";

        //DemulShooter's lightgun plugin for this game, if installed : the START buttons are handled here instead
        private const String DEMULSHOOTER_PLUGIN_GUID = "argonlefou.demulshooter.hotd2remake";

        public static Hotd2r_ArcadePlugin Instance = null;
        public static ManualLogSource MyLogger;

        public static ConfManager Configurator;
        public static Dictionary<LanguageStrings.StringName, String> TextLangs = LanguageStrings.EnglishDictionnary;
        private static string _CurrentLanguageCode = string.Empty;

        public const int MAX_PLAYERS = 2;
        private const int MAX_CREDITS = 99;
        public static int CurrentCredits = 0;

        private static PluginControllerButton[] _StartButtons = new PluginControllerButton[MAX_PLAYERS];
        private static PluginControllerButton[] _CoinButtons = new PluginControllerButton[MAX_PLAYERS];
        private static PluginControllerButton _ExitButton;

        //Players who have paid to play the current game
        public static bool[] IsPlayerPlaying = new bool[MAX_PLAYERS];
        //Set when a game is started from the title screen with START : only players who have paid are allowed in the game
        public static bool IsArcadeGameRunning = false;
        private static bool _IsLoadingNewGame = false;

        //Title screen, set by mCR_MainMenuMovieController
        public static CR_MainMenuMovieController TitleScreen = null;
        private static AccessTools.FieldRef<CR_MainMenuMovieController, CR_SimplePressAnyKeyDisplay> _TitlePressAnyKey = AccessTools.FieldRefAccess<CR_MainMenuMovieController, CR_SimplePressAnyKeyDisplay>("simplePressAnyKey");
        private static AccessTools.FieldRef<CR_MainMenuMovieController, CR_MainMenu> _TitleMainMenu = AccessTools.FieldRefAccess<CR_MainMenuMovieController, CR_MainMenu>("mainMenu");

        //Ready check panel singleton is private in the game
        private static FieldInfo _ReadyCheckPanelInstance = AccessTools.Field(typeof(CR_ReadyCheckPanel), "instance");

        //Game's main font, saved from the title screen text to be used on our own texts
        public static TMP_FontAsset GameFont = null;
        public static Material GameFontMaterial = null;

        public static string NvramFolder;

        public void Awake()
        {
            Instance = this;
            MyLogger = Logger;
            MyLogger.LogMessage("Plugin Loaded");
            MyLogger.LogMessage("Detected Application Version : " + Application.version);

            Configurator = new ConfManager();
            Configurator.ReadConf(Paths.PluginPath + "/" + pluginConfigFile);

            for (int i = 0; i < MAX_PLAYERS; i++)
            {
                _StartButtons[i] = new PluginControllerButton(Configurator.StartKeys[i]);
                _CoinButtons[i] = new PluginControllerButton(Configurator.CoinKeys[i]);
                //Credits are replacing the game's own continue tokens
                CR_Data.UNLIMITED_CREDITS_ENABLED_FROM_CONTROL_PANEL[i] = true;
            }
            _ExitButton = new PluginControllerButton(Configurator.ExitKey);

            NvramFolder = Paths.GameRootPath + "/NVRAM";
            if (Configurator.SaveInGameFolder)
                PrepareNvramSave();

            Harmony harmony = new Harmony(pluginGuid);
            harmony.PatchAll();
            TakeOverDemulShooterStartButtons(harmony);

            gameObject.AddComponent<ArcadeOverlay>();
        }

        public void Update()
        {
            UpdateLanguage();
            mCR_CutsceneUI.ReplaceSkipHintIcon();

            for (int i = 0; i < MAX_PLAYERS; i++)
            {
                _StartButtons[i].SetButton(GetKey(_StartButtons[i].KeyCode));
                _CoinButtons[i].SetButton(GetKey(_CoinButtons[i].KeyCode));
            }

            //No in-game menu (see mMP_GameManager) : quitting directly, like argonlefou's House of the Dead Remake Arcade Plugin
            _ExitButton.SetButton(GetKey(_ExitButton.KeyCode));
            if (_ExitButton.GetButtonDown())
            {
                MyLogger.LogMessage("Hotd2Remake_ArcadePlugin.Update() => EXIT key pressed, quitting");
                Application.Quit();
            }

            //Coins are going in a common credits pool, like most arcade cabinets
            for (int i = 0; i < MAX_PLAYERS; i++)
            {
                if (_CoinButtons[i].GetButtonDown() && CurrentCredits < MAX_CREDITS)
                {
                    CurrentCredits++;
                    MyLogger.LogMessage("Coin inserted (P" + (i + 1) + "), credits = " + CurrentCredits);
                    CR_AudioManager.PlayUIAudioSelection(gameObject);
                    CR_DeathScreenManager.RedrawDeathScreen();
                }
            }

            if (IsOnTitleScreen())
            {
                //Back on the title screen after a game : nobody is playing anymore
                if (!_IsLoadingNewGame)
                {
                    if (IsArcadeGameRunning)
                        MyLogger.LogMessage("Hotd2Remake_ArcadePlugin.Update() => Back on the title screen, game is over");
                    IsArcadeGameRunning = false;
                    for (int i = 0; i < MAX_PLAYERS; i++)
                        IsPlayerPlaying[i] = false;
                }

                for (int i = 0; i < MAX_PLAYERS; i++)
                {
                    if (_StartButtons[i].GetButtonDown())
                        HandleTitleScreenStart((PlayerType)i);
                }
            }
            else if (CR_LevelLoader.IsGameplayChapterLoaded)
            {
                _IsLoadingNewGame = false;

                for (int i = 0; i < MAX_PLAYERS; i++)
                {
                    if (_StartButtons[i].GetButtonDown())
                        HandleGameplayStart((PlayerType)i);
                }

                //The game is always activating P1 : removing him if only P2 has paid
                if (IsArcadeGameRunning)
                {
                    for (int i = 0; i < MAX_PLAYERS; i++)
                    {
                        CR_Player GamePlayer = CR_Player.Players[i];
                        if (GamePlayer != null && GamePlayer.IsActivated && !IsPlayerPlaying[i])
                        {
                            MyLogger.LogMessage("Hotd2Remake_ArcadePlugin.Update() => " + ((PlayerType)i).ToString() + " has not paid, removing him from the game");
                            GamePlayer.Deactivate(false);
                        }
                    }
                }
            }
            else
            {
                //Loading screens, menus...
                for (int i = 0; i < MAX_PLAYERS; i++)
                    _StartButtons[i].GetButtonDown();
            }
        }

        #region START buttons

        public static bool IsAnyStartHeld()
        {
            for (int i = 0; i < MAX_PLAYERS; i++)
            {
                if (_StartButtons[i].GetButton())
                    return true;
            }
            return false;
        }

        public static bool CanPay(int Credits)
        {
            return Configurator.Freeplay || CurrentCredits >= Credits;
        }

        public static bool TryToPay(int Credits)
        {
            if (Configurator.Freeplay)
                return true;
            if (CurrentCredits < Credits)
                return false;
            CurrentCredits -= Credits;
            return true;
        }

        /// <summary>
        /// START on the title screen : starts a new Multiplayer campaign for this player, skipping all menus.
        /// The other player can still join during the loading
        /// </summary>
        private void HandleTitleScreenStart(PlayerType Player)
        {
            if (IsPlayerPlaying[(int)Player] || !MP_SaveDataHandler.IsReady)
                return;

            if (!TryToPay(Configurator.CreditsToStart))
            {
                CR_AudioManager.PlayUIAudioError(gameObject);
                return;
            }

            IsPlayerPlaying[(int)Player] = true;
            CR_AudioManager.PlayUIAudioAccept(gameObject);

            if (!_IsLoadingNewGame)
            {
                if (StartNewGame(Player))
                {
                    _IsLoadingNewGame = true;
                    IsArcadeGameRunning = true;
                }
                else
                {
                    IsPlayerPlaying[(int)Player] = false;
                }
            }
            else
            {
                //2nd player joining before the game is loaded
                MyLogger.LogMessage("Hotd2Remake_ArcadePlugin.HandleTitleScreenStart() => " + Player.ToString() + " joining the game");
                MP_SaveDataHandler.SaveData.MultiplayerCampaign.SetMultiplayerActive(true);
            }
        }

        /// <summary>
        /// Same thing as the game's own New Game menu : using CR_NewGampaignMenu.prepareSaveDataForNewGame() with our own settings, then loading the first chapter
        /// </summary>
        private bool StartNewGame(PlayerType Player)
        {
            CR_NewGampaignMenu Menu = null;
            foreach (CR_NewGampaignMenu m in Resources.FindObjectsOfTypeAll<CR_NewGampaignMenu>())
            {
                if (m.gameObject.scene.IsValid())
                {
                    Menu = m;
                    break;
                }
            }
            if (Menu == null)
            {
                MyLogger.LogError("Hotd2Remake_ArcadePlugin.StartNewGame() => CR_NewGampaignMenu not found !");
                return false;
            }

            MyLogger.LogMessage("Hotd2Remake_ArcadePlugin.StartNewGame() => " + Player.ToString() + " is starting a new game : " + Configurator.Mode + ", " + Configurator.GameDifficulty + ", " + Configurator.GameScoreMode);
            Traverse MenuTraverse = Traverse.Create(Menu);
            MenuTraverse.Field("activeMode").SetValue(Configurator.Mode);
            MenuTraverse.Field("multiplayerMode").SetValue(MultiplayerMode.Competitive);
            MenuTraverse.Field("scoreMode").SetValue(Configurator.GameScoreMode);
            MenuTraverse.Field("difficulty").SetValue(Configurator.GameDifficulty);
            Menu.prepareSaveDataForNewGame();

            //P1 is always activated by the game, P2 only if the multiplayer is active in the campaign
            MP_SaveDataHandler.SaveData.MultiplayerCampaign.SetMultiplayerActive(Player == PlayerType.Player2);
            MP_SaveDataHandler.LastPlayedCampaign.SetCurrentChapter(Chapter.Ch1_APrelude);
            MP_GameManager.LevelAborted();
            CR_LevelLoader.LoadGameplayLevel(Chapter.Ch1_APrelude);
            return true;
        }

        /// <summary>
        /// START during the game :
        /// - A player not in the game can join (only possible in a Multiplayer campaign, like the original game)
        /// - A dead player can continue (credits are checked in mCR_Player.Revive)
        /// </summary>
        private void HandleGameplayStart(PlayerType Player)
        {
            if (!MP_SaveDataHandler.IsReady || MP_SaveDataHandler.LastPlayedCampaign == null)
                return;

            CR_Player GamePlayer = CR_Player.Players[(int)Player];
            if (GamePlayer == null)
                return;

            //Same as the game, no joining or continuing while in Menu, Pause or Cutscene
            InputMapsFlag ActiveMaps = MP_InputManager.ActiveInputMaps;
            if ((ActiveMaps & (InputMapsFlag.Gameplay | InputMapsFlag.ReadyPanel | InputMapsFlag.DeathScreen)) == 0)
                return;

            if (!GamePlayer.IsActivated)
            {
                if (!MP_SaveDataHandler.LastPlayedCampaign.IsMultiplayerCampaign || MP_GameManager.IsGameplayFinished || MP_GameManager.IsLevelCompleted || MP_ControllerDisconnectedPopup.IsVisible)
                    return;

                if (!TryToPay(Configurator.CreditsToStart))
                {
                    CR_AudioManager.PlayUIAudioError(gameObject);
                    return;
                }

                MyLogger.LogMessage("Hotd2Remake_ArcadePlugin.HandleGameplayStart() => " + Player.ToString() + " joining the game");
                IsPlayerPlaying[(int)Player] = true;
                if (Player == PlayerType.Player2)
                    CR_GameManager.SetMultiplayerActive(true);
                else
                    GamePlayer.Activate();
                CR_ReadyCheckPanel.TryRefreshingMultiplayerStatus();
                CR_AudioManager.PlayUIAudioAccept(gameObject);
                if (CR_ReadyCheckPanel.IsActive)
                    SetPlayerReady(Player);
            }
            else if (CR_ReadyCheckPanel.IsActive)
            {
                SetPlayerReady(Player);
            }
            else if (GamePlayer.HasDied && (ActiveMaps & InputMapsFlag.DeathScreen) != 0)
            {
                if (CanPay(Configurator.CreditsToContinue))
                    GamePlayer.Revive();
                else
                    CR_AudioManager.PlayUIAudioError(gameObject);
            }
        }

        /// <summary>
        /// DemulShooter's lightgun plugin is also using START to join and continue, for free.
        /// Disabling it so that everything goes through the credits
        /// </summary>
        private void TakeOverDemulShooterStartButtons(Harmony harmony)
        {
            PluginInfo DsPlugin;
            if (!Chainloader.PluginInfos.TryGetValue(DEMULSHOOTER_PLUGIN_GUID, out DsPlugin) || DsPlugin.Instance == null)
                return;

            MethodInfo HandleStartButton = AccessTools.Method(DsPlugin.Instance.GetType(), "HandleStartButton");
            if (HandleStartButton == null)
            {
                MyLogger.LogWarning("Hotd2Remake_ArcadePlugin.TakeOverDemulShooterStartButtons() => DemulShooter plugin found, but not its START handler");
                return;
            }
            harmony.Patch(HandleStartButton, new HarmonyMethod(typeof(Hotd2r_ArcadePlugin), "SkipDemulShooterStartButton"));
            MyLogger.LogMessage("Hotd2Remake_ArcadePlugin.TakeOverDemulShooterStartButtons() => START buttons now handled by the Arcade plugin");
        }

        private static bool SkipDemulShooterStartButton()
        {
            return false;
        }

        /// <summary>
        /// Ready screen at the beginning of a chapter : START is validating it, like the trigger
        /// </summary>
        private static void SetPlayerReady(PlayerType Player)
        {
            CR_ReadyCheckPanel Panel = _ReadyCheckPanelInstance.GetValue(null) as CR_ReadyCheckPanel;
            if (Panel != null)
                Panel.SetPlayerReady(Player);
        }

        #endregion

        #region Title screen

        public static bool IsOnTitleScreen()
        {
            if (TitleScreen == null || !TitleScreen.isActiveAndEnabled)
                return false;
            CR_SimplePressAnyKeyDisplay PressAnyKey = _TitlePressAnyKey(TitleScreen);
            CR_MainMenu MainMenu = _TitleMainMenu(TitleScreen);
            if (PressAnyKey == null || !PressAnyKey.IsVisible)
                return false;
            return MainMenu == null || !(MainMenu.IsMainMenuOpen || MainMenu.IsMainMenuOpening);
        }

        #endregion

        #region Player Strings information

        /// <summary>
        /// Check the game state (Freeplay, coins, etc...) to generate the strings for Players
        /// Same as argonlefou's House of the Dead Remake Arcade Plugin
        /// </summary>
        public static String GetScreenPlayerString(int CreditReference)
        {
            if (Configurator.Freeplay)
            {
                return (TextLangs[LanguageStrings.StringName.PressStart] + "<br>" + TextLangs[LanguageStrings.StringName.Freeplay]);
            }
            else
            {
                if (CurrentCredits <= 0)
                    return (TextLangs[LanguageStrings.StringName.InsertCoins] + "<br>" + GetCreditsString());
                else if (CurrentCredits < CreditReference)
                    return (TextLangs[LanguageStrings.StringName.InsertMoreCoins] + "<br>" + GetCreditsString());
                else
                    return (TextLangs[LanguageStrings.StringName.PressStart] + "<br>" + GetCreditsString());
            }
        }

        public static String GetCreditsString()
        {
            if (Configurator.Freeplay)
                return TextLangs[LanguageStrings.StringName.Freeplay];
            return TextLangs[LanguageStrings.StringName.Credits] + " : " + CurrentCredits.ToString("D2");
        }

        /// <summary>
        /// Our strings are following the game's language
        /// </summary>
        private static void UpdateLanguage()
        {
            string Code = LocalizationManager.CurrentLanguageCode;
            if (Code == null || Code.Equals(_CurrentLanguageCode))
                return;
            _CurrentLanguageCode = Code;

            if (Code.StartsWith("fr"))
                TextLangs = LanguageStrings.FrenchDictionnary;
            else if (Code.StartsWith("de"))
                TextLangs = LanguageStrings.GermanDictionnary;
            else if (Code.StartsWith("it"))
                TextLangs = LanguageStrings.ItalianDictionnary;
            else if (Code.StartsWith("es"))
                TextLangs = LanguageStrings.SpainDictionnary;
            else if (Code.StartsWith("pl"))
                TextLangs = LanguageStrings.PolishDictionnary;
            else if (Code.StartsWith("ru"))
                TextLangs = LanguageStrings.RussianDictionnary;
            else if (Code.StartsWith("ja"))
                TextLangs = LanguageStrings.JapaneseDictionnary;
            else if (Code.StartsWith("zh"))
                TextLangs = LanguageStrings.ChineseDictionnary;
            else
                TextLangs = LanguageStrings.EnglishDictionnary;
            MyLogger.LogMessage("Hotd2Remake_ArcadePlugin.UpdateLanguage() => Game language : " + Code);
        }

        #endregion

        #region Save file

        /// <summary>
        /// First run : copying the existing save file to the game folder, to keep the settings and unlocked items
        /// </summary>
        private void PrepareNvramSave()
        {
            try
            {
                if (!Directory.Exists(NvramFolder))
                    Directory.CreateDirectory(NvramFolder);

                string OriginalSave = Application.persistentDataPath + "/saveData.sav";
                string NvramSave = NvramFolder + "/saveData.sav";
                if (!File.Exists(NvramSave) && File.Exists(OriginalSave))
                {
                    File.Copy(OriginalSave, NvramSave);
                    MyLogger.LogMessage("Hotd2Remake_ArcadePlugin.PrepareNvramSave() => Copied " + OriginalSave + " to " + NvramSave);
                }
            }
            catch (Exception Ex)
            {
                MyLogger.LogError("Hotd2Remake_ArcadePlugin.PrepareNvramSave() => " + Ex.Message);
            }
        }

        #endregion

        #region Keyboard

        /// <summary>
        /// The game is only using the InputSystem package : reading the legacy UnityEngine.Input class is throwing an exception
        /// Keys are still set as legacy KeyCode values in the config file, like for DemulShooter plugins
        /// </summary>
        public static bool GetKey(int iKeyCode)
        {
            Key k = KeyCodeToKey((KeyCode)iKeyCode);
            if (k == Key.None || Keyboard.current == null)
                return false;
            return Keyboard.current[k].isPressed;
        }

        private static Key KeyCodeToKey(KeyCode k)
        {
            if (k >= KeyCode.A && k <= KeyCode.Z)
                return Key.A + (k - KeyCode.A);
            if (k >= KeyCode.Alpha1 && k <= KeyCode.Alpha9)
                return Key.Digit1 + (k - KeyCode.Alpha1);
            if (k >= KeyCode.Keypad0 && k <= KeyCode.Keypad9)
                return Key.Numpad0 + (k - KeyCode.Keypad0);
            if (k >= KeyCode.F1 && k <= KeyCode.F12)
                return Key.F1 + (k - KeyCode.F1);

            switch (k)
            {
                case KeyCode.Alpha0: return Key.Digit0;
                case KeyCode.Escape: return Key.Escape;
                case KeyCode.Return: return Key.Enter;
                case KeyCode.Space: return Key.Space;
                case KeyCode.Tab: return Key.Tab;
                case KeyCode.Backspace: return Key.Backspace;
                case KeyCode.UpArrow: return Key.UpArrow;
                case KeyCode.DownArrow: return Key.DownArrow;
                case KeyCode.LeftArrow: return Key.LeftArrow;
                case KeyCode.RightArrow: return Key.RightArrow;
                case KeyCode.Insert: return Key.Insert;
                case KeyCode.Delete: return Key.Delete;
                case KeyCode.Home: return Key.Home;
                case KeyCode.End: return Key.End;
                case KeyCode.PageUp: return Key.PageUp;
                case KeyCode.PageDown: return Key.PageDown;
                case KeyCode.LeftShift: return Key.LeftShift;
                case KeyCode.RightShift: return Key.RightShift;
                case KeyCode.LeftControl: return Key.LeftCtrl;
                case KeyCode.RightControl: return Key.RightCtrl;
                case KeyCode.LeftAlt: return Key.LeftAlt;
                case KeyCode.RightAlt: return Key.RightAlt;
                case KeyCode.Minus: return Key.Minus;
                case KeyCode.Equals: return Key.Equals;
                case KeyCode.Comma: return Key.Comma;
                case KeyCode.Period: return Key.Period;
                case KeyCode.Slash: return Key.Slash;
                case KeyCode.Backslash: return Key.Backslash;
                case KeyCode.Semicolon: return Key.Semicolon;
                case KeyCode.Quote: return Key.Quote;
                case KeyCode.BackQuote: return Key.Backquote;
                case KeyCode.LeftBracket: return Key.LeftBracket;
                case KeyCode.RightBracket: return Key.RightBracket;
                case KeyCode.KeypadEnter: return Key.NumpadEnter;
                case KeyCode.KeypadPlus: return Key.NumpadPlus;
                case KeyCode.KeypadMinus: return Key.NumpadMinus;
                case KeyCode.KeypadMultiply: return Key.NumpadMultiply;
                case KeyCode.KeypadDivide: return Key.NumpadDivide;
                case KeyCode.KeypadPeriod: return Key.NumpadPeriod;
                default: return Key.None;
            }
        }

        #endregion
    }
}
