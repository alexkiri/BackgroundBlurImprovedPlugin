using System.ComponentModel;

namespace BackgroundBlurImproved;

public enum BlurHeight {
    VeryLow_240 = 240,
    VeryLow_270 = 270,
    Vanilla_360 = 360,
    Low_480 = 480,
    Low_540 = 540,
    Medium_600 = 600,
    Medium_720 = 720,
    High_900 = 900,
    High_1080 = 1080,
    VeryHigh_1280 = 1280,
    VeryHigh_1440 = 1440,
    UltraHigh_1800 = 1800,
    UltraHigh_2160 = 2160
}

public enum BlurPreset {
    [Description("Vanilla")]
    Vanilla,
    [Description("Medium")]
    Medium,
    [Description("High")]
    High,
    [Description("VeryHigh")]
    VeryHigh,
    [Description("Custom")]
    Custom
}