using HarmonyLib;
using MegaPixel.Menus;

namespace Hotd2Remake_ArcadePlugin
{
    class mCR_ScoreDisplay
    {
        /// <summary>
        /// Like argonlefou's House of the Dead Remake Arcade Plugin : no score during the game, like the original arcade
        /// The game is showing it at the end of each cutscene
        /// </summary>
        [HarmonyPatch(typeof(CR_ScoreDisplay), "toggleScore")]
        class toggleScore
        {
            static bool Prefix(MP_CanvasGroupController ___scoreCanvasGroupController)
            {
                if (!Hotd2r_ArcadePlugin.Configurator.ArcadeHud)
                    return true;

                if (___scoreCanvasGroupController != null)
                    ___scoreCanvasGroupController.Hide();
                //In case the game did not apply its settings yet (see SetScoreMessagesEnabled below)
                CR_ScoreDisplay.SetScoreMessagesEnabled(ScoreNotificationsMode.Disabled);
                return false;
            }
        }

        /// <summary>
        /// No points flying from the crosshair to the (hidden) score either
        /// </summary>
        [HarmonyPatch(typeof(CR_ScoreDisplay), "SetScoreMessagesEnabled")]
        class SetScoreMessagesEnabled
        {
            static void Prefix(ref ScoreNotificationsMode _scoreNotificationsMode)
            {
                if (Hotd2r_ArcadePlugin.Configurator.ArcadeHud)
                    _scoreNotificationsMode = ScoreNotificationsMode.Disabled;
            }
        }
    }
}
