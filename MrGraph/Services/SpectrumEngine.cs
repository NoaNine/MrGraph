using MrGraph.Models.Frames;
using MrGraph.Services.Interface;
using MrGraph.Settings;
using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace MrGraph.Services;

public sealed class SpectrumEngine : ISpectrumEngine, IDisposable
{
    public IObservable<SpectrumFrame> Frames => _subject;

    private readonly IDataGenerator _generator;
    private readonly Subject<SpectrumFrame> _subject;
    private IDisposable? _timer;

    public SpectrumEngine(IDataGenerator generator)
    {
        _generator = generator;
        _subject = new Subject<SpectrumFrame>();
    }

    public void Start()
    {
        if (_timer != null)
            return;

        _timer = Observable
            .Interval(SpectrumSettings.FrameInterval)
            .Subscribe(_ =>
            {
                var buffer = new float[SpectrumSettings.SpectrumSize];

                _generator.GenerateData(buffer);

                _subject.OnNext(new SpectrumFrame
                {
                    Data = buffer
                });
            });
    }

    public void Stop()
    {
        _timer?.Dispose();
        _timer = null;
    }

    public void Dispose()
    {
        Stop();
        _subject.OnCompleted();
        _subject.Dispose();
    }
}
