using HarmonyLib;
using TMPro;

namespace Hotd2Remake_ArcadePlugin
{
    class mCR_MainMenuMovieController
    {
        private static AccessTools.FieldRef<CR_SimplePressAnyKeyDisplay, TMP_Text> _PressAnyKeyText = AccessTools.FieldRefAccess<CR_SimplePressAnyKeyDisplay, TMP_Text>("textField");

        /// <summary>
        /// Title screen (video loop with "PRESS ANY KEY") :
        /// - Text replaced by the credits status
        /// - START is handled in the plugin Update() loop, to start a new game
        /// - Any other input is opening the game's main menu, unless disabled in the config file
        /// </summary>
        [HarmonyPatch(typeof(CR_MainMenuMovieController), "Update")]
        class Update
        {
            static bool Prefix(CR_MainMenuMovieController __instance, CR_SimplePressAnyKeyDisplay ___simplePressAnyKey, CR_MainMenu ___mainMenu)
            {
                Hotd2r_ArcadePlugin.TitleScreen = __instance;

                if (___mainMenu.IsMainMenuOpen || ___mainMenu.IsMainMenuOpening || !___simplePressAnyKey.IsVisible)
                    return true;

                TMP_Text Text = _PressAnyKeyText(___simplePressAnyKey);
                if (Text != null)
                {
                    //Saving the game font for our own texts
                    if (Hotd2r_ArcadePlugin.GameFont == null)
                    {
                        Hotd2r_ArcadePlugin.GameFont = Text.font;
                        Hotd2r_ArcadePlugin.GameFontMaterial = Text.fontSharedMaterial;
                    }
                    Text.text = Hotd2r_ArcadePlugin.GetScreenPlayerString(Hotd2r_ArcadePlugin.Configurator.CreditsToStart);
                }

                return !Hotd2r_ArcadePlugin.Configurator.DisableMainMenu;
            }
        }
    }
}
