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
        static event Action? Exit;

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

                var app = new WinUIApp();
                Exit += () =>
                {
                    var notifyIcon = app.Services.GetRequiredService<NotifyIcon>();
                    notifyIcon.Dispose();
                };
            });

            Exit?.Invoke();
        }
    }
}