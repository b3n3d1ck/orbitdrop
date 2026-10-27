namespace OrbitDrop.Infrastructure;

/// <summary>Where Flickwheel keeps its own data. Nothing is written anywhere else except user-requested outputs.</summary>
public sealed class AppPaths
{
    public AppPaths(string? roamingRoot = null, string? localRoot = null)
    {
        RoamingDirectory = roamingRoot ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Flickwheel");
        LocalDirectory = localRoot ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Flickwheel");
    }

    /// <summary>%APPDATA%\Flickwheel: settings (small, follows the user).</summary>
    public string RoamingDirectory { get; }

    /// <summary>%LOCALAPPDATA%\Flickwheel: logs (machine-local).</summary>
    public string LocalDirectory { get; }

    public string SettingsFile => Path.Combine(RoamingDirectory, "settings.json");
    public string LogsDirectory => Path.Combine(LocalDirectory, "logs");
}
