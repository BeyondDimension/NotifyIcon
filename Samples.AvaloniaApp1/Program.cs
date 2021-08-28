using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows;
using GtkApplication = Gtk.Application;

namespace Samples.AvaloniaApp1
{
    static class Program
    {
        public const string ArgsNotifyIcon = "-NotifyIcon";

        static IServiceProvider? services;

        static NotifyIconPipeClient? notifyIconPipeClient;

        public static bool IsNotifyIconProcess { get; private set; }

        public static bool IsLinuxNotifyIconProcess { get; private set; }

        internal static IServiceProvider Services => services ?? throw new ArgumentNullException(nameof(services));

        public static bool IsLinux
        //=> OperatingSystem2.IsWindows;
        => OperatingSystem2.IsLinux;

        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        static void Main(string[] args)
        {
            bool callBuildAvaloniaApp = true;
            try
            {
                IsNotifyIconProcess = args.Length == 3 && args[0] == ArgsNotifyIcon;
                IsLinuxNotifyIconProcess = IsLinux && IsNotifyIconProcess;

                var services = new ServiceCollection();
                ConfigureServices(services);
                Program.services = services.BuildServiceProvider();

                if (IsLinuxNotifyIconProcess && OperatingSystem2.IsLinux)
                {
                    // https://www.mono-project.com/docs/gui/gtksharp/widgets/notification-icon/
                    // Initialize GTK#
                    GtkApplication.Init();
                }
                if (OperatingSystem2.IsMacOS)
                {
                    AppDelegate.Init();
                }

                if (!IsLinux || IsLinuxNotifyIconProcess)
                {
                    var notifyIcon = Program.services.GetRequiredService<NotifyIcon>();
                    if (IsLinuxNotifyIconProcess)
                    {
                        notifyIconPipeClient = new(args[1], args[2]);
                        notifyIconPipeClient.OnStart();
                        notifyIcon.Click += (_, _) =>
                        {
                            notifyIconPipeClient.Append(NotifyIconPipeCore.CommandNotifyIconClick);
                        };
                    }
                    NotifyIconHelper.Init(notifyIcon, Shutdown);
                    if (IsLinuxNotifyIconProcess)
                    {
                        if (OperatingSystem2.IsLinux)
                        {
                            GtkApplication.Run();
                            callBuildAvaloniaApp = false;
                        }
                    }
                }

                if (callBuildAvaloniaApp)
                {
                    BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
               .UsePlatformDetect()
               .LogToTrace();

        /// <summary>
        ///
        /// </summary>
        /// <param name="services"></param>
        static void ConfigureServices(IServiceCollection services)
        {
            services.AddNotifyIcon();
        }

        internal static void Shutdown()
        {
            if (IsLinuxNotifyIconProcess)
            {
                notifyIconPipeClient?.Append(NotifyIconPipeCore.CommandExit);
            }
            if (App.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                Dispatcher.UIThread.Post(() =>
                {
                    desktop.Shutdown();
                }, DispatcherPriority.MaxValue);
            }
        }
    }
}