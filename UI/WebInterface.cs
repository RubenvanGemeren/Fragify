using FragifyTracker.Models;
using FragifyTracker.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace FragifyTracker.UI;

public class WebInterface : IUserInterface
{
    private WebApplication? _app;
    private GameStats? _lastStats;
    public bool IsRunning { get; private set; } = false;
    private readonly int _port;
    private readonly WebMapThemeService _mapThemeService;
    private readonly MinimapImageService _minimapImageService;

    public WebInterface(int port = 5000)
    {
        _port = port;
        _mapThemeService = new WebMapThemeService();
        _minimapImageService = new MinimapImageService();
    }

    public void Initialize()
    {
        var builder = WebApplication.CreateBuilder();
        _app = builder.Build();

        // Configure middleware
        _app.UseStaticFiles();

        // Configure routes
        ConfigureRoutes();

        // Start the web server
        Task.Run(() => _app.Run($"http://localhost:{_port}"));
        IsRunning = true;
    }

    public void Shutdown()
    {
        if (_app != null)
        {
            _app.StopAsync().Wait();
            IsRunning = false;
        }
    }

    public void UpdateDisplay(GameStats? stats)
    {
        if (stats == null) return;
        _lastStats = stats.Clone();
    }

    public void HandleInput(ConsoleKeyInfo? key = null)
    {
        // Web interface handles input through HTTP requests
    }

    private void ConfigureRoutes()
    {
        if (_app == null) return;

        // API endpoint for getting current game stats
        _app.MapGet("/api/stats", () =>
        {
            if (_lastStats == null)
                return Results.NotFound("No game stats available");

            return Results.Json(_lastStats, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
        });

        // API endpoint for getting the last raw GSI message
        _app.MapGet("/api/last-raw-message", () =>
        {
            if (_lastStats == null)
                return Results.NotFound("No game stats available");

            var rawMessageData = new
            {
                messageCount = _lastStats.MessagesReceived,
                lastMessageTime = _lastStats.LastMessageTime,
                mapName = _lastStats.MapName,
                rawMessage = _lastStats.LastMessageContent,
                messageSize = _lastStats.LastMessageContent?.Length ?? 0
            };

            return Results.Json(rawMessageData, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
        });

        // API endpoint for getting current map theme
        _app.MapGet("/api/theme", (string? mapName) =>
        {
            if (string.IsNullOrEmpty(mapName))
            {
                if (_lastStats?.MapName == null)
                    return Results.Json(_mapThemeService.GetDefaultTheme());
                mapName = _lastStats.MapName;
            }

            Console.WriteLine($"[THEME API] Requested theme for map: '{mapName}'");
            var theme = _mapThemeService.GetMapTheme(mapName);

            if (theme != null)
            {
                Console.WriteLine($"[THEME API] Found theme: {theme.Name}");
            }
            else
            {
                Console.WriteLine($"[THEME API] No theme found for map: '{mapName}'");
            }

            return Results.Json(theme);
        });

        // Main dashboard page - serve index.html
        _app.MapGet("/", async (HttpContext context) =>
        {
            var indexPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "index.html");
            if (File.Exists(indexPath))
            {
                var html = await File.ReadAllTextAsync(indexPath);
                context.Response.ContentType = "text/html";
                await context.Response.WriteAsync(html);
            }
            else
            {
                context.Response.StatusCode = 404;
                await context.Response.WriteAsync("Dashboard not found");
            }
        });

        // Test page - serve test.html
        _app.MapGet("/test", async (HttpContext context) =>
        {
            var testPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "test.html");
            if (File.Exists(testPath))
            {
                var html = await File.ReadAllTextAsync(testPath);
                context.Response.ContentType = "text/html";
                await context.Response.WriteAsync(html);
            }
            else
            {
                context.Response.StatusCode = 404;
                await context.Response.WriteAsync("Test page not found");
            }
        });

        // API endpoint for getting map data
        _app.MapGet("/api/maps", () =>
        {
            try
            {
                var mapDataPath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "comprehensive_maps.json");
                if (File.Exists(mapDataPath))
                {
                    var jsonContent = File.ReadAllText(mapDataPath);
                    return Results.Content(jsonContent, "application/json");
                }
                return Results.NotFound("Map data not found");
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error loading map data: {ex.Message}");
            }
        });
    }
}

