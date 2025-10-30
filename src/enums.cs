using System.ComponentModel;

namespace BackgroundBlurImproved;

public enum BlurHeight {
    VeryLow_240 = 240,
    Low_270 = 270,
    Vanilla_360 = 360,
    Medium_720 = 720,
    High_1080 = 1080,
    VeryHigh_1440 = 1440,
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