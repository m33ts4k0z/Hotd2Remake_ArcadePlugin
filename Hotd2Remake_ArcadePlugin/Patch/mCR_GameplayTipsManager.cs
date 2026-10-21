using HarmonyLib;
using TMPro;

namespace Hotd2Remake_ArcadePlugin
{
    class mCR_GameplayTipsManager
    {
        /// <summary>
        /// Like argonlefou's House of the Dead Remake Arcade Plugin : simply removing the tips displayed during the loading screen
        /// Some of them have nothing to do with the arcade game mode
        /// </summary>
        [HarmonyPatch(typeof(CR_GameplayTipsManager), "updateText")]
        class updateText
        {
            static void Postfix(TMP_Text ___textField)
            {
                if (___textField != null)
                    ___textField.text = Hotd2r_ArcadePlugin.TextLangs[LanguageStrings.StringName.Loading];
            }
        }
    }
}
