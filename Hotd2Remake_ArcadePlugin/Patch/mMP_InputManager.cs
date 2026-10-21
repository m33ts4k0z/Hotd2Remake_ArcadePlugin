using HarmonyLib;
using MegaPixel.Input;

namespace Hotd2Remake_ArcadePlugin
{
    class mMP_InputManager
    {
        /// <summary>
        /// Holding START (any player) is skipping the in-game cutscenes, like the game's own skip input
        /// Used by CR_TimelineCutscene and CR_GameOverCutscene
        /// </summary>
        [HarmonyPatch(typeof(MP_InputManager), "IsAnyPlayerHoldingSkip")]
        class IsAnyPlayerHoldingSkip
        {
            static void Postfix(ref bool __result)
            {
                if (!__result && Hotd2r_ArcadePlugin.IsAnyStartHeld())
                    __result = true;
            }
        }
    }
}
