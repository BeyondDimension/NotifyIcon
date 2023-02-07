using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using NotifyIcon = System.Windows.NotifyIcon;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Samples.WinUI3App1;

/// <summary>
/// Provides application-specific behavior to supplement the default Application class.
/// </summary>
public sealed partial class App : Application
{
    IServiceProvider? services;

    public IServiceProvider Services => services ?? throw new ArgumentNullException(nameof(services));

    /// <summary>
    /// Initializes the singleton application object.  This is the first line of authored code
    /// executed, and as such is the logical equivalent of main() or WinMain().
    /// </summary>
    public App()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Invoked when the application is launched normally by the end user.  Other entry points
    /// will be used such as when the application is launched to open a specific file.
    /// </summary>
    /// <param name="args">Details about the launch request and process.</param>
    protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
    {
        var services = new ServiceCollection();
        ConfigureServices(services);
        this.services = services.BuildServiceProvider();

        var notifyIcon = Services.GetRequiredService<NotifyIcon>();
        NotifyIconHelper.Init(notifyIcon, Exit);

        m_window = new MainWindow();
        m_window.Activate();
    }

    private Window? m_window;

    /// <summary>
    ///
    /// </summary>
    /// <param name="services"></param>
    static void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton(typeof(NotifyIcon), NotifyIcon.ImplType);
    }
}
