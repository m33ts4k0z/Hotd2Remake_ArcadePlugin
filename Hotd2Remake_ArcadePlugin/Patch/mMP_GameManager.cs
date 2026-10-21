using HarmonyLib;

namespace Hotd2Remake_ArcadePlugin
{
    class mMP_GameManager
    {
        /// <summary>
        /// Like argonlefou's House of the Dead Remake Arcade Plugin : the game is not paused when the window is losing the focus
        /// </summary>
        [HarmonyPatch(typeof(MP_GameManager), "OnApplicationFocus")]
        class OnApplicationFocus
        {
            static bool Prefix()
            {
                return false;
            }
        }

        /// <summary>
        /// Like argonlefou's House of the Dead Remake Arcade Plugin : no in-game pause menu (the pause input is ignored, Escape is quitting the game)
        /// The game is still allowed to pause itself without menu (all players down, tutorials...)
        /// </summary>
        [HarmonyPatch(typeof(MP_GameManager), "pause")]
        class pause
        {
            static bool Prefix(bool _instantiatePauseMenu, ref bool __result)
            {
                if (_instantiatePauseMenu)
                {
                    __result = false;
                    return false;
                }
                return true;
            }
        }
    }
}
