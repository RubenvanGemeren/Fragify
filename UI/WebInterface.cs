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

        // API endpoint for getting session data
        _app.MapGet("/api/session", () =>
        {
            var session = _lastStats?.GetType().GetProperty("SessionData")?.GetValue(_lastStats);
            if (session == null)
                return Results.NotFound("No session data available");

            return Results.Json(session, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
        });

        // API endpoint for getting all players
        _app.MapGet("/api/players", () =>
        {
            // This would need to be implemented in the service layer
            return Results.Json(new { message = "Player data endpoint - to be implemented" });
        });

        // API endpoint for getting round data
        _app.MapGet("/api/rounds", () =>
        {
            // This would need to be implemented in the service layer
            return Results.Json(new { message = "Round data endpoint - to be implemented" });
        });

        // API endpoint for getting current map theme
        _app.MapGet("/api/theme", (string? mapName) =>
        {
            // If no mapName provided, use current game map
            if (string.IsNullOrEmpty(mapName))
            {
                if (_lastStats?.MapName == null)
                    return Results.Json(_mapThemeService.GetDefaultTheme());

                mapName = _lastStats.MapName;
            }

            var theme = _mapThemeService.GetMapTheme(mapName);

            // Add minimap URLs from the MinimapImageService
            if (theme != null && !string.IsNullOrEmpty(mapName))
            {
                var minimapUrls = _minimapImageService.GetMinimapUrls(mapName);
                if (minimapUrls.Count > 0)
                {
                    // Create a dynamic object with theme properties and minimap URLs
                    var themeWithMinimap = new
                    {
                        theme.Name,
                        theme.Description,
                        theme.PrimaryColor,
                        theme.SecondaryColor,
                        theme.BackgroundGradient,
                        theme.CardBackground,
                        theme.CardBorder,
                        theme.TextColor,
                        theme.AccentColor,
                        theme.DangerColor,
                        theme.SuccessColor,
                        theme.WarningColor,
                        minimapUrl = minimapUrls[0], // Primary URL
                        minimapUrls = minimapUrls // All available URLs
                    };
                    return Results.Json(themeWithMinimap);
                }
            }

            return Results.Json(theme);
        });

        // Main dashboard page
        _app.MapGet("/", async (HttpContext context) =>
        {
            var html = GenerateDashboardHtml();
            context.Response.ContentType = "text/html";
            await context.Response.WriteAsync(html);
        });
    }

    private string GenerateDashboardHtml()
    {
        return @"
<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Fragify - CS:GO Web Dashboard</title>
    <script src=""https://cdn.jsdelivr.net/npm/chart.js""></script>
    <style>
        * {
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }

        body {
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            background: var(--background-gradient, linear-gradient(135deg, #1e3c72 0%, #2a5298 100%));
            color: var(--text-color, white);
            min-height: 100vh;
        }

        .header {
            background: rgba(0, 0, 0, 0.3);
            padding: 1rem;
            text-align: center;
            backdrop-filter: blur(10px);
            margin-top: 8px; /* Account for fixed banner */
        }

        .header h1 {
            font-size: 2.5rem;
            color: var(--primary-color, #4ade80);
            text-shadow: 0 0 20px var(--primary-color-shadow, rgba(74, 222, 128, 0.5));
        }

        .header p {
            color: var(--text-color, #cbd5e1);
            margin-top: 0.5rem;
        }

        .theme-info {
            margin-top: 1rem;
            padding: 0.5rem 1rem;
            background: rgba(255, 255, 255, 0.1);
            border-radius: 8px;
            display: inline-block;
        }

        .theme-name {
            display: block;
            font-weight: bold;
            color: var(--primary-color, #4ade80);
            font-size: 1.1rem;
        }

        .theme-description {
            display: block;
            color: var(--text-color, #cbd5e1);
            font-size: 0.9rem;
            margin-top: 0.25rem;
        }

        .dashboard {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(400px, 1fr));
            gap: 1.5rem;
            padding: 1.5rem;
            max-width: 1400px;
            margin: 0 auto;
            margin-bottom: 120px; /* Account for debug controls */
        }

        .card {
            background: var(--card-background, rgba(255, 255, 255, 0.1));
            border-radius: 15px;
            padding: 1.5rem;
            backdrop-filter: blur(10px);
            border: 1px solid var(--card-border, rgba(255, 255, 255, 0.2));
            transition: transform 0.3s ease, box-shadow 0.3s ease;
        }

        .card:hover {
            transform: translateY(-5px);
            box-shadow: 0 10px 30px rgba(0, 0, 0, 0.3);
        }

        .card h2 {
            color: var(--primary-color, #4ade80);
            margin-bottom: 1rem;
            font-size: 1.5rem;
            border-bottom: 2px solid var(--primary-color, #4ade80);
            padding-bottom: 0.5rem;
        }

        .stats-grid {
            display: grid;
            grid-template-columns: 1fr 1fr;
            gap: 1rem;
        }

        .stat-item {
            display: flex;
            justify-content: space-between;
            padding: 0.5rem;
            background: rgba(255, 255, 255, 0.05);
            border-radius: 8px;
        }

        .stat-label {
            color: var(--text-color, #cbd5e1);
            font-weight: 500;
        }

        .stat-value {
            color: var(--primary-color, #4ade80);
            font-weight: bold;
        }

        .progress-bar {
            width: 100%;
            height: 20px;
            background: rgba(255, 255, 255, 0.1);
            border-radius: 10px;
            overflow: hidden;
            margin: 0.5rem 0;
        }

        .progress-fill {
            height: 100%;
            background: linear-gradient(90deg, var(--primary-color, #4ade80), var(--secondary-color, #22c55e));
            transition: width 0.3s ease;
        }

        .chart-container {
            position: relative;
            height: 300px;
            margin-top: 1rem;
        }

        .status-indicator {
            display: inline-block;
            width: 12px;
            height: 12px;
            border-radius: 50%;
            margin-right: 0.5rem;
        }

        .status-connected {
            background: var(--success-color, #4ade80);
            box-shadow: 0 0 10px var(--success-color-shadow, rgba(74, 222, 128, 0.5));
        }

        .status-disconnected {
            background: var(--danger-color, #ef4444);
            box-shadow: 0 0 10px var(--danger-color-shadow, rgba(239, 68, 68, 0.5));
        }

        .refresh-info {
            text-align: center;
            color: var(--text-color, #cbd5e1);
            margin-top: 1rem;
            font-size: 0.9rem;
        }

        @media (max-width: 768px) {
            .dashboard {
                grid-template-columns: 1fr;
                padding: 1rem;
            }

            .stats-grid {
                grid-template-columns: 1fr;
            }
        }

        /* Game State Banner - Top Edge Only */
        .game-state-banner {
            position: fixed;
            top: 0;
            left: 0;
            right: 0;
            height: 8px;
            background: var(--banner-color, #4A90E2);
            box-shadow: 0 0 20px var(--banner-color, #4A90E2);
            z-index: 1000;
            transition: all 0.8s cubic-bezier(0.4, 0, 0.2, 1);
            overflow: hidden;
        }

        .game-state-banner::before {
            content: '';
            position: absolute;
            top: 0;
            left: -100%;
            width: 100%;
            height: 100%;
            background: linear-gradient(90deg, transparent, rgba(255, 255, 255, 0.4), transparent);
            animation: shimmer 4s infinite;
        }

        /* Banner States */
        .game-state-banner.freezetime {
            background: linear-gradient(90deg, #87CEEB, #B0E0E6, #87CEEB);
            box-shadow: 0 0 30px #87CEEB, 0 0 60px rgba(135, 206, 235, 0.5);
        }

        .game-state-banner.round-won {
            background: linear-gradient(90deg, #32CD32, #00FF7F, #32CD32);
            box-shadow: 0 0 30px #32CD32, 0 0 60px rgba(50, 205, 50, 0.5);
        }

        .game-state-banner.round-lost {
            background: linear-gradient(90deg, #DC143C, #FF4500, #DC143C);
            box-shadow: 0 0 30px #DC143C, 0 0 60px rgba(220, 20, 60, 0.5);
        }

        .game-state-banner.bomb-planted {
            background: linear-gradient(90deg, #FFD700, #FFA500, #FFD700);
            box-shadow: 0 0 30px #FFD700, 0 0 60px rgba(255, 215, 0, 0.5);
            animation: bombPlantedPulse 1s ease-in-out infinite;
        }

        .game-state-banner.bomb-exploded {
            background: linear-gradient(90deg, #FF4500, #FF0000, #FF4500);
            box-shadow: 0 0 40px #FF4500, 0 0 80px rgba(255, 69, 0, 0.7);
            animation: bombExplosion 0.5s ease-out;
        }

        /* Fire effect for bomb explosion */
        .game-state-banner.bomb-exploded::after {
            content: '';
            position: absolute;
            top: 0;
            left: 0;
            right: 0;
            bottom: 0;
            background:
                radial-gradient(circle at 20% 50%, rgba(255, 255, 0, 0.8) 0%, transparent 50%),
                radial-gradient(circle at 80% 50%, rgba(255, 69, 0, 0.8) 0%, transparent 50%),
                radial-gradient(circle at 50% 50%, rgba(255, 215, 0, 0.6) 0%, transparent 50%);
            animation: fireFlicker 0.1s infinite alternate;
        }

        /* Freezetime specific effects */
        .game-state-banner.freezetime::after {
            content: '';
            position: absolute;
            top: 0;
            left: 0;
            right: 0;
            bottom: 0;
            background:
                radial-gradient(circle at 30% 50%, rgba(135, 206, 235, 0.6) 0%, transparent 50%),
                radial-gradient(circle at 70% 50%, rgba(176, 224, 230, 0.6) 0%, transparent 50%);
            animation: frostGlow 2s ease-in-out infinite alternate;
        }

        /* Round won/lost pulse effects */
        .game-state-banner.round-won,
        .game-state-banner.round-lost {
            animation: roundResultPulse 1.5s ease-in-out infinite;
        }

        /* Bomb Timer Box - Styled like theme box */
        .bomb-overlay {
            display: none;
            margin-left: 1rem;
            vertical-align: top;
            margin-top: 1rem;
        }

        .bomb-timer {
            background: rgba(255, 255, 255, 0.1);
            border-radius: 8px;
            padding: 0.5rem 1rem;
            min-width: 200px;
            border: 1px solid rgba(255, 255, 255, 0.2);
            display: flex;
            align-items: center;
            gap: 0.75rem;
        }

        .bomb-icon {
            font-size: 1.5rem;
            color: #FFD700;
            text-shadow: 0 0 10px #FFD700;
            animation: bombIconPulse 1s ease-in-out infinite;
            flex-shrink: 0;
        }

        .bomb-timer-content {
            flex: 1;
            text-align: center;
        }

        .bomb-timer-text {
            color: #FFD700;
            font-size: 1.2rem;
            font-weight: bold;
            margin-bottom: 0.5rem;
            display: block;
        }

        .bomb-progress-bar {
            width: 100%;
            height: 8px;
            background: rgba(255, 255, 255, 0.2);
            border-radius: 4px;
            overflow: hidden;
            position: relative;
        }

        .bomb-progress-fill {
            height: 100%;
            background: linear-gradient(90deg, #FFD700, #FFA500);
            transition: width 0.1s linear;
            border-radius: 4px;
        }

        .bomb-progress-fill.critical {
            background: linear-gradient(90deg, #FF4500, #FF0000);
            animation: criticalPulse 0.5s ease-in-out infinite;
        }

        /* Debug Controls */
        .debug-controls {
            position: fixed;
            bottom: 0;
            left: 0;
            right: 0;
            background: rgba(0, 0, 0, 0.8);
            padding: 1rem;
            backdrop-filter: blur(10px);
            border-top: 1px solid rgba(255, 255, 255, 0.2);
            z-index: 999;
        }

        .debug-controls h3 {
            color: var(--primary-color, #4ade80);
            margin-bottom: 1rem;
            text-align: center;
        }

        .debug-buttons {
            display: flex;
            flex-wrap: wrap;
            gap: 0.5rem;
            justify-content: center;
            max-width: 1200px;
            margin: 0 auto;
        }

        .debug-btn {
            padding: 0.5rem 1rem;
            border: none;
            border-radius: 6px;
            background: var(--card-background, rgba(255, 255, 255, 0.1));
            color: var(--text-color, white);
            cursor: pointer;
            transition: all 0.3s ease;
            border: 1px solid var(--card-border, rgba(255, 255, 255, 0.2));
            font-size: 0.9rem;
        }

        .debug-btn:hover {
            transform: translateY(-2px);
            box-shadow: 0 5px 15px rgba(0, 0, 0, 0.3);
        }

        .debug-btn.active {
            background: var(--primary-color, #4ade80);
            color: white;
        }

        /* Animations */
        @keyframes shimmer {
            0% { left: -100%; }
            100% { left: 100%; }
        }

        @keyframes bombPlantedPulse {
            0%, 100% {
                box-shadow: 0 0 30px #FFD700, 0 0 60px rgba(255, 215, 0, 0.5);
            }
            50% {
                box-shadow: 0 0 40px #FFD700, 0 0 80px rgba(255, 215, 0, 0.7);
            }
        }

        @keyframes bombExplosion {
            0% {
                transform: scaleY(1);
                box-shadow: 0 0 20px #FF4500;
            }
            50% {
                transform: scaleY(2);
                box-shadow: 0 0 60px #FF4500, 0 0 120px rgba(255, 69, 0, 0.8);
            }
            100% {
                transform: scaleY(1);
                box-shadow: 0 0 40px #FF4500, 0 0 80px rgba(255, 69, 0, 0.7);
            }
        }

        @keyframes fireFlicker {
            0% { opacity: 0.8; }
            100% { opacity: 1; }
        }

        @keyframes frostGlow {
            0% { opacity: 0.6; }
            100% { opacity: 1; }
        }

        @keyframes roundResultPulse {
            0%, 100% {
                box-shadow: 0 0 30px currentColor, 0 0 60px rgba(0, 0, 0, 0.3);
            }
            50% {
                box-shadow: 0 0 40px currentColor, 0 0 80px rgba(0, 0, 0, 0.5);
            }
        }

        @keyframes bombIconPulse {
            0%, 100% {
                transform: scale(1);
                text-shadow: 0 0 20px #FFD700;
            }
            50% {
                transform: scale(1.1);
                text-shadow: 0 0 30px #FFD700, 0 0 40px rgba(255, 215, 0, 0.5);
            }
        }

        @keyframes criticalPulse {
            0%, 100% {
                box-shadow: 0 0 20px #FF4500;
            }
            50% {
                box-shadow: 0 0 40px #FF4500, 0 0 60px rgba(255, 69, 0, 0.7);
            }
        }

        /* Custom Interactive Map Styles */
        .custom-interactive-map {
            position: relative;
            width: 100%;
            height: 400px;
            border: 2px solid var(--card-border, rgba(255, 255, 255, 0.2));
            border-radius: 8px;
            box-shadow: 0 4px 12px rgba(0, 0, 0, 0.3);
            background: rgba(255, 255, 255, 0.05);
            overflow: hidden;
        }

        .custom-interactive-map img {
            width: 100%;
            height: 100%;
            object-fit: contain;
        }

        .custom-interactive-map svg {
            position: absolute;
            top: 0;
            left: 0;
            width: 100%;
            height: 100%;
            pointer-events: none;
        }

        .callout-polygon {
            fill: rgba(255, 255, 255, 0.1);
            stroke: rgba(255, 255, 255, 0.3);
            stroke-width: 1;
            cursor: pointer;
            transition: all 0.2s ease;
        }

        .callout-polygon:hover {
            fill: rgba(255, 255, 255, 0.2);
            stroke: rgba(255, 255, 255, 0.6);
            stroke-width: 2;
        }

        .callout-tooltip {
            display: none;
            position: absolute;
            background: rgba(0, 0, 0, 0.9);
            color: white;
            padding: 0.5rem;
            border-radius: 4px;
            font-size: 0.9rem;
            pointer-events: none;
            z-index: 1000;
            max-width: 200px;
            text-align: center;
            box-shadow: 0 4px 12px rgba(0, 0, 0, 0.5);
        }
    </style>
</head>
<body>
    <div class=""game-state-banner""></div>

    <div class=""header"">
        <h1>🎯 Fragify</h1>
        <p>Counter-Strike: Global Offensive Web Dashboard</p>
        <div class=""theme-info"">
            <span class=""theme-name"" id=""theme-name"">Default Theme</span>
            <span class=""theme-description"" id=""theme-description"">CS:GO Dashboard</span>
        </div>

        <!-- Bomb Timer Box -->
        <div class=""bomb-overlay"" id=""bomb-overlay"">
            <div class=""bomb-timer"">
                <div class=""bomb-icon"">💣</div>
                <div class=""bomb-timer-content"">
                    <div class=""bomb-timer-text"" id=""bomb-timer-text"">40</div>
                    <div class=""bomb-progress-bar"">
                        <div class=""bomb-progress-fill"" id=""bomb-progress-fill""></div>
                    </div>
                </div>
            </div>
        </div>
    </div>

    <div class=""dashboard"">
        <!-- Game Information Card -->
        <div class=""card"">
            <h2>🎮 Game Information</h2>
            <div class=""stats-grid"">
                <div class=""stat-item"">
                    <span class=""stat-label"">Map:</span>
                    <span class=""stat-value"" id=""map-name"">Loading...</span>
                </div>
                <div class=""stat-item"">
                    <span class=""stat-label"">Mode:</span>
                    <span class=""stat-value"" id=""game-mode"">Loading...</span>
                </div>
                <div class=""stat-item"">
                    <span class=""stat-label"">Round:</span>
                    <span class=""stat-value"" id=""round-number"">Loading...</span>
                </div>
                <div class=""stat-item"">
                    <span class=""stat-label"">Phase:</span>
                    <span class=""stat-value"" id=""round-phase"">Loading...</span>
                </div>
                <div class=""stat-item"">
                    <span class=""stat-label"">Score:</span>
                    <span class=""stat-value"" id=""score"">Loading...</span>
                </div>
                <div class=""stat-item"">
                    <span class=""stat-label"">Round Time:</span>
                    <span class=""stat-value"" id=""round-time"">Loading...</span>
                </div>
            </div>

                        <!-- Interactive Map Display -->
            <div class=""interactive-map-container"" style=""margin-top: 1.5rem; text-align: center;"">
                <h3 style=""margin-bottom: 1rem; color: var(--accent-color, #fbbf24);"">🗺️ Interactive Map with Callouts</h3>
                <div class=""map-wrapper"" style=""display: inline-block; position: relative; width: 100%; max-width: 600px;"">
                    <div id=""interactive-map"" class=""custom-interactive-map"" style=""display: none; position: relative; width: 100%; height: 400px; border: 2px solid var(--card-border, rgba(255, 255, 255, 0.2)); border-radius: 8px; box-shadow: 0 4px 12px rgba(0, 0, 0, 0.3); background: rgba(255, 255, 255, 0.05); overflow: hidden;"">
                        <img id=""map-image"" src="""" alt=""Map minimap"" style=""width: 100%; height: 100%; object-fit: contain;"">
                        <svg id=""callout-overlay"" viewBox=""0 0 800 800"" preserveAspectRatio=""none"" style=""position: absolute; top: 0; left: 0; width: 100%; height: 100%; pointer-events: none;"">
                            <!-- Callout polygons will be dynamically inserted here -->
                        </svg>
                        <div id=""callout-tooltip"" class=""callout-tooltip"" style=""display: none; position: absolute; background: rgba(0, 0, 0, 0.9); color: white; padding: 0.5rem; border-radius: 4px; font-size: 0.9rem; pointer-events: none; z-index: 1000; max-width: 200px; text-align: center; box-shadow: 0 4px 12px rgba(0, 0, 0, 0.5);""></div>
                    </div>
                    <div id=""map-fallback"" style=""display: none; padding: 2rem; background: rgba(255, 255, 255, 0.05); border-radius: 8px; border: 2px dashed var(--card-border, rgba(255, 255, 255, 0.2)); height: 400px; display: flex; align-items: center; justify-content: center; flex-direction: column;"">
                        <p style=""color: var(--text-color, #cbd5e1); margin-bottom: 0.5rem; font-size: 1.1rem;"">🗺️ Interactive map not available</p>
                        <p style=""color: var(--text-color, #cbd5e1); font-size: 0.9rem; opacity: 0.8; margin-bottom: 1rem;"">Map: <span id=""fallback-map-name"">Unknown</span></p>
                        <a href=""#"" id=""external-map-link"" target=""_blank"" style=""color: var(--accent-color, #fbbf24); text-decoration: none; padding: 0.5rem 1rem; border: 1px solid var(--accent-color, #fbbf24); border-radius: 4px; transition: all 0.3s ease;"">Open in New Tab</a>
                    </div>
                </div>
                <div style=""margin-top: 0.5rem; font-size: 0.9rem; color: var(--text-color, #cbd5e1); opacity: 0.8;"">
                    Interactive map with custom callouts | <a href=""https://totalcsgo.com"" target=""_blank"" style=""color: var(--accent-color, #fbbf24);"">Powered by Total CS</a>
                </div>
            </div>
        </div>

        <!-- Player Statistics Card -->
        <div class=""card"">
            <h2>👤 Player Statistics</h2>
            <div class=""stats-grid"">
                <div class=""stat-item"">
                    <span class=""stat-label"">Kills:</span>
                    <span class=""stat-value"" id=""player-kills"">0</span>
                </div>
                <div class=""stat-item"">
                    <span class=""stat-label"">Deaths:</span>
                    <span class=""stat-value"" id=""player-deaths"">0</span>
                </div>
                <div class=""stat-item"">
                    <span class=""stat-label"">Assists:</span>
                    <span class=""stat-value"" id=""player-assists"">0</span>
                </div>
                <div class=""stat-item"">
                    <span class=""stat-label"">MVPs:</span>
                    <span class=""stat-value"" id=""player-mvps"">0</span>
                </div>
                <div class=""stat-item"">
                    <span class=""stat-label"">Score:</span>
                    <span class=""stat-value"" id=""player-score"">0</span>
                </div>
                <div class=""stat-item"">
                    <span class=""stat-label"">Team:</span>
                    <span class=""stat-value"" id=""player-team"">Unknown</span>
                </div>
            </div>
        </div>

        <!-- Player Status Card -->
        <div class=""card"">
            <h2>📊 Player Status</h2>
            <div class=""stats-grid"">
                <div class=""stat-item"">
                    <span class=""stat-label"">Health:</span>
                    <span class=""stat-value"" id=""player-health"">100</span>
                </div>
                <div class=""stat-item"">
                    <span class=""stat-label"">Armor:</span>
                    <span class=""stat-value"" id=""player-armor"">100</span>
                </div>
                <div class=""stat-item"">
                    <span class=""stat-label"">Money:</span>
                    <span class=""stat-value"" id=""player-money"">$800</span>
                </div>
                <div class=""stat-item"">
                    <span class=""stat-label"">Weapon:</span>
                    <span class=""stat-value"" id=""active-weapon"">Unknown</span>
                </div>
            </div>

            <div style=""margin-top: 1rem;"">
                <div class=""stat-label"">Health</div>
                <div class=""progress-bar"">
                    <div class=""progress-fill"" id=""health-bar"" style=""width: 100%""></div>
                </div>
            </div>

            <div style=""margin-top: 1rem;"">
                <div class=""stat-label"">Armor</div>
                <div class=""progress-bar"">
                    <div class=""progress-fill"" id=""armor-bar"" style=""width: 100%""></div>
                </div>
            </div>
        </div>

        <!-- Session Statistics Card -->
        <div class=""card"">
            <h2>📈 Session Statistics</h2>
            <div class=""stats-grid"">
                <div class=""stat-item"">
                    <span class=""stat-label"">Duration:</span>
                    <span class=""stat-value"" id=""session-duration"">00:00:00</span>
                </div>
                <div class=""stat-item"">
                    <span class=""stat-label"">Total Rounds:</span>
                    <span class=""stat-value"" id=""total-rounds"">0</span>
                </div>
                <div class=""stat-item"">
                    <span class=""stat-label"">Wins:</span>
                    <span class=""stat-value"" id=""rounds-won"">0</span>
                </div>
                <div class=""stat-item"">
                    <span class=""stat-label"">Losses:</span>
                    <span class=""stat-value"" id=""rounds-lost"">0</span>
                </div>
                <div class=""stat-item"">
                    <span class=""stat-label"">Win Rate:</span>
                    <span class=""stat-value"" id=""win-rate"">0%</span>
                </div>
            </div>

            <div class=""chart-container"">
                <canvas id=""rounds-chart""></canvas>
            </div>
        </div>

        <!-- Debug Information Card -->
        <div class=""card"">
            <h2>🔍 Debug Information</h2>
            <div class=""stats-grid"">
                <div class=""stat-item"">
                    <span class=""stat-label"">Status:</span>
                    <span class=""stat-value"">
                        <span class=""status-indicator status-disconnected"" id=""status-indicator""></span>
                        <span id=""connection-status"">Disconnected</span>
                    </span>
                </div>
                <div class=""stat-item"">
                    <span class=""stat-label"">Messages:</span>
                    <span class=""stat-value"" id=""messages-received"">0</span>
                </div>
                <div class=""stat-item"">
                    <span class=""stat-label"">Last Message:</span>
                    <span class=""stat-value"" id=""last-message-time"">Never</span>
                </div>
                <div class=""stat-item"">
                    <span class=""stat-label"">Last Update:</span>
                    <span class=""stat-value"" id=""last-update"">Never</span>
                </div>
            </div>

            <div style=""margin-top: 1rem;"">
                <div class=""stat-label"">Last Message Content:</div>
                <div style=""background: rgba(255, 255, 255, 0.05); padding: 0.5rem; border-radius: 8px; margin-top: 0.5rem; font-family: monospace; font-size: 0.9rem;"" id=""last-message-content"">
                    No messages yet
                </div>
            </div>
        </div>
    </div>

    <div class=""refresh-info"">
        Auto-refreshing every 2 seconds | Last update: <span id=""last-refresh"">Never</span>
    </div>

    <!-- Debug Controls -->
    <div class=""debug-controls"">
        <h3>🎮 Debug Controls - Test Game States Live</h3>
        <div class=""debug-buttons"">
            <button class=""debug-btn"" onclick=""setGameState('default')"">Default</button>
            <button class=""debug-btn"" onclick=""setGameState('freezetime')"">Freezetime</button>
            <button class=""debug-btn"" onclick=""setGameState('round-won')"">Round Won</button>
            <button class=""debug-btn"" onclick=""setGameState('round-lost')"">Round Lost</button>
            <button class=""debug-btn"" onclick=""setGameState('bomb-planted')"">Bomb Planted</button>
            <button class=""debug-btn"" onclick=""setGameState('bomb-exploded')"">Bomb Exploded</button>
            <button class=""debug-btn"" onclick=""simulateKill()"">Simulate Kill</button>
            <button class=""debug-btn"" onclick=""simulateDeath()"">Simulate Death</button>
            <button class=""debug-btn"" onclick=""simulateAssist()"">Simulate Assist</button>
            <button class=""debug-btn"" onclick=""simulateMVP()"">Simulate MVP</button>
        </div>
        <div style=""margin-top: 1rem; text-align: center; color: var(--text-color, #cbd5e1);"">
            <strong>Current State:</strong> <span id=""current-game-state"">Default</span>
        </div>
    </div>

    <script>
        let roundsChart;
        let currentGameState = 'default';
        let bombTimer = null;
        let bombCountdown = 40;

        // Game State Banner Control
        function setGameState(state) {
            const banner = document.querySelector('.game-state-banner');

            // Remove all existing state classes
            banner.className = 'game-state-banner';

            // Add new state class
            if (state !== 'default') {
                banner.classList.add(state);
            }

            currentGameState = state;
            document.getElementById('current-game-state').textContent = state.charAt(0).toUpperCase() + state.slice(1).replace('-', ' ');

            // Special handling for bomb planted state - only start if no timer is active
            if (state === 'bomb-planted' && !bombTimer) {
                startBombCountdown();
            }

            // Special handling for bomb explosion
            if (state === 'bomb-exploded') {
                stopBombCountdown();
                // Reset explosion animation after it completes
                setTimeout(() => {
                    if (currentGameState === 'bomb-exploded') {
                        banner.style.animation = 'none';
                        banner.offsetHeight; // Trigger reflow
                        banner.style.animation = 'bombExplosion 0.5s ease-out';
                    }
                }, 500);
            }
        }

        // Bomb countdown functions
        function startBombCountdown() {
            // Prevent starting if timer is already active
            if (bombTimer) {
                return;
            }

            bombCountdown = 40;
            const bombOverlay = document.getElementById('bomb-overlay');
            const bombTimerText = document.getElementById('bomb-timer-text');
            const bombProgressFill = document.getElementById('bomb-progress-fill');

            // Reset progress bar
            bombProgressFill.style.width = '100%';
            bombProgressFill.classList.remove('critical');
            bombTimerText.style.color = '#FFD700';

            bombOverlay.style.display = 'inline-block';

            bombTimer = setInterval(() => {
                bombCountdown--;
                bombTimerText.textContent = bombCountdown;

                // Update progress bar
                const progressPercent = (bombCountdown / 40) * 100;
                bombProgressFill.style.width = progressPercent + '%';

                // Critical phase (last 10 seconds)
                if (bombCountdown <= 10) {
                    bombProgressFill.classList.add('critical');
                    bombTimerText.style.color = '#FF4500';
                } else {
                    bombProgressFill.classList.remove('critical');
                    bombTimerText.style.color = '#FFD700';
                }

                if (bombCountdown <= 0) {
                    clearInterval(bombTimer);
                    bombTimer = null;
                    bombOverlay.style.display = 'none';
                    setGameState('bomb-exploded');
                }
            }, 1000);
        }

        function stopBombCountdown() {
            if (bombTimer) {
                clearInterval(bombTimer);
                bombTimer = null;
            }
            document.getElementById('bomb-overlay').style.display = 'none';
            bombCountdown = 40;

            // Reset progress bar
            const bombProgressFill = document.getElementById('bomb-progress-fill');
            const bombTimerText = document.getElementById('bomb-timer-text');
            bombProgressFill.style.width = '100%';
            bombProgressFill.classList.remove('critical');
            bombTimerText.style.color = '#FFD700';
            bombTimerText.textContent = '40';
        }

        function updateDashboard(stats) {
            // Update game information
            document.getElementById('map-name').textContent = stats.mapName || 'Unknown';
            document.getElementById('game-mode').textContent = stats.gameMode || 'Unknown';
            document.getElementById('round-number').textContent = stats.roundNumber || '0';
            document.getElementById('round-phase').textContent = stats.roundPhase || 'Unknown';
            document.getElementById('score').textContent = `${stats.scoreT || 0} - ${stats.scoreCT || 0}`;
            document.getElementById('round-time').textContent = formatTime(stats.roundTime || 0);

            // Update minimap display
            updateMinimap(stats.mapName);

            // Update player statistics
            document.getElementById('player-kills').textContent = stats.playerKills || 0;
            document.getElementById('player-deaths').textContent = stats.playerDeaths || 0;
            document.getElementById('player-assists').textContent = stats.playerAssists || 0;
            document.getElementById('player-mvps').textContent = stats.playerMvps || 0;
            document.getElementById('player-score').textContent = stats.playerScore || 0;
            document.getElementById('player-team').textContent = stats.playerTeam || 'Unknown';

            // Update player status
            document.getElementById('player-health').textContent = stats.playerHealth || 100;
            document.getElementById('player-armor').textContent = stats.playerArmor || 100;
            document.getElementById('player-money').textContent = `$${(stats.playerMoney || 800).toLocaleString()}`;
            document.getElementById('active-weapon').textContent = stats.activeWeapon || 'Unknown';

            // Update progress bars
            const healthPercent = Math.max(0, Math.min(100, (stats.playerHealth || 100)));
            const armorPercent = Math.max(0, Math.min(100, (stats.playerArmor || 100)));

            document.getElementById('health-bar').style.width = healthPercent + '%';
            document.getElementById('armor-bar').style.width = armorPercent + '%';

            // Update session statistics
            document.getElementById('session-duration').textContent = stats.sessionDuration || '00:00:00';
            document.getElementById('total-rounds').textContent = stats.totalRounds || 0;
            document.getElementById('rounds-won').textContent = stats.roundsWon || 0;
            document.getElementById('rounds-lost').textContent = stats.roundsLost || 0;
            document.getElementById('win-rate').textContent = `${(stats.winRate || 0).toFixed(1)}%`;

            // Update debug information
            const isConnected = stats.isConnected || false;
            const statusIndicator = document.getElementById('status-indicator');
            const connectionStatus = document.getElementById('connection-status');

            if (isConnected) {
                statusIndicator.className = 'status-indicator status-connected';
                connectionStatus.textContent = 'Connected';
            } else {
                statusIndicator.className = 'status-indicator status-disconnected';
                connectionStatus.textContent = 'Disconnected';
            }

            document.getElementById('messages-received').textContent = stats.messagesReceived || 0;
            document.getElementById('last-message-time').textContent = stats.lastMessageTime || 'Never';
            document.getElementById('last-message-content').textContent = stats.lastMessageContent || 'No messages yet';

            // Update last refresh time
            const now = new Date();
            document.getElementById('last-refresh').textContent = now.toLocaleTimeString();
            document.getElementById('last-update').textContent = now.toLocaleTimeString();

            // Update game state banner based on real game data
            updateBannerFromGameState(stats);
        }

                        function updateMinimap(mapName) {
            if (!mapName || mapName === 'Unknown') {
                document.getElementById('interactive-map').style.display = 'none';
                document.getElementById('map-fallback').style.display = 'block';
                document.getElementById('fallback-map-name').textContent = 'Unknown';
                return;
            }

            // Get the map image URL and callout data for the selected map
            const mapData = getMapData(mapName);

            if (mapData) {
                const interactiveMap = document.getElementById('interactive-map');
                const fallback = document.getElementById('map-fallback');
                const mapImage = document.getElementById('map-image');
                const calloutOverlay = document.getElementById('callout-overlay');

                // Show the interactive map
                interactiveMap.style.display = 'block';
                fallback.style.display = 'none';

                // Set the map image
                mapImage.src = mapData.imageUrl;

                // Update the external link
                const externalLink = document.getElementById('external-map-link');
                externalLink.href = mapData.totalCsUrl;

                // Generate callout polygons
                generateCalloutPolygons(calloutOverlay, mapData.callouts);

                console.log(`Loading interactive map for ${mapName}`);
            } else {
                // No interactive map available for this map
                document.getElementById('interactive-map').style.display = 'none';
                document.getElementById('map-fallback').style.display = 'block';
                document.getElementById('fallback-map-name').textContent = mapName;

                // Disable the external link
                const externalLink = document.getElementById('external-map-link');
                externalLink.href = '#';
                externalLink.style.opacity = '0.5';
                externalLink.style.pointerEvents = 'none';
            }
        }

                function getMapData(mapName) {
            // Map of CS:GO/CS2 map names to map data (image URL, Total CS URL, and callouts)
            const mapDataMap = {
                'de_dust2': {
                    imageUrl: 'https://static.totalcsgo.com/totalcsgo-strapi/dust2_11347b32ec.png',
                    totalCsUrl: 'https://totalcsgo.com/callouts/dust2',
                    callouts: [
                        { points: '357 308,591 305,588 416,443 413,443 392,352 397', name: 'B Site', description: 'Bomb site B' },
                        { points: '147 264,218 267,223 192,147 192', name: 'T Spawn', description: 'Terrorist spawn area' },
                        { points: '380 669,490 650,500 580,378 592', name: 'A Site', description: 'Bomb site A' },
                        { points: '673 468,729 471,738 245,670 243', name: 'CT Spawn', description: 'Counter-Terrorist spawn area' }
                    ]
                },
                'de_mirage': {
                    imageUrl: 'https://static.totalcsgo.com/totalcsgo-strapi/mirage_11347b32ec.png',
                    totalCsUrl: 'https://totalcsgo.com/callouts/mirage',
                    callouts: [
                        { points: '357 308,591 305,588 416,443 413,443 392,352 397', name: 'B Site', description: 'Bomb site B' },
                        { points: '147 264,218 267,223 192,147 192', name: 'T Spawn', description: 'Terrorist spawn area' },
                        { points: '380 669,490 650,500 580,378 592', name: 'A Site', description: 'Bomb site A' },
                        { points: '673 468,729 471,738 245,670 243', name: 'CT Spawn', description: 'Counter-Terrorist spawn area' },
                        { points: '261 650,392 682,373 618,289 600,242 609', name: 'Palace', description: 'Palace area' },
                        { points: '234 536,184 541,185 608,282 607,275 384,230 385', name: 'T Ramp', description: 'T ramp to mid' },
                        { points: '301 514,392 522,388 465,295 457', name: 'Mid', description: 'Mid area' },
                        { points: '370 215,364 395,338 395,342 187,494 193,492 212', name: 'A Ramp', description: 'A ramp' },
                        { points: '135 378,268 384,264 342,225 343,218 311,133 310', name: 'B Ramp', description: 'B ramp' },
                        { points: '179 325,216 324,218 303,178 304', name: 'B Apps', description: 'B apartments' }
                    ]
                },
                'de_inferno': {
                    imageUrl: 'https://static.totalcsgo.com/totalcsgo-strapi/inferno_11347b32ec.png',
                    totalCsUrl: 'https://totalcsgo.com/callouts/inferno',
                    callouts: [
                        { points: '357 308,591 305,588 416,443 413,443 392,352 397', name: 'B Site', description: 'Bomb site B' },
                        { points: '147 264,218 267,223 192,147 192', name: 'T Spawn', description: 'Terrorist spawn area' },
                        { points: '380 669,490 650,500 580,378 592', name: 'A Site', description: 'Bomb site A' },
                        { points: '673 468,729 471,738 245,670 243', name: 'CT Spawn', description: 'Counter-Terrorist spawn area' }
                    ]
                },
                'de_cache': {
                    imageUrl: 'https://static.totalcsgo.com/totalcsgo-strapi/cache_11347b32ec.png',
                    totalCsUrl: 'https://totalcsgo.com/callouts/cache',
                    callouts: [
                        { points: '357 308,591 305,588 416,443 413,443 392,352 397', name: 'B Site', description: 'Bomb site B' },
                        { points: '147 264,218 267,223 192,147 192', name: 'T Spawn', description: 'Terrorist spawn area' },
                        { points: '380 669,490 650,500 580,378 592', name: 'A Site', description: 'Bomb site A' },
                        { points: '673 468,729 471,738 245,670 243', name: 'CT Spawn', description: 'Counter-Terrorist spawn area' }
                    ]
                },
                'de_overpass': {
                    imageUrl: 'https://static.totalcsgo.com/totalcsgo-strapi/overpass_11347b32ec.png',
                    totalCsUrl: 'https://totalcsgo.com/callouts/overpass',
                    callouts: [
                        { points: '357 308,591 305,588 416,443 413,443 392,352 397', name: 'B Site', description: 'Bomb site B' },
                        { points: '147 264,218 267,223 192,147 192', name: 'T Spawn', description: 'Terrorist spawn area' },
                        { points: '380 669,490 650,500 580,378 592', name: 'A Site', description: 'Bomb site A' },
                        { points: '673 468,729 471,738 245,670 243', name: 'CT Spawn', description: 'Counter-Terrorist spawn area' }
                    ]
                },
                'de_nuke': {
                    imageUrl: 'https://static.totalcsgo.com/totalcsgo-strapi/nuke_11347b32ec.png',
                    totalCsUrl: 'https://totalcsgo.com/callouts/nuke',
                    callouts: [
                        { points: '357 308,591 305,588 416,443 413,443 392,352 397', name: 'B Site', description: 'Bomb site B' },
                        { points: '147 264,218 267,223 192,147 192', name: 'T Spawn', description: 'Terrorist spawn area' },
                        { points: '380 669,490 650,500 580,378 592', name: 'A Site', description: 'Bomb site A' },
                        { points: '673 468,729 471,738 245,670 243', name: 'CT Spawn', description: 'Counter-Terrorist spawn area' }
                    ]
                },
                'de_ancient': {
                    imageUrl: 'https://static.totalcsgo.com/totalcsgo-strapi/ancient_11347b32ec.png',
                    totalCsUrl: 'https://totalcsgo.com/callouts/ancient',
                    callouts: [
                        { points: '357 308,591 305,588 416,443 413,443 392,352 397', name: 'B Site', description: 'Bomb site B' },
                        { points: '147 264,218 267,223 192,147 192', name: 'T Spawn', description: 'Terrorist spawn area' },
                        { points: '380 669,490 650,500 580,378 592', name: 'A Site', description: 'Bomb site A' },
                        { points: '673 468,729 471,738 245,670 243', name: 'CT Spawn', description: 'Counter-Terrorist spawn area' }
                    ]
                },
                'de_vertigo': {
                    imageUrl: 'https://static.totalcsgo.com/totalcsgo-strapi/vertigo_11347b32ec.png',
                    totalCsUrl: 'https://totalcsgo.com/callouts/vertigo',
                    callouts: [
                        { points: '357 308,591 305,588 416,443 413,443 392,352 397', name: 'B Site', description: 'Bomb site B' },
                        { points: '147 264,218 267,223 192,147 192', name: 'T Spawn', description: 'Terrorist spawn area' },
                        { points: '380 669,490 650,500 580,378 592', name: 'A Site', description: 'Bomb site A' },
                        { points: '673 468,729 471,738 245,670 243', name: 'CT Spawn', description: 'Counter-Terrorist spawn area' }
                    ]
                }
            };

            return mapDataMap[mapName.toLowerCase()] || null;
        }

        function generateCalloutPolygons(svgElement, callouts) {
            // Clear existing polygons
            svgElement.innerHTML = '';

            // Enable pointer events on the SVG
            svgElement.style.pointerEvents = 'auto';

            callouts.forEach((callout, index) => {
                const polygon = document.createElementNS('http://www.w3.org/2000/svg', 'polygon');
                polygon.setAttribute('points', callout.points);
                polygon.setAttribute('class', 'callout-polygon');
                polygon.setAttribute('data-name', callout.name);
                polygon.setAttribute('data-description', callout.description);
                polygon.setAttribute('data-index', index);

                // Style the polygon
                polygon.style.fill = 'rgba(255, 255, 255, 0.1)';
                polygon.style.stroke = 'rgba(255, 255, 255, 0.3)';
                polygon.style.strokeWidth = '1';
                polygon.style.cursor = 'pointer';
                polygon.style.transition = 'all 0.2s ease';

                // Add hover effects
                polygon.addEventListener('mouseenter', function(e) {
                    this.style.fill = 'rgba(255, 255, 255, 0.2)';
                    this.style.stroke = 'rgba(255, 255, 255, 0.6)';
                    this.style.strokeWidth = '2';
                    showCalloutTooltip(e, callout.name, callout.description);
                });

                polygon.addEventListener('mouseleave', function() {
                    this.style.fill = 'rgba(255, 255, 255, 0.1)';
                    this.style.stroke = 'rgba(255, 255, 255, 0.3)';
                    this.style.strokeWidth = '1';
                    hideCalloutTooltip();
                });

                svgElement.appendChild(polygon);
            });
        }

        function showCalloutTooltip(event, name, description) {
            const tooltip = document.getElementById('callout-tooltip');
            const mapContainer = document.getElementById('interactive-map');

            // Calculate tooltip position
            const rect = mapContainer.getBoundingClientRect();
            const x = event.clientX - rect.left;
            const y = event.clientY - rect.top;

            // Position tooltip above the cursor
            tooltip.style.left = (x + 10) + 'px';
            tooltip.style.top = (y - 40) + 'px';

            // Set tooltip content
            tooltip.innerHTML = `<strong>${name}</strong><br>${description}`;
            tooltip.style.display = 'block';
        }

        function hideCalloutTooltip() {
            const tooltip = document.getElementById('callout-tooltip');
            tooltip.style.display = 'none';
        }

            // Update session statistics
            document.getElementById('session-duration').textContent = stats.sessionDuration || '00:00:00';
            document.getElementById('total-rounds').textContent = stats.totalRounds || 0;
            document.getElementById('rounds-won').textContent = stats.roundsWon || 0;
            document.getElementById('rounds-lost').textContent = stats.roundsLost || 0;
            document.getElementById('win-rate').textContent = `${(stats.winRate || 0).toFixed(1)}%`;

            // Update debug information
            const isConnected = stats.isConnected || false;
            const statusIndicator = document.getElementById('status-indicator');
            const connectionStatus = document.getElementById('connection-status');

            if (isConnected) {
                statusIndicator.className = 'status-indicator status-connected';
                connectionStatus.textContent = 'Connected';
            } else {
                statusIndicator.className = 'status-indicator status-disconnected';
                connectionStatus.textContent = 'Disconnected';
            }

            document.getElementById('messages-received').textContent = stats.messagesReceived || 0;
            document.getElementById('last-message-time').textContent = stats.lastMessageTime || 'Never';
            document.getElementById('last-message-content').textContent = stats.lastMessageContent || 'No messages yet';

            // Update last refresh time
            const now = new Date();
            document.getElementById('last-refresh').textContent = now.toLocaleTimeString();
            document.getElementById('last-update').textContent = now.toLocaleTimeString();

            // Update game state banner based on real game data
            updateBannerFromGameState(stats);
        }

        // Function to update banner based on real game state data
        function updateBannerFromGameState(gameStateData) {
            if (!gameStateData) return;

            // Check for round results first
            if (gameStateData.IsRoundWon) {
                setGameState('round-won');
                return;
            }

            if (gameStateData.IsRoundLost) {
                setGameState('round-lost');
                return;
            }

            // Check bomb state
            if (gameStateData.BombState === 'Planted') {
                setGameState('bomb-planted');
                return;
            }

            if (gameStateData.BombState === 'Exploded') {
                setGameState('bomb-exploded');
                return;
            }

            // Check game phase
            if (gameStateData.CurrentPhase === 'Freezetime') {
                setGameState('freezetime');
                return;
            }

                    // Default state
        setGameState('default');
    }

    // Simulation functions for debugging
    function simulateKill() {
        const currentKills = parseInt(document.getElementById('player-kills').textContent) || 0;
        document.getElementById('player-kills').textContent = currentKills + 1;

        // Update player score
        const currentScore = parseInt(document.getElementById('player-score').textContent) || 0;
        document.getElementById('player-score').textContent = currentScore + 100;

        console.log('Simulated kill - Kills:', currentKills + 1, 'Score:', currentScore + 100);
    }

    function simulateDeath() {
        const currentDeaths = parseInt(document.getElementById('player-deaths').textContent) || 0;
        document.getElementById('player-deaths').textContent = currentDeaths + 1;

        // Update player score
        const currentScore = parseInt(document.getElementById('player-score').textContent) || 0;
        document.getElementById('player-score').textContent = Math.max(0, currentScore - 300);

        console.log('Simulated death - Deaths:', currentDeaths + 1, 'Score:', Math.max(0, currentScore - 300));
    }

    function simulateAssist() {
        const currentAssists = parseInt(document.getElementById('player-assists').textContent) || 0;
        document.getElementById('player-assists').textContent = currentAssists + 1;

        // Update player score
        const currentScore = parseInt(document.getElementById('player-score').textContent) || 0;
        document.getElementById('player-score').textContent = currentScore + 50;

        console.log('Simulated assist - Assists:', currentAssists + 1, 'Score:', currentScore + 50);
    }

    function simulateMVP() {
        const currentMvps = parseInt(document.getElementById('player-mvps').textContent) || 0;
        document.getElementById('player-mvps').textContent = currentMvps + 1;

        // Update player score
        const currentScore = parseInt(document.getElementById('player-score').textContent) || 0;
        document.getElementById('player-score').textContent = currentScore + 200;

        console.log('Simulated MVP - MVPs:', currentMvps + 1, 'Score:', currentScore + 200);
    }

        function formatTime(seconds) {
            const mins = Math.floor(seconds / 60);
            const secs = seconds % 60;
            return `${mins.toString().padStart(2, '0')}:${secs.toString().padStart(2, '0')}`;
        }

        function initializeCharts() {
            const ctx = document.getElementById('rounds-chart').getContext('2d');
            roundsChart = new Chart(ctx, {
                type: 'line',
                data: {
                    labels: ['Round 1', 'Round 2', 'Round 3', 'Round 4', 'Round 5'],
                    datasets: [{
                        label: 'Kills per Round',
                        data: [0, 0, 0, 0, 0],
                        borderColor: '#4ade80',
                        backgroundColor: 'rgba(74, 222, 128, 0.1)',
                        tension: 0.4
                    }]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: false,
                    plugins: {
                        legend: {
                            labels: {
                                color: 'white'
                            }
                        }
                    },
                    scales: {
                        x: {
                            ticks: {
                                color: 'white'
                            },
                            grid: {
                                color: 'rgba(255, 255, 255, 0.1)'
                            }
                        },
                        y: {
                            ticks: {
                                color: 'white'
                            },
                            grid: {
                                color: 'rgba(255, 255, 255, 0.1)'
                            }
                        }
                    }
                }
            });
        }

        async function fetchStats() {
            try {
                const response = await fetch('/api/stats');
                if (response.ok) {
                    const stats = await response.json();
                    updateDashboard(stats);

                    // Fetch and apply theme based on current map
                    await fetchAndApplyTheme(stats.mapName);
                }
            } catch (error) {
                console.error('Failed to fetch stats:', error);
            }
        }

        async function fetchAndApplyTheme(mapName) {
            try {
                const response = await fetch('/api/theme');
                if (response.ok) {
                    const theme = await response.json();
                    applyTheme(theme);
                }
            } catch (error) {
                console.error('Failed to fetch theme:', error);
            }
        }

        function applyTheme(theme) {
            // Update theme info display
            document.getElementById('theme-name').textContent = theme.name;
            document.getElementById('theme-description').textContent = theme.description;

            // Apply CSS variables to root element
            const root = document.documentElement;
            root.style.setProperty('--primary-color', theme.primaryColor);
            root.style.setProperty('--secondary-color', theme.secondaryColor);
            root.style.setProperty('--background-gradient', theme.backgroundGradient);
            root.style.setProperty('--card-background', theme.cardBackground);
            root.style.setProperty('--card-border', theme.cardBorder);
            root.style.setProperty('--text-color', theme.textColor);
            root.style.setProperty('--accent-color', theme.accentColor);
            root.style.setProperty('--danger-color', theme.dangerColor);
            root.style.setProperty('--success-color', theme.successColor);
            root.style.setProperty('--warning-color', theme.warningColor);

            // Add shadow variants for better visual effects
            root.style.setProperty('--primary-color-shadow', theme.primaryColor + '80');
            root.style.setProperty('--success-color-shadow', theme.successColor + '80');
            root.style.setProperty('--danger-color-shadow', theme.dangerColor + '80');

            // Update chart colors if chart exists
            if (roundsChart) {
                roundsChart.data.datasets[0].borderColor = theme.primaryColor;
                roundsChart.data.datasets[0].backgroundColor = theme.primaryColor + '20';
                roundsChart.update();
            }
        }

        // Initialize dashboard
        document.addEventListener('DOMContentLoaded', function() {
            initializeCharts();

            // Apply default theme first
            fetchAndApplyTheme();

            // Fetch initial stats
            fetchStats();

            // Refresh every 2 seconds
            setInterval(fetchStats, 2000);
        });
    </script>
</body>
</html>";
    }
}
