using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MrGraph.Models.Frames;
using MrGraph.Services.Interface;
using System;

namespace MrGraph.ViewModels;

public partial class MainViewModel(ISpectrumEngine engine) : ViewModelBase
{
    public double ZoomX { get; set; } = 1.0;

    public ISpectrumEngine Engine { get; } = engine;

    public ISpectrumFrameSource FrameSource => Engine;

    [ObservableProperty]
    private bool _isStartButtonEnabled = true;

    [ObservableProperty]
    private bool _isStopButtonEnabled = false;

    [RelayCommand]
    private void Start()
    {
        Engine.Start();

        IsStartButtonEnabled = false;
        IsStopButtonEnabled = true;
    }

    [RelayCommand]
    private void Stop()
    {
        Engine.Stop();

        IsStartButtonEnabled = true;
        IsStopButtonEnabled = false;
    }
}
