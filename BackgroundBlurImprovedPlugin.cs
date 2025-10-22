using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;

namespace BackgroundBlurImproved;
[BepInAutoPlugin("com.alexkiri.silksong.blurimproved", "Background Blur Improved", "0.7.0")]
public partial class BackgroundBlurImprovedPlugin : BaseUnityPlugin {
    // public static ManualLogSource Logger;
    internal static new ManualLogSource Log;
    private readonly Harmony harmony = new(Id);

    public static ConfigEntry<BlurHeight> blurRenderTextureHeightConfigEntry;
    public static ConfigEntry<int> blurPassGroupCountConfigEntry;
    public static ConfigEntry<bool> blurEnableConfigEntry;

    public static LightBlurredBackground? lightBlurredBackground;

    private void Awake() {
        Log = base.Logger;
        harmony.PatchAll();

        blurRenderTextureHeightConfigEntry = Config.Bind(
            "General",
            "RenderTextureHeight",
            BlurHeight.Medium_720,
            new ConfigDescription(
                "The height of the BlurredBackground layer. Improves the quality, but lowers the blur effect intensity, with low impact on performance. Game default is 360.",
                null,
                new ConfigurationManagerAttributes { Order = 3 }
            )
        );
        blurRenderTextureHeightConfigEntry.SettingChanged += (sender, args) => {
            var newValue = (int)blurRenderTextureHeightConfigEntry.Value;
            var blurEnable = blurEnableConfigEntry.Value;
            Logger.LogInfo($"blurRenderTextureHeightConfigEntry.SettingChanged -> {newValue}");
            if (lightBlurredBackground != null) {
                Logger.LogInfo($"will set RenderTextureHeight for {lightBlurredBackground} {lightBlurredBackground.renderTextureHeight} -> {newValue}");
                lightBlurredBackground.lightBlur.enabled = true;
                lightBlurredBackground.RenderTextureHeight = newValue;
                lightBlurredBackground.enabled = false;
                lightBlurredBackground.enabled = true;
                lightBlurredBackground.lightBlur.enabled = blurEnable;
            }
        };

        blurPassGroupCountConfigEntry = Config.Bind(
            "General",
            "PassGroupCount",
            4,
            new ConfigDescription(
                "The number of passes of the BlurredBackground layer. Increases the intensity of the blur effect, with medium / heavy impact on performance. Game default is 2.",
                new AcceptableValueRange<int>(1, 32),
                new ConfigurationManagerAttributes { Order = 2 }
            )
        );
        blurPassGroupCountConfigEntry.SettingChanged += (sender, args) => {
            var newValue = blurPassGroupCountConfigEntry.Value;
            var blurEnable = blurEnableConfigEntry.Value;
            Logger.LogInfo($"blurPassGroupCountConfigEntry.SettingChanged -> {newValue}");
            if (lightBlurredBackground != null) {
                Logger.LogInfo($"will set PassGroupCount for {lightBlurredBackground} {lightBlurredBackground.passGroupCount} -> {newValue}");
                lightBlurredBackground.lightBlur.enabled = true;
                lightBlurredBackground.PassGroupCount = newValue;
                lightBlurredBackground.enabled = false;
                lightBlurredBackground.enabled = true;
                lightBlurredBackground.lightBlur.enabled = blurEnable;
            }
        };

        blurEnableConfigEntry = Config.Bind(
            "General",
            "EnableEffect",
            true,
            new ConfigDescription(
                "When disabled, the blur effect is completely removed, and the PassGroupCount setting will have no effect.",
                null,
                new ConfigurationManagerAttributes { IsAdvanced = true, Order = 1 }
            )
        );
        blurEnableConfigEntry.SettingChanged += (sender, args) => {
            var blurEnable = blurEnableConfigEntry.Value;
            Logger.LogInfo($"blurEnableConfigEntry.SettingChanged -> {blurEnable}");
            if (lightBlurredBackground != null) {
                lightBlurredBackground.lightBlur.enabled = blurEnable;
            }
        };

        Config.Bind(
            "General",
            "Apply Presets",
            "",
            new ConfigDescription(
                "Click to apply any preset.",
                null,
                new ConfigurationManagerAttributes { HideDefaultButton = true, CustomDrawer = PresetsCustomDrawer, Order = 0 }
            )
        );
        
        Logger.LogInfo($"Plugin {Name} ({Id}) has loaded!");
    }

    private void PresetsCustomDrawer(ConfigEntryBase configEntry) {
        if (GUILayout.Button("Vanilla", GUILayout.ExpandWidth(true))) {
            blurEnableConfigEntry.Value = true;
            blurRenderTextureHeightConfigEntry.Value = BlurHeight.Vanilla_360;
            blurPassGroupCountConfigEntry.Value = 2;
        }

        if (GUILayout.Button("Medium", GUILayout.ExpandWidth(true))) {
            blurEnableConfigEntry.Value = true;
            blurRenderTextureHeightConfigEntry.Value = BlurHeight.Medium_720;
            blurPassGroupCountConfigEntry.Value = 4;
        }

        if (GUILayout.Button("High", GUILayout.ExpandWidth(true))) {
            blurEnableConfigEntry.Value = true;
            blurRenderTextureHeightConfigEntry.Value = BlurHeight.High_1080;
            blurPassGroupCountConfigEntry.Value = 6;
        }

        if (GUILayout.Button("VeryHigh", GUILayout.ExpandWidth(true))) {
            blurEnableConfigEntry.Value = true;
            blurRenderTextureHeightConfigEntry.Value = BlurHeight.VeryHigh_1440;
            blurPassGroupCountConfigEntry.Value = 8;
        }
    }
}
