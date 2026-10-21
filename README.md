# Hotd2Remake_ArcadePlugin

Arcade Plugin for **THE HOUSE OF THE DEAD 2: Remake** (Steam) on Windows.
This BepInEx plugin turns the home game into an arcade cabinet experience: coins and credits, START buttons, arcade continues.

It is based on [argonlefou](https://github.com/argonlefou)'s [House of the Dead Remake Arcade Plugin](https://github.com/argonlefou/HotdRemake_ArcadePlugin) for the first game: same design, and some of its code (texts and their translations, credits handling).

## Status
Built in stages, all done:

| Stage | Content | Status |
|---|---|---|
| 1 | Coins, credits, free play, START to play, join, continue and skip cutscenes. No in-game menu or controller hints, Esc quits | ✅ |
| 2 | Attract mode: title video loop with the game logo, credits text at the bottom center. Arcade presentation: `LOADING...` instead of the loading screen tips, mouse cursor always hidden, no achievements (no popups during the game) | ✅ |
| 3 | Arcade continue countdown (9 to 0 per player, GAME OVER per player) tied to credits. Arcade HUD: no score and no weapon name during the game, like the original arcade | ✅ |
| 4 | Name entry and high scores, rankings in the attract loop. Chapter results and end of game screens validated with START | ✅ |
| 5 | Settings tool like argonlefou's `ArcadeMod_Config` (video, sound, language, difficulty, life torches...), START lamps and credits outputs for DemulShooter | ✅ |

## Features
- **Coins**: `5` / `6` add a credit (common credits pool, up to 99).
- **Attract mode**: the title screen loops the game's videos, starting with the main one and the game logo, like argonlefou's plugin. It shows `INSERT COIN(S)` / `PRESS START` with the credits, or `FREEPLAY`, at the bottom center of the screen. START starts a new game straight away, skipping all menus. The other player can join with START at any time.
- **Arcade presentation**: `LOADING...` instead of the gameplay tips on the loading screens, mouse cursor always hidden, no achievements (and no achievement popups during the game).
- **Credits**: starting/joining costs `CREDITS_TO_START`, a continue costs `CREDITS_TO_CONTINUE`. They replace the game's own continue tokens.
- **Continue countdown**, like argonlefou's plugin (it replaces the game's own 10 s timer). The game's death screen boxes (continue and buy buttons, tokens, credits used) are hidden: only `CONTINUE ?` with the countdown, and a fading `PRESS START` under it (`INSERT COIN(S)` when there are not enough credits).
  - a player dies while the other one is still playing: `9` to `0` on his side of the screen, then `GAME OVER` for him. His side then shows `PRESS START` / `INSERT COIN(S)`, and joining again costs `CREDITS_TO_START`.
  - all players are dead: `9` to `0` in the middle of the screen, then game over and back to the title screen.
  - inserting a coin restarts the countdowns from `9`. They are paused during cutscenes.
- **Arcade HUD**: no score (and no points flying to it) and no weapon name during the game, like the original arcade. The life torches, the weapon icon and the ammo are still displayed.
- **Chapter results**: no restart, chapter selection, main menu or exit buttons. `START` (or 20 s) goes on to the next chapter.
- **High scores**: the game's own local leaderboard (the one of `MODE` and `SCORE_MODE`), one entry per game with the total score of both players, like the game.
  - **Game over**: the game itself goes straight back to the main menu. If the score is in the top 10, an arcade name entry is displayed instead: shoot the letters with any gun (the guns are the mouse outside of the gameplay), `END` or `START` to save (30 s timer, `AMS` if empty). Then the ranking is displayed, with the new entry highlighted.
  - **Game completed**: the game's own screens (results, agent rank, name on the typewriter, leaderboard, credits and ending). `START` (or a timer) goes on to the next one. On the typewriter, the keys are shot with the guns and `START` saves the name.
  - **Attract mode**: the ranking is displayed for 10 s after the main video of the title screen, like argonlefou's plugin.
  - **Default ranking**: like an arcade cabinet, an empty ranking is filled with a default top 10 (50000 to 5000 points). Only in the arcade save of the game folder (`SAVE_IN_GAME_FOLDER=1`), the home save is never modified.
- **No READY screen**: like argonlefou's plugin, each chapter starts straight after its loading screen. Holding START skips cutscenes ("HOLD START TO SKIP!").
- **Like an arcade cabinet** (and argonlefou's plugin): no in-game pause menu, no controller hints (keyboard/gamepad icons) in menus and on the death screen, no pause when the game window loses the focus. `Esc` quits the game.
- **Settings tool** (`Hotd2Remake_ArcadeConfig.exe`), like argonlefou's `ArcadeMod_Config`: the game's own option menus are not available in the arcade mode, so the video, sound, language and life torches settings are set in the plugin's INI file and replace the ones of the save when the game starts. Each one can also be left to `(GAME SETTING)` (or its slider unchecked). Same layout as argonlefou's tool: game logo, *Save Config* button, *Arcade Menu* tab with the *Mod Options*, *Display Settings* tab with the HUD scale and brightness sliders, *Sound Option* tab with the volume sliders, and a *Controls* tab for the START/COIN/quit keys.
- **Title screen sound** can be turned off (`ATTRACT_SOUND=0`), like argonlefou's plugin.
- **START lamps and credits** for DemulShooter (`P1_LmpStart`, `P2_LmpStart` and `Credits` outputs, for MameHooker or QMamehook): a lamp blinks when its player can press START (start a game, join, continue, go on to the next screen), like argonlefou's plugin.
- **Separate save file** in the game folder (`NVRAM\saveData.sav`), so that the arcade games don't change the home save. On first run, the home save is copied there to keep the settings and unlocked items.
- Texts follow the game's language.

Games are always started as a **Multiplayer** campaign (the only mode where a second player can join), in Competitive mode (separate scores, like the arcade).

## Lightguns
The plugin works with a mouse/keyboard, but it is made to be used with [DemulShooter](https://github.com/argonlefou/DemulShooter) and its House of the Dead 2 Remake plugin (`-target=windows -rom=hotd2r`), which handles the two guns, recoil, rumble and crosshairs. When both plugins are installed, the START buttons are handled by this one, and the START lamps and credits are sent to DemulShooter by its plugin (if it has these outputs).

## Installation
1. Install BepInEx 5 (x64) in the game folder, next to `THE HOUSE OF THE DEAD 2 Remake.exe` (DemulShooter's HOTD2 Remake package already contains it).
2. Copy `Hotd2Remake_ArcadePlugin.dll` and `Hotd2Remake_ArcadePlugin.ini` to `BepInEx\plugins\`.
3. Copy `Hotd2Remake_ArcadeConfig.exe` next to the game's exe, to change the settings.

To uninstall, remove these files. The home save file is never modified while the plugin is installed.

## Configuration
`BepInEx\plugins\Hotd2Remake_ArcadePlugin.ini`, or `Hotd2Remake_ArcadeConfig.exe`:

| Setting | Default | |
|---|---|---|
| `FREEPLAY` | `0` | `1` = no coins needed |
| `CREDITS_TO_START` / `CREDITS_TO_CONTINUE` | `1` | 1 to 9 |
| `P1_START` / `P2_START` | `49` / `50` | Unity KeyCode values (`1` / `2`) |
| `P1_COIN` / `P2_COIN` | `53` / `54` | Unity KeyCode values (`5` / `6`) |
| `EXIT` | `27` | Quits the game (`Esc`), `0` = disabled |
| `MODE` | `Original` | `Original`, `Arcade` or `Horde` (the game's own modes) |
| `DIFFICULTY` | `Normal` | `VeryEasy` to `VeryHard` |
| `SCORE_MODE` | `Classic` | `Classic` or `Modern` |
| `DISABLE_MAIN_MENU` | `1` | `0` = any input other than START still opens the game's menu (settings...) |
| `SAVE_IN_GAME_FOLDER` | `1` | `0` = use the normal save file |
| `HIDE_CURSOR` | `1` | `0` = show the mouse cursor (to use the game's menus with `DISABLE_MAIN_MENU=0`) |
| `ARCADE_HUD` | `1` | `0` = the game's own HUD, with the score and the weapon name |
| `ATTRACT_SOUND` | `1` | `0` = no sound on the title screen |

Game settings, replacing the ones of the save when the game starts. Empty = the game's own setting:

| Setting | Values |
|---|---|
| `LIVES` | Life torches, `1` to `6` (the game's default is 3) |
| `LANGUAGE` | `English`, `French`, `German`, `Italian`, `Spanish`, `Portuguese`, `Polish`, `Japanese` or `Chinese` |
| `RESOLUTION` | Width x height, for example `1920x1080` |
| `FULLSCREEN_MODE` | `ExclusiveFullScreen`, `FullScreenWindow`, `MaximizedWindow` or `Windowed` |
| `VSYNC` | `Off`, `EveryVBlank` or `EverySecondVBlank` |
| `ANTI_ALIASING` | `Off`, `SMAA` or `TAA` |
| `SHADOW_QUALITY` | `Low`, `Medium` or `High` |
| `BLOOM`, `MOTION_BLUR`, `AMBIENT_OCCLUSION` | `1` = on, `0` = off |
| `BRIGHTNESS` | `0` to `100` |
| `HUD_SCALE` | `50` to `150` (%) |
| `MASTER_VOLUME`, `MUSIC_VOLUME`, `EFFECTS_VOLUME`, `UI_VOLUME`, `DIALOGS_VOLUME`, `SHOT_VOLUME` | `0` to `100` |
| `CLASSIC_MUSIC` | `1` = the original House of the Dead 2 music |

## Building
Visual Studio 2019 or later (or MSBuild 16+), .NET Framework 4.8. The solution has two projects: the plugin, and the settings tool (WinForms).
The game DLLs needed to build are in `Hotd2Remake_ArcadePlugin\UnityLibs` as stripped reference copies (names and signatures only, no game code), see `About_these_dlls.txt` there to make them again after a game update.

GitHub Actions builds `main` once a day (`.github/workflows/build.yml`, Debug and Release), only if it has commits that were not built successfully yet; *Run workflow* in the Actions tab builds it straight away. Each build publishes a GitHub release (tag from the date, for example `v2026.10.22`, `.2` for a second one on the same day) with the commits since the previous release as notes, and a zip of the Release files laid out like the game folder (`BepInEx\plugins\` + the settings tool). The same files are also in the `Hotd2Remake_ArcadePlugin` artifact of the run.

The Debug build adds test keys: `F5` / `F6` kill P1 / P2, `F7` finishes the current chapter, `F8` goes to the last chapter and `F9` gives 100000 points to P1, to check the continue, results, name entry and ending screens without playing the whole game.
