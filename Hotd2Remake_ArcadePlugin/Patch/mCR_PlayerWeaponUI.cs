using HarmonyLib;

namespace Hotd2Remake_ArcadePlugin
{
    class mCR_PlayerWeaponUI
    {
        /// <summary>
        /// Like argonlefou's House of the Dead Remake Arcade Plugin : no weapon name next to the ammo, like the original arcade
        /// </summary>
        [HarmonyPatch(typeof(CR_PlayerWeaponUI), "updateWeaponText")]
        class updateWeaponText
        {
            static void Prefix(ref string _name)
            {
                if (Hotd2r_ArcadePlugin.Configurator.ArcadeHud)
                    _name = string.Empty;
            }
        }
    }
}
