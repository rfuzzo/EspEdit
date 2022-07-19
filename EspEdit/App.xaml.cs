using EspEdit.Services;
using EspEdit.ViewModels;

namespace EspEdit;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        MainPage = new AppShell();

        Services = ConfigureServices();
    }

    /// <summary>
    /// Gets the current <see cref="App"/> instance in use
    /// </summary>
    public static new App Current => (App)Application.Current;

    /// <summary>
    /// Gets the <see cref="IServiceProvider"/> instance to resolve application services.
    /// </summary>
    public IServiceProvider Services { get; }

    /// <summary>
    /// Configures the services for the application.
    /// </summary>
    private static IServiceProvider ConfigureServices()
    {
        ServiceCollection services = new();

        //services.AddSingleton<ISettingsService, SettingsService>();
        services.AddTransient<IDialogService, DialogService>();
        services.AddTransient<ITes3ConvService, Tes3ConvService>();

        //services.AddTransient<MainPageViewModel>();


        return services.BuildServiceProvider();
    }
}
