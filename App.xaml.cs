using Microsoft.Extensions.DependencyInjection;
using WpfApp.Services;          // 添加 Services 引用
using WpfApp.ViewModels;        // 添加 ViewModels 引用
using System.Windows;

namespace WpfApp;

public partial class App : Application
{
    private readonly ServiceProvider _serviceProvider;

    public App()
    {
        this.DispatcherUnhandledException += App_DispatcherUnhandledException;
        
        var services = new ServiceCollection();
        ConfigureServices(services);
        _serviceProvider = services.BuildServiceProvider();
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<PlcCommunicationService>(_ => 
            new PlcCommunicationService("127.0.0.1", 502));

        services.AddSingleton<MongoDbService>(_ => 
            new MongoDbService("mongodb://localhost:27017"));

        services.AddSingleton<MainViewModel>();
        services.AddSingleton<MainWindow>();
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }

    private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
    {
        MessageBox.Show($"发生错误：{e.Exception.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        e.Handled = true;
    }
} 