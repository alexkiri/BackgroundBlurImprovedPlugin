using BepInEx.Logging;
using HarmonyLib;

namespace BackgroundBlurImproved;

[HarmonyPatch]
public class Patcher {
    private static ManualLogSource Logger = BackgroundBlurImprovedPlugin.Logger;

    [HarmonyPatch(typeof(BlurManager), nameof(BlurManager.Awake))]
    [HarmonyPostfix]
    static void BlurManager_Awake(BlurManager __instance) {
        var renderTextureHeight = (int)BackgroundBlurImprovedPlugin.blurRenderTextureHeightConfigEntry.Value;
        Logger.LogDebug($"BlurManager.Awake() called on {__instance}[{__instance.GetHashCode()}] baseHeight: {__instance.baseHeight} -> {renderTextureHeight}");
        __instance.baseHeight = renderTextureHeight;
        BackgroundBlurImprovedPlugin.lightBlurredBackground = __instance.lightBlurredBackground;
    }

    [HarmonyPatch(typeof(BlurManager), nameof(BlurManager.Update))]
    [HarmonyPrefix]
    static void BlurManager_Update(BlurManager __instance) {
        var gm = GameManager.instance;
        if (gm != null) {
            // set the `appliedShaderQuality` so that when the real `Update` is called it doesn't overwrite the setting
            ShaderQualities shaderQuality = gm.gameSettings.shaderQuality;
            __instance.appliedShaderQuality = shaderQuality;
        }
    }

    [HarmonyPatch(typeof(LightBlurredBackground), nameof(LightBlurredBackground.Awake))]
    [HarmonyPostfix]
    static void LightBlurredBackground_Awake(LightBlurredBackground __instance) {
        var renderTextureHeight = (int)BackgroundBlurImprovedPlugin.blurRenderTextureHeightConfigEntry.Value;
        var passGroupCount = BackgroundBlurImprovedPlugin.blurPassGroupCountConfigEntry.Value;

        Logger.LogDebug($"LightBlurredBackground.Awake called on {__instance}[{__instance.GetHashCode()}], renderTextureHeight: {__instance.renderTextureHeight} -> {renderTextureHeight}, passGroupCount: {__instance.passGroupCount} -> {passGroupCount}");
        __instance.passGroupCount = passGroupCount;
        __instance.renderTextureHeight = renderTextureHeight;
    }

    [HarmonyPatch(typeof(LightBlur), nameof(LightBlur.Awake))]
    [HarmonyPrefix]
    static void LightBlur_Awake(LightBlur __instance) {
        var blurEnable = (bool)BackgroundBlurImprovedPlugin.blurEnableConfigEntry.Value;
        Logger.LogDebug($"LightBlur.Awake called on {__instance}[{__instance.GetHashCode()}], will enable {blurEnable}");
        __instance.enabled = blurEnable;
    }

    [HarmonyPatch(typeof(UIManager), nameof(UIManager.Awake))]
    [HarmonyPostfix]
    static void UIManager_Awake() {
        var advancedVideoMenuScreen = UIManager.instance.advancedVideoMenuScreen;
        if (advancedVideoMenuScreen == null) { return; }
        var contentTransform = advancedVideoMenuScreen.transform.Find("Content");
        if (contentTransform == null) { return; }
        var shaderSettingTransform = contentTransform.Find("ShaderSetting");
        if (shaderSettingTransform == null) { return; }

        shaderSettingTransform.gameObject.SetActive(false);
    }
}
