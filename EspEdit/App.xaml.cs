using EspEdit.Services;
using Tes3Json.Services;
using Tes3Json.ViewModels;

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

        services.AddTransient<IDialogService, DialogService>();
        services.AddTransient<ITes3ConvService, Tes3ConvService>();

        // Viewmodels
        services.AddTransient<MainPageViewModel>();


        return services.BuildServiceProvider();
    }
}
