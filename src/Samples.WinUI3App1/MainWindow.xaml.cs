using Microsoft.UI.Xaml;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Samples.WinUI3App1;

/// <summary>
/// An empty window that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class MainWindow : Window
{
    int count = 0;

    public MainWindow()
    {
        InitializeComponent();
    }

    private void OnCounterClicked(object sender, RoutedEventArgs e)
    {
        count++;

        if (count == 1)
            myButton.Content = $"Clicked {count} time";
        else
            myButton.Content = $"Clicked {count} times";
    }
}
