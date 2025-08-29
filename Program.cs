using CSGSI;
using CSGSI.Nodes;
using FragifyTracker.Services;
using FragifyTracker.UI;
using Spectre.Console;

namespace FragifyTracker;

class Program
{
    private static GameStateListener? _gameStateListener;
    private static GameTrackerService? _trackerService;
    private static bool _isRunning = true;
    private static DateTime _lastMessageTime = DateTime.MinValue;
    private static bool _testMode = false;
    private static TestDataGenerator? _testDataGenerator;

    static async Task Main(string[] args)
    {
        Console.Title = "Fragify - CS:GO Game Tracker";

        AnsiConsole.Write(
            new FigletText("Fragify")
                .Centered()
                .Color(Color.Blue));

        AnsiConsole.MarkupLine("[bold blue]Counter-Strike: Global Offensive Game State Tracker[/]");
        AnsiConsole.MarkupLine("[dim]Press Ctrl+C to exit[/]\n");

        // Parse command line arguments
        var (port, testMode) = ParseArgs(args);
        _testMode = testMode;

        try
        {
            if (_testMode)
            {
                await InitializeTestMode();
            }
            else
            {
                await InitializeGameTracker(port);
            }

            await RunMainLoop();
        }
        catch (Exception ex)
        {
            AnsiConsole.WriteException(ex);
        }
        finally
        {
            Cleanup();
        }
    }

    private static (int port, bool testMode) ParseArgs(string[] args)
    {
        // Default values
        var port = 3000;
        var testMode = false;

        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "--port" || args[i] == "-p")
            {
                if (i + 1 < args.Length && int.TryParse(args[i + 1], out int parsedPort))
                {
                    port = parsedPort;
                }
            }
            else if (args[i] == "--test" || args[i] == "-t")
            {
                testMode = true;
            }
        }

        return (port, testMode);
    }

    private static async Task InitializeTestMode()
    {
        AnsiConsole.MarkupLine("[bold yellow]🧪 TEST MODE ENABLED[/]\n");
        AnsiConsole.MarkupLine("[yellow]This mode simulates CS:GO game state events for testing.[/]\n");

        _trackerService = new GameTrackerService();
        _trackerService.OnConnectionEstablished();

        _testDataGenerator = new TestDataGenerator(_trackerService);

        AnsiConsole.MarkupLine("[green]✓[/] Test mode initialized");
        AnsiConsole.MarkupLine("[cyan]Simulating game events every 3 seconds...[/]\n");

        AnsiConsole.MarkupLine("[bold yellow]Test Controls:[/]");
        AnsiConsole.MarkupLine("[yellow]Press '1' - Simulate round start[/]");
        AnsiConsole.MarkupLine("[yellow]Press '2' - Simulate bomb planted[/]");
        AnsiConsole.MarkupLine("[yellow]Press '3' - Simulate bomb defused[/]");
        AnsiConsole.MarkupLine("[yellow]Press '4' - Simulate round end[/]");
        AnsiConsole.MarkupLine("[yellow]Press '5' - Simulate player flash[/]");
        AnsiConsole.MarkupLine("[yellow]Press '6' - Toggle auto-simulation[/]");
        AnsiConsole.MarkupLine("[yellow]Press 'r' - Reset session[/]\n");
    }

    private static async Task InitializeGameTracker(int port)
    {
        AnsiConsole.Status()
            .Start("Initializing Game State Listener...", ctx =>
            {
                ctx.Status("Creating listener...");
                _gameStateListener = new GameStateListener(port);

                ctx.Status("Setting up event handlers...");
                _gameStateListener.NewGameState += OnNewGameState;
                // Note: Disabling intricate events for now due to signature mismatches
                // _gameStateListener.EnableRaisingIntricateEvents = true;

                ctx.Status("Starting listener...");
                if (!_gameStateListener.Start())
                {
                    throw new InvalidOperationException($"Failed to start listener on port {port}. Make sure you have the necessary permissions and the port is available.");
                }

                return Task.CompletedTask;
            });

        _trackerService = new GameTrackerService();
        _trackerService.OnConnectionEstablished();

        AnsiConsole.MarkupLine($"[green]✓[/] Game State Listener started on port [bold]{port}[/]");
        AnsiConsole.MarkupLine("[yellow]Make sure you have the gamestate_integration config file set up in your CS:GO cfg folder![/]\n");
        AnsiConsole.MarkupLine("[cyan]Waiting for CS:GO to send game state data...[/]\n");

        // Additional debugging info
        AnsiConsole.MarkupLine("[bold yellow]Debugging Tips:[/]");
        AnsiConsole.MarkupLine("[yellow]1. Make sure CS:GO is running[/]");
        AnsiConsole.MarkupLine("[yellow]2. Execute: exec gamestate_integration_fragify in CS:GO console[/]");
        AnsiConsole.MarkupLine("[yellow]3. Join a game (workshop maps, competitive, etc.)[/]");
        AnsiConsole.MarkupLine("[yellow]4. Check CS:GO console for any error messages[/]\n");
    }

    private static async Task RunMainLoop()
    {
        var displayManager = new DisplayManager();

        while (_isRunning)
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

                displayManager.UpdateDisplay(_trackerService?.GetCurrentStats());
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
        if (_trackerService != null)
        {
            var stats = _trackerService.GetCurrentStats();
            var timeSinceLastMessage = DateTime.Now - _lastMessageTime;

            // If no messages for more than 10 seconds, mark as disconnected
            if (stats.MessagesReceived > 0 && timeSinceLastMessage.TotalSeconds > 10)
            {
                _trackerService.OnConnectionLost();
            }
        }
    }

    private static void OnNewGameState(GameState gameState)
    {
        _lastMessageTime = DateTime.Now;

        // Log the incoming message for debugging
        AnsiConsole.MarkupLine($"[bold green]🎯 Received message from CS:GO![/]");
        AnsiConsole.MarkupLine($"[green]Map: {gameState.Map.Name}[/]");
        AnsiConsole.MarkupLine($"[green]Phase: {gameState.Round.Phase}[/]");
        AnsiConsole.MarkupLine($"[green]Player Health: {gameState.Player?.State.Health ?? 0}[/]\n");

        _trackerService?.UpdateGameState(gameState);

        // Check for round phase changes manually since we can't use the intricate events
        CheckForRoundPhaseChanges(gameState);
    }

    private static void CheckForRoundPhaseChanges(GameState gameState)
    {
        if (gameState.Round.Phase == RoundPhase.Live)
        {
            _trackerService?.OnRoundBegin();
            AnsiConsole.MarkupLine("[bold green]Round Started![/]");
        }
        else if (gameState.Round.Phase == RoundPhase.Over)
        {
            _trackerService?.OnRoundEnd();
            AnsiConsole.MarkupLine("[bold red]Round Ended![/]");
        }

        // Check bomb state changes
        if (gameState.Bomb.State == BombState.Planted)
        {
            _trackerService?.OnBombPlanted();
            AnsiConsole.MarkupLine("[bold yellow]💣 Bomb Planted![/]");
        }
        else if (gameState.Bomb.State == BombState.Defused)
        {
            _trackerService?.OnBombDefused();
            AnsiConsole.MarkupLine("[bold green]✅ Bomb Defused![/]");
        }
        else if (gameState.Bomb.State == BombState.Exploded)
        {
            _trackerService?.OnBombExploded();
            AnsiConsole.MarkupLine("[bold red]💥 Bomb Exploded![/]");
        }
    }

    private static void Cleanup()
    {
        _gameStateListener?.Stop();
        AnsiConsole.MarkupLine("\n[yellow]Shutting down...[/]");
    }
}
