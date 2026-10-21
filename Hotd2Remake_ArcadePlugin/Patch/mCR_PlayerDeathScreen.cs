using HarmonyLib;
using TMPro;
using UnityEngine;

namespace Hotd2Remake_ArcadePlugin
{
    class mCR_PlayerDeathScreen
    {
        /// <summary>
        /// The game is displaying "FREE PLAY" as tokens are unlimited : displaying the credits count instead, unless the plugin is set to Freeplay
        /// </summary>
        [HarmonyPatch(typeof(CR_PlayerDeathScreen), "updateContinues")]
        class updateContinues
        {
            static void Postfix(TMP_Text ___currentContinuesText)
            {
                if (___currentContinuesText != null && !Hotd2r_ArcadePlugin.Configurator.Freeplay)
                    ___currentContinuesText.text = Hotd2r_ArcadePlugin.CurrentCredits.ToString("D2");
            }
        }

        /// <summary>
        /// "Continue" hint is only available if there are enough credits
        /// </summary>
        [HarmonyPatch(typeof(CR_PlayerDeathScreen), "updateHintState")]
        class updateHintState
        {
            static void Postfix(CR_PlayerDeathScreen.HintGroup ___continueHint, GameObject ___continueGroup, Color ___hintVisibleColor, Color ___hintDisabledColor, ref bool ___playerCanContinue)
            {
                ___playerCanContinue = Hotd2r_ArcadePlugin.CanPay(Hotd2r_ArcadePlugin.Configurator.CreditsToContinue);
                if (___continueHint != null)
                    ___continueHint.SetColor(___playerCanContinue ? ___hintVisibleColor : ___hintDisabledColor);
                if (___continueGroup != null)
                    ___continueGroup.SetActive(___playerCanContinue);
            }
        }
    }
}
