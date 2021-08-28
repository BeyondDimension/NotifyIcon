using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows;

namespace Samples.WpfApp1
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        IServiceProvider? value;

        /// <summary>
        ///
        /// </summary>
        /// <param name="e"></param>
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var services = new ServiceCollection();
            ConfigureServices(services);
            value = services.BuildServiceProvider();

            var notifyIcon = value.GetRequiredService<NotifyIcon>();
            Exit += (_, _) =>
            {
                notifyIcon.Dispose();
            };
            NotifyIconHelper.Init(notifyIcon, Shutdown);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="services"></param>
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddNotifyIcon();
        }
    }
}