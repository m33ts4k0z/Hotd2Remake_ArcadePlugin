using HarmonyLib;
using MegaPixel.Menus;

namespace Hotd2Remake_ArcadePlugin
{
    class mCR_ReadyCheckPanel
    {
        private static AccessTools.FieldRef<CR_ReadyCheckPanel, bool> _IsSinglePlayerReady = AccessTools.FieldRefAccess<CR_ReadyCheckPanel, bool>("isSinglePlayerReady");
        private static AccessTools.FieldRef<CR_ReadyCheckPanel, bool> _IsMultiplayerFirstPlayerReady = AccessTools.FieldRefAccess<CR_ReadyCheckPanel, bool>("isMultiplayerFirstPlayerReady");
        private static AccessTools.FieldRef<CR_ReadyCheckPanel, bool> _IsMultiplayerSecondPlayerReady = AccessTools.FieldRefAccess<CR_ReadyCheckPanel, bool>("isMultiplayerSecondPlayerReady");

        /// <summary>
        /// Like argonlefou's House of the Dead Remake Arcade Plugin : no "Press SHOOT when ready" at the beginning of each chapter,
        /// the panel is validated as soon as it is shown so that the level starts directly after the loading screen.
        /// HidePanel() is the game's own path when all players are ready (fade to black, intro cutscene, chapter music)
        /// </summary>
        [HarmonyPatch(typeof(CR_ReadyCheckPanel), "ShowPanel")]
        class ShowPanel
        {
            static void Postfix(CR_ReadyCheckPanel __instance, bool __result, MP_CanvasGroupController ___canvasController)
            {
                if (!__result)
                    return;

                Hotd2r_ArcadePlugin.MyLogger.LogMessage("CR_ReadyCheckPanel.ShowPanel() => Players ready");
                //Both players already ready : no "ready" sound if the trigger is pulled while the panel is fading out
                _IsSinglePlayerReady(__instance) = true;
                _IsMultiplayerFirstPlayerReady(__instance) = true;
                _IsMultiplayerSecondPlayerReady(__instance) = true;
                __instance.HidePanel();
                //Never visible, not even during its fade in
                ___canvasController.HideImmediately();
            }
        }
    }
}
