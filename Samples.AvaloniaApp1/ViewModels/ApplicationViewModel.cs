using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using ReactiveUI;
using System.Windows.Input;

namespace Samples.AvaloniaApp1.ViewModels;

public sealed class ApplicationViewModel : ReactiveObject
{
    public ApplicationViewModel()
    {
        ExitCommand = ReactiveCommand.Create(() =>
        {
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime lifetime)
            {
                lifetime.Shutdown();
            }
        });

        ToggleCommand = ReactiveCommand.Create(() =>
        {

        });
    }

    public ICommand ExitCommand { get; }

    public ICommand ToggleCommand { get; }
}
