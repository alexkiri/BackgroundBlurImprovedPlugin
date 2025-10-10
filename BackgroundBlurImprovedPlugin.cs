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

    public enum BlurHeight {
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
    public static ConfigEntry<BlurHeight> blurHeightConfigEntry;

    static LightBlurredBackground? lightBlurredBackground;

    private void Awake() {
        Logger.LogInfo($"Plugin {Name} ({Id}) has loaded!");
        blurHeightConfigEntry = Config.Bind(
            "General",
            "BlurredBackgroundLayerHeight",
            BlurHeight.res360Default,
            "The height of the BlurredBackground layer"
        );
        blurHeightConfigEntry.SettingChanged += (_, _) => {
            var newValue = (int)blurHeightConfigEntry.Value;
            Logger.LogInfo($"blurHeightConfigEntry.SettingChanged -> {newValue}");
            if (lightBlurredBackground != null) {
                Logger.LogInfo($"will set RenderTextureHeight for {lightBlurredBackground} {lightBlurredBackground.renderTextureHeight} -> {newValue}");
                lightBlurredBackground.RenderTextureHeight = newValue;
                lightBlurredBackground.enabled = false;
                lightBlurredBackground.enabled = true;
            }
        };
        harmony.PatchAll();
    }

    [HarmonyPatch]
    internal class Patcher {
        [HarmonyPatch(typeof(BlurManager), nameof(BlurManager.Awake))]
        [HarmonyPrefix]
        static void BlurManager_Awake_Prefix(BlurManager __instance) {
            var adjustedBaseHeight = (int)blurHeightConfigEntry.Value;
            Debug.Log($"BlurManager.Awake()_prefix called on {__instance} hash:{__instance.GetHashCode()} baseHeight:{__instance.baseHeight} -> {adjustedBaseHeight}");
            __instance.baseHeight = adjustedBaseHeight;
        }
        
        [HarmonyPatch(typeof(BlurManager), nameof(BlurManager.Awake))]
        [HarmonyPostfix]
        static void BlurManager_Awake_Postfix(BlurManager __instance) {
            lightBlurredBackground = __instance.lightBlurredBackground;
            Debug.Log($"BlurManager.Awake()_postfix called on {__instance} hash:{__instance.GetHashCode()} baseHeight:{__instance.baseHeight}");
        }

        /*
        [HarmonyPatch(typeof(LightBlur), nameof(LightBlur.PassGroupCount), MethodType.Setter)]
        [HarmonyPrefix]
        static void LightBlur_PassGroupCount_Setter(LightBlur __instance) {
            Debug.Log($"LightBlur.PassGroupCount.setter called on {__instance} hash:{__instance.GetHashCode()} passGroupCount: {__instance.passGroupCount} -> 8");
            __instance.passGroupCount = 8; // works, but it doesn't do anything
        }

        [HarmonyPatch(typeof(LightBlur), nameof(LightBlur.Awake))]
        [HarmonyPrefix]
        static void LightBlur_Awake(LightBlur __instance) {
            Debug.Log($"LightBlur.Awake called on {__instance} hash:{__instance.GetHashCode()}, will set blurShader");
            // var blur = new BlurOptimized();
            var blurShader = Shader.Find("Hidden/FastBlur");
            Debug.Log($"Instantiated {blurShader}");
            __instance.blurShader = blurShader;
        } */
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
