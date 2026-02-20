using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MrGraph.ViewModels.Interface;
using System;

namespace MrGraph.ViewModels;

public partial class MainViewModel : ObservableObject, ISpectrumDataProvider
{
    private readonly DispatcherTimer _timer;
    private readonly Random _random = new();
    private readonly float[] _spectrumData = new float[1024];

    public double ZoomX { get; set; } = 1.0;

    public MainViewModel()
    {
        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(50)
        };

        _timer.Tick += (_, _) =>
        {
            GenerateData();
        };
    }

    public ReadOnlySpan<float> GetData() => _spectrumData;

    [RelayCommand]
    private void Start() => _timer.Start();

    [RelayCommand]
    private void Stop() => _timer.Stop();

    private void GenerateData()
    {
        const float baseValue = -70f;

        for (int i = 0; i < _spectrumData.Length; i++)
        {
            float noise = (float)(_random.NextDouble() * 20 - 10);
            _spectrumData[i] = baseValue + noise;
        }
    }
}
