using System;
using MegaPixel.SaveLoadSystem;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Hotd2Remake_ArcadePlugin
{
    /// <summary>
    /// Arcade name entry and ranking, using the game's own local leaderboard (the board of the MODE and SCORE_MODE used for the arcade games) :
    /// - After a GAME OVER, the game is going straight back to the main menu : if the score of the game is in the top 10, the players can enter a name
    ///   by shooting the letters (the guns are the system mouse outside of the gameplay), then the ranking is displayed
    /// - On the title screen, after the main video (see mCR_MainMenuMovieController), like argonlefou's House of the Dead Remake Arcade Plugin
    /// A completed game is using the game's own name entry and leaderboard screens instead (see EndOfGameScreens)
    /// </summary>
    public class ArcadeRanking : MonoBehaviour
    {
        public static ArcadeRanking Instance { get; private set; }

        private const float NAME_ENTRY_DURATION = 30f;
        private const float GAME_OVER_RANKING_DURATION = 8f;
        private const float ATTRACT_RANKING_DURATION = 10f;
        private const int NAME_MAX_LENGTH = 10;
        //Same as the game's own name entry
        private const string DEFAULT_NAME = "AMS";
        private const string KEY_DELETE = "DEL";
        private const string KEY_END = "END";
        private static readonly string[] KEYS = new string[]
        {
            "A", "B", "C", "D", "E", "F", "G", "H",
            "I", "J", "K", "L", "M", "N", "O", "P",
            "Q", "R", "S", "T", "U", "V", "W", "X",
            "Y", "Z", ".", "-", "!", "?", KEY_DELETE, KEY_END
        };
        private const int KEYS_PER_ROW = 8;
        private static readonly Vector2 KEY_SIZE = new Vector2(130f, 110f);
        private const float KEY_SPACING = 20f;

        private static readonly Color BACKGROUND_COLOR = new Color(0f, 0f, 0f, 0.85f);
        private static readonly Color KEY_COLOR = new Color(0f, 0f, 0f, 0.6f);
        private static readonly Color KEY_HOVER_COLOR = new Color(0.6f, 0f, 0f, 0.9f);
        private static readonly Color HIGHLIGHT_COLOR = new Color(1f, 0.85f, 0.2f, 1f);

        private enum State
        {
            None,
            NameEntry,
            GameOverRanking,
            AttractRanking
        }
        private State _State = State.None;
        private float _Timer = 0f;
        private Action _OnAttractRankingFinished = null;

        //Game over
        private bool _IsNameEntryPending = false;
        private int _P1Score = 0;
        private int _P2Score = 0;
        private string _Name = string.Empty;

        private Canvas _Canvas = null;
        private RectTransform _CanvasTransform;
        private GameObject _NameEntryPanel;
        private TextMeshProUGUI _NameEntryTitle;
        private TextMeshProUGUI _ScoreText;
        private TextMeshProUGUI _NameText;
        private TextMeshProUGUI _TimerText;
        private Image[] _Keys = new Image[KEYS.Length];
        private TextMeshProUGUI[] _KeyTexts = new TextMeshProUGUI[KEYS.Length];
        private RectTransform _Crosshair;
        private GameObject _RankingPanel;
        private TextMeshProUGUI _RankingTitle;
        private TextMeshProUGUI[] _RankTexts;
        private TextMeshProUGUI[] _RankNames;
        private TextMeshProUGUI[] _RankScores;
        private TextMeshProUGUI _RankingFooter;

        public bool IsEndOfGameScreenActive
        {
            get { return _State == State.NameEntry || _State == State.GameOverRanking; }
        }

        //The title screen text is hidden, our ranking has its own
        public bool IsAttractRankingActive
        {
            get { return _State == State.AttractRanking; }
        }

        private void Awake()
        {
            Instance = this;
        }

        /// <summary>
        /// All players are dead and did not continue : is the score of this game good enough for the ranking ?
        /// </summary>
        public void OnGameOver()
        {
            _P1Score = CR_ScoreManager.GetPlayerTotalScore(PlayerType.Player1);
            _P2Score = CR_ScoreManager.GetPlayerTotalScore(PlayerType.Player2);
            _IsNameEntryPending = getRankingPosition(_P1Score + _P2Score) >= 0;
            Hotd2r_ArcadePlugin.MyLogger.LogMessage("ArcadeRanking.OnGameOver() => Score = " + (_P1Score + _P2Score) + " (P1 = " + _P1Score + ", P2 = " + _P2Score + "), name entry : " + _IsNameEntryPending);
        }

        /// <summary>
        /// A new game is starting
        /// </summary>
        public void Reset()
        {
            _IsNameEntryPending = false;
            if (_State == State.AttractRanking)
            {
                hide();
                _OnAttractRankingFinished = null;
            }
        }

        /// <summary>
        /// The game is going back to the main menu after the game over map (see mCR_LevelLoader) : name entry first, if the score is in the ranking
        /// </summary>
        public bool TryStartNameEntry()
        {
            if (!_IsNameEntryPending || !createCanvas())
                return false;

            _IsNameEntryPending = false;
            _State = State.NameEntry;
            _Timer = NAME_ENTRY_DURATION;
            _Name = string.Empty;
            _NameEntryTitle.text = Hotd2r_ArcadePlugin.TextLangs[LanguageStrings.StringName.EnterYourName];
            _ScoreText.text = Hotd2r_ArcadePlugin.TextLangs[LanguageStrings.StringName.Score] + "  " + (_P1Score + _P2Score).ToString("D7");
            _RankingPanel.SetActive(false);
            _NameEntryPanel.SetActive(true);
            _Canvas.enabled = true;
            MP_MusicManager.PlayMusic(MusicID.RankingMenu);
            Hotd2r_ArcadePlugin.MyLogger.LogMessage("ArcadeRanking.TryStartNameEntry() => Name entry");
            return true;
        }

        /// <summary>
        /// START is validating the name, or closing the ranking
        /// </summary>
        public void HandleStart()
        {
            if (_State == State.NameEntry)
                endNameEntry();
            else if (_State == State.GameOverRanking)
                endGameOverRanking();
        }

        /// <summary>
        /// Ranking between the title screen videos, if there is any
        /// </summary>
        public bool ShowAttractRanking(Action OnFinished)
        {
            Leaderboard Board = getLeaderboard();
            if (Board == null || Board.BestScore <= 0f || !createCanvas())
                return false;

            _State = State.AttractRanking;
            _Timer = ATTRACT_RANKING_DURATION;
            _OnAttractRankingFinished = OnFinished;
            showRanking(Board, -1, true);
            Hotd2r_ArcadePlugin.MyLogger.LogMessage("ArcadeRanking.ShowAttractRanking() => Ranking on the title screen");
            return true;
        }

        private void Update()
        {
            switch (_State)
            {
                case State.NameEntry:
                    updateNameEntry();
                    break;

                case State.GameOverRanking:
                    _Timer -= Time.unscaledDeltaTime;
                    if (_Timer <= 0f)
                        endGameOverRanking();
                    break;

                case State.AttractRanking:
                    //A game is starting
                    if (!Hotd2r_ArcadePlugin.IsOnTitleScreen())
                    {
                        hide();
                        _OnAttractRankingFinished = null;
                        break;
                    }
                    _RankingFooter.text = Hotd2r_ArcadePlugin.GetScreenPlayerString(Hotd2r_ArcadePlugin.Configurator.CreditsToStart);
                    _Timer -= Time.unscaledDeltaTime;
                    if (_Timer <= 0f)
                    {
                        hide();
                        Action OnFinished = _OnAttractRankingFinished;
                        _OnAttractRankingFinished = null;
                        if (OnFinished != null)
                            OnFinished();
                    }
                    break;
            }
        }

        private void updateNameEntry()
        {
            _Timer -= Time.unscaledDeltaTime;
            _TimerText.text = Mathf.CeilToInt(Mathf.Max(_Timer, 0f)).ToString();
            if (_Timer <= 0f)
            {
                endNameEntry();
                return;
            }

            //Our own crosshair, the game cursor is hidden
            Vector2 Pointer = Mouse.current != null ? Mouse.current.position.ReadValue() : new Vector2(-1000f, -1000f);
            Vector2 LocalPointer;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(_CanvasTransform, Pointer, null, out LocalPointer))
                _Crosshair.anchoredPosition = LocalPointer;

            int HoveredKey = -1;
            for (int i = 0; i < _Keys.Length; i++)
            {
                bool IsHovered = RectTransformUtility.RectangleContainsScreenPoint(_Keys[i].rectTransform, Pointer, null);
                _Keys[i].color = IsHovered ? KEY_HOVER_COLOR : KEY_COLOR;
                _KeyTexts[i].color = IsHovered ? HIGHLIGHT_COLOR : Color.white;
                if (IsHovered)
                    HoveredKey = i;
            }
            if (HoveredKey >= 0 && Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
                pressKey(KEYS[HoveredKey]);

            bool IsCursorVisible = _Name.Length < NAME_MAX_LENGTH && (Time.unscaledTime % 0.6f) < 0.3f;
            _NameText.text = _Name + (IsCursorVisible ? "_" : " ");
        }

        private void pressKey(string Key)
        {
            if (Key == KEY_END)
            {
                endNameEntry();
            }
            else if (Key == KEY_DELETE)
            {
                if (_Name.Length > 0)
                    _Name = _Name.Substring(0, _Name.Length - 1);
                CR_AudioManager.PlayUIAudioCancel(gameObject);
            }
            else if (_Name.Length < NAME_MAX_LENGTH)
            {
                _Name += Key;
                CR_AudioManager.PlayUIAudioSelection(gameObject);
            }
            else
            {
                CR_AudioManager.PlayUIAudioError(gameObject);
            }
        }

        /// <summary>
        /// Saving the score in the game's leaderboard, then displaying the ranking
        /// </summary>
        private void endNameEntry()
        {
            string Name = _Name.Trim();
            if (Name.Length == 0)
                Name = DEFAULT_NAME;
            int Score = _P1Score + _P2Score;
            int Position = getRankingPosition(Score);

            MP_SaveDataHandler.Leaderboards.TryToSavePlayerResults(Hotd2r_ArcadePlugin.Configurator.Mode, Hotd2r_ArcadePlugin.Configurator.GameScoreMode, Score, Name, _P1Score, _P2Score);
            MP_SaveLoad.Save(this, false, null);
            Hotd2r_ArcadePlugin.MyLogger.LogMessage("ArcadeRanking.endNameEntry() => " + Name + " saved in the ranking, position " + (Position + 1));
            CR_AudioManager.PlayUIAudioAccept(gameObject);

            _State = State.GameOverRanking;
            _Timer = GAME_OVER_RANKING_DURATION;
            showRanking(getLeaderboard(), Position, false);
        }

        private void endGameOverRanking()
        {
            hide();
            MP_MusicManager.StopMusic();
            CR_LevelLoader.LoadMainMenu();
        }

        private void showRanking(Leaderboard Board, int HighlightedPosition, bool ShowFooter)
        {
            _RankingTitle.text = Hotd2r_ArcadePlugin.TextLangs[LanguageStrings.StringName.Ranking];
            for (int i = 0; i < _RankTexts.Length; i++)
            {
                bool HasData = Board != null && i < Board.Data.Length && Board.Data[i].Score > 0f;
                _RankTexts[i].text = (i + 1).ToString();
                _RankNames[i].text = HasData ? Board.Data[i].PlayerName : "---";
                _RankScores[i].text = HasData ? ((int)Board.Data[i].Score).ToString("D7") : "-------";
                Color RowColor = i == HighlightedPosition ? HIGHLIGHT_COLOR : Color.white;
                _RankTexts[i].color = RowColor;
                _RankNames[i].color = RowColor;
                _RankScores[i].color = RowColor;
            }
            _RankingFooter.gameObject.SetActive(ShowFooter);
            _NameEntryPanel.SetActive(false);
            _RankingPanel.SetActive(true);
            _Canvas.enabled = true;
        }

        private void hide()
        {
            _State = State.None;
            if (_Canvas != null)
                _Canvas.enabled = false;
        }

        private static Leaderboard getLeaderboard()
        {
            if (!MP_SaveDataHandler.IsReady || MP_SaveDataHandler.Leaderboards == null)
                return null;
            return MP_SaveDataHandler.Leaderboards.GetLeaderboard(Hotd2r_ArcadePlugin.Configurator.Mode, Hotd2r_ArcadePlugin.Configurator.GameScoreMode);
        }

        /// <summary>
        /// Same rule as the game's leaderboard : -1 if the score is not good enough
        /// </summary>
        private static int getRankingPosition(int Score)
        {
            Leaderboard Board = getLeaderboard();
            if (Board == null || Score <= 0)
                return -1;
            for (int i = 0; i < Board.Data.Length; i++)
            {
                if (Score > Board.Data[i].Score)
                    return i;
            }
            return -1;
        }

        #region UI creation

        private bool createCanvas()
        {
            if (_Canvas != null)
                return true;
            if (Hotd2r_ArcadePlugin.GameFont == null)
                return false;

            GameObject CanvasObject = new GameObject("Hotd2Remake_ArcadeRanking");
            DontDestroyOnLoad(CanvasObject);
            _Canvas = CanvasObject.AddComponent<Canvas>();
            _Canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            //Above ArcadeOverlay
            _Canvas.sortingOrder = 1100;
            CanvasScaler Scaler = CanvasObject.AddComponent<CanvasScaler>();
            Scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            Scaler.referenceResolution = new Vector2(1920f, 1080f);
            Scaler.matchWidthOrHeight = 0.5f;
            _CanvasTransform = (RectTransform)CanvasObject.transform;

            //Name entry : title, score, name, timer, keyboard and crosshair
            _NameEntryPanel = createPanel("NameEntry");
            _NameEntryTitle = createText(_NameEntryPanel.transform, "Title", new Vector2(0f, 430f), 80f, TextAlignmentOptions.Center);
            _ScoreText = createText(_NameEntryPanel.transform, "Score", new Vector2(0f, 340f), 50f, TextAlignmentOptions.Center);
            _NameText = createText(_NameEntryPanel.transform, "Name", new Vector2(0f, 230f), 100f, TextAlignmentOptions.Center);
            _NameText.color = HIGHLIGHT_COLOR;
            _TimerText = createText(_NameEntryPanel.transform, "Timer", new Vector2(820f, 430f), 80f, TextAlignmentOptions.Center);
            int Rows = (KEYS.Length + KEYS_PER_ROW - 1) / KEYS_PER_ROW;
            for (int i = 0; i < KEYS.Length; i++)
            {
                int Column = i % KEYS_PER_ROW;
                int Row = i / KEYS_PER_ROW;
                Vector2 Position = new Vector2((Column - (KEYS_PER_ROW - 1) / 2f) * (KEY_SIZE.x + KEY_SPACING), -120f - (Row - (Rows - 1) / 2f) * (KEY_SIZE.y + KEY_SPACING));
                _Keys[i] = createImage(_NameEntryPanel.transform, "Key_" + KEYS[i], Position, KEY_SIZE, KEY_COLOR);
                _KeyTexts[i] = createText(_Keys[i].transform, "Text", Vector2.zero, KEYS[i].Length > 1 ? 45f : 70f, TextAlignmentOptions.Center, KEY_SIZE.x);
                _KeyTexts[i].text = KEYS[i];
            }
            GameObject CrosshairObject = new GameObject("Crosshair", typeof(RectTransform));
            CrosshairObject.transform.SetParent(_NameEntryPanel.transform, false);
            _Crosshair = (RectTransform)CrosshairObject.transform;
            _Crosshair.sizeDelta = Vector2.zero;
            createImage(_Crosshair, "Horizontal", Vector2.zero, new Vector2(50f, 6f), Color.white);
            createImage(_Crosshair, "Vertical", Vector2.zero, new Vector2(6f, 50f), Color.white);

            //Ranking : title, 10 rows (position, name, score) and the credits text for the title screen
            _RankingPanel = createPanel("Ranking");
            _RankingTitle = createText(_RankingPanel.transform, "Title", new Vector2(0f, 430f), 90f, TextAlignmentOptions.Center);
            int RowCount = CR_Data.NUMBER_OF_RESULTS_STORED_IN_LEADERBOARD;
            _RankTexts = new TextMeshProUGUI[RowCount];
            _RankNames = new TextMeshProUGUI[RowCount];
            _RankScores = new TextMeshProUGUI[RowCount];
            for (int i = 0; i < RowCount; i++)
            {
                float y = 300f - i * 70f;
                _RankTexts[i] = createText(_RankingPanel.transform, "Position_" + i, new Vector2(-450f, y), 55f, TextAlignmentOptions.Right, 120f);
                _RankNames[i] = createText(_RankingPanel.transform, "Name_" + i, new Vector2(-50f, y), 55f, TextAlignmentOptions.Left, 600f);
                _RankScores[i] = createText(_RankingPanel.transform, "Score_" + i, new Vector2(430f, y), 55f, TextAlignmentOptions.Right, 400f);
            }
            _RankingFooter = createText(_RankingPanel.transform, "Footer", new Vector2(0f, -440f), 40f, TextAlignmentOptions.Center);

            _Canvas.enabled = false;
            Hotd2r_ArcadePlugin.MyLogger.LogMessage("ArcadeRanking.createCanvas() => Ranking created");
            return true;
        }

        private GameObject createPanel(string Name)
        {
            Image Background = createImage(_Canvas.transform, Name, Vector2.zero, Vector2.zero, BACKGROUND_COLOR);
            RectTransform PanelTransform = Background.rectTransform;
            PanelTransform.anchorMin = Vector2.zero;
            PanelTransform.anchorMax = Vector2.one;
            PanelTransform.sizeDelta = Vector2.zero;
            return Background.gameObject;
        }

        private Image createImage(Transform Parent, string Name, Vector2 Position, Vector2 Size, Color ImageColor)
        {
            GameObject ImageObject = new GameObject(Name);
            ImageObject.transform.SetParent(Parent, false);
            Image NewImage = ImageObject.AddComponent<Image>();
            NewImage.color = ImageColor;
            NewImage.raycastTarget = false;
            NewImage.rectTransform.anchoredPosition = Position;
            NewImage.rectTransform.sizeDelta = Size;
            return NewImage;
        }

        private TextMeshProUGUI createText(Transform Parent, string Name, Vector2 Position, float FontSize, TextAlignmentOptions Alignment, float Width = 1600f)
        {
            GameObject TextObject = new GameObject(Name);
            TextObject.transform.SetParent(Parent, false);
            TextMeshProUGUI Text = TextObject.AddComponent<TextMeshProUGUI>();
            Text.font = Hotd2r_ArcadePlugin.GameFont;
            if (Hotd2r_ArcadePlugin.GameFontMaterial != null)
                Text.fontSharedMaterial = Hotd2r_ArcadePlugin.GameFontMaterial;
            Text.fontSize = FontSize;
            Text.alignment = Alignment;
            Text.color = Color.white;
            Text.raycastTarget = false;
            Text.enableWordWrapping = false;
            Text.rectTransform.anchoredPosition = Position;
            Text.rectTransform.sizeDelta = new Vector2(Width, FontSize * 1.5f);
            return Text;
        }

        #endregion
    }
}
