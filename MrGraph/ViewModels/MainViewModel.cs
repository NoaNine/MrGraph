using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MrGraph.ViewModels.Interface;
using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace MrGraph.ViewModels;

public partial class MainViewModel : ObservableObject, ISpectrumDataProvider, IDisposable
{
    private readonly Random _random = new();
    private readonly float[] _spectrumData = new float[1024];

    private IDisposable? _timerSubscription;

    private readonly Subject<Unit> _frameSubject = new();
    public IObservable<Unit> Frames => _frameSubject;

    public double ZoomX { get; set; } = 1.0;

    public ReadOnlySpan<float> GetData() => _spectrumData;

    [RelayCommand]
    private void Start()
    {
        if (_timerSubscription != null)
            return;

        _timerSubscription = Observable
            .Interval(TimeSpan.FromMilliseconds(50))
            .Subscribe(_ =>
            {
                Dispatcher.UIThread.Post(() =>
                {
                    GenerateData();
                    _frameSubject.OnNext(Unit.Default);
                });
            });
    }

    [RelayCommand]
    private void Stop()
    {
        _timerSubscription?.Dispose();
        _timerSubscription = null;
    }

    private void GenerateData()
    {
        const float baseValue = -70f;

        for (int i = 0; i < _spectrumData.Length; i++)
        {
            float noise = (float)(_random.NextDouble() * 20 - 10);
            _spectrumData[i] = baseValue + noise;
        }
    }

    public void Dispose()
    {
        _timerSubscription?.Dispose();
        _frameSubject.Dispose();
    }
}
