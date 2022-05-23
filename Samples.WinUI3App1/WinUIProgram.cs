using System;
using System.Windows;
using System.Threading;
using System.Runtime.InteropServices;
using WinRT;
using Microsoft.UI.Dispatching;
using Microsoft.Extensions.DependencyInjection;
using WinUIApplication = Microsoft.UI.Xaml.Application;

namespace Samples.WinUI3App1
{
    static class WinUIProgram
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

                var app = new App();

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