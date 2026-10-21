# Hotd2Remake_ArcadePlugin

Arcade Plugin for **THE HOUSE OF THE DEAD 2: Remake** (Steam) on Windows.
This BepInEx plugin turns the home game into an arcade cabinet experience: coins and credits, START buttons, arcade continues.

It is based on [argonlefou](https://github.com/argonlefou)'s [House of the Dead Remake Arcade Plugin](https://github.com/argonlefou/HotdRemake_ArcadePlugin) for the first game: same design, and some of its code (texts and their translations, credits handling).

## Status
Work in progress, built in stages:

| Stage | Content | Status |
|---|---|---|
| 1 | Coins, credits, free play, START to play, join, continue and skip cutscenes. No in-game menu or controller hints, Esc quits | ✅ |
| 2 | Attract mode: title video loop with the game logo, credits text at the bottom center. Arcade presentation: `LOADING...` instead of the loading screen tips, mouse cursor always hidden, no achievements (no popups during the game) | ✅ |
| 3 | Arcade continue countdown (9 to 0 per player) tied to credits. Arcade HUD: no score and no weapon name during the game, like the original arcade | ⏳ |
| 4 | Name entry and high scores, rankings in the attract loop | ⏳ |
| 5 | Settings tool like argonlefou's `ArcadeMod_Config` (graphics, sound, language, difficulty, starting and maximum lives...), start lamps and credits outputs for DemulShooter | ⏳ |

## Features
- **Coins**: `5` / `6` add a credit (common credits pool, up to 99).
- **Attract mode**: the title screen loops the game's videos, starting with the main one and the game logo, like argonlefou's plugin. It shows `INSERT COIN(S)` / `PRESS START` with the credits, or `FREEPLAY`, at the bottom center of the screen. START starts a new game straight away, skipping all menus. The other player can join with START at any time.
- **Arcade presentation**: `LOADING...` instead of the gameplay tips on the loading screens, mouse cursor always hidden, no achievements (and no achievement popups during the game).
- **Credits**: starting/joining costs `CREDITS_TO_START`, a continue costs `CREDITS_TO_CONTINUE`. They replace the game's own continue tokens. The death screen shows the credits and only offers to continue when there are enough.
- **No credits left**: the game's own 10 s countdown runs, then it's game over and back to the title screen.
- **No READY screen**: like argonlefou's plugin, each chapter starts straight after its loading screen. Holding START skips cutscenes ("HOLD START TO SKIP!").
- **Like an arcade cabinet** (and argonlefou's plugin): no in-game pause menu, no controller hints (keyboard/gamepad icons) in menus and on the death screen, no pause when the game window loses the focus. `Esc` quits the game.
- Game settings (graphics, sound, language...) are not available in game. They are taken from the save file: set `DISABLE_MAIN_MENU=0` to reach the game's own Options menu from the title screen if needed. A settings tool like argonlefou's `ArcadeMod_Config` is planned for stage 5.
- **Separate save file** in the game folder (`NVRAM\saveData.sav`), so that the arcade games don't change the home save. On first run, the home save is copied there to keep the settings and unlocked items.
- Texts follow the game's language.

Games are always started as a **Multiplayer** campaign (the only mode where a second player can join), in Competitive mode (separate scores, like the arcade).

## Lightguns
The plugin works with a mouse/keyboard, but it is made to be used with [DemulShooter](https://github.com/argonlefou/DemulShooter) and its House of the Dead 2 Remake plugin (`-target=windows -rom=hotd2r`), which handles the two guns, recoil, rumble and crosshairs. When both plugins are installed, the START buttons are handled by this one.

## Installation
1. Install BepInEx 5 (x64) in the game folder, next to `THE HOUSE OF THE DEAD 2 Remake.exe` (DemulShooter's HOTD2 Remake package already contains it).
2. Copy `Hotd2Remake_ArcadePlugin.dll` and `Hotd2Remake_ArcadePlugin.ini` to `BepInEx\plugins\`.

To uninstall, remove these two files. The home save file is never modified while the plugin is installed.

## Configuration
`BepInEx\plugins\Hotd2Remake_ArcadePlugin.ini`:

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

## Building
Visual Studio 2019 or later (or MSBuild 16+), .NET Framework 4.8.
Copy the game DLLs listed in `Hotd2Remake_ArcadePlugin\UnityLibs\List_of_dll_to_copy_here.txt` to that folder first (they are not part of this repository).
