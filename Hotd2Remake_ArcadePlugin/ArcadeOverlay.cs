using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Hotd2Remake_ArcadePlugin
{
    /// <summary>
    /// Our own texts on top of the game, during gameplay :
    /// - "PRESS START" / "INSERT COIN(S)" on the side of a player who is not in the game
    /// - Credits at the bottom of the screen
    /// - Continue countdowns (see ContinueCountdown), like argonlefou's House of the Dead Remake Arcade Plugin : "CONTINUE ?" with the countdown and
    ///   "PRESS START" / "INSERT COIN(S)" under it, in the middle of the dead player's half of the screen, or in the middle of the screen when everybody is dead
    /// Created once the game font is known (saved from the title screen)
    /// </summary>
    public class ArcadeOverlay : MonoBehaviour
    {
        //PRESS START / INSERT COIN(S) are fading out and in, like argonlefou's plugin
        private const float BLINK_VISIBLE_DURATION = 0.5f;
        private const float BLINK_FADE_DURATION = 0.5f;

        private const float COUNTDOWN_FONT_SIZE = 220f;
        private const float CONTINUE_FONT_SIZE = 80f;
        private const float GAME_OVER_FONT_SIZE = 100f;
        private const float PROMPT_FONT_SIZE = 55f;
        private const float COUNTDOWN_HEIGHT = 40f;
        private const float PROMPT_HEIGHT = -190f;

        private Canvas _Canvas = null;
        private TextMeshProUGUI[] _PlayerTexts = new TextMeshProUGUI[Hotd2r_ArcadePlugin.MAX_PLAYERS];
        private TextMeshProUGUI _CreditsText;
        private TextMeshProUGUI[] _CountdownTexts = new TextMeshProUGUI[Hotd2r_ArcadePlugin.MAX_PLAYERS];
        private TextMeshProUGUI[] _CountdownPrompts = new TextMeshProUGUI[Hotd2r_ArcadePlugin.MAX_PLAYERS];
        private TextMeshProUGUI _GeneralCountdownText;
        private TextMeshProUGUI _GeneralCountdownPrompt;

        private void Update()
        {
            if (_Canvas == null)
            {
                if (Hotd2r_ArcadePlugin.GameFont == null)
                    return;
                createCanvas();
            }

            bool IsInGame = CR_LevelLoader.IsGameplayChapterLoaded && !CR_LevelLoader.IsLoadingScreenVisible;
            _Canvas.enabled = IsInGame;
            if (!IsInGame)
                return;

            //Joining is only possible in a Multiplayer campaign
            bool CanJoin = MP_SaveDataHandler.IsReady && MP_SaveDataHandler.LastPlayedCampaign != null && MP_SaveDataHandler.LastPlayedCampaign.IsMultiplayerCampaign;
            float BlinkAlpha = getBlinkAlpha();
            for (int i = 0; i < Hotd2r_ArcadePlugin.MAX_PLAYERS; i++)
            {
                CR_Player GamePlayer = CR_Player.Players[i];
                bool IsWaiting = CanJoin && (GamePlayer == null || !GamePlayer.IsActivated);
                _PlayerTexts[i].enabled = IsWaiting;
                if (IsWaiting)
                {
                    _PlayerTexts[i].text = getPromptString(Hotd2r_ArcadePlugin.Configurator.CreditsToStart);
                    _PlayerTexts[i].alpha = BlinkAlpha;
                }
            }
            _CreditsText.text = Hotd2r_ArcadePlugin.GetCreditsString();

            //Hidden with the death screens during cutscenes
            bool ShowCountdowns = !CR_Cutscene.IsCutsceneActive && !CR_ChapterCompleteMenu.IsActive;
            for (int i = 0; i < Hotd2r_ArcadePlugin.MAX_PLAYERS; i++)
                setCountdownTexts(_CountdownTexts[i], _CountdownPrompts[i], ShowCountdowns ? ContinueCountdown.GetPlayerText(i) : string.Empty, BlinkAlpha);
            setCountdownTexts(_GeneralCountdownText, _GeneralCountdownPrompt, ShowCountdowns ? ContinueCountdown.GetGeneralText() : string.Empty, BlinkAlpha);
        }

        private void setCountdownTexts(TextMeshProUGUI Countdown, TextMeshProUGUI Prompt, string s, float BlinkAlpha)
        {
            bool IsVisible = s.Length > 0;
            Countdown.enabled = IsVisible;
            Prompt.enabled = IsVisible;
            if (!IsVisible)
                return;

            //A digit, or GAME OVER
            if (s.Length == 1)
                Countdown.text = "<size=" + CONTINUE_FONT_SIZE + ">" + Hotd2r_ArcadePlugin.TextLangs[LanguageStrings.StringName.Continue] + "</size>\n" + s;
            else
                Countdown.text = "<size=" + GAME_OVER_FONT_SIZE + ">" + s + "</size>";
            Prompt.text = getPromptString(Hotd2r_ArcadePlugin.Configurator.CreditsToContinue);
            Prompt.alpha = BlinkAlpha;
        }

        /// <summary>
        /// Visible, fading out, fading in
        /// </summary>
        private static float getBlinkAlpha()
        {
            float t = Time.unscaledTime % (BLINK_VISIBLE_DURATION + 2f * BLINK_FADE_DURATION);
            if (t < BLINK_VISIBLE_DURATION)
                return 1f;
            t -= BLINK_VISIBLE_DURATION;
            if (t < BLINK_FADE_DURATION)
                return 1f - t / BLINK_FADE_DURATION;
            return (t - BLINK_FADE_DURATION) / BLINK_FADE_DURATION;
        }

        private string getPromptString(int Credits)
        {
            if (Hotd2r_ArcadePlugin.CanPay(Credits))
                return Hotd2r_ArcadePlugin.TextLangs[LanguageStrings.StringName.PressStart];
            if (Hotd2r_ArcadePlugin.CurrentCredits > 0)
                return Hotd2r_ArcadePlugin.TextLangs[LanguageStrings.StringName.InsertMoreCoins];
            return Hotd2r_ArcadePlugin.TextLangs[LanguageStrings.StringName.InsertCoins];
        }

        private void createCanvas()
        {
            GameObject CanvasObject = new GameObject("Hotd2Remake_ArcadeOverlay");
            DontDestroyOnLoad(CanvasObject);
            _Canvas = CanvasObject.AddComponent<Canvas>();
            _Canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _Canvas.sortingOrder = 1000;
            CanvasScaler Scaler = CanvasObject.AddComponent<CanvasScaler>();
            Scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            Scaler.referenceResolution = new Vector2(1920f, 1080f);
            Scaler.matchWidthOrHeight = 0.5f;

            _PlayerTexts[0] = createText("P1_Text", new Vector2(0.0f, 0.0f), new Vector2(0.0f, 0.0f), TextAlignmentOptions.BottomLeft, new Vector2(40f, 30f), 40f);
            _PlayerTexts[1] = createText("P2_Text", new Vector2(1.0f, 0.0f), new Vector2(1.0f, 0.0f), TextAlignmentOptions.BottomRight, new Vector2(-40f, 30f), 40f);
            _CreditsText = createText("Credits_Text", new Vector2(0.5f, 0.0f), new Vector2(0.5f, 0.0f), TextAlignmentOptions.Bottom, new Vector2(0f, 30f), 40f);

            //Centered on the middle of each half of the screen, or of the screen
            Vector2 Center = new Vector2(0.5f, 0.5f);
            Vector2[] CountdownAnchors = new Vector2[] { new Vector2(0.25f, 0.5f), new Vector2(0.75f, 0.5f) };
            for (int i = 0; i < Hotd2r_ArcadePlugin.MAX_PLAYERS; i++)
            {
                _CountdownTexts[i] = createText("P" + (i + 1) + "_Countdown", CountdownAnchors[i], Center, TextAlignmentOptions.Center, new Vector2(0f, COUNTDOWN_HEIGHT), COUNTDOWN_FONT_SIZE);
                _CountdownPrompts[i] = createText("P" + (i + 1) + "_CountdownPrompt", CountdownAnchors[i], Center, TextAlignmentOptions.Center, new Vector2(0f, PROMPT_HEIGHT), PROMPT_FONT_SIZE);
            }
            _GeneralCountdownText = createText("Countdown", Center, Center, TextAlignmentOptions.Center, new Vector2(0f, COUNTDOWN_HEIGHT), COUNTDOWN_FONT_SIZE);
            _GeneralCountdownPrompt = createText("CountdownPrompt", Center, Center, TextAlignmentOptions.Center, new Vector2(0f, PROMPT_HEIGHT), PROMPT_FONT_SIZE);
            Hotd2r_ArcadePlugin.MyLogger.LogMessage("ArcadeOverlay.createCanvas() => Overlay created with font " + Hotd2r_ArcadePlugin.GameFont.name);
        }

        private TextMeshProUGUI createText(string Name, Vector2 Anchor, Vector2 Pivot, TextAlignmentOptions Alignment, Vector2 Offset, float FontSize)
        {
            GameObject TextObject = new GameObject(Name);
            TextObject.transform.SetParent(_Canvas.transform, false);
            TextMeshProUGUI Text = TextObject.AddComponent<TextMeshProUGUI>();
            Text.font = Hotd2r_ArcadePlugin.GameFont;
            if (Hotd2r_ArcadePlugin.GameFontMaterial != null)
                Text.fontSharedMaterial = Hotd2r_ArcadePlugin.GameFontMaterial;
            Text.fontSize = FontSize;
            Text.alignment = Alignment;
            Text.color = Color.white;
            Text.raycastTarget = false;
            Text.enableWordWrapping = false;

            RectTransform TextTransform = Text.rectTransform;
            TextTransform.anchorMin = Anchor;
            TextTransform.anchorMax = Anchor;
            TextTransform.pivot = Pivot;
            TextTransform.anchoredPosition = Offset;
            TextTransform.sizeDelta = new Vector2(900f, 100f);
            return Text;
        }
    }
}
