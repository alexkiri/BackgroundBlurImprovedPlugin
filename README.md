# Background Blur Improved
A mod for Hollow Knight: Silksong that improves the quality of the blurred background, and allows some customizations.


## Features
The game natively renders the background at a fixed 360p resolution. While this isn't very obvious because of the blur effect applied on top, it is noticeable and distracting when in motion, it appear flickery.

This mod allows:
- increasing the resolution of the render target that displays the effect
  - this improves the quality
  - lowers the blur intensity
  - has low performance impact
- increasing the number of passes of the blur effect
  - has a medium performance impact
  - can increase intensity of the effect
- disable the blur effect completely

Using a combination of these parameters can make the game look the same, but completely remove the distracting flickering.

Before: 

https://github.com/user-attachments/assets/83eef05f-16b2-497f-8402-fe771f915fc8

After:

https://github.com/user-attachments/assets/aebc2904-8367-448e-a53f-d6d412184327




## Installation

### Manual
1. Download [BepInExPack Silksong](https://thunderstore.io/c/hollow-knight-silksong/p/BepInEx/BepInExPack_Silksong/) and extract it to the game folder, next to the game executable
2. Download [BepinExConfigurationManager](https://thunderstore.io/c/hollow-knight-silksong/p/Yukikaco/BepinExConfigurationManager/)
2. Download []() and extract it inside of `<game folder>/BepInEx/plugins`
3.
    - (Windows) Run the game normally
    - (Linux/MacOS) Run `run_bepinex.sh`


## Configuration

Use `BepInEx.ConfigurationManager` to adjust the parameters (Default: F1).

## TODO
Add controls to the the game's main menu
