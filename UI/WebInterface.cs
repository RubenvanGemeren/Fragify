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
    <style>
        * {
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }

        body {
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            background: linear-gradient(135deg, #1e3c72 0%, #2a5298 100%);
            color: white;
            min-height: 100vh;
            transition: all 0.3s ease;
        }

        .header {
            background: rgba(0, 0, 0, 0.3);
            padding: 1rem;
            text-align: center;
            backdrop-filter: blur(10px);
        }

        .header h1 {
            font-size: 2.5rem;
            color: #4ade80;
            text-shadow: 0 0 20px rgba(74, 222, 128, 0.5);
            transition: color 0.3s ease;
        }

        .header p {
            color: #cbd5e1;
            margin-top: 0.5rem;
            transition: color 0.3s ease;
        }

        .dashboard {
            display: grid;
            grid-template-columns: 1fr 1fr;
            gap: 1.5rem;
            padding: 1.5rem;
            max-width: 1400px;
            margin: 0 auto;
        }

        .left-panel {
            grid-column: 1;
        }

        .right-panel {
            grid-column: 2;
            display: flex;
            flex-direction: column;
            gap: 1.5rem;
        }

        .card {
            background: rgba(255, 255, 255, 0.1);
            border-radius: 15px;
            padding: 1.5rem;
            backdrop-filter: blur(10px);
            border: 1px solid rgba(255, 255, 255, 0.2);
            transition: all 0.3s ease;
        }

        .card h2 {
            color: #4ade80;
            margin-bottom: 1rem;
            font-size: 1.5rem;
            border-bottom: 2px solid #4ade80;
            padding-bottom: 0.5rem;
            transition: color 0.3s ease, border-color 0.3s ease;
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
            color: #cbd5e1;
            font-weight: 500;
            transition: color 0.3s ease;
        }

        .stat-value {
            color: #4ade80;
            font-weight: bold;
            transition: color 0.3s ease;
        }

        .map-selector {
            margin-bottom: 1rem;
        }

        .map-selector select {
            padding: 0.5rem 1rem;
            font-size: 14px;
            border-radius: 5px;
            border: 1px solid rgba(255, 255, 255, 0.2);
            background: rgba(255, 255, 255, 0.1);
            color: white;
            cursor: pointer;
        }

        .map-selector select option {
            background: #2d3748;
            color: white;
            padding: 0.5rem;
        }

        .interactive-map {
            width: 100%;
            height: 600px;
            border: 2px solid rgba(255, 255, 255, 0.2);
            border-radius: 8px;
            background: rgba(255, 255, 255, 0.05);
            display: flex;
            align-items: center;
            justify-content: center;
            margin-top: 1rem;
        }

        .map-placeholder {
            text-align: center;
            color: #cbd5e1;
        }

        .refresh-info {
            text-align: center;
            color: #cbd5e1;
            margin-top: 1rem;
            font-size: 0.9rem;
        }

        .test-buttons {
            margin-top: 1rem;
            display: flex;
            gap: 0.5rem;
            flex-wrap: wrap;
        }

        .test-btn {
            padding: 0.5rem 1rem;
            border: none;
            border-radius: 5px;
            color: white;
            cursor: pointer;
            font-size: 0.8rem;
            transition: all 0.3s ease;
        }

        .test-btn:hover {
            transform: translateY(-2px);
            box-shadow: 0 4px 8px rgba(0, 0, 0, 0.3);
        }
    </style>
</head>
<body>
    <div class=""header"">
        <h1 id=""header-title"">🎯 Fragify</h1>
        <p id=""header-subtitle"">Counter-Strike: Global Offensive Web Dashboard</p>
    </div>

    <div class=""dashboard"">
        <!-- Left Panel - Map Information -->
        <div class=""left-panel"">
            <div class=""card"">
                <h2>🗺️ Map Info</h2>
                <div class=""map-selector"">
                    <h3>Interactive Map</h3>
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
                    <div class=""test-buttons"">
                        <button class=""test-btn"" onclick=""testManualTheme()"" style=""background: #ef4444;"">Test Manual Theme</button>
                        <button class=""test-btn"" onclick=""testDust2Theme()"" style=""background: #f59e0b;"">Test Dust2 Theme</button>
                        <button class=""test-btn"" onclick=""testMirageTheme()"" style=""background: #10b981;"">Test Mirage Theme</button>
                        <button class=""test-btn"" onclick=""resetTheme()"" style=""background: #6b7280;"">Reset Theme</button>
                    </div>
                </div>
                <div class=""interactive-map"" id=""map-display"">
                    <div class=""map-placeholder"">
                        <p>Select a map to view</p>
                    </div>
                </div>
            </div>
        </div>

        <!-- Right Panel - All Other Information -->
        <div class=""right-panel"">
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
    </div>

    <div class=""refresh-info"">
        Auto-refreshing every 2 seconds | Last update: <span id=""last-refresh"">Never</span>
    </div>

    <script>
        let comprehensiveMapData = {};

        // SIMPLE THEME APPLICATION - Direct style changes
        function applyTheme(theme) {
            console.log('Applying theme:', theme);

            const body = document.body;
            const headerTitle = document.getElementById('header-title');
            const headerSubtitle = document.getElementById('header-subtitle');
            const cards = document.querySelectorAll('.card');
            const cardHeaders = document.querySelectorAll('.card h2');
            const statValues = document.querySelectorAll('.stat-value');
            const statLabels = document.querySelectorAll('.stat-label');

            // Apply background
            if (theme.backgroundGradient) {
                body.style.background = theme.backgroundGradient;
                console.log('Set background to:', theme.backgroundGradient);
            }

            // Apply header colors
            if (theme.primaryColor) {
                headerTitle.style.color = theme.primaryColor;
                console.log('Set header title color to:', theme.primaryColor);
            }

            if (theme.textColor) {
                headerSubtitle.style.color = theme.textColor;
                console.log('Set header subtitle color to:', theme.textColor);
            }

            // Apply card colors
            if (theme.cardBackground) {
                cards.forEach(card => {
                    card.style.background = theme.cardBackground;
                });
                console.log('Set card background to:', theme.cardBackground);
            }

            if (theme.cardBorder) {
                cards.forEach(card => {
                    card.style.borderColor = theme.cardBorder;
                });
                console.log('Set card border to:', theme.cardBorder);
            }

            // Apply text colors
            if (theme.primaryColor) {
                cardHeaders.forEach(header => {
                    header.style.color = theme.primaryColor;
                    header.style.borderBottomColor = theme.primaryColor;
                });
                statValues.forEach(value => {
                    value.style.color = theme.primaryColor;
                });
                console.log('Set primary colors to:', theme.primaryColor);
            }

            if (theme.textColor) {
                statLabels.forEach(label => {
                    label.style.color = theme.textColor;
                });
                console.log('Set text colors to:', theme.textColor);
            }

            console.log('Theme application complete');
        }

        // Fetch and apply theme from API
        async function updateTheme(mapName) {
            try {
                console.log('=== THEME UPDATE START ===');
                console.log('Fetching theme for map:', mapName);
                const response = await fetch(`/api/theme?mapName=${encodeURIComponent(mapName)}`);
                console.log('Theme response status:', response.status);

                if (response.ok) {
                    const theme = await response.json();
                    console.log('Theme received from API:', theme);
                    console.log('Theme object keys:', Object.keys(theme));
                    console.log('Theme properties:', {
                        PrimaryColor: theme.PrimaryColor,
                        BackgroundGradient: theme.BackgroundGradient,
                        CardBackground: theme.CardBackground,
                        TextColor: theme.TextColor
                    });

                    if (theme && typeof theme === 'object') {
                        console.log('Theme is valid object, applying...');
                        applyTheme(theme);
                    } else {
                        console.error('Theme is not a valid object:', typeof theme);
                    }
                } else {
                    console.error('Failed to fetch theme, status:', response.status);
                    const errorText = await response.text();
                    console.error('Error response:', errorText);
                }
                console.log('=== THEME UPDATE END ===');
            } catch (error) {
                console.error('Error fetching theme:', error);
            }
        }

        // Test themes
        function testManualTheme() {
            const theme = {
                BackgroundGradient: 'linear-gradient(135deg, #1A0F0F 0%, #8B0000 50%, #FF4500 100%)',
                PrimaryColor: '#FF4500',
                TextColor: '#FFE4E1',
                CardBackground: 'rgba(26, 15, 15, 0.2)',
                CardBorder: 'rgba(255, 69, 0, 0.3)'
            };
            applyTheme(theme);
        }

        function testDust2Theme() {
            const theme = {
                BackgroundGradient: 'linear-gradient(135deg, #8B4513 0%, #D4AF37 50%, #F4A460 100%)',
                PrimaryColor: '#d4af37',
                TextColor: '#F5DEB3',
                CardBackground: 'rgba(139, 69, 19, 0.2)',
                CardBorder: 'rgba(212, 175, 55, 0.3)'
            };
            applyTheme(theme);
        }

        function testMirageTheme() {
            const theme = {
                BackgroundGradient: 'linear-gradient(135deg, #2F1B14 0%, #8B4513 50%, #DAA520 100%)',
                PrimaryColor: '#8B4513',
                TextColor: '#F5DEB3',
                CardBackground: 'rgba(47, 27, 20, 0.2)',
                CardBorder: 'rgba(139, 69, 19, 0.3)'
            };
            applyTheme(theme);
        }

        function resetTheme() {
            const theme = {
                BackgroundGradient: 'linear-gradient(135deg, #1e3c72 0%, #2a5298 100%)',
                PrimaryColor: '#4ade80',
                TextColor: 'white',
                CardBackground: 'rgba(255, 255, 255, 0.1)',
                CardBorder: 'rgba(255, 255, 255, 0.2)'
            };
            applyTheme(theme);
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

        async function updateMapSelector(gameMapName) {
            const mapSelect = document.getElementById('map-select');
            const currentGameMapSpan = document.getElementById('current-game-map');

            if (gameMapName && gameMapName !== 'Unknown') {
                currentGameMapSpan.textContent = gameMapName;
                mapSelect.value = gameMapName;
                loadSelectedMap();

                // Automatically apply theme based on the new map
                await updateTheme(gameMapName);
            } else {
                currentGameMapSpan.textContent = 'Unknown';
            }
        }

        async function updateDashboard(stats) {
            // Update game information
            document.getElementById('map-name').textContent = stats.mapName || 'Unknown';
            document.getElementById('game-mode').textContent = stats.gameMode || 'Unknown';
            document.getElementById('round-number').textContent = stats.roundNumber || '0';
            document.getElementById('round-phase').textContent = stats.roundPhase || 'Unknown';
            document.getElementById('score').textContent = `${stats.scoreT || 0} - ${stats.scoreCT || 0}`;

            // Update map selector
            await updateMapSelector(stats.mapName);

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
                    await updateDashboard(stats);
                }
            } catch (error) {
                console.error('Failed to fetch stats:', error);
            }
        }

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

