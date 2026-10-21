using HarmonyLib;

namespace Hotd2Remake_ArcadePlugin
{
    class mHD_SkipController
    {
        /// <summary>
        /// Holding START (any player) is skipping cutscenes, like the game's own skip input
        /// </summary>
        [HarmonyPatch(typeof(HD_SkipController), "Update")]
        class Update
        {
            static void Postfix(ref bool ___anyInputReceived, ref bool ___skipInputReceived)
            {
                if (Hotd2r_ArcadePlugin.IsAnyStartHeld())
                {
                    ___anyInputReceived = true;
                    ___skipInputReceived = true;
                }
            }
        }
    }
}
