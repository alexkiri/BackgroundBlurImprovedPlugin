using BepInEx;
using BepInEx.Configuration;
using GenericVariableExtension;
using HarmonyLib;
using UnityEngine;

namespace BackgroundBlurImproved;

[BepInAutoPlugin("com.alexkiri.silksong.blurimproved", "Background Blur Improved", "0.2.0")]
public partial class BackgroundBlurImprovedPlugin : BaseUnityPlugin {
    private readonly Harmony harmony = new(Id);

    private static ConfigEntry<BlurHeight> blurRenderTextureHeightConfigEntry;
    private static ConfigEntry<int> blurPassGroupCountConfigEntry;
    private static ConfigEntry<bool> blurEnableConfigEntry;

    private static LightBlurredBackground? lightBlurredBackground;

    private void Awake() {
        Logger.LogInfo($"Plugin {Name} ({Id}) has loaded!");
        harmony.PatchAll();

        blurRenderTextureHeightConfigEntry = Config.Bind(
            "General",
            "RenderTextureHeight",
            BlurHeight.High_720,
            "The height of the BlurredBackground layer. The game default is 360. Low impact on performance."
        );
        blurRenderTextureHeightConfigEntry.SettingChanged += (sender, args) => {
            var newValue = (int)blurRenderTextureHeightConfigEntry.Value;
            Logger.LogInfo($"blurRenderTextureHeightConfigEntry.SettingChanged -> {newValue}");
            if (lightBlurredBackground != null) {
                Logger.LogInfo($"will set RenderTextureHeight for {lightBlurredBackground} {lightBlurredBackground.renderTextureHeight} -> {newValue}");
                blurEnableConfigEntry.Value = true;
                lightBlurredBackground.RenderTextureHeight = newValue;
                lightBlurredBackground.enabled = false;
                lightBlurredBackground.enabled = true;
            }
        };
        blurPassGroupCountConfigEntry = Config.Bind(
            "General",
            "PassGroupCount",
            4,
            new ConfigDescription(
                "The number of passes of the BlurredBackground layer. The game default is 2. Medium impact on performance.",
                new AcceptableValueRange<int>(1, 64)
            )
        );
        blurPassGroupCountConfigEntry.SettingChanged += (sender, args) => {
            var newValue = (int)blurPassGroupCountConfigEntry.Value;
            Logger.LogInfo($"blurPassGroupCountConfigEntry.SettingChanged -> {newValue}");
            if (lightBlurredBackground != null) {
                Logger.LogInfo($"will set PassGroupCount for {lightBlurredBackground} {lightBlurredBackground.passGroupCount} -> {newValue}");
                blurEnableConfigEntry.Value = true;
                lightBlurredBackground.PassGroupCount = newValue;
                lightBlurredBackground.enabled = false;
                lightBlurredBackground.enabled = true;
            }
        };
        blurEnableConfigEntry = Config.Bind(
            "General",
            "EnableEffect",
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
}
