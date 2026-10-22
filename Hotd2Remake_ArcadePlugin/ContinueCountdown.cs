using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace Hotd2Remake_ArcadePlugin
{
    /// <summary>
    /// Arcade continue countdown, like argonlefou's House of the Dead Remake Arcade Plugin. It replaces the game's own 10 seconds timer (see mCR_DeathScreenManager) :
    /// - A player is dead while the other one is still playing : 9 to 0 on his side of the screen, then GAME OVER for him.
    ///   He is then out of the game, and can join again with START like a new player
    /// - All players are dead : 9 to 0 in the middle of the screen, then GAME OVER
    /// - Inserting a coin is restarting the countdowns, and they are paused during cutscenes
    /// Displayed by ArcadeOverlay
    /// </summary>
    public static class ContinueCountdown
    {
        private const float COUNTDOWN_DURATION = 9.999f;
        private const float GAME_OVER_DURATION = 3.0f;

        private static FieldInfo _DeathScreenInstance = AccessTools.Field(typeof(CR_DeathScreenManager), "instance");
        private static MethodInfo _HandleGameOver = AccessTools.Method(typeof(CR_DeathScreenManager), "handleGameOver");

        private static float[] _PlayerTimers = new float[Hotd2r_ArcadePlugin.MAX_PLAYERS];
        private static bool[] _IsPlayerTimerRunning = new bool[Hotd2r_ArcadePlugin.MAX_PLAYERS];
        private static float _GeneralTimer = 0f;
        private static bool _IsGeneralTimerRunning = false;
        //The game over cutscene is played in the gameplay level, after the death screen is destroyed : nothing to count until the next game
        private static bool _IsGameOver = false;

        public static void Update()
        {
            if (_IsGameOver || CR_DeathScreenManager.IsGameOver)
            {
                _IsGameOver = true;
                Stop();
                return;
            }

            int ActivePlayers = 0;
            int DeadPlayers = 0;
            for (int i = 0; i < Hotd2r_ArcadePlugin.MAX_PLAYERS; i++)
            {
                CR_Player GamePlayer = CR_Player.Players[i];
                if (GamePlayer != null && GamePlayer.IsActivated)
                {
                    ActivePlayers++;
                    if (GamePlayer.HasDied)
                        DeadPlayers++;
                }
            }
            bool AreAllPlayersDead = ActivePlayers > 0 && DeadPlayers == ActivePlayers;

            //Unscaled time, in case the game is slowing down the time when everybody is dead
            //Stopped while the game is paused (Steam overlay, controller popups...) : START is not possible then
            bool IsPaused = CR_Cutscene.IsCutsceneActive || CR_ChapterCompleteMenu.IsActive || CR_LevelLoader.IsLoadingScreenVisible || MP_GameManager.IsLevelCompleted || MP_GameManager.IsGameplayFinished
                || MP_GameManager.IsGamePaused;
            float Delta = IsPaused ? 0f : Time.unscaledDeltaTime;

            if (AreAllPlayersDead)
            {
                if (!_IsGeneralTimerRunning)
                {
                    Hotd2r_ArcadePlugin.MyLogger.LogMessage("ContinueCountdown.Update() => All players are dead, starting the countdown");
                    _IsGeneralTimerRunning = true;
                    _GeneralTimer = COUNTDOWN_DURATION;
                }
                _GeneralTimer -= Delta;
                if (_GeneralTimer <= 0f)
                    gameOver();
            }
            else
            {
                _IsGeneralTimerRunning = false;
            }

            for (int i = 0; i < Hotd2r_ArcadePlugin.MAX_PLAYERS; i++)
            {
                CR_Player GamePlayer = CR_Player.Players[i];
                if (AreAllPlayersDead || GamePlayer == null || !GamePlayer.IsActivated || !GamePlayer.HasDied)
                {
                    _IsPlayerTimerRunning[i] = false;
                    continue;
                }

                if (!_IsPlayerTimerRunning[i])
                {
                    Hotd2r_ArcadePlugin.MyLogger.LogMessage("ContinueCountdown.Update() => " + ((PlayerType)i).ToString() + " is dead, starting his countdown");
                    _IsPlayerTimerRunning[i] = true;
                    _PlayerTimers[i] = COUNTDOWN_DURATION;
                }
                _PlayerTimers[i] -= Delta;

                //After GAME OVER is displayed : out of the game, he is removed by the plugin's Update() as he is not playing anymore
                if (_PlayerTimers[i] <= -GAME_OVER_DURATION)
                {
                    Hotd2r_ArcadePlugin.MyLogger.LogMessage("ContinueCountdown.Update() => " + ((PlayerType)i).ToString() + " did not continue, GAME OVER for him");
                    _IsPlayerTimerRunning[i] = false;
                    Hotd2r_ArcadePlugin.IsPlayerPlaying[i] = false;
                }
            }
        }

        /// <summary>
        /// Coin inserted : giving time to the players to continue, like argonlefou's plugin
        /// </summary>
        public static void Restart()
        {
            for (int i = 0; i < Hotd2r_ArcadePlugin.MAX_PLAYERS; i++)
                _PlayerTimers[i] = COUNTDOWN_DURATION;
            _GeneralTimer = COUNTDOWN_DURATION;
        }

        /// <summary>
        /// Back on the title screen, ready for the next game
        /// </summary>
        public static void Reset()
        {
            Stop();
            _IsGameOver = false;
        }

        private static void Stop()
        {
            for (int i = 0; i < Hotd2r_ArcadePlugin.MAX_PLAYERS; i++)
                _IsPlayerTimerRunning[i] = false;
            _IsGeneralTimerRunning = false;
        }

        /// <summary>
        /// 9 to 0, then GAME OVER. Empty if the player is not dead, or if all players are dead (general countdown instead)
        /// </summary>
        public static string GetPlayerText(int Player)
        {
            if (!_IsPlayerTimerRunning[Player])
                return string.Empty;
            if (_PlayerTimers[Player] <= 0f)
                return Hotd2r_ArcadePlugin.TextLangs[LanguageStrings.StringName.GameOver];
            return ((int)_PlayerTimers[Player]).ToString();
        }

        public static string GetGeneralText()
        {
            if (!_IsGeneralTimerRunning)
                return string.Empty;
            return ((int)Mathf.Max(_GeneralTimer, 0f)).ToString();
        }

        private static void gameOver()
        {
            _IsGeneralTimerRunning = false;
            _IsGameOver = true;
            object DeathScreen = _DeathScreenInstance != null ? _DeathScreenInstance.GetValue(null) : null;
            if (DeathScreen == null || _HandleGameOver == null)
            {
                Hotd2r_ArcadePlugin.MyLogger.LogWarning("ContinueCountdown.gameOver() => Death screen not found");
                return;
            }
            Hotd2r_ArcadePlugin.MyLogger.LogMessage("ContinueCountdown.gameOver() => Nobody continued, GAME OVER");
            _HandleGameOver.Invoke(DeathScreen, null);
        }
    }
}
