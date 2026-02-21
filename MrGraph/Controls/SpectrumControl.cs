using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using MrGraph.ViewModels;
using MrGraph.ViewModels.Interface;
using System;

namespace MrGraph.Controls;

public class SpectrumControl : Control
{
    public static readonly StyledProperty<object?> DataSourceProperty =
        AvaloniaProperty.Register<SpectrumControl, object?>(nameof(DataSource));

    public object? DataSource
    {
        get => GetValue(DataSourceProperty);
        set => SetValue(DataSourceProperty, value);
    }

    public static readonly StyledProperty<double> ZoomXProperty =
        AvaloniaProperty.Register<SpectrumControl, double>(nameof(ZoomX), 1.0);

    public double ZoomX
    {
        get => GetValue(ZoomXProperty);
        set => SetValue(ZoomXProperty, value);
    }

    public static readonly StyledProperty<double> OffsetXProperty =
        AvaloniaProperty.Register<SpectrumControl, double>(nameof(OffsetX), 0);

    public double OffsetX
    {
        get => GetValue(OffsetXProperty);
        set => SetValue(OffsetXProperty, value);
    }

    private IDisposable? _subscription;

    private const double MinZoomX = 1.0;
    private const double MaxZoomX = 50.0;

    private const double MinFrequency = 90.0;
    private const double MaxFrequency = 110.0;

    private const double MinDb = -120.0;
    private const double MaxDb = -20.0;

    public SpectrumControl()
    {

    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        if (DataSource is not ISpectrumDataProvider provider)
            return;

        var data = provider.GetData();
        if (data.Length == 0)
            return;

        var bounds = Bounds;

        using (context.PushClip(bounds))
        {
            DrawPowerGrid(context, bounds);
            DrawFrequencyGrid(context, bounds);

            const double minDb = -120;
            const double maxDb = -20;
            double dbRange = maxDb - minDb;

            var pen = new Pen(Brushes.Lime, 1);
            var geometry = new StreamGeometry();

            using var geo = geometry.Open();

            for (int i = 0; i < data.Length; i++)
            {
                double x = (i / (double)(data.Length - 1)) *
                           bounds.Width * ZoomX + OffsetX;

                double normalized = (data[i] - minDb) / dbRange;
                normalized = Math.Clamp(normalized, 0, 1);

                double y = bounds.Bottom -
                           (normalized * bounds.Height);

                if (i == 0)
                    geo.BeginFigure(new Point(x, y), false);
                else
                    geo.LineTo(new Point(x, y));
            }

            context.DrawGeometry(null, pen, geometry);
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == DataSourceProperty)
        {
            _subscription?.Dispose();

            if (change.NewValue is MainViewModel vm)
            {
                _subscription = vm.Frames.Subscribe(_ =>
                {
                    InvalidateVisual();
                });
            }
        }
    }

    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        base.OnPointerWheelChanged(e);

        var pos = e.GetPosition(this);
        var bounds = Bounds;

        double zoomFactor = e.Delta.Y > 0 ? 1.1 : 0.9;

        double newZoom = ZoomX * zoomFactor;
        newZoom = Math.Clamp(newZoom, MinZoomX, MaxZoomX);

        zoomFactor = newZoom / ZoomX;
        ZoomX = newZoom;

        double relativeX = (pos.X - bounds.X) / bounds.Width;

        OffsetX -= relativeX * bounds.Width * (zoomFactor - 1);

        ClampOffsetX();

        InvalidateVisual();
    }

    private void ClampOffsetX()
    {
        var bounds = Bounds;

        double contentWidth = bounds.Width * ZoomX;

        if (contentWidth <= bounds.Width)
        {
            OffsetX = 0;
        }
        else
        {
            double minOffsetX = bounds.Width - contentWidth;
            OffsetX = Math.Clamp(OffsetX, minOffsetX, 0);
        }
    }

    private void DrawPowerGrid(DrawingContext context, Rect bounds)
    {
        var gridPen = new Pen(new SolidColorBrush(Color.FromRgb(60, 60, 60)), 1);
        var textBrush = Brushes.LightGray;

        double dbRange = MaxDb - MinDb;

        for (double db = MinDb; db <= MaxDb; db += 10)
        {
            double normalized = (db - MinDb) / dbRange;
            double y = bounds.Bottom - normalized * bounds.Height;

            context.DrawLine(gridPen,
                new Point(bounds.Left, y),
                new Point(bounds.Right, y));

            var text = new FormattedText(
                $"{db:0}",
                System.Globalization.CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight,
                new Typeface("Segoe UI"),
                10,
                textBrush);

            context.DrawText(text, new Point(bounds.Left + 5, y - 8));
        }
    }


    private void DrawFrequencyGrid(DrawingContext context, Rect bounds)
    {
        var gridPen = new Pen(new SolidColorBrush(Color.FromRgb(60, 60, 60)), 1);
        var textBrush = Brushes.LightGray;

        double span = MaxFrequency - MinFrequency;

        double contentWidth = bounds.Width * ZoomX;

        for (double freq = MinFrequency; freq <= MaxFrequency; freq += 1.0)
        {
            double normalized = (freq - MinFrequency) / span;

            double x = normalized * contentWidth + OffsetX;

            if (x < bounds.Left || x > bounds.Right)
                continue;

            context.DrawLine(gridPen,
                new Point(x, bounds.Top),
                new Point(x, bounds.Bottom));

            var text = new FormattedText(
                $"{freq:0}",
                System.Globalization.CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight,
                new Typeface("Segoe UI"),
                10,
                textBrush);

            context.DrawText(text, new Point(x + 3, bounds.Bottom - 18));
        }
    }
}
