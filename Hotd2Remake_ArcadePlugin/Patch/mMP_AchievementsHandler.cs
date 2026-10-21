using HarmonyLib;
using MegaPixel.Achievements;

namespace Hotd2Remake_ArcadePlugin
{
    class mMP_AchievementsHandler
    {
        /// <summary>
        /// Like argonlefou's House of the Dead Remake Arcade Plugin : blocking the achievements (and their popups during the game)
        /// </summary>
        [HarmonyPatch(typeof(MP_AchievementsHandler), "unlockAchievement")]
        class unlockAchievement
        {
            static bool Prefix(AchievementType _achievementID)
            {
                Hotd2r_ArcadePlugin.MyLogger.LogMessage("MP_AchievementsHandler.unlockAchievement() => Blocked : " + _achievementID.ToString());
                return false;
            }
        }
    }
}
