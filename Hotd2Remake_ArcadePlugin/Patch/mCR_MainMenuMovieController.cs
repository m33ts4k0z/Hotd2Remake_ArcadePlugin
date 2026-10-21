using System.Reflection;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace Hotd2Remake_ArcadePlugin
{
    /// <summary>
    /// Title screen (video loop with "PRESS ANY KEY") turned into an attract mode, like argonlefou's House of the Dead Remake Arcade Plugin :
    /// - Starts with the main video, with the game logo at the beginning
    /// - Text replaced by the credits status, at the bottom center of the screen
    /// - START is handled in the plugin Update() loop, to start a new game
    /// - Any other input is opening the game's main menu, unless disabled in the config file
    /// </summary>
    class mCR_MainMenuMovieController
    {
        private static AccessTools.FieldRef<CR_SimplePressAnyKeyDisplay, TMP_Text> _PressAnyKeyText = AccessTools.FieldRefAccess<CR_SimplePressAnyKeyDisplay, TMP_Text>("textField");
        private static MethodInfo _SetVideo = AccessTools.Method(typeof(CR_MainMenuMovieController), "setVideo");
        private static MethodInfo _SwapVideo = AccessTools.Method(typeof(CR_MainMenuMovieController), "swapVideo");

        //Game logo, the same as in the game's main menu
        private const string LOGO_SPRITE_NAME = "sp_ui_LogoRender";
        private const float LOGO_WIDTH_RATIO = 0.45f;
        private const float LOGO_FADE_IN_END = 0.5f;
        private const float LOGO_FADE_OUT_START = 5.0f;
        private const float LOGO_FADE_OUT_DURATION = 4.0f;
        private static Image _Logo = null;
        //The video player is going back to the beginning of the main video when it ends, before the game changes the clip : the logo is only shown once per clip
        private static bool _LogoShownForThisClip = false;

        [HarmonyPatch(typeof(CR_MainMenuMovieController), "Start")]
        class Start
        {
            static void Postfix(CR_MainMenuMovieController __instance, CR_SimplePressAnyKeyDisplay ___simplePressAnyKey, RawImage ___rawImageReference)
            {
                _Logo = createLogo(___rawImageReference);
                moveText(___simplePressAnyKey);

                //Game started with "PRESS ANY KEY" : the game is playing the tutorial video first, we want the main one with the logo
                if (___simplePressAnyKey.IsVisible && _SetVideo != null)
                    _SetVideo.Invoke(__instance, new object[] { true });
            }
        }

        [HarmonyPatch(typeof(CR_MainMenuMovieController), "Update")]
        class Update
        {
            static bool Prefix(CR_MainMenuMovieController __instance, CR_SimplePressAnyKeyDisplay ___simplePressAnyKey, CR_MainMenu ___mainMenu, MP_VideoPlayerController ___mainMenuVideoController, VideoClip ___mainVideoClip, Image ___blackScreenImage)
            {
                Hotd2r_ArcadePlugin.TitleScreen = __instance;

                bool IsTitleScreen = !(___mainMenu.IsMainMenuOpen || ___mainMenu.IsMainMenuOpening) && ___simplePressAnyKey.IsVisible;
                updateLogo(IsTitleScreen && ___mainMenuVideoController != null && ___mainMenuVideoController.CurrentVideoClip == ___mainVideoClip ? ___mainMenuVideoController.CurrentVideoTime : -1.0, ___blackScreenImage);

                if (!IsTitleScreen)
                    return true;

                TMP_Text Text = _PressAnyKeyText(___simplePressAnyKey);
                if (Text != null)
                {
                    //Saving the game font for our own texts
                    if (Hotd2r_ArcadePlugin.GameFont == null)
                    {
                        Hotd2r_ArcadePlugin.GameFont = Text.font;
                        Hotd2r_ArcadePlugin.GameFontMaterial = Text.fontSharedMaterial;
                    }
                    Text.text = Hotd2r_ArcadePlugin.GetScreenPlayerString(Hotd2r_ArcadePlugin.Configurator.CreditsToStart);
                    //The ranking has its own text
                    Text.alpha = ArcadeRanking.Instance != null && ArcadeRanking.Instance.IsAttractRankingActive ? 0f : 1f;
                }

                return !Hotd2r_ArcadePlugin.Configurator.DisableMainMenu;
            }
        }

        /// <summary>
        /// Like argonlefou's plugin : the ranking is displayed between the videos, at the end of the main one.
        /// The next video is started once it is closed
        /// </summary>
        [HarmonyPatch(typeof(CR_MainMenuMovieController), "onVideoPlayerLoopPointReached")]
        class onVideoPlayerLoopPointReached
        {
            static bool Prefix(CR_MainMenuMovieController __instance, MP_VideoPlayerController ___mainMenuVideoController, VideoClip ___mainVideoClip)
            {
                if (!Hotd2r_ArcadePlugin.IsOnTitleScreen() || ___mainMenuVideoController == null || ___mainMenuVideoController.CurrentVideoClip != ___mainVideoClip || _SwapVideo == null || ArcadeRanking.Instance == null)
                    return true;

                return !ArcadeRanking.Instance.ShowAttractRanking(delegate
                {
                    if (__instance != null)
                        _SwapVideo.Invoke(__instance, null);
                });
            }
        }

        /// <summary>
        /// Logo at the top of the screen, using the game's own sprite
        /// </summary>
        private static Image createLogo(RawImage VideoImage)
        {
            Sprite LogoSprite = null;
            foreach (Sprite s in Resources.FindObjectsOfTypeAll<Sprite>())
            {
                if (s.name == LOGO_SPRITE_NAME)
                {
                    LogoSprite = s;
                    break;
                }
            }
            if (LogoSprite == null || VideoImage == null || VideoImage.canvas == null)
            {
                Hotd2r_ArcadePlugin.MyLogger.LogWarning("mCR_MainMenuMovieController.createLogo() => Game logo not found");
                return null;
            }

            Canvas RootCanvas = VideoImage.canvas.rootCanvas;
            GameObject LogoObject = new GameObject("ArcadeGameLogo");
            RectTransform LogoTransform = LogoObject.AddComponent<RectTransform>();
            LogoTransform.SetParent(RootCanvas.transform, false);
            LogoTransform.anchorMin = new Vector2(0.5f, 1.0f);
            LogoTransform.anchorMax = new Vector2(0.5f, 1.0f);
            LogoTransform.pivot = new Vector2(0.5f, 1.0f);
            LogoTransform.anchoredPosition = new Vector2(0.0f, -25.0f);
            float Width = ((RectTransform)RootCanvas.transform).rect.width * LOGO_WIDTH_RATIO;
            LogoTransform.sizeDelta = new Vector2(Width, Width * LogoSprite.rect.height / LogoSprite.rect.width);

            Image Logo = LogoObject.AddComponent<Image>();
            Logo.sprite = LogoSprite;
            Logo.raycastTarget = false;
            Logo.color = new Color(1.0f, 1.0f, 1.0f, 0.0f);
            return Logo;
        }

        /// <summary>
        /// Fading in at the beginning of the main video, and out after a few seconds
        /// VideoTime < 0 when the main video is not playing (other video, or no clip while the game is changing it)
        /// </summary>
        private static void updateLogo(double VideoTime, Image BlackScreen)
        {
            if (_Logo == null)
                return;

            float Alpha = 0.0f;
            if (VideoTime < 0.0)
            {
                _LogoShownForThisClip = false;
            }
            else if (!_LogoShownForThisClip)
            {
                if (VideoTime < LOGO_FADE_IN_END)
                {
                    Alpha = (float)(VideoTime / LOGO_FADE_IN_END);
                }
                else if (VideoTime < LOGO_FADE_OUT_START)
                {
                    Alpha = 1.0f;
                }
                else
                {
                    Alpha = Mathf.Clamp01(1.0f - (float)(VideoTime - LOGO_FADE_OUT_START) / LOGO_FADE_OUT_DURATION);
                    if (Alpha <= 0.0f)
                        _LogoShownForThisClip = true;
                }
            }

            //Following the game's own fades to black between the videos
            if (BlackScreen != null)
                Alpha *= 1.0f - BlackScreen.color.a;

            _Logo.color = new Color(1.0f, 1.0f, 1.0f, Alpha);
        }

        /// <summary>
        /// Credits text at the bottom center of the screen (it is on the bottom right corner in the original game)
        /// </summary>
        private static void moveText(CR_SimplePressAnyKeyDisplay PressAnyKey)
        {
            TMP_Text Text = _PressAnyKeyText(PressAnyKey);
            if (Text == null || Text.canvas == null)
                return;

            RectTransform TextTransform = Text.rectTransform;
            TextTransform.SetParent(Text.canvas.rootCanvas.transform, true);
            TextTransform.anchorMin = new Vector2(0.5f, 0.0f);
            TextTransform.anchorMax = new Vector2(0.5f, 0.0f);
            TextTransform.pivot = new Vector2(0.5f, 0.0f);
            TextTransform.anchoredPosition = new Vector2(0.0f, 60.0f);
            TextTransform.sizeDelta = new Vector2(1400.0f, 200.0f);
            TextTransform.localScale = Vector3.one;
            TextTransform.localRotation = Quaternion.identity;
            Text.enableAutoSizing = false;
            Text.enableWordWrapping = false;
            Text.alignment = TextAlignmentOptions.Bottom;
        }
    }
}
