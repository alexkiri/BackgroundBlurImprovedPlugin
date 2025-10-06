using BepInEx;

namespace hksBetterBlur;

// TODO - adjust the plugin guid as needed
[BepInAutoPlugin(id: "io.github.hksbetterblur")]
public partial class hksBetterBlurPlugin : BaseUnityPlugin
{
    private void Awake()
    {
        // Put your initialization logic here
        Logger.LogInfo($"Plugin {Name} ({Id}) has loaded!");
    }
}

