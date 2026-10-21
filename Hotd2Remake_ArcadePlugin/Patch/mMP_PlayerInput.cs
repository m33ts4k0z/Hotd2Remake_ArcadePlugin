using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using MegaPixel.Input;
using UnityEngine.InputSystem;

namespace Hotd2Remake_ArcadePlugin
{
    class mMP_PlayerInput
    {
        /// <summary>
        /// The game is using the number keys to select weapons, but they are also the arcade START and COIN keys
        /// </summary>
        [HarmonyPatch]
        class BlockWeaponSelectionKeys
        {
            static IEnumerable<MethodBase> TargetMethods()
            {
                for (int i = 1; i <= 5; i++)
                {
                    yield return AccessTools.Method(typeof(MP_PlayerInput), "SelectWeapon_" + i, new Type[] { typeof(InputAction.CallbackContext) });
                }
            }

            static bool Prefix()
            {
                return false;
            }
        }
    }
}
