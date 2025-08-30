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

        .dashboard {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(400px, 1fr));
            gap: 1.5rem;
            padding: 1.5rem;
            max-width: 1400px;
            margin: 0 auto;
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

        .map-selector {
            margin-bottom: 1rem;
        }

        .map-selector select {
            padding: 0.5rem 1rem;
            font-size: 14px;
            border-radius: 5px;
            border: 1px solid var(--card-border, rgba(255, 255, 255, 0.2));
            background: var(--card-background, rgba(255, 255, 255, 0.1));
            color: var(--text-color, white);
            cursor: pointer;
        }

        .interactive-map {
            width: 100%;
            height: 400px;
            border: 2px solid var(--card-border, rgba(255, 255, 255, 0.2));
            border-radius: 8px;
            background: rgba(255, 255, 255, 0.05);
            display: flex;
            align-items: center;
            justify-content: center;
            margin-top: 1rem;
        }

        .map-placeholder {
            text-align: center;
            color: var(--text-color, #cbd5e1);
        }

        .refresh-info {
            text-align: center;
            color: var(--text-color, #cbd5e1);
            margin-top: 1rem;
            font-size: 0.9rem;
        }
    </style>
</head>
<body>
    <div class=""header"">
        <h1>🎯 Fragify</h1>
        <p>Counter-Strike: Global Offensive Web Dashboard</p>
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
            </div>

            <!-- Map Selector and Display -->
            <div class=""map-selector"">
                <h3>🗺️ Interactive Map</h3>
                <select id=""map-select"" onchange=""loadSelectedMap()"">
                    <option value="""">Select a map manually...</option>
                    <option value=""de_dust2"">de_dust2 - Dust II</option>
                    <option value=""de_mirage"">de_mirage - Mirage</option>
                    <option value=""de_inferno"">de_inferno - Inferno</option>
                    <option value=""de_cache"">de_cache - Cache</option>
                    <option value=""de_overpass"">de_overpass - Overpass</option>
                    <option value=""de_nuke"">de_nuke - Nuke</option>
                    <option value=""de_ancient"">de_ancient - Ancient</option>
                    <option value=""de_vertigo"">de_vertigo - Vertigo</option>
                    <option value=""de_cobblestone"">de_cobblestone - Cobblestone</option>
                    <option value=""de_train"">de_train - Train</option>
                    <option value=""de_anubis"">de_anubis - Anubis</option>
                </select>
                <div style=""margin-top: 0.5rem; font-size: 0.8rem; opacity: 0.7;"">
                    Current game map: <span id=""current-game-map"">Unknown</span>
                </div>
            </div>

            <div class=""interactive-map"" id=""map-display"">
                <div class=""map-placeholder"">
                    <p>Select a map to view</p>
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
        </div>

        <!-- Debug Information Card -->
        <div class=""card"">
            <h2>🔍 Debug Information</h2>
            <div class=""stats-grid"">
                <div class=""stat-item"">
                    <span class=""stat-label"">Status:</span>
                    <span class=""stat-value"" id=""connection-status"">Disconnected</span>
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
        </div>
    </div>

    <div class=""refresh-info"">
        Auto-refreshing every 2 seconds | Last update: <span id=""last-refresh"">Never</span>
    </div>

    <script>
        let comprehensiveMapData = {};

        async function loadComprehensiveMapData() {
            try {
                const response = await fetch('/api/maps');
                if (response.ok) {
                    comprehensiveMapData = await response.json();
                    console.log('Loaded map data:', Object.keys(comprehensiveMapData).length, 'maps');
                }
            } catch (error) {
                console.error('Error loading map data:', error);
            }
        }

        function loadSelectedMap() {
            const mapSelect = document.getElementById('map-select');
            const selectedMap = mapSelect.value;
            const mapDisplay = document.getElementById('map-display');

            if (!selectedMap) {
                mapDisplay.innerHTML = '<div class=""map-placeholder""><p>Select a map to view</p></div>';
                return;
            }

            const mapData = comprehensiveMapData[selectedMap];
            if (mapData && mapData.imageUrl) {
                mapDisplay.innerHTML = '<img src=""' + mapData.imageUrl + '"" alt=""Map minimap"" style=""width: 100%; height: 100%; object-fit: contain;"">';
            } else {
                mapDisplay.innerHTML = '<div class=""map-placeholder""><p>Map not available: ' + selectedMap + '</p></div>';
            }
        }

        function updateMapSelector(gameMapName) {
            const mapSelect = document.getElementById('map-select');
            const currentGameMapSpan = document.getElementById('current-game-map');

            if (gameMapName && gameMapName !== 'Unknown') {
                currentGameMapSpan.textContent = gameMapName;
                if (mapSelect.value === '') {
                    mapSelect.value = gameMapName;
                    loadSelectedMap();
                }
            } else {
                currentGameMapSpan.textContent = 'Unknown';
            }
        }

        function updateDashboard(stats) {
            // Update game information
            document.getElementById('map-name').textContent = stats.mapName || 'Unknown';
            document.getElementById('game-mode').textContent = stats.gameMode || 'Unknown';
            document.getElementById('round-number').textContent = stats.roundNumber || '0';
            document.getElementById('round-phase').textContent = stats.roundPhase || 'Unknown';
            document.getElementById('score').textContent = `${stats.scoreT || 0} - ${stats.scoreCT || 0}`;

            // Update map selector
            updateMapSelector(stats.mapName);

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

            // Update session statistics
            document.getElementById('session-duration').textContent = stats.sessionDuration || '00:00:00';
            document.getElementById('total-rounds').textContent = stats.totalRounds || 0;
            document.getElementById('rounds-won').textContent = stats.roundsWon || 0;
            document.getElementById('rounds-lost').textContent = stats.roundsLost || 0;
            document.getElementById('win-rate').textContent = `${(stats.winRate || 0).toFixed(1)}%`;

            // Update debug information
            const isConnected = stats.isConnected || false;
            document.getElementById('connection-status').textContent = isConnected ? 'Connected' : 'Disconnected';
            document.getElementById('messages-received').textContent = stats.messagesReceived || 0;
            document.getElementById('last-message-time').textContent = stats.lastMessageTime || 'Never';
            document.getElementById('last-update').textContent = stats.lastMessageTime || 'Never';

            // Update last refresh time
            const now = new Date();
            document.getElementById('last-refresh').textContent = now.toLocaleTimeString();
        }

        async function fetchStats() {
            try {
                const response = await fetch('/api/stats');
                if (response.ok) {
                    const stats = await response.json();
                    updateDashboard(stats);
                }
            } catch (error) {
                console.error('Failed to fetch stats:', error);
            }
        }

        // Initialize dashboard
        document.addEventListener('DOMContentLoaded', function() {
            loadComprehensiveMapData();
            fetchStats();
            setInterval(fetchStats, 2000);
        });
    </script>
</body>
</html>";
    }
}

