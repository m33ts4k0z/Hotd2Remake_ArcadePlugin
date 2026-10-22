using HarmonyLib;

namespace Hotd2Remake_ArcadePlugin
{
    class mMP_SettingsHandler
    {
        /// <summary>
        /// The game is applying its sound settings again at the end of each loading, after the title screen is displayed :
        /// keeping the title screen muted (ATTRACT_SOUND=0)
        /// </summary>
        [HarmonyPatch(typeof(MP_SettingsHandler), "OnSoundSettingsChanged")]
        class OnSoundSettingsChanged
        {
            static void Postfix()
            {
                if (Hotd2r_ArcadePlugin.IsAttractMuted)
                    MP_SoundSettingsHandler.ToggleMuteMaster(true);
            }
        }
    }
}
