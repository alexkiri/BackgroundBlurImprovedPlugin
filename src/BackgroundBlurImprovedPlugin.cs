using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;
using Silksong.ModMenu.Plugin;

namespace BackgroundBlurImproved;

[BepInAutoPlugin(id: "io.github.alexkiri.backgroundblurimproved")]
public partial class BackgroundBlurImprovedPlugin : BaseUnityPlugin {
    internal static ManualLogSource Log;
    private readonly Harmony harmony = new(Id);
    private bool isApplyingPreset = false;

    public static ConfigEntry<BlurHeight> blurRenderTextureHeightConfigEntry;
    public static ConfigEntry<int> blurPassGroupCountConfigEntry;
    public static ConfigEntry<bool> blurEnableConfigEntry;
    public static ConfigEntry<BlurPreset> presetConfigEntry;

    public static ConfigEntry<BloomOptimized.Resolution> bloomOptimizedResolutionConfig;
    public static ConfigEntry<int> bloomOptimizedBlurIterationsConfig;

    public static LightBlurredBackground? lightBlurredBackground;
    public static BloomOptimized? bloomOptimized;

    public static void ApplyBlurredBackgroundSettings() {
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

    public static void ApplyBloomOptimizedSettings() {
        var newResolution = bloomOptimizedResolutionConfig.Value;
        var newBlurIterations = bloomOptimizedBlurIterationsConfig.Value;
        if (bloomOptimized != null) {
            Log.LogInfo($"will apply settings for {bloomOptimized} resolution: {bloomOptimized.resolution}->{newResolution} blurIterations: {bloomOptimized.blurIterations}->{newBlurIterations}");
            bloomOptimized.resolution = newResolution;
            bloomOptimized.blurIterations = newBlurIterations;
        }
    }

    private void Awake() {
        Log = base.Logger;
        harmony.PatchAll();

        blurRenderTextureHeightConfigEntry = Config.Bind(
            "BlurredBackground",
            "BlurTextureHeight",
            BlurHeight.Medium_720,
            new ConfigDescription(
                "Height of the BlurredBackground layer. Improves quality, lowers effect intensity.",
                null,
                new ConfigurationManagerAttributes { Order = 3 }
            )
        );
        blurRenderTextureHeightConfigEntry.SettingChanged += (sender, args) => {
            if (!isApplyingPreset) {
                presetConfigEntry.Value = BlurPreset.Custom;
            }
            ApplyBlurredBackgroundSettings();
        };

        blurPassGroupCountConfigEntry = Config.Bind(
            "BlurredBackground",
            "BlurPassCount",
            4,
            new ConfigDescription(
                "Number of blur effect passes. Medium / heavy performance impact.",
                new AcceptableValueRange<int>(1, 16),
                new ConfigurationManagerAttributes { Order = 2 },
                MenuElementGenerators.CreateIntSliderGenerator()
            )
        );
        blurPassGroupCountConfigEntry.SettingChanged += (sender, args) => {
            if (!isApplyingPreset) {
                presetConfigEntry.Value = BlurPreset.Custom;
            }
            ApplyBlurredBackgroundSettings();
        };

        blurEnableConfigEntry = Config.Bind(
            "BlurredBackground",
            "BlurEffectEnabled",
            true,
            new ConfigDescription(
                "Uncheck to completely disable the blur effect.",
                null,
                new ConfigurationManagerAttributes { Order = 1 }
            )
        );
        blurEnableConfigEntry.SettingChanged += (sender, args) => {
            var blurEnable = blurEnableConfigEntry.Value;
            Log.LogInfo($"blurEnableConfigEntry.SettingChanged -> {blurEnable}");
            if (!isApplyingPreset) {
                presetConfigEntry.Value = BlurPreset.Custom;
            }
            if (lightBlurredBackground != null) {
                lightBlurredBackground.lightBlur.enabled = blurEnable;
            }
        };

        presetConfigEntry = Config.Bind(
            "BlurredBackground",
            "ApplyBlurPreset",
            BlurPreset.Medium,
            new ConfigDescription(
                "Pick any predefined preset.",
                null,
                new ConfigurationManagerAttributes { HideDefaultButton = true, CustomDrawer = PresetsCustomDrawer, Order = 4 }
            )
        );
        presetConfigEntry.SettingChanged += (_, _) => ApplyPreset(presetConfigEntry.Value);

        bloomOptimizedResolutionConfig = Config.Bind(
            "Bloom",
            "BloomResolution",
            BloomOptimized.Resolution.Low,
            "Resolution of the Bloom effect."
        );
        bloomOptimizedResolutionConfig.SettingChanged += (_, _) => {
            ApplyBloomOptimizedSettings();
        };

        bloomOptimizedBlurIterationsConfig = Config.Bind(
            "Bloom",
            "BloomBlurIterations",
            1,
            new ConfigDescription(
                "Number of iterations for the Bloom effect.",
                new AcceptableValueRange<int>(1, 16),
                MenuElementGenerators.CreateIntSliderGenerator()
            )
        );
        bloomOptimizedBlurIterationsConfig.SettingChanged += (_, _) => {
            ApplyBloomOptimizedSettings();
        };

        var gc = GameCameras.instance;
        if (gc != null) {
            Log.LogInfo($"Existing GameCameras object found {gc}");
            var lbb = gc.GetComponent<LightBlurredBackground>();
            if (lbb != null) {
                Log.LogInfo($"Existing LightBlurredBackground object found {lbb}");
                lightBlurredBackground = lbb;
                ApplyBlurredBackgroundSettings();
            }
            var bo = gc.tk2dCam.GetComponent<BloomOptimized>();
            if (bo != null) {
                Log.LogInfo($"Existing BloomOptimized object found {bo}");
                bloomOptimized = bo;
                ApplyBloomOptimizedSettings();
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

    private void ApplyPreset(BlurPreset preset) {
        isApplyingPreset = true;
        try {
            switch (preset) {
                case BlurPreset.Vanilla:
                    blurEnableConfigEntry.Value = true;
                    blurRenderTextureHeightConfigEntry.Value = BlurHeight.Vanilla_360;
                    blurPassGroupCountConfigEntry.Value = 2;
                    break;
                case BlurPreset.Medium:
                    blurEnableConfigEntry.Value = true;
                    blurRenderTextureHeightConfigEntry.Value = BlurHeight.Medium_720;
                    blurPassGroupCountConfigEntry.Value = 4;
                    break;
                case BlurPreset.High:
                    blurEnableConfigEntry.Value = true;
                    blurRenderTextureHeightConfigEntry.Value = BlurHeight.High_1080;
                    blurPassGroupCountConfigEntry.Value = 6;
                    break;
                case BlurPreset.VeryHigh:
                    blurEnableConfigEntry.Value = true;
                    blurRenderTextureHeightConfigEntry.Value = BlurHeight.VeryHigh_1440;
                    blurPassGroupCountConfigEntry.Value = 8;
                    break;
            }
        } finally {
            isApplyingPreset = false;
        }
    }

    private void PresetsCustomDrawer(ConfigEntryBase configEntry) {
        if (GUILayout.Button(BlurPreset.Vanilla.ToString(), GUILayout.ExpandWidth(true))) {
            presetConfigEntry.Value = BlurPreset.Vanilla;
        }

        if (GUILayout.Button(BlurPreset.Medium.ToString(), GUILayout.ExpandWidth(true))) {
            presetConfigEntry.Value = BlurPreset.Medium;
        }

        if (GUILayout.Button(BlurPreset.High.ToString(), GUILayout.ExpandWidth(true))) {
            presetConfigEntry.Value = BlurPreset.High;
        }

        if (GUILayout.Button(BlurPreset.VeryHigh.ToString(), GUILayout.ExpandWidth(true))) {
            presetConfigEntry.Value = BlurPreset.VeryHigh;
        }
    }

    public static void CustomizeMenu(UIManager um) {
        var shaderSetting = um.advancedVideoMenuScreen.transform.Find("Content/ShaderSetting");
        if (shaderSetting == null) { return; }
        shaderSetting.gameObject.SetActive(false);
    }
}
