using HarmonyLib;
using MegaPixel.Menus;
using TMPro;
using UnityEngine.UI;

namespace Hotd2Remake_ArcadePlugin
{
    class mCR_ChapterCompleteMenu
    {
        /// <summary>
        /// Chapter results, like argonlefou's House of the Dead Remake Arcade Plugin : no restart, chapter selection, main menu or exit buttons.
        /// Only "Next", displayed as PRESS START : START (or a timer) is going on to the next chapter (see EndOfGameScreens)
        /// Called once the results are displayed
        /// </summary>
        [HarmonyPatch(typeof(CR_ChapterCompleteMenu), "registerButtonListeners")]
        class registerButtonListeners
        {
            static void Postfix(CR_ChapterCompleteMenu __instance, MP_Button ___nextButton, MP_Button ___restartButton, MP_Button ___chapterSelectionButton, MP_Button ___backToMainMenuButton, MP_Button ___exitButton)
            {
                if (!Hotd2r_ArcadePlugin.IsArcadeGameRunning)
                    return;

                foreach (MP_Button Button in new MP_Button[] { ___restartButton, ___chapterSelectionButton, ___backToMainMenuButton, ___exitButton })
                {
                    if (Button != null)
                        Button.gameObject.SetActive(false);
                }

                if (___nextButton != null && ___nextButton.gameObject.activeSelf)
                {
                    TMP_Text Label = ___nextButton.GetComponentInChildren<TMP_Text>(true);
                    if (Label != null)
                    {
                        Label.text = Hotd2r_ArcadePlugin.TextLangs[LanguageStrings.StringName.PressStart];
                        //Controller icon
                        foreach (Image Icon in Label.GetComponentsInChildren<Image>(true))
                            Icon.enabled = false;
                    }
                    ___nextButton.Select();
                }
                Hotd2r_ArcadePlugin.MyLogger.LogMessage("CR_ChapterCompleteMenu.registerButtonListeners() => Chapter results");
                EndOfGameScreens.OnChapterResultsShown(__instance, ___nextButton);
            }
        }
    }
}
