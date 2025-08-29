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

    public WebInterface(int port = 5000)
    {
        _port = port;
        _mapThemeService = new WebMapThemeService();
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
        _app.MapGet("/api/theme", () =>
        {
            if (_lastStats?.MapName == null)
                return Results.Json(_mapThemeService.GetDefaultTheme());

            var theme = _mapThemeService.GetMapTheme(_lastStats.MapName);
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
    </style>
</head>
<body>
    <div class=""header"">
        <h1>🎯 Fragify</h1>
        <p>Counter-Strike: Global Offensive Web Dashboard</p>
        <div class=""theme-info"">
            <span class=""theme-name"" id=""theme-name"">Default Theme</span>
            <span class=""theme-description"" id=""theme-description"">CS:GO Dashboard</span>
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

    <script>
        let roundsChart;

        function updateDashboard(stats) {
            // Update game information
            document.getElementById('map-name').textContent = stats.mapName || 'Unknown';
            document.getElementById('game-mode').textContent = stats.gameMode || 'Unknown';
            document.getElementById('round-number').textContent = stats.roundNumber || '0';
            document.getElementById('round-phase').textContent = stats.roundPhase || 'Unknown';
            document.getElementById('score').textContent = `${stats.scoreT || 0} - ${stats.scoreCT || 0}`;
            document.getElementById('round-time').textContent = formatTime(stats.roundTime || 0);

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
