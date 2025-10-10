using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

namespace BackgroundBlurImproved;

[BepInAutoPlugin("io.github.hksbetterblur", "BackgroundBlurImproved", "0.1.0")]
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
    public static ConfigEntry<BlurHeight> BaseHeightConfig;

    private void Awake() {
        Logger.LogInfo($"Plugin {Name} ({Id}) has loaded!");
        BaseHeightConfig = Config.Bind(
            "General",
            "BackgroundBlur layer height",
            BlurHeight.res360Default,
            "The height of the BackgroundBlur layer. Quit & reload is required in order to apply the changes."
        );
        harmony.PatchAll();
    }

    [HarmonyPatch]
    internal class Patcher {
        [HarmonyPatch(typeof(BlurManager), nameof(BlurManager.Awake))]
        [HarmonyPrefix]
        static void BlurManager_Awake(BlurManager __instance) {
            var adjustedBaseHeight = (int)BaseHeightConfig.Value;
            Debug.Log($"BlurManager.Awake() called on {__instance} hash:{__instance.GetHashCode()} baseHeight:{__instance.baseHeight} -> {adjustedBaseHeight}");
            __instance.baseHeight = adjustedBaseHeight;
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
