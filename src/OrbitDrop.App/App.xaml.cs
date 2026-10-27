using System.Windows;
using System.Windows.Threading;

namespace OrbitDrop.App;

public partial class App : Application
{
    private bool _showingError;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Offline-first: surface unexpected errors to the user instead of crashing silently.
        DispatcherUnhandledException += OnUnhandledException;
    }

    private void OnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        e.Handled = true;

        // Guard against a dialog storm: if an error dialog is already up, swallow further
        // repeats rather than stacking a new modal window for each one.
        if (_showingError)
            return;

        try
        {
            _showingError = true;
            MessageBox.Show(
                $"OrbitDrop hit an unexpected error:\n\n{e.Exception.Message}",
                "OrbitDrop",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
        finally
        {
            _showingError = false;
        }
    }
}
