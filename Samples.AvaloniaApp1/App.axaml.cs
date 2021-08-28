using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows;

namespace Samples.AvaloniaApp1
{
    /// <summary>
    ///
    /// </summary>
    public class App : Application
    {
        NotifyIconPipeServer? notifyIconPipeServer;

        public static new App? Current => Application.Current is App app ? app : null;

        /// <summary>
        ///
        /// </summary>
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        /// <summary>
        ///
        /// </summary>
        public override void OnFrameworkInitializationCompleted()
        {
#if DEBUG
            if (Program.IsLinuxNotifyIconProcess)
            {
                base.OnFrameworkInitializationCompleted();
                return;
            }
#endif

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.Exit += (_, _) =>
                {
                    notifyIconPipeServer?.Dispose();
                };

                desktop.MainWindow = new MainWindow();

                if (!Program.IsLinux)
                {
                    var notifyIcon = Program.Services.GetRequiredService<NotifyIcon>();
                    notifyIcon.Click += OnNotifyIconClick;
                }

                if (Program.IsLinux && !Program.IsNotifyIconProcess)
                {
                    notifyIconPipeServer = new();
                    notifyIconPipeServer.OnStart();
                }
            }

            base.OnFrameworkInitializationCompleted();
        }

        internal void OnNotifyIconClick(object? sender, EventArgs e)
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                if (desktop.MainWindow != null)
                {
                    Dispatcher.UIThread.Post(() =>
                    {
                        // Show/Hide the window (even from the Panel/Taskbar) when the TrayIcon has been clicked.
                        desktop.MainWindow.IsVisible = !desktop.MainWindow.IsVisible;
                    }, DispatcherPriority.MaxValue);
                }
            }
        }
    }
}