using System.Windows;
using System.Runtime.InteropServices;
using WinRT;
using Microsoft.UI.Dispatching;
using Microsoft.Extensions.DependencyInjection;
using WinUIApplication = Microsoft.UI.Xaml.Application;

namespace Samples.WinUI3App1;

static class WinUIProgram
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

            var app = new App();
            Exit += () =>
            {
                var notifyIcon = app.Services.GetRequiredService<NotifyIcon>();
                notifyIcon.Dispose();
            };
        });

        Exit?.Invoke();
    }
}