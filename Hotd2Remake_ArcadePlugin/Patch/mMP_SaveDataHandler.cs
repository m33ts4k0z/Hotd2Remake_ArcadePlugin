using HarmonyLib;
using MegaPixel.SaveLoadSystem;

namespace Hotd2Remake_ArcadePlugin
{
    class mMP_SaveDataHandler
    {
        /// <summary>
        /// The save is loaded : the settings of the config file (video, sound, language, lives...) are replacing the ones of the save,
        /// just before the game applies them (see ConfManager.ApplyGameSettings)
        /// Same checks as the game, for a missing save
        /// </summary>
        [HarmonyPatch(typeof(MP_SaveDataHandler), "Initialize")]
        class Initialize
        {
            static void Prefix()
            {
                if (MP_SaveLoad.SaveData == null)
                    MP_SaveLoad.SaveData = new MP_SaveData();
                if (MP_SaveLoad.SaveData.GameSettings == null)
                    MP_SaveLoad.SaveData.GameSettings = new MP_GameSettings();
                Hotd2r_ArcadePlugin.Configurator.ApplyGameSettings(MP_SaveLoad.SaveData.GameSettings);
            }
        }
    }
}
