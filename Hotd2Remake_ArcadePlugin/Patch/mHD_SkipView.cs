using HarmonyLib;
using TMPro;

namespace Hotd2Remake_ArcadePlugin
{
    class mHD_SkipView
    {
        /// <summary>
        /// "HOLD [button icon] TO SKIP!" is showing the keyboard/mouse/gamepad icon : replaced by START, keeping the game's own translations
        /// </summary>
        [HarmonyPatch(typeof(HD_SkipView), "updateSkipHintText")]
        class updateSkipHintText
        {
            static void Postfix(TMP_Text ___skipHint, string ___skipHintPart1Translation, string ___skipHintPart2Translation)
            {
                if (___skipHint != null)
                    ___skipHint.text = ___skipHintPart1Translation + "START" + ___skipHintPart2Translation;
            }
        }
    }
}
