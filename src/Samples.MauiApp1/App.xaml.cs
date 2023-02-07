namespace Samples.MauiApp1;

public sealed partial class App : Application
{
    public App()
    {
        InitializeComponent();

        MainPage = new AppShell();
    }
}