# Background Blur Improved

A mod for Hollow Knight: Silksong that improves the quality of the blurred background, and allows some customizations.


## Features

The game natively renders the background at a fixed 360p resolution. While this isn't very obvious because of the blur effect applied on top, it is noticeable and distracting when in motion, it appear flickery, as can be seen below:

Before:

https://github.com/user-attachments/assets/83eef05f-16b2-497f-8402-fe771f915fc8

After:

https://github.com/user-attachments/assets/aebc2904-8367-448e-a53f-d6d412184327

Image comparison:

https://imgsli.com/NDIyNDYx/0/2

This mod allows adjusting the blur effect by:
- increasing the resolution
- increasing the intensity
- disable the effect completely


## Installation

### The Simple Way

Use r2modman or Gale.

### Manual

1. Download [BepInExPack Silksong](https://thunderstore.io/c/hollow-knight-silksong/p/BepInEx/BepInExPack_Silksong/) and extract it to the game folder, next to the game executable
2. Download [BepinExConfigurationManager](https://thunderstore.io/c/hollow-knight-silksong/p/Yukikaco/BepinExConfigurationManager/)
2. Download from [BackgroundBlurImproved thunderstore](https://thunderstore.io/c/hollow-knight-silksong/p/alexkiri/BackgroundBlurImproved/) or [BackgroundBlurImproved github](https://github.com/alexkiri/BackgroundBlurImprovedPlugin/releases/download/0.7.0/BackgroundBlurImproved.zip) and extract it inside of `<game folder>/BepInEx/plugins`
3.
    - (Windows) Run the game normally
    - (Linux/MacOS) Run `run_bepinex.sh`


## Configuration

Use `BepinExConfigurationManager` (open with F1 by default) to adjust the parameters in realtime, or edit the `com.alexkiri.silksong.blurimproved.cfg` file inside `<game folder>/BepInEx/config`

Using a combination of these parameters can make the game look the same, but completely remove the distracting flickering.

- `RenderTextureHeight`
  - increases the resolution of the render target that displays the effect
  - improves the quality
  - lowers the blur intensity
  - has low impact on performance
- `PassGroupCount`
  - increases the number of passes of the blur effect
  - can increase intensity of the effect
  - has a medium / heavy impact on performance, values > 10 are not recommended
- `EnableEffect`
  - can disable the blur effect completely
  - not recommended, some source textures are low res, and without the blur effect, the background will look inconsistent
- `Apply Presets`
  - allows applying presets from the UI (`BepinExConfigurationManager` only)

## TODO
- Add controls to the the game's main menu
- Update the background drawing logic to allow multiple background layers blurred at different intensities
