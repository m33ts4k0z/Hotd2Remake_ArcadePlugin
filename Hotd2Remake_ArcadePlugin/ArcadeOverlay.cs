using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Hotd2Remake_ArcadePlugin
{
    /// <summary>
    /// Our own texts on top of the game, during gameplay :
    /// - "PRESS START" / "INSERT COIN(S)" blinking on the side of a player who is not in the game
    /// - Credits at the bottom of the screen
    /// Created once the game font is known (saved from the title screen)
    /// </summary>
    public class ArcadeOverlay : MonoBehaviour
    {
        private const float BLINK_PERIOD = 1.0f;
        private const float BLINK_VISIBLE_RATIO = 0.6f;
        private const float PLAYER_TEXT_HEIGHT = 30f;
        private const float PLAYER_TEXT_HEIGHT_DEATHSCREEN = 230f;

        private Canvas _Canvas = null;
        private TextMeshProUGUI[] _PlayerTexts = new TextMeshProUGUI[Hotd2r_ArcadePlugin.MAX_PLAYERS];
        private TextMeshProUGUI _CreditsText;

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
            bool IsBlinkVisible = (Time.unscaledTime % BLINK_PERIOD) < BLINK_PERIOD * BLINK_VISIBLE_RATIO;
            //The death screen has its own boxes in the bottom corners
            float TextHeight = isAnyPlayerDead() ? PLAYER_TEXT_HEIGHT_DEATHSCREEN : PLAYER_TEXT_HEIGHT;
            for (int i = 0; i < Hotd2r_ArcadePlugin.MAX_PLAYERS; i++)
            {
                CR_Player GamePlayer = CR_Player.Players[i];
                bool IsWaiting = CanJoin && (GamePlayer == null || !GamePlayer.IsActivated);
                _PlayerTexts[i].enabled = IsWaiting && IsBlinkVisible;
                if (IsWaiting)
                    _PlayerTexts[i].text = getPlayerString();
                _PlayerTexts[i].rectTransform.anchoredPosition = new Vector2(_PlayerTexts[i].rectTransform.anchoredPosition.x, TextHeight);
            }
            _CreditsText.text = Hotd2r_ArcadePlugin.GetCreditsString();
        }

        private bool isAnyPlayerDead()
        {
            for (int i = 0; i < Hotd2r_ArcadePlugin.MAX_PLAYERS; i++)
            {
                CR_Player GamePlayer = CR_Player.Players[i];
                if (GamePlayer != null && GamePlayer.IsActivated && GamePlayer.HasDied)
                    return true;
            }
            return false;
        }

        private string getPlayerString()
        {
            if (Hotd2r_ArcadePlugin.CanPay(Hotd2r_ArcadePlugin.Configurator.CreditsToStart))
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

            _PlayerTexts[0] = createText("P1_Text", new Vector2(0.0f, 0.0f), TextAlignmentOptions.BottomLeft, new Vector2(40f, 30f));
            _PlayerTexts[1] = createText("P2_Text", new Vector2(1.0f, 0.0f), TextAlignmentOptions.BottomRight, new Vector2(-40f, 30f));
            _CreditsText = createText("Credits_Text", new Vector2(0.5f, 0.0f), TextAlignmentOptions.Bottom, new Vector2(0f, 30f));
            Hotd2r_ArcadePlugin.MyLogger.LogMessage("ArcadeOverlay.createCanvas() => Overlay created with font " + Hotd2r_ArcadePlugin.GameFont.name);
        }

        private TextMeshProUGUI createText(string Name, Vector2 Anchor, TextAlignmentOptions Alignment, Vector2 Offset)
        {
            GameObject TextObject = new GameObject(Name);
            TextObject.transform.SetParent(_Canvas.transform, false);
            TextMeshProUGUI Text = TextObject.AddComponent<TextMeshProUGUI>();
            Text.font = Hotd2r_ArcadePlugin.GameFont;
            if (Hotd2r_ArcadePlugin.GameFontMaterial != null)
                Text.fontSharedMaterial = Hotd2r_ArcadePlugin.GameFontMaterial;
            Text.fontSize = 40;
            Text.alignment = Alignment;
            Text.color = Color.white;
            Text.raycastTarget = false;
            Text.enableWordWrapping = false;

            RectTransform TextTransform = Text.rectTransform;
            TextTransform.anchorMin = Anchor;
            TextTransform.anchorMax = Anchor;
            TextTransform.pivot = Anchor;
            TextTransform.anchoredPosition = Offset;
            TextTransform.sizeDelta = new Vector2(700f, 100f);
            return Text;
        }
    }
}
