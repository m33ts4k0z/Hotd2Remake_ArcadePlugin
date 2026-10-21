using HarmonyLib;

namespace Hotd2Remake_ArcadePlugin
{
    class mCR_VictoryManager
    {
        /// <summary>
        /// Last chapter completed : results, agent rank, name entry and leaderboard screens, validated by START or a timer (see EndOfGameScreens)
        /// </summary>
        [HarmonyPatch(typeof(CR_VictoryManager), "OnEnable")]
        class OnEnable
        {
            static void Postfix(CR_VictoryManager __instance)
            {
                if (!Hotd2r_ArcadePlugin.IsArcadeGameRunning)
                    return;

                Hotd2r_ArcadePlugin.MyLogger.LogMessage("CR_VictoryManager.OnEnable() => Game completed");
                EndOfGameScreens.OnVictory(__instance);
            }
        }
    }
}
