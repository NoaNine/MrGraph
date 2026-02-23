namespace MrGraph.Services.Interface
{
    public interface ISpectrumEngine : ISpectrumFrameSource
    {
        void Start();
        void Stop();
    }
}
