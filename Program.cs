using CSGSI;
using FragifyTracker.Services;
using FragifyTracker.UI;
using Spectre.Console;

namespace FragifyTracker;

class Program
{
    private static GameStateListener? _listener;
    private static GameTrackerService? _trackerService;
    private static TestDataGenerator? _testDataGenerator;
    private static bool _testMode = false;
    private static bool _isRunning = true;
    private static IUserInterface? _userInterface;

    static async Task Main(string[] args)
    {
        try
        {
            var (uiType, webPort, testMode) = ParseArgs(args);

            if (uiType == "help")
            {
                UIFactory.ShowUsage();
                return;
            }

            _testMode = testMode;

            // Initialize the appropriate UI interface
            _userInterface = UIFactory.CreateInterface(uiType, webPort);
            _userInterface.Initialize();

            AnsiConsole.MarkupLine($"[bold green]🎯 Fragify - CS:GO Live Tracker[/]");
            AnsiConsole.MarkupLine($"[bold blue]UI Mode: {uiType.ToUpper()}[/]");

            if (uiType == "web")
            {
                AnsiConsole.MarkupLine($"[bold yellow]🌐 Web Dashboard: http://localhost:{webPort}[/]");
                AnsiConsole.MarkupLine("[dim]Press Ctrl+C to stop the application[/]");
            }

            AnsiConsole.WriteLine();

            // Initialize game tracker
            InitializeGameTracker();

            // Set up event handlers
            if (_listener != null)
            {
                _listener.NewGameState += OnNewGameState;
            }

            // Start listening
            if (_listener != null)
            {
                _listener.Start();
                AnsiConsole.MarkupLine("[bold green]✅ Game State Listener started![/]");
            }

            if (_testMode)
            {
                AnsiConsole.MarkupLine("[bold yellow]🧪 Test Mode Enabled[/]");
                AnsiConsole.MarkupLine("[dim]Use keys 1-6, r for test events[/]");
            }

            // Run the main loop
            await RunMainLoop();
        }
        catch (Exception ex)
        {
            AnsiConsole.WriteException(ex);
        }
        finally
        {
            _userInterface?.Shutdown();
            _listener?.Stop();
        }
    }

    private static (string uiType, int webPort, bool testMode) ParseArgs(string[] args)
    {
        var uiType = "cli"; // default
        var webPort = 5000; // default
        var testMode = false;

        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i].ToLower())
            {
                case "--ui" when i + 1 < args.Length:
                    uiType = args[++i];
                    break;
                case "--web-port" when i + 1 < args.Length:
                    if (int.TryParse(args[++i], out var port))
                        webPort = port;
                    break;
                case "--test" or "-t":
                    testMode = true;
                    break;
                case "--help" or "-h":
                    return ("help", 5000, false);
            }
        }

        return (uiType, webPort, testMode);
    }

    private static void InitializeGameTracker()
    {
        try
        {
            _trackerService = new GameTrackerService();

            if (_testMode)
            {
                _testDataGenerator = new TestDataGenerator(_trackerService);
            }

            _listener = new GameStateListener(3000);
            AnsiConsole.MarkupLine("[bold green]✅ Game Tracker initialized![/]");
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[bold red]❌ Failed to initialize game tracker: {ex.Message}[/]");
            throw;
        }
    }

    private static void OnNewGameState(GameState gameState)
    {
        try
        {
            _trackerService?.UpdateGameState(gameState);

            if (!_testMode)
            {
                AnsiConsole.MarkupLine("[bold green]🎯 Received message from CS:GO![/]");
            }
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[bold red]❌ Error processing game state: {ex.Message}[/]");
        }
    }

    private static async Task RunMainLoop()
    {
        while (_isRunning && _userInterface?.IsRunning == true)
        {
            try
            {
                if (_testMode)
                {
                    // Handle test mode input
                    HandleTestInput();

                    // Auto-simulation
                    _testDataGenerator?.Update();
                }
                else
                {
                    // Check if we're still receiving messages
                    CheckConnectionStatus();
                }

                _userInterface.UpdateDisplay(_trackerService?.GetCurrentStats());
                await Task.Delay(100); // Update every 100ms
            }
            catch (Exception ex)
            {
                AnsiConsole.WriteException(ex);
                await Task.Delay(1000);
            }
        }
    }

    private static void HandleTestInput()
    {
        if (Console.KeyAvailable)
        {
            var key = Console.ReadKey(true);
            switch (key.KeyChar)
            {
                case '1':
                    _testDataGenerator?.SimulateRoundStart();
                    AnsiConsole.MarkupLine("[bold green]🎯 Test: Round Started![/]");
                    break;
                case '2':
                    _testDataGenerator?.SimulateBombPlanted();
                    AnsiConsole.MarkupLine("[bold yellow]💣 Test: Bomb Planted![/]");
                    break;
                case '3':
                    _testDataGenerator?.SimulateBombDefused();
                    AnsiConsole.MarkupLine("[bold green]✅ Test: Bomb Defused![/]");
                    break;
                case '4':
                    _testDataGenerator?.SimulateRoundEnd();
                    AnsiConsole.MarkupLine("[bold red]🏁 Test: Round Ended![/]");
                    break;
                case '5':
                    _testDataGenerator?.SimulatePlayerFlash();
                    AnsiConsole.MarkupLine("[bold yellow]😵 Test: Player Flashed![/]");
                    break;
                case '6':
                    _testDataGenerator?.ToggleAutoSimulation();
                    AnsiConsole.MarkupLine($"[bold cyan]🔄 Test: Auto-simulation {(_testDataGenerator?.IsAutoSimulationEnabled ?? false ? "enabled" : "disabled")}[/]");
                    break;
                case 'r':
                    _trackerService?.ResetSession();
                    AnsiConsole.MarkupLine("[bold magenta]🔄 Test: Session Reset![/]");
                    break;
            }
        }
    }

    private static void CheckConnectionStatus()
    {
        var stats = _trackerService?.GetCurrentStats();
        if (stats != null)
        {
            var timeSinceLastMessage = DateTime.Now - (stats.LastMessageTime ?? DateTime.MinValue);
            if (timeSinceLastMessage.TotalMinutes > 1)
            {
                _trackerService?.OnConnectionLost();
            }
        }
    }
}
