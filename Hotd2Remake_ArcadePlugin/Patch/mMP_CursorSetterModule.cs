using HarmonyLib;
using MegaPixel.ApplicationManagement;

namespace Hotd2Remake_ArcadePlugin
{
    class mMP_CursorSetterModule
    {
        /// <summary>
        /// Like argonlefou's House of the Dead Remake Arcade Plugin : force hiding the mouse cursor (HIDE_CURSOR in the config file)
        /// </summary>
        [HarmonyPatch(typeof(MP_CursorSetterModule), "changeCursorVisibility")]
        class changeCursorVisibility
        {
            static bool Prefix(ref bool _shouldBeVisible)
            {
                if (Hotd2r_ArcadePlugin.Configurator.HideCursor)
                    _shouldBeVisible = false;
                return true;
            }
        }
    }
}
