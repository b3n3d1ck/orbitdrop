using System.Windows;
using System.Windows.Threading;

namespace OrbitDrop.App;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Offline-first: surface unexpected errors to the user instead of crashing silently.
        DispatcherUnhandledException += OnUnhandledException;
    }

    private void OnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        MessageBox.Show(
            $"OrbitDrop hit an unexpected error:\n\n{e.Exception.Message}",
            "OrbitDrop",
            MessageBoxButton.OK,
            MessageBoxImage.Error);
        e.Handled = true;
    }
}
