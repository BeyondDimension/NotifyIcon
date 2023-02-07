using Microsoft.Maui;
using Microsoft.Maui.Hosting;
using System;

namespace Samples.MauiApp1
{
    class Program : MauiApplication, MauiProgram.IPlatformApplication
    {
        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

        static void Main(string[] args)
        {
            var app = new Program();
            app.Run(args);
        }

        void MauiProgram.IPlatformApplication.Exit()
        {
        }
    }
}