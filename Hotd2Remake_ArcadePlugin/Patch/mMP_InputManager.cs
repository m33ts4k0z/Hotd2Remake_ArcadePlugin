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
        /// <summary>
        /// The game is also stopping the time and the videos when the window is losing the focus
        /// Disabled like the pause on focus loss (mMP_GameManager)
        /// </summary>
        [HarmonyPatch(typeof(MP_InputManager), "OnApplicationFocus")]
        class OnApplicationFocus
        {
            static bool Prefix()
            {
                return false;
            }
        }

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
