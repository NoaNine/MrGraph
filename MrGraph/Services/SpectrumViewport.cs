using MrGraph.Settings;
using System;

namespace MrGraph.Services;

public class SpectrumViewport
{
    public double ZoomX { get; private set; } = SpectrumSettings.DefaultZoomX;

    public double OffsetX { get; private set; }

    public void Zoom(double factor, double pivot, double width)
    {
        var newZoom = Math.Clamp(ZoomX * factor, SpectrumSettings.MinZoomX, SpectrumSettings.MaxZoomX);

        factor = newZoom / ZoomX;
        ZoomX = newZoom;

        OffsetX -= pivot * width * (factor - 1);
        Clamp(width);
    }

    private void Clamp(double width)
    {
        double contentWidth = width * ZoomX;

        if (contentWidth <= width)
        {
            OffsetX = 0;
        }
        else
        {
            double minOffsetX = width - contentWidth;
            OffsetX = Math.Clamp(OffsetX, minOffsetX, 0);
        }
    }
}
