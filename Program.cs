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
        var port = ParsePortFromArgs(args);

        try
        {
            await InitializeGameTracker(port);
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

    private static int ParsePortFromArgs(string[] args)
    {
        // Default port
        var port = 3000;

        for (int i = 0; i < args.Length - 1; i++)
        {
            if (args[i] == "--port" || args[i] == "-p")
            {
                if (int.TryParse(args[i + 1], out int parsedPort))
                {
                    port = parsedPort;
                }
            }
        }

        return port;
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
                // Check if we're still receiving messages
                CheckConnectionStatus();

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
