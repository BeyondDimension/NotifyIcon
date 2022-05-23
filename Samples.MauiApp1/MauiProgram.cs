#if WINDOWS
using NotifyIcon = System.Windows.NotifyIcon;
#endif

namespace Samples.MauiApp1
{
    public static partial class MauiProgram
    {
        public static MauiApp CreateMauiApplication(this IPlatformApplication application)
        {
            var builder = MauiApp.CreateBuilder();
            ConfigureServices(builder.Services);
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            var app = builder.Build();

#if WINDOWS
            var notifyIcon = app.Services.GetRequiredService<NotifyIcon>();
            NotifyIconHelper.Init(notifyIcon, application.Exit);
#endif

            return app;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="services"></param>
        static void ConfigureServices(IServiceCollection services)
        {
#if WINDOWS
            services.AddSingleton(typeof(NotifyIcon), NotifyIcon.ImplType);
#endif
        }

        public interface IPlatformApplication
        {
            IServiceProvider Services { get; }

            void Exit();
        }
    }
}