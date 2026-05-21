using System.ComponentModel;

namespace BackgroundBlurImproved;

public enum BlurHeight {
    Verylow240 = 240,
    Verylow270 = 270,
    Vanilla360 = 360,
    Low480 = 480,
    Low540 = 540,
    Medium600 = 600,
    Medium720 = 720,
    High900 = 900,
    High1080 = 1080,
    Veryhigh1280 = 1280,
    Veryhigh1440 = 1440,
    Ultrahigh1800 = 1800,
    Ultrahigh2160 = 2160
}

public enum BlurPreset {
    Vanilla,
    Medium,
    High,
    VeryHigh,
    Custom
}