using HarmonyLib;
using BepInEx.Logging;

namespace BackgroundBlurImproved;

[HarmonyPatch]
public class Patcher {
    private static readonly ManualLogSource Log = BackgroundBlurImprovedPlugin.Log;

    [HarmonyPatch(typeof(BlurManager), nameof(BlurManager.Awake))]
    [HarmonyPostfix]
    static void BlurManager_Awake(BlurManager __instance) {
        var renderTextureHeight = (int)BackgroundBlurImprovedPlugin.blurRenderTextureHeightConfigEntry.Value;
        Log.LogDebug($"BlurManager.Awake() called on {__instance}[{__instance.GetHashCode()}] baseHeight: {__instance.baseHeight} -> {renderTextureHeight}");
        __instance.baseHeight = renderTextureHeight;
        BackgroundBlurImprovedPlugin.lightBlurredBackground = __instance.lightBlurredBackground;
    }

    [HarmonyPatch(typeof(BlurManager), nameof(BlurManager.Update))]
    [HarmonyPrefix]
    static void BlurManager_Update(BlurManager __instance) {
        var gameManager = GameManager.instance;
        if (gameManager != null) {
            // set the `appliedShaderQuality` so that when the real `Update` is called it doesn't overwrite the setting
            ShaderQualities shaderQuality = gameManager.gameSettings.shaderQuality;
            __instance.appliedShaderQuality = shaderQuality;
        }
    }

    [HarmonyPatch(typeof(LightBlurredBackground), nameof(LightBlurredBackground.Awake))]
    [HarmonyPostfix]
    static void LightBlurredBackground_Awake(LightBlurredBackground __instance) {
        var renderTextureHeight = (int)BackgroundBlurImprovedPlugin.blurRenderTextureHeightConfigEntry.Value;
        var passGroupCount = BackgroundBlurImprovedPlugin.blurPassGroupCountConfigEntry.Value;

        Log.LogDebug($"LightBlurredBackground.Awake called on {__instance}[{__instance.GetHashCode()}], renderTextureHeight: {__instance.renderTextureHeight} -> {renderTextureHeight}, passGroupCount: {__instance.passGroupCount} -> {passGroupCount}");
        __instance.passGroupCount = passGroupCount;
        __instance.renderTextureHeight = renderTextureHeight;
    }

    [HarmonyPatch(typeof(LightBlur), nameof(LightBlur.Awake))]
    [HarmonyPrefix]
    static void LightBlur_Awake(LightBlur __instance) {
        var blurEnabled = (bool)BackgroundBlurImprovedPlugin.blurEnableConfigEntry.Value;
        Log.LogDebug($"LightBlur.Awake called on {__instance}[{__instance.GetHashCode()}], will enable {blurEnabled}");
        __instance.enabled = blurEnabled;
    }

    [HarmonyPatch(typeof(BloomOptimized), nameof(BloomOptimized.Awake))]
    [HarmonyPostfix]
    static void BloomOptimized_Awake(BloomOptimized __instance) {
        Log.LogDebug($"BloomOptimized.Awake called on {__instance}[{__instance.GetHashCode()}]");
        BackgroundBlurImprovedPlugin.bloomOptimized = __instance;
        BackgroundBlurImprovedPlugin.ApplyBloomOptimizedSettings();
    }

    [HarmonyPatch(typeof(UIManager), nameof(UIManager.Awake))]
    [HarmonyPostfix]
    public static void UIManager_Awake(UIManager __instance) {
        Log.LogDebug($"UIManager.Awake called on {__instance}[{__instance.GetHashCode()}]");
        BackgroundBlurImprovedPlugin.CustomizeMenu(__instance);
    }
}