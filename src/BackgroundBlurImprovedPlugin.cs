using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;

namespace BackgroundBlurImproved;

[BepInAutoPlugin(id: "io.github.alexkiri.backgroundblurimproved")]
public partial class BackgroundBlurImprovedPlugin : BaseUnityPlugin {
    internal static ManualLogSource Log;
    private readonly Harmony harmony = new(Id);

    public static ConfigEntry<BlurHeight> blurRenderTextureHeightConfigEntry;
    public static ConfigEntry<int> blurPassGroupCountConfigEntry;
    public static ConfigEntry<bool> blurEnableConfigEntry;
    public static ConfigEntry<BlurPreset> presetConfigEntry;

    public static LightBlurredBackground? lightBlurredBackground;

    public static void applyCurrentSettings() {
        var newRenderTextureHeightValue = (int)blurRenderTextureHeightConfigEntry.Value;
        var newPassGroupCountValue = blurPassGroupCountConfigEntry.Value;
        var newBlurEnableValue = blurEnableConfigEntry.Value;
        if (lightBlurredBackground != null) {
            Log.LogInfo($"will apply settings for {lightBlurredBackground} RenderTextureHeight: {lightBlurredBackground.renderTextureHeight}->{newRenderTextureHeightValue} PassGroupCount: {lightBlurredBackground.passGroupCount}->{newPassGroupCountValue}");
            lightBlurredBackground.lightBlur.enabled = true;
            lightBlurredBackground.RenderTextureHeight = newRenderTextureHeightValue;
            lightBlurredBackground.PassGroupCount = newPassGroupCountValue;
            lightBlurredBackground.enabled = false;
            lightBlurredBackground.enabled = true;
            lightBlurredBackground.lightBlur.enabled = newBlurEnableValue;
        }
    }

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
            presetConfigEntry.Value = BlurPreset.Custom;
            applyCurrentSettings();
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
            presetConfigEntry.Value = BlurPreset.Custom;
            applyCurrentSettings();
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
            Log.LogInfo($"blurEnableConfigEntry.SettingChanged -> {blurEnable}");
            presetConfigEntry.Value = BlurPreset.Custom;
            if (lightBlurredBackground != null) {
                lightBlurredBackground.lightBlur.enabled = blurEnable;
            }
        };

        presetConfigEntry = Config.Bind(
            "General",
            "Apply Preset",
            BlurPreset.Medium,
            new ConfigDescription(
                "Click to apply any preset.",
                null,
                new ConfigurationManagerAttributes { HideDefaultButton = true, CustomDrawer = PresetsCustomDrawer, Order = 0 }
            )
        );

        var gc = GameCameras.instance;
        if (gc != null) {
            Log.LogInfo($"Existing GameCameras object found {gc}");
            var lbb = gc.GetComponent<LightBlurredBackground>();
            if (lbb != null) {
                Log.LogInfo($"Existing LightBlurredBackground object found {lbb}");
                lightBlurredBackground = lbb;
                applyCurrentSettings();
            }
        }

        var um = UIManager.instance;
        if (um != null) {
            Log.LogInfo($"Existing UIManager object found {um}");
            CustomizeMenu(um);
        }

        Log.LogInfo($"Plugin {Name} ({Id}) has loaded!");
    }

    private void OnDestroy() {
        harmony.UnpatchSelf();
        lightBlurredBackground = null;
        Log.LogInfo($"Plugin {Name} ({Id}) has unloaded!");
    }

    private void PresetsCustomDrawer(ConfigEntryBase configEntry) {
        if (GUILayout.Button(BlurPreset.Vanilla.ToString(), GUILayout.ExpandWidth(true))) {
            blurEnableConfigEntry.Value = true;
            blurRenderTextureHeightConfigEntry.Value = BlurHeight.Vanilla_360;
            blurPassGroupCountConfigEntry.Value = 2;
            presetConfigEntry.Value = BlurPreset.Vanilla;
        }

        if (GUILayout.Button(BlurPreset.Medium.ToString(), GUILayout.ExpandWidth(true))) {
            blurEnableConfigEntry.Value = true;
            blurRenderTextureHeightConfigEntry.Value = BlurHeight.Medium_720;
            blurPassGroupCountConfigEntry.Value = 4;
            presetConfigEntry.Value = BlurPreset.Medium;
        }

        if (GUILayout.Button(BlurPreset.High.ToString(), GUILayout.ExpandWidth(true))) {
            blurEnableConfigEntry.Value = true;
            blurRenderTextureHeightConfigEntry.Value = BlurHeight.High_1080;
            blurPassGroupCountConfigEntry.Value = 6;
            presetConfigEntry.Value = BlurPreset.High;
        }

        if (GUILayout.Button(BlurPreset.VeryHigh.ToString(), GUILayout.ExpandWidth(true))) {
            blurEnableConfigEntry.Value = true;
            blurRenderTextureHeightConfigEntry.Value = BlurHeight.VeryHigh_1440;
            blurPassGroupCountConfigEntry.Value = 8;
            presetConfigEntry.Value = BlurPreset.VeryHigh;
        }
    }

    public static void CustomizeMenu(UIManager um) {
        var shaderSetting = um.advancedVideoMenuScreen.transform.Find("Content/ShaderSetting");
        if (shaderSetting == null) { return; }
        shaderSetting.gameObject.SetActive(false);
    }
}
