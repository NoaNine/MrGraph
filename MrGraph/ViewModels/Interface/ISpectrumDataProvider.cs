using System;

namespace MrGraph.ViewModels.Interface
{
    public interface ISpectrumDataProvider
    {
        ReadOnlySpan<float> GetData();
    }
}
