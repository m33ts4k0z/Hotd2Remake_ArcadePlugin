using HarmonyLib;

namespace Hotd2Remake_ArcadePlugin
{
    class mCR_DeathScreenManager
    {
        /// <summary>
        /// The continue countdown is started when a death screen is shown, but only if the manager is already enabled,
        /// which is done just after (in RedrawElement). So the countdown sometimes never starts, and without credits the game is stuck on the death screen.
        /// Checking again once the manager is enabled, as the pause menu is doing with UpdateDeathScrean()
        /// </summary>
        [HarmonyPatch(typeof(CR_DeathScreenManager), "OnEnable")]
        class OnEnable
        {
            static void Postfix(CR_DeathScreenManager __instance)
            {
                if (__instance.enabled)
                    CR_DeathScreenManager.UpdateDeathScrean();
            }
        }

        /// <summary>
        /// The game's own 10 seconds timer (only running when all players are dead) is replaced by the arcade countdown (see ContinueCountdown)
        /// Still used for a game started from the game's own menus
        /// </summary>
        [HarmonyPatch(typeof(CR_DeathScreenManager), "restartTimer")]
        class restartTimer
        {
            static bool Prefix()
            {
                return !Hotd2r_ArcadePlugin.IsArcadeGameRunning;
            }
        }
    }
}
