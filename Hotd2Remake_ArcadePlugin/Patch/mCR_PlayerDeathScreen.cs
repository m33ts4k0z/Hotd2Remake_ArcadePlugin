using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

        private static AccessTools.FieldRef<CR_PlayerDeathScreen.HintGroup, TMP_Text> _HintText = AccessTools.FieldRefAccess<CR_PlayerDeathScreen.HintGroup, TMP_Text>("hintText");
        private static AccessTools.FieldRef<CR_PlayerDeathScreen.HintGroup, Image> _HintImage = AccessTools.FieldRefAccess<CR_PlayerDeathScreen.HintGroup, Image>("hintImage");

        /// <summary>
        /// Arcade hints instead of the controller ones :
        /// - "Continue" (with the keyboard/gamepad button icon) is replaced by PRESS START, or INSERT COIN(S) if there are not enough credits
        /// - "Buy credit" (continue paid with the score) is hidden, continues are paid with credits
        /// - "CONTINUE ?" banner (continueGroup) stays visible without credits, as a coin can still be inserted during the countdown
        /// </summary>
        [HarmonyPatch(typeof(CR_PlayerDeathScreen), "updateHintState")]
        class updateHintState
        {
            static void Postfix(CR_PlayerDeathScreen.HintGroup ___continueHint, CR_PlayerDeathScreen.HintGroup ___buyCreditsHint, GameObject ___continueGroup, Color ___hintVisibleColor, ref bool ___playerCanContinue)
            {
                ___playerCanContinue = Hotd2r_ArcadePlugin.CanPay(Hotd2r_ArcadePlugin.Configurator.CreditsToContinue);
                if (___continueGroup != null)
                    ___continueGroup.SetActive(true);

                if (___continueHint != null)
                {
                    ___continueHint.SetColor(___hintVisibleColor);
                    TMP_Text Text = _HintText(___continueHint);
                    if (Text != null)
                    {
                        if (___playerCanContinue)
                            Text.text = Hotd2r_ArcadePlugin.TextLangs[LanguageStrings.StringName.PressStart];
                        else if (Hotd2r_ArcadePlugin.CurrentCredits > 0)
                            Text.text = Hotd2r_ArcadePlugin.TextLangs[LanguageStrings.StringName.InsertMoreCoins];
                        else
                            Text.text = Hotd2r_ArcadePlugin.TextLangs[LanguageStrings.StringName.InsertCoins];
                    }
                    Image Icon = _HintImage(___continueHint);
                    if (Icon != null)
                        Icon.enabled = false;
                }

                //The whole row is hidden : the text is a child of the row background image (img_BuyCreditsButton)
                if (___buyCreditsHint != null)
                {
                    TMP_Text Text = _HintText(___buyCreditsHint);
                    if (Text != null && Text.transform.parent != null)
                        Text.transform.parent.gameObject.SetActive(false);
                }
            }
        }
    }
}
