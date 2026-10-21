using HarmonyLib;
using MegaPixel.SaveLoadSystem;

namespace Hotd2Remake_ArcadePlugin
{
    class mMP_SaveLoad
    {
        private static bool _IsLogged = false;

        /// <summary>
        /// Using a save file in the game folder (NVRAM), so that the arcade games don't change the home save file
        /// </summary>
        [HarmonyPatch(typeof(MP_SaveLoad), "FullPathToPersistentSaveData", MethodType.Getter)]
        class FullPathToPersistentSaveData
        {
            static bool Prefix(ref string __result)
            {
                if (!Hotd2r_ArcadePlugin.Configurator.SaveInGameFolder)
                    return true;

                __result = Hotd2r_ArcadePlugin.NvramFolder + "/saveData.sav";
                if (!_IsLogged)
                {
                    Hotd2r_ArcadePlugin.MyLogger.LogMessage("MP_SaveLoad.FullPathToPersistentSaveData => Replaced by " + __result);
                    _IsLogged = true;
                }
                return false;
            }
        }
    }
}
