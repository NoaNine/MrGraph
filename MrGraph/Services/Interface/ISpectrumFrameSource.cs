using MrGraph.Models.Frames;
using System;

namespace MrGraph.Services.Interface;

public interface ISpectrumFrameSource
{
    IObservable<SpectrumFrame> Frames { get; }
}
