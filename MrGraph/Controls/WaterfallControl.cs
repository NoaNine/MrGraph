using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.ReactiveUI;
using MrGraph.Services.Interface;
using MrGraph.Settings;
using System;
using System.Reactive.Linq;

namespace MrGraph.Controls;

public class WaterfallControl : Control
{
    private readonly uint[] _colorLut = new uint[256];
    private int _writeIndex;

    private WriteableBitmap? _bitmap;
    private IDisposable? _subscription;

    private int _width;
    private readonly int _height = SpectrumSettings.WaterfallHeight;

    public static readonly StyledProperty<ISpectrumFrameSource?> DataSourceProperty =
        AvaloniaProperty.Register<WaterfallControl, ISpectrumFrameSource?>(nameof(DataSource));

    public ISpectrumFrameSource? DataSource
    {
        get => GetValue(DataSourceProperty);
        set => SetValue(DataSourceProperty, value);
    }

    public WaterfallControl()
    {
        InitializeLut();
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        if (_bitmap == null)
            return;

        var bounds = Bounds;

        using (context.PushClip(bounds))
        {
            context.DrawRectangle(Brushes.Black, null, bounds);

            context.DrawImage(_bitmap, new Rect(0, 0, _width, _height), bounds);
        }
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        _width = SpectrumSettings.SpectrumSize;

        _bitmap = new WriteableBitmap(
            new PixelSize(_width, _height),
            new Vector(96, 96),
            Avalonia.Platform.PixelFormat.Bgra8888,
            Avalonia.Platform.AlphaFormat.Opaque);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property != DataSourceProperty)
            return;

        _subscription?.Dispose();

        if (change.NewValue is ISpectrumFrameSource data)
        {
            _subscription = data.Frames
                .ObserveOn(AvaloniaScheduler.Instance)
                .Subscribe(frame =>
                {
                    CaptureFrame(frame.Data);
                    InvalidateVisual();
                });
        }
    }

    private void CaptureFrame(float[] data)
    {
        if (_bitmap == null)
            return;

        if (data.Length != _width)
            return;

        WriteRow(data);

        _writeIndex++;
        if (_writeIndex >= _height)
            _writeIndex = 0;
    }

    private void WriteRow(ReadOnlySpan<float> spectrum)
    {
        if (_bitmap == null)
            return;

        using var fb = _bitmap.Lock();

        unsafe
        {
            uint* ptr = (uint*)fb.Address;
            int stride = fb.RowBytes / 4;

            uint* rowPtr = ptr + _writeIndex * stride;

            double dbRange = SpectrumSettings.MaxDb - SpectrumSettings.MinDb;

            for (int x = 0; x < _width; x++)
            {
                double normalized = (spectrum[x] - SpectrumSettings.MinDb) / dbRange;

                normalized = Math.Clamp(normalized, 0, 1);

                int lutIndex = (int)(normalized * 255);
                rowPtr[x] = _colorLut[lutIndex];
            }
        }
    }

    private void InitializeLut()
    {
        for (int i = 0; i < 256; i++)
        {
            double normalized = i / 255.0;
            var color = GetColor(normalized);

            _colorLut[i] =
                (uint)(255 << 24 |
                       color.R << 16 |
                       color.G << 8 |
                       color.B);
        }
    }

    private Color GetColor(double normalized)
    {
        normalized = Math.Clamp(normalized, 0, 1);

        if (normalized < 0.25)
            return Lerp(Color.Parse("#0000ff"),
                        Color.Parse("#00ffff"),
                        normalized / 0.25);

        if (normalized < 0.5)
            return Lerp(Color.Parse("#00ffff"),
                        Color.Parse("#00ff00"),
                        (normalized - 0.25) / 0.25);

        if (normalized < 0.75)
            return Lerp(Color.Parse("#00ff00"),
                        Color.Parse("#ffff00"),
                        (normalized - 0.5) / 0.25);

        return Lerp(Color.Parse("#ffff00"),
                    Color.Parse("#ff0000"),
                    (normalized - 0.75) / 0.25);
    }

    private static Color Lerp(Color a, Color b, double t)
    {
        return Color.FromRgb(
            (byte)(a.R + (b.R - a.R) * t),
            (byte)(a.G + (b.G - a.G) * t),
            (byte)(a.B + (b.B - a.B) * t));
    }
}