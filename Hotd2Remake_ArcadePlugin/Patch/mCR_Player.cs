using HarmonyLib;

namespace Hotd2Remake_ArcadePlugin
{
    class mCR_Player
    {
        /// <summary>
        /// Continues are paid with credits instead of the game's own tokens (set as unlimited by the plugin)
        /// Whatever is calling it (our START button, the game's own input...)
        /// </summary>
        [HarmonyPatch(typeof(CR_Player), "Revive")]
        class Revive
        {
            static bool Prefix(CR_Player __instance, out bool __state)
            {
                __state = __instance.HasDied;
                if (__state && !Hotd2r_ArcadePlugin.CanPay(Hotd2r_ArcadePlugin.Configurator.CreditsToContinue))
                {
                    Hotd2r_ArcadePlugin.MyLogger.LogMessage("CR_Player.Revive() => " + __instance.Type.ToString() + " has not enough credits to continue");
                    __state = false;
                    return false;
                }
                return true;
            }

            static void Postfix(CR_Player __instance, bool __state)
            {
                if (__state && !__instance.HasDied)
                {
                    Hotd2r_ArcadePlugin.TryToPay(Hotd2r_ArcadePlugin.Configurator.CreditsToContinue);
                    Hotd2r_ArcadePlugin.IsPlayerPlaying[__instance.Id] = true;
                    Hotd2r_ArcadePlugin.MyLogger.LogMessage("CR_Player.Revive() => " + __instance.Type.ToString() + " continue, credits left = " + Hotd2r_ArcadePlugin.CurrentCredits);
                }
            }
        }
    }
}
