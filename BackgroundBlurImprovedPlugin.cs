using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

namespace BackgroundBlurImproved;

[BepInAutoPlugin("mod.silksong.backgroundblurimproved", "BackgroundBlurImproved", "0.1.0")]
public partial class BackgroundBlurImprovedPlugin : BaseUnityPlugin {
    private readonly Harmony harmony = new(Id);

    private enum BlurHeight {
        res2160 = 2160,
        res1440 = 1440,
        res1080 = 1080,
        res720 = 720,
        res576 = 576,
        res540 = 540,
        res432 = 432,
        res360Default = 360,
        res270 = 270,
        res240 = 240,
        res216 = 216,
    }
    private static ConfigEntry<BlurHeight> blurRenderTextureHeightConfigEntry;
    private static ConfigEntry<int> blurPassGroupCountConfigEntry;
    private static ConfigEntry<bool> blurEnableConfigEntry;

    private static LightBlurredBackground? lightBlurredBackground;

    private void Awake() {
        Logger.LogInfo($"Plugin {Name} ({Id}) has loaded!");
        harmony.PatchAll();

        blurRenderTextureHeightConfigEntry = Config.Bind(
            "General",
            "BlurRenderTextureHeight",
            BlurHeight.res360Default,
            "The height of the BlurredBackground layer"
        );
        blurRenderTextureHeightConfigEntry.SettingChanged += (sender, args) => {
            var newValue = (int)blurRenderTextureHeightConfigEntry.Value;
            var blurEnable = (bool)blurEnableConfigEntry.Value;
            Logger.LogInfo($"blurRenderTextureHeightConfigEntry.SettingChanged -> {newValue}");
            if (lightBlurredBackground != null && blurEnable) {
                Logger.LogInfo($"will set RenderTextureHeight for {lightBlurredBackground} {lightBlurredBackground.renderTextureHeight} -> {newValue}");
                lightBlurredBackground.RenderTextureHeight = newValue;
                lightBlurredBackground.enabled = false;
                lightBlurredBackground.enabled = true;
            }
        };
        blurPassGroupCountConfigEntry = Config.Bind(
            "General",
            "BlurPassGroupCount",
            2,
            new ConfigDescription(
                "The number of passes of the BlurredBackground layer",
                new AcceptableValueRange<int>(1, 64)
            )
        );
        blurPassGroupCountConfigEntry.SettingChanged += (sender, args) => {
            var newValue = (int)blurPassGroupCountConfigEntry.Value;
            var blurEnable = (bool)blurEnableConfigEntry.Value;
            Logger.LogInfo($"blurPassGroupCountConfigEntry.SettingChanged -> {newValue}");
            if (lightBlurredBackground != null && blurEnable) {
                Logger.LogInfo($"will set PassGroupCount for {lightBlurredBackground} {lightBlurredBackground.passGroupCount} -> {newValue}");
                lightBlurredBackground.PassGroupCount = newValue;
                lightBlurredBackground.enabled = false;
                lightBlurredBackground.enabled = true;
            }
        };
        blurEnableConfigEntry = Config.Bind(
            "General",
            "BlurEnable",
            true,
            "When disabled, the BlurredBackground effect is completely removed, and the other settings have no effect"
        );
        blurEnableConfigEntry.SettingChanged += (sender, args) => {
            var blurEnable = (bool)blurEnableConfigEntry.Value;
            Debug.Log($"blurEnableConfigEntry.SettingChanged -> {blurEnable}");
            if (lightBlurredBackground != null) {
                lightBlurredBackground.lightBlur.enabled = blurEnable;
            }
        };
    }

    [HarmonyPatch]
    internal class Patcher {
        [HarmonyPatch(typeof(BlurManager), nameof(BlurManager.Awake))]
        [HarmonyPostfix]
        static void BlurManager_Awake(BlurManager __instance) {
            lightBlurredBackground = __instance.lightBlurredBackground;
            Debug.Log($"BlurManager.Awake()_postfix called on {__instance} hash:{__instance.GetHashCode()} baseHeight:{__instance.baseHeight}");
        }

        [HarmonyPatch(typeof(LightBlurredBackground), nameof(LightBlurredBackground.Awake))]
        [HarmonyPostfix]
        static void LightBlurredBackground_Awake(LightBlurredBackground __instance) {
            var renderTextureHeight = (int)blurRenderTextureHeightConfigEntry.Value;
            var passGroupCount = (int)blurPassGroupCountConfigEntry.Value;

            Debug.Log($"LightBlurredBackground.Awake called on {__instance} hash:{__instance.GetHashCode()}, renderTextureHeight: {__instance.renderTextureHeight} -> {renderTextureHeight}, passGroupCount: {__instance.passGroupCount} -> {passGroupCount}");
            __instance.passGroupCount = passGroupCount;
            __instance.renderTextureHeight = renderTextureHeight;
        }

        [HarmonyPatch(typeof(LightBlur), nameof(LightBlur.Awake))]
        [HarmonyPrefix]
        static void LightBlur_Awake(LightBlur __instance) {
            var blurEnable = (bool)blurEnableConfigEntry.Value;
            Debug.Log($"LightBlur.Awake called on {__instance} hash:{__instance.GetHashCode()}, will enable {blurEnable}");
            __instance.enabled = blurEnable;
        }
    }

    private void OnEnable() {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable() {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        GameObject[] allResources = Resources.FindObjectsOfTypeAll<GameObject>();
        foreach (var resource in allResources) {
            if (resource.name.ToLower().Contains("blurmanager")) {
                Logger.LogInfo($"found!!! {resource}");
            }
        }

        GameObject[] allGameObjects = GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        foreach (var gameObject in allGameObjects) {
            if (gameObject.name.ToLower().Contains("blurmanager")) {
                Logger.LogInfo($"found!!! {gameObject}");
            }
        }

        String[] names = [
            "BlurManager",
            "Blur Manager",
            "_GameCameras (BlurManager)"
        ];
    }
}
