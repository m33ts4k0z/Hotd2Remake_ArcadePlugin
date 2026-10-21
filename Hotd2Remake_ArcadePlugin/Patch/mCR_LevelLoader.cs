using HarmonyLib;

namespace Hotd2Remake_ArcadePlugin
{
    class mCR_LevelLoader
    {
        /// <summary>
        /// After a GAME OVER, the game over map is going back to the main menu : arcade name entry and ranking first, if the score is good enough (see ArcadeRanking)
        /// ArcadeRanking is calling it again once it is done
        /// </summary>
        [HarmonyPatch(typeof(CR_LevelLoader), "LoadMainMenu")]
        class LoadMainMenu
        {
            static bool Prefix(ref bool __result)
            {
                if (ArcadeRanking.Instance != null && ArcadeRanking.Instance.TryStartNameEntry())
                {
                    __result = true;
                    return false;
                }
                return true;
            }
        }
    }
}
