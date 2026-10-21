using HarmonyLib;
using UnityEngine;

namespace Hotd2Remake_ArcadePlugin
{
    class mCR_PlayerDeathScreen
    {
        /// <summary>
        /// Like argonlefou's House of the Dead Remake Arcade Plugin : all the game's boxes of the player's death screen are hidden
        /// (continue and buy credit buttons with their controller icons, continue tokens, credits used, CONTINUE? banner).
        /// Only the blood background of CR_DeathScreenManager stays, with our own CONTINUE ?, countdown and PRESS START / INSERT COIN(S) (see ArcadeOverlay)
        /// RedrawElement() is the only place where the game is showing a player's death screen
        /// </summary>
        [HarmonyPatch(typeof(CR_PlayerDeathScreen), "RedrawElement")]
        class RedrawElement
        {
            static void Postfix(CR_PlayerDeathScreen __instance)
            {
                foreach (Transform Child in __instance.transform)
                {
                    if (Child.gameObject.activeSelf)
                        Child.gameObject.SetActive(false);
                }
            }
        }
    }
}
