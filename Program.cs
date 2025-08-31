using CounterStrike2GSI;
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

            AnsiConsole.MarkupLine($"[bold green]🎯 Fragify - CS2 Live Tracker[/]");
            AnsiConsole.MarkupLine($"[bold blue]UI Mode: {uiType.ToUpper()}[/]");

            if (uiType == "web")
            {
                AnsiConsole.MarkupLine($"[bold yellow]🌐 Web Dashboard: http://localhost:{webPort}[/]");
                AnsiConsole.MarkupLine("[dim]Press Ctrl+C to stop the application[/]");
            }

            AnsiConsole.WriteLine();

            // Initialize game tracker
            InitializeGameTracker();

            // Set up event handlers for the new library
            if (_listener != null)
            {
                // Main game state event - this is the only reliable event for players
                _listener.NewGameState += OnNewGameState;
            }

            // Start listening
            if (_listener != null)
            {
                if (_listener.Start())
                {
                    AnsiConsole.MarkupLine("[bold green]✅ CS2 Game State Listener started on port 3000![/]");
                    AnsiConsole.MarkupLine("[bold yellow]💡 Make sure your CS2 GSI config points to http://localhost:3000[/]");
                }
                else
                {
                    AnsiConsole.MarkupLine("[bold red]❌ Failed to start Game State Listener![/]");
                    AnsiConsole.MarkupLine("[dim]Check if port 3000 is available and you have sufficient permissions[/]");
                    return;
                }
            }

            if (_testMode)
            {
                AnsiConsole.MarkupLine("[bold yellow]🧪 Test Mode Enabled[/]");
                AnsiConsole.MarkupLine("[dim]Test Commands:[/]");
                AnsiConsole.MarkupLine("[dim]  1 - Start Round    2 - End Round    3 - Flash Player[/]");
                AnsiConsole.MarkupLine("[dim]  4 - Toggle Auto    5 - Full Game    6 - Next Round[/]");
                AnsiConsole.MarkupLine("[dim]  7 - Game Status   0 - Reset Game   r - Reset Session[/]");

                // Generate initial test data to show something in the UI
                AnsiConsole.MarkupLine("[dim]Generating initial test data...[/]");
                _testDataGenerator?.GenerateInitialData(); // This will create initial stats
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

            // Create the new GameStateListener
            _listener = new GameStateListener(3000);
            AnsiConsole.MarkupLine("[bold green]✅ Game Tracker initialized![/]");
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[bold red]❌ Failed to initialize game tracker: {ex.Message}[/]");
            throw;
        }
    }

    // Main game state event handler
    private static void OnNewGameState(GameState gameState)
    {
        try
        {
            _trackerService?.UpdateGameState(gameState);

            if (!_testMode)
            {
                AnsiConsole.MarkupLine($"[bold green]🎯 Received CS2 game state - Map: {gameState.Map.Name}, Phase: {gameState.Round.Phase}[/]");
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
                    _testDataGenerator?.SimulateRoundEnd();
                    AnsiConsole.MarkupLine("[bold red]🏁 Test: Round Ended![/]");
                    break;
                case '3':
                    _testDataGenerator?.SimulatePlayerFlash();
                    AnsiConsole.MarkupLine("[bold yellow]😵 Test: Player Flashed![/]");
                    break;
                case '4':
                    _testDataGenerator?.ToggleAutoSimulation();
                    AnsiConsole.MarkupLine($"[bold cyan]🔄 Test: Auto-simulation {(_testDataGenerator?.IsAutoSimulationEnabled ?? false ? "enabled" : "disabled")}[/]");
                    break;
                case '5':
                    _testDataGenerator?.SimulateFullGame();
                    AnsiConsole.MarkupLine("[bold magenta]🎮 Test: Full Game Simulation Started![/]");
                    break;
                case '6':
                    _testDataGenerator?.SimulateNextRound();
                    AnsiConsole.MarkupLine("[bold blue]⏭️ Test: Next Round![/]");
                    break;
                case '7':
                    _testDataGenerator?.ShowGameStatus();
                    break;
                case '0':
                    _testDataGenerator?.ResetGame();
                    AnsiConsole.MarkupLine("[bold red]🔄 Test: Game Reset![/]");
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
