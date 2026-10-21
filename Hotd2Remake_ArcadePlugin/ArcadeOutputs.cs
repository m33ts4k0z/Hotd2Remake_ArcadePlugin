using System.Reflection;
using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace Hotd2Remake_ArcadePlugin
{
    /// <summary>
    /// START lamps and credits, like argonlefou's House of the Dead Remake Arcade Plugin.
    /// They are sent to DemulShooter by its own plugin for this game (P1_LmpStart, P2_LmpStart and Credits outputs), if it has them
    /// A lamp is blinking when the player can press START : to start a game, join, continue, or go on to the next screen
    /// </summary>
    public static class ArcadeOutputs
    {
        //On for 0.5s, off for 0.5s, like argonlefou's plugin
        private const float LAMP_BLINK_DURATION = 0.5f;

        private static object _OutputData = null;
        private static FieldInfo _StartLedField = null;
        private static FieldInfo _CreditsField = null;

        public static void Init(BaseUnityPlugin DemulShooterPlugin)
        {
            FieldInfo OutputDataField = AccessTools.Field(DemulShooterPlugin.GetType(), "OutputData");
            object OutputData = OutputDataField != null ? OutputDataField.GetValue(null) : null;
            if (OutputData == null)
                return;

            _StartLedField = AccessTools.Field(OutputData.GetType(), "Start_Led");
            _CreditsField = AccessTools.Field(OutputData.GetType(), "Credits");
            if (_StartLedField == null || _CreditsField == null)
            {
                Hotd2r_ArcadePlugin.MyLogger.LogWarning("ArcadeOutputs.Init() => This DemulShooter plugin has no START lamps and credits outputs");
                return;
            }
            _OutputData = OutputData;
            Hotd2r_ArcadePlugin.MyLogger.LogMessage("ArcadeOutputs.Init() => START lamps and credits are sent to DemulShooter");
        }

        public static void Update()
        {
            if (_OutputData == null)
                return;

            byte[] StartLeds = _StartLedField.GetValue(_OutputData) as byte[];
            if (StartLeds != null)
            {
                bool IsBlinkOn = (Time.unscaledTime % (2f * LAMP_BLINK_DURATION)) < LAMP_BLINK_DURATION;
                for (int i = 0; i < StartLeds.Length && i < Hotd2r_ArcadePlugin.MAX_PLAYERS; i++)
                    StartLeds[i] = (byte)(IsBlinkOn && canPressStart(i) ? 1 : 0);
            }
            _CreditsField.SetValue(_OutputData, Hotd2r_ArcadePlugin.Configurator.Freeplay ? 0 : Hotd2r_ArcadePlugin.CurrentCredits);
        }

        private static bool canPressStart(int Player)
        {
            if (CR_LevelLoader.IsLoadingScreenVisible)
                return false;
            //Chapter results, name entry, ranking...
            if (EndOfGameScreens.IsActive())
                return true;
            if (Hotd2r_ArcadePlugin.IsOnTitleScreen())
                return Hotd2r_ArcadePlugin.CanPay(Hotd2r_ArcadePlugin.Configurator.CreditsToStart);
            if (!CR_LevelLoader.IsGameplayChapterLoaded || CR_Cutscene.IsCutsceneActive || MP_GameManager.IsGameplayFinished)
                return false;

            CR_Player GamePlayer = CR_Player.Players[Player];
            //Joining is only possible in a Multiplayer campaign
            if (GamePlayer == null || !GamePlayer.IsActivated)
                return MP_SaveDataHandler.IsReady && MP_SaveDataHandler.LastPlayedCampaign != null && MP_SaveDataHandler.LastPlayedCampaign.IsMultiplayerCampaign
                    && Hotd2r_ArcadePlugin.CanPay(Hotd2r_ArcadePlugin.Configurator.CreditsToStart);
            if (GamePlayer.HasDied)
                return Hotd2r_ArcadePlugin.CanPay(Hotd2r_ArcadePlugin.Configurator.CreditsToContinue);
            return false;
        }
    }
}
