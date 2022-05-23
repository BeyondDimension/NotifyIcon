using System.Windows;
using System.Runtime.InteropServices;
using WinRT;
using Microsoft.UI.Dispatching;
using WinUIApplication = Microsoft.UI.Xaml.Application;
using WinUIApp = Samples.MauiApp1.WinUI.App;

namespace Samples.MauiApp1
{
    partial class MauiProgram
    {
        [DllImport("Microsoft.ui.xaml.dll")]
        static extern void XamlCheckProcessRequirements();

        [STAThread]
        static void Main(string[] args)
        {
            XamlCheckProcessRequirements();
            ComWrappersSupport.InitializeComWrappers();
            WinUIApplication.Start(p =>
            {
                var context = new DispatcherQueueSynchronizationContext(DispatcherQueue.GetForCurrentThread());
                SynchronizationContext.SetSynchronizationContext(context);

                var services = new ServiceCollection();
                ConfigureServices(services);
                value = services.BuildServiceProvider();

                var app = new WinUIApp();

                var notifyIcon = value.GetRequiredService<NotifyIcon>();
                Exit += (_, _) =>
                {
                    notifyIcon.Dispose();
                };
                NotifyIconHelper.Init(notifyIcon, app.Exit);
            });

            Exit?.Invoke(null, EventArgs.Empty);
        }

        static IServiceProvider? value;
        static event EventHandler? Exit;

        /// <summary>
        ///
        /// </summary>
        /// <param name="services"></param>
        static void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(typeof(NotifyIcon), NotifyIcon.ImplType);
        }
    }
}