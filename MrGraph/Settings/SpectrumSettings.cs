using System;

namespace MrGraph.Settings;

public static class SpectrumSettings
{
    public static readonly int SpectrumSize = 1024;
    public static readonly TimeSpan FrameInterval = TimeSpan.FromMilliseconds(50);

    public static readonly double MinZoomX = 1.0;
    public static readonly double MaxZoomX = 50.0;
    public static readonly double DefaultZoomX = 1.0;

    public static readonly double MinFrequency = 90.0;
    public static readonly double MaxFrequency = 110.0;
    public static readonly double FrequencyStep = 1.0;

    public static readonly double MinDb = -120.0;
    public static readonly double MaxDb = -20.0;
    public static readonly double GridDbStep = 10.0;

    public static readonly int WaterfallHeight = 200;
}
