using HarmonyLib;
using MegaPixel.Menus;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Hotd2Remake_ArcadePlugin
{
    /// <summary>
    /// Screens between the chapters and at the end of the game, made for an arcade cabinet like argonlefou's House of the Dead Remake Arcade Plugin :
    /// START (or a timer) is validating them, instead of the menu buttons
    /// - Chapter results : going on to the next chapter (see mCR_ChapterCompleteMenu)
    /// - Game completed (see mCR_VictoryManager) : results, agent rank, name entry on the game's typewriter (the letters are shot with the guns), leaderboard, then the credits
    /// - Game over : arcade name entry and ranking (see ArcadeRanking)
    /// </summary>
    public static class EndOfGameScreens
    {
        private const float CHAPTER_RESULTS_DURATION = 20f;

        private enum VictoryStep
        {
            None,
            Results,
            Rank,
            Name,
            Leaderboard
        }
        private static readonly float[] VICTORY_STEP_DURATIONS = new float[] { 0f, 15f, 12f, 60f, 10f };

        private static AccessTools.FieldRef<CR_VictoryManager, MP_Menu> _LevelCompleteMenu = AccessTools.FieldRefAccess<CR_VictoryManager, MP_Menu>("levelCompleteMenu");
        private static AccessTools.FieldRef<CR_VictoryManager, MP_Menu> _AgentRankMenu = AccessTools.FieldRefAccess<CR_VictoryManager, MP_Menu>("agentRankMenu");
        private static AccessTools.FieldRef<CR_VictoryManager, MP_Menu> _TypewriterParent = AccessTools.FieldRefAccess<CR_VictoryManager, MP_Menu>("typewriterParent");
        private static AccessTools.FieldRef<CR_VictoryManager, MP_Menu> _LeaderboardsMenu = AccessTools.FieldRefAccess<CR_VictoryManager, MP_Menu>("leaderboardsMenu");
        private static AccessTools.FieldRef<CR_VictoryManager, MP_Button> _ContinueToAgentRankButton = AccessTools.FieldRefAccess<CR_VictoryManager, MP_Button>("continueToAgentRankButton");
        private static AccessTools.FieldRef<CR_VictoryManager, MP_Button> _ContinueToTypewriterButton = AccessTools.FieldRefAccess<CR_VictoryManager, MP_Button>("continueToTypewriterButton");
        private static AccessTools.FieldRef<CR_AgentNameMenu, CR_LeaderboardMenu> _NameMenuLeaderboard = AccessTools.FieldRefAccess<CR_AgentNameMenu, CR_LeaderboardMenu>("leaderboardMenu");
        private static AccessTools.FieldRef<CR_LeaderboardMenu, GameObject> _LeaderboardContinueButton = AccessTools.FieldRefAccess<CR_LeaderboardMenu, GameObject>("continueButtonRoot");

        private static CR_ChapterCompleteMenu _ChapterResults = null;
        private static MP_Button _NextChapterButton = null;
        private static CR_VictoryManager _Victory = null;
        private static CR_AgentNameMenu _NameMenu = null;
        private static VictoryStep _VictoryStep = VictoryStep.None;
        private static bool _IsVictoryStepValidated = false;
        private static float _Timer = 0f;

        public static void OnChapterResultsShown(CR_ChapterCompleteMenu Menu, MP_Button NextButton)
        {
            _ChapterResults = Menu;
            _NextChapterButton = NextButton;
            _Timer = 0f;
        }

        public static void OnVictory(CR_VictoryManager Victory)
        {
            _Victory = Victory;
            _NameMenu = null;
            _VictoryStep = VictoryStep.None;
        }

        /// <summary>
        /// When one of these screens is displayed, START is used to validate it
        /// </summary>
        public static bool IsActive()
        {
            return (ArcadeRanking.Instance != null && ArcadeRanking.Instance.IsEndOfGameScreenActive) || isVictoryActive() || isChapterResultsActive();
        }

        /// <summary>
        /// Our credits text would be on top of the game's buttons (see ArcadeOverlay)
        /// </summary>
        public static bool IsFullScreenMenuActive()
        {
            return (ArcadeRanking.Instance != null && ArcadeRanking.Instance.IsEndOfGameScreenActive) || isVictoryActive();
        }

        public static void Update(bool StartPressed)
        {
            if (ArcadeRanking.Instance != null && ArcadeRanking.Instance.IsEndOfGameScreenActive)
            {
                if (StartPressed)
                    ArcadeRanking.Instance.HandleStart();
            }
            else if (isVictoryActive())
            {
                updateVictory(StartPressed);
            }
            else if (isChapterResultsActive())
            {
                _Timer += Time.unscaledDeltaTime;
                if (StartPressed || _Timer >= CHAPTER_RESULTS_DURATION)
                {
                    Hotd2r_ArcadePlugin.MyLogger.LogMessage("EndOfGameScreens.Update() => Chapter results closed, loading the next chapter");
                    CR_ChapterCompleteMenu Menu = _ChapterResults;
                    _ChapterResults = null;
                    CR_AudioManager.PlayUIAudioAccept(Menu.gameObject);
                    Menu.LoadNextLevel();
                }
            }
        }

        //In the last chapter, the game is hiding the "Next" button and the victory screens are coming next
        private static bool isChapterResultsActive()
        {
            return _ChapterResults != null && CR_ChapterCompleteMenu.IsActive && _NextChapterButton != null && _NextChapterButton.gameObject.activeInHierarchy;
        }

        private static bool isVictoryActive()
        {
            return _Victory != null && _Victory.isActiveAndEnabled;
        }

        private static void updateVictory(bool StartPressed)
        {
            VictoryStep Step = getVictoryStep();
            if (Step != _VictoryStep)
            {
                Hotd2r_ArcadePlugin.MyLogger.LogMessage("EndOfGameScreens.updateVictory() => " + Step.ToString());
                _VictoryStep = Step;
                _IsVictoryStepValidated = false;
                _Timer = 0f;
            }
            if (Step == VictoryStep.None || _IsVictoryStepValidated)
                return;

            //Like argonlefou's plugin, the "Continue" buttons are telling to press START
            switch (Step)
            {
                case VictoryStep.Results:
                    setPressStartLabel(_ContinueToAgentRankButton(_Victory));
                    break;
                case VictoryStep.Rank:
                    setPressStartLabel(_ContinueToTypewriterButton(_Victory));
                    break;
                case VictoryStep.Leaderboard:
                    CR_LeaderboardMenu Leaderboard = getOpenLeaderboard();
                    if (Leaderboard != null)
                        setPressStartLabel(_LeaderboardContinueButton(Leaderboard));
                    break;
            }

            _Timer += Time.unscaledDeltaTime;
            if (!StartPressed && _Timer < VICTORY_STEP_DURATIONS[(int)Step])
                return;

            switch (Step)
            {
                case VictoryStep.Results:
                    clickButton(_ContinueToAgentRankButton(_Victory));
                    break;

                case VictoryStep.Rank:
                    clickButton(_ContinueToTypewriterButton(_Victory));
                    break;

                //Same as the game's own "Confirm name" input. An empty name is replaced by the game's default one
                case VictoryStep.Name:
                    _IsVictoryStepValidated = getNameMenu() != null && getNameMenu().OnContinue();
                    _Timer = 0f;
                    return;

                //Same as the game's own "Continue" button : credits, ending and back to the title screen
                case VictoryStep.Leaderboard:
                    CR_LeaderboardMenu Leaderboard = getOpenLeaderboard();
                    if (Leaderboard != null)
                        Leaderboard.OnContinue();
                    break;
            }
            _IsVictoryStepValidated = true;
        }

        private static VictoryStep getVictoryStep()
        {
            if (getOpenLeaderboard() != null)
                return VictoryStep.Leaderboard;
            if (isOpen(_TypewriterParent(_Victory)))
                return VictoryStep.Name;
            if (isOpen(_AgentRankMenu(_Victory)))
                return VictoryStep.Rank;
            if (isOpen(_LevelCompleteMenu(_Victory)))
                return VictoryStep.Results;
            return VictoryStep.None;
        }

        private static CR_AgentNameMenu getNameMenu()
        {
            if (_NameMenu == null)
            {
                _NameMenu = _TypewriterParent(_Victory) as CR_AgentNameMenu;
                if (_NameMenu == null)
                    _NameMenu = Object.FindObjectOfType<CR_AgentNameMenu>();
            }
            return _NameMenu;
        }

        //The leaderboard can be opened by the name entry, or by the victory manager itself
        private static CR_LeaderboardMenu getOpenLeaderboard()
        {
            CR_LeaderboardMenu Leaderboard = _LeaderboardsMenu(_Victory) as CR_LeaderboardMenu;
            if (isOpen(Leaderboard))
                return Leaderboard;
            CR_AgentNameMenu NameMenu = getNameMenu();
            if (NameMenu != null)
            {
                Leaderboard = _NameMenuLeaderboard(NameMenu);
                if (isOpen(Leaderboard))
                    return Leaderboard;
            }
            return null;
        }

        private static bool isOpen(MP_Menu Menu)
        {
            return Menu != null && Menu.State == MP_Menu.MenuState.Open;
        }

        private static void clickButton(MP_Button Button)
        {
            if (Button != null)
                Button.onClick.Invoke();
        }

        private static void setPressStartLabel(Component Button)
        {
            setPressStartLabel(Button != null ? Button.gameObject : null);
        }

        /// <summary>
        /// Checked every frame, the game is translating the button texts again when they are enabled
        /// </summary>
        private static void setPressStartLabel(GameObject Button)
        {
            if (Button == null)
                return;
            TMP_Text Label = Button.GetComponentInChildren<TMP_Text>(true);
            string PressStart = Hotd2r_ArcadePlugin.TextLangs[LanguageStrings.StringName.PressStart];
            if (Label == null || Label.text == PressStart)
                return;
            Label.text = PressStart;
            //Controller icon
            foreach (Image Icon in Label.GetComponentsInChildren<Image>(true))
                Icon.enabled = false;
        }
    }
}
