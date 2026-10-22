<img width="3840" height="2160" alt="20261021222916_1" src="https://github.com/user-attachments/assets/5aaebb39-dc57-4254-b6d3-19d97e8354ba" />
<img width="3840" height="2160" alt="20261021223047_1" src="https://github.com/user-attachments/assets/51976a2f-21a5-4d26-b882-ceb4f3214956" />
<img width="3840" height="2160" alt="20261021223056_1" src="https://github.com/user-attachments/assets/9bca996b-6b11-4f97-a65e-d10861b6500c" />
<img width="3840" height="2160" alt="20261021223806_1" src="https://github.com/user-attachments/assets/6d218b25-14a4-4b6f-8ad7-d7138aaf327c" />


# Hotd2Remake_ArcadePlugin
<!-- Screenshots, like argonlefou's README: drag the 2 images in GitHub's editor to get their links, put them in src and remove this comment
<p align="center">
  <img align="left" width="400" src="SCREENSHOT_1_LINK">
  <img width="400" src="SCREENSHOT_2_LINK">
</p>
-->
Arcade Plugin for "THE HOUSE OF THE DEAD 2: Remake" game on Windows  
This UNITY plugin will modify the game in order to make it as close as possible to the original arcade version.  
Based on argonlefou's [House Of The Dead Remake Arcade Plugin](https://github.com/argonlefou/HotdRemake_ArcadePlugin).
<br>
<br>

## Compatibility
Tested with the STEAM version v1.1.1 of the game.  
<br>

## Installation
Unzip the content of the latest [release](../../releases) to the base directory of the game, where the file `THE HOUSE OF THE DEAD 2 Remake.exe` is located.  
BepInEx is included.  
<br>

## Configuration
Run the `Hotd2Remake_ArcadeConfig.exe` tool to setup the game options as you want.  
This config tool has the inner options of the game, plus some custom ones related to the arcade way of playing it.  
They are saved in `BepInEx\plugins\Hotd2Remake_ArcadePlugin.ini` :

| Setting | Default | |
|---|---|---|
| `FREEPLAY` | `0` | `1` = no coins needed |
| `CREDITS_TO_START` / `CREDITS_TO_CONTINUE` | `1` | 1 to 9 |
| `P1_START` / `P2_START` | `49` / `50` | Unity KeyCode values (`1` / `2`) |
| `P1_COIN` / `P2_COIN` | `53` / `54` | Unity KeyCode values (`5` / `6`) |
| `EXIT` | `27` | Quits the game (`Esc`), `0` = disabled |
| `MODE` | `Original` | `Original`, `Arcade` or `Horde` |
| `DIFFICULTY` | `Normal` | `VeryEasy` to `VeryHard` |
| `SCORE_MODE` | `Classic` | `Classic` or `Modern` |
| `DISABLE_MAIN_MENU` | `1` | `0` = the game's menu can still be opened |
| `SAVE_IN_GAME_FOLDER` | `1` | `0` = use the normal save file |
| `HIDE_CURSOR` | `1` | `0` = show the mouse cursor |
| `ARCADE_HUD` | `1` | `0` = the game's HUD, with the score and the weapon name |
| `ATTRACT_SOUND` | `1` | `0` = no sound on the title screen |

Game settings, empty = the game's own setting :

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

<br>

## Play
Use the following keyboard keys to play the game :  
`1`	-	P1 Start  
`2`	-	P2 Start  
`5` / `6`	-	Coins  
`Esc`	-	Exit  
<br>
Then use the mouse, or lightguns with [DemulShooter](https://github.com/argonlefou/DemulShooter).  
<br>

## Uninstall
Simply remove the dll file located in the `BepInEx\plugins\` folder of your game to play the original game as before. Your original save is not modified by the plugin.
