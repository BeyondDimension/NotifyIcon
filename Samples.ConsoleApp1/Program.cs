using Gtk;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace Samples.ConsoleApp1
{
    static class Program
    {
        const string Text = "Samples: NotifyIcon for Console";
        static readonly TaskCompletionSource tcs = new();
        static IServiceProvider? value;

        [STAThread]
        static void Main(string[] args)
        {
            var services = new ServiceCollection();
            ConfigureServices(services);
            value = services.BuildServiceProvider();

            Console.WriteLine(Text);

            Window? window = null;
            if (OperatingSystem2.IsLinux)
            {
                // https://www.mono-project.com/docs/gui/gtksharp/widgets/notification-icon/
                // Initialize GTK#
                Application.Init();

                // Create a Window with title
                window = new Window(Text);

                // Attach to the Delete Event when the window has been closed.
                window.DeleteEvent += (_, _) => Shutdown();

                window.ShowAll();
            }
            if (OperatingSystem2.IsMacOS)
            {
                AppDelegate.Init();
            }

            var notifyIcon = value.GetRequiredService<NotifyIcon>();
            NotifyIconHelper.Init(notifyIcon, Shutdown);
            Console.CancelKeyPress += (s, e) =>
            {
                Shutdown();
                e.Cancel = true;
            };

            if (OperatingSystem2.IsLinux)
            {
                notifyIcon.Click += (_, _) =>
                {
                    if (window != null)
                    {
                        // Show/Hide the window (even from the Panel/Taskbar) when the TrayIcon has been clicked.
                        window.Visible = !window.Visible;
                    }
                };
                Task.Factory.StartNew(Application.Run);
            }

            tcs.Task.GetAwaiter().GetResult();

            notifyIcon.Dispose();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="services"></param>
        static void ConfigureServices(IServiceCollection services)
        {
            services.AddNotifyIcon();
        }

        static void Shutdown()
        {
            tcs.TrySetResult();
            if (OperatingSystem2.IsLinux)
            {
                Application.Quit();
            }
        }
    }
}