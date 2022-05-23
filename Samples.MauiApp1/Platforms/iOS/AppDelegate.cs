using Foundation;

namespace Samples.MauiApp1
{
    [Register("AppDelegate")]
    public class AppDelegate : MauiUIApplicationDelegate, MauiProgram.IPlatformApplication
    {
        protected override MauiApp CreateMauiApp() => this.CreateMauiApplication();

        void MauiProgram.IPlatformApplication.Exit()
        {
        }
    }
}