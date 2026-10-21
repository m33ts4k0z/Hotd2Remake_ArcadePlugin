using System.Text.RegularExpressions;
using HarmonyLib;
using TMPro;
using UnityEngine;

namespace Hotd2Remake_ArcadePlugin
{
    class mCR_CutsceneUI
    {
        private static TMP_Text[] _SkipHintTexts = new TMP_Text[0];
        private static Regex _SpriteTag = new Regex("<sprite[^>]*>");

        /// <summary>
        /// Cutscenes "Hold [button icon] to skip!" hint is a localized text with an inline sprite of the current controller (mouse, keyboard, gamepad)
        /// Saving the texts, to replace the icon by START
        /// </summary>
        [HarmonyPatch(typeof(CR_CutsceneUI), "Awake")]
        class Awake
        {
            static void Postfix(GameObject ___skipHint)
            {
                if (___skipHint != null)
                    _SkipHintTexts = ___skipHint.GetComponentsInChildren<TMP_Text>(true);
            }
        }

        /// <summary>
        /// Called every frame by the plugin, as the game's localization component is setting the original text back each time the hint is displayed
        /// </summary>
        public static void ReplaceSkipHintIcon()
        {
            for (int i = 0; i < _SkipHintTexts.Length; i++)
            {
                TMP_Text Text = _SkipHintTexts[i];
                if (Text != null && Text.isActiveAndEnabled && Text.text != null && Text.text.Contains("<sprite"))
                    Text.text = _SpriteTag.Replace(Text.text, "START");
            }
        }
    }
}
