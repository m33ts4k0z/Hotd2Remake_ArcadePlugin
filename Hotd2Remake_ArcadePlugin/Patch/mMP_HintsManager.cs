using HarmonyLib;
using MegaPixel.Menus;

namespace Hotd2Remake_ArcadePlugin
{
    class mMP_HintsManager
    {
        /// <summary>
        /// Like argonlefou's House of the Dead Remake Arcade Plugin : no controller hints (Navigation / Accept / Back with keyboard or gamepad icons) in menus
        /// Tutorial hints are kept
        /// All menus and popups are using this hint bar, so hiding it here instead of in each menu
        /// </summary>
        [HarmonyPatch(typeof(MP_HintsManager), "SetManagerActive")]
        class SetManagerActive
        {
            static bool Prefix(MP_HintsManager __instance, ref bool _active)
            {
                if (_active && (__instance.EnabledHints & HintType.Tutorial) == 0)
                    _active = false;
                return true;
            }
        }
    }
}
