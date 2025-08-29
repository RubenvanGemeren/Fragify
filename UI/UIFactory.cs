using FragifyTracker.Models;

namespace FragifyTracker.UI;

public static class UIFactory
{
    public static IUserInterface CreateInterface(string uiType, int webPort = 5000)
    {
        return uiType.ToLower() switch
        {
            "cli" or "console" => new CliInterface(),
            "web" or "browser" => new WebInterface(webPort),
            _ => throw new ArgumentException($"Unknown UI type: {uiType}. Supported types: cli, web")
        };
    }

    public static void ShowUsage()
    {
        Console.WriteLine("Available UI modes:");
        Console.WriteLine("  --ui cli     - Command Line Interface (default)");
        Console.WriteLine("  --ui web     - Web-based Dashboard");
        Console.WriteLine("  --web-port   - Port for web interface (default: 5000)");
        Console.WriteLine();
        Console.WriteLine("Examples:");
        Console.WriteLine("  dotnet run -- --ui cli --test");
        Console.WriteLine("  dotnet run -- --ui web --web-port 8080");
        Console.WriteLine("  dotnet run -- --ui web --test");
    }
}
