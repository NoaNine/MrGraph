using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MrGraph.Services;
using MrGraph.Services.Interface;
using MrGraph.ViewModels;
using MrGraph.Views;

namespace MrGraph;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override async void OnFrameworkInitializationCompleted()
    {
        // Line below is needed to remove Avalonia data validation.
        // Without this line you will get duplicate validations from both Avalonia and CT
        BindingPlugins.DataValidators.RemoveAt(0);

        var hostBuilder = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                services.AddSingleton<IDataGenerator, DataGenerator>();
                services.AddSingleton<ISpectrumEngine, SpectrumEngine>();
                services.AddSingleton<ISpectrumFrameSource>(sp =>
                    sp.GetRequiredService<ISpectrumEngine>());

                services.AddSingleton<MainWindow>();
                services.AddSingleton<MainViewModel>();
            });

        var host = hostBuilder.Build();

        await host.StartAsync();

        var services = host.Services;

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var window = services.GetRequiredService<MainWindow>();
            window.DataContext = services.GetRequiredService<MainViewModel>();
            desktop.MainWindow = window;
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = services.GetRequiredService<MainView>();
        }

        base.OnFrameworkInitializationCompleted();
    }
}
