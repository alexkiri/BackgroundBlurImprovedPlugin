using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Linq;
// using System.Collections;

namespace BackgroundBlurImproved;

[BepInAutoPlugin("io.github.hksbetterblur", "BackgroundBlurImproved", "0.1.0")]
public partial class BackgroundBlurImprovedPlugin : BaseUnityPlugin {
    private readonly Harmony harmony = new(Id);

    private void Awake()
    {
        Logger.LogInfo($"Plugin {Name} ({Id}) has loaded!");
        harmony.PatchAll();
    }

    [HarmonyPatch(typeof(BlurManager), nameof(BlurManager.Awake))]
    public class BlurManagerPatched
    {
        [HarmonyPrefix]
        static void Awake_Postfix(BlurManager __instance)
        {
            Debug.Log($"BlurManager.Awake() called on {__instance}, with baseHeight {__instance.baseHeight}");
            __instance.baseHeight = 540;
            /*
            1	2160
            2	1080
            3	720
            4	540
            5	432
            6	360
            7	308.5714286
            8	270
            9	240
            10	216
            */
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable() {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {

        GameObject[] allResources = Resources.FindObjectsOfTypeAll<GameObject>();
        foreach (var resource in allResources) {
            if (resource.name.ToLower().Contains("blurmanager"))
            {
                Logger.LogInfo($"found!!! {resource}");
            }
        }

        GameObject[] allGameObjects = GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        foreach (var gameObject in allGameObjects) {
            if (gameObject.name.ToLower().Contains("blurmanager"))
            {
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
