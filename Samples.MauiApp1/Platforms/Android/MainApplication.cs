using Android.App;
using Android.Runtime;

namespace Samples.MauiApp1
{
    [Application]
    public class MainApplication : MauiApplication, MauiProgram.IPlatformApplication
    {
        public MainApplication(IntPtr handle, JniHandleOwnership ownership)
            : base(handle, ownership)
        {
        }

        protected override MauiApp CreateMauiApp() => this.CreateMauiApplication();

        void MauiProgram.IPlatformApplication.Exit()
        {
        }
    }
}