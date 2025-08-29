using Spectre.Console;
using Spectre.Console.Rendering;
using FragifyTracker.Models;

namespace FragifyTracker.UI;

public class CliInterface : IUserInterface
{
    private readonly Table _gameInfoTable;
    private readonly Table _playerStatsTable;
    private readonly Table _sessionStatsTable;
    private readonly Table _debugInfoTable;
    private readonly Panel _progressPanel;
    private GameStats? _lastStats;
    private bool _isFirstDisplay = true;
    public bool IsRunning { get; private set; } = true;

    public CliInterface()
    {
        // Create tables once
        _gameInfoTable = CreateGameInfoTable();
        _playerStatsTable = CreatePlayerStatsTable();
        _sessionStatsTable = CreateSessionStatsTable();
        _debugInfoTable = CreateDebugInfoTable();
        _progressPanel = CreateProgressPanel();
    }

    public void Initialize()
    {
        // CLI interface is ready immediately
        IsRunning = true;
    }

    public void Shutdown()
    {
        IsRunning = false;
    }

    public void UpdateDisplay(GameStats? stats)
    {
        if (stats == null) return;

        // Only update if stats have actually changed
        if (_lastStats != null && !HasStatsChanged(_lastStats, stats))
        {
            return;
        }

        _lastStats = stats.Clone();

        if (_isFirstDisplay)
        {
            // First time: display everything
            DisplayFullLayout(stats);
            _isFirstDisplay = false;
        }
        else
        {
            // Subsequent updates: update tables and refresh display
            UpdateTables(stats);
            RefreshDisplay();
        }
    }

    public void HandleInput(ConsoleKeyInfo? key = null)
    {
        // CLI interface doesn't need external input handling
        // Input is handled in the main loop
    }

    private void RefreshDisplay()
    {
        // Force a refresh by redrawing the current layout
        // This ensures table updates are visible
        var currentLayout = new Grid()
            .AddColumn(new GridColumn().NoWrap().PadRight(2))
            .AddColumn(new GridColumn().NoWrap().PadRight(2));

        currentLayout.AddRow(_gameInfoTable, _playerStatsTable);

        // Clear and redraw to show updates
        AnsiConsole.Clear();
        DisplayHeader();
        DisplayDebugInfo(_lastStats!);
        AnsiConsole.WriteLine();
        AnsiConsole.Write(currentLayout);
        AnsiConsole.WriteLine();
        DisplayProgressBars(_lastStats!);
        AnsiConsole.WriteLine();

        var bottomGrid = new Grid()
            .AddColumn(new GridColumn().NoWrap().PadRight(2))
            .AddColumn(new GridColumn().NoWrap().PadRight(2));
        bottomGrid.AddRow(_sessionStatsTable, _debugInfoTable);
        AnsiConsole.Write(bottomGrid);
    }

    private bool HasStatsChanged(GameStats oldStats, GameStats newStats)
    {
        return oldStats.MapName != newStats.MapName ||
               oldStats.GameMode != newStats.GameMode ||
               oldStats.RoundPhase != newStats.RoundPhase ||
               oldStats.PlayerHealth != newStats.PlayerHealth ||
               oldStats.PlayerArmor != newStats.PlayerArmor ||
               oldStats.PlayerMoney != newStats.PlayerMoney ||
               oldStats.PlayerKills != newStats.PlayerKills ||
               oldStats.PlayerDeaths != newStats.PlayerDeaths ||
               oldStats.PlayerAssists != newStats.PlayerAssists ||
               oldStats.PlayerMVPs != newStats.PlayerMVPs ||
               oldStats.PlayerScore != newStats.PlayerScore ||
               oldStats.ScoreT != newStats.ScoreT ||
               oldStats.ScoreCT != newStats.ScoreCT ||
               oldStats.BombState != newStats.BombState ||
               oldStats.RoundNumber != newStats.RoundNumber ||
               oldStats.RoundTime != newStats.RoundTime ||
               oldStats.SessionDuration != newStats.SessionDuration ||
               oldStats.TotalRounds != newStats.TotalRounds ||
               oldStats.RoundsWon != newStats.RoundsWon ||
               oldStats.RoundsLost != newStats.RoundsLost ||
               oldStats.WinRate != newStats.WinRate ||
               oldStats.LastFlashDuration != newStats.LastFlashDuration ||
               oldStats.MessagesReceived != newStats.MessagesReceived ||
               oldStats.LastMessageTime != newStats.LastMessageTime ||
               oldStats.IsConnected != newStats.IsConnected ||
               oldStats.ConnectionStatus != newStats.ConnectionStatus;
    }

    private void DisplayHeader()
    {
        AnsiConsole.Write(
            new Rule("[bold blue]Fragify - CS:GO Live Tracker (CLI Mode)[/]")
                .RuleStyle("blue")
                .Centered());
        AnsiConsole.WriteLine();
    }

    private void DisplayDebugInfo(GameStats stats)
    {
        var statusColor = stats.IsConnected ? "green" : "red";
        var statusIcon = stats.IsConnected ? "✓" : "✗";

        var panel = new Panel(
            new Grid()
                .AddColumn(new GridColumn().NoWrap().PadRight(1))
                .AddColumn(new GridColumn().NoWrap().PadRight(1))
                .AddColumn(new GridColumn().NoWrap().PadRight(1))
                .AddColumn(new GridColumn().NoWrap().PadRight(1))
                .AddRow(
                    new Markup($"[bold {statusColor}]{statusIcon} Connection:[/]"),
                    new Markup($"[{statusColor}]{stats.ConnectionStatus}[/]"),
                    new Markup($"[bold blue]Messages:[/]"),
                    new Markup($"[blue]{stats.MessagesReceived}[/]")
                )
                .AddRow(
                    new Markup("[bold yellow]Last Message:[/]"),
                    new Markup($"[yellow]{stats.GetLastMessageTimeDisplay()}[/]"),
                    new Markup("[bold cyan]Session:[/]"),
                    new Markup($"[cyan]{stats.GetSessionDurationDisplay()}[/]")
                )
        )
        {
            Header = new PanelHeader("[bold]🔍 Debug Information[/]"),
            Border = BoxBorder.Rounded
        };

        AnsiConsole.Write(panel);
    }

    private void DisplayProgressBars(GameStats stats)
    {
        var panel = new Panel(
            new Grid()
                .AddColumn(new GridColumn().NoWrap().PadRight(1))
                .AddColumn(new GridColumn().NoWrap().PadRight(1))
                .AddColumn(new GridColumn().NoWrap().PadRight(1))
                .AddColumn(new GridColumn().NoWrap().PadRight(1))
                .AddColumn(new GridColumn().NoWrap().PadRight(1))
                .AddColumn(new GridColumn().NoWrap().PadRight(1))
                .AddRow(
                    new Text($"Health: {stats.PlayerHealth}"),
                    CreateProgressBar(stats.PlayerHealth, 100),
                    new Text($"Armor: {stats.PlayerArmor}"),
                    CreateProgressBar(stats.PlayerArmor, 100),
                    new Text($"Round Time: {stats.GetRoundTimeDisplay()}"),
                    CreateProgressBar(Math.Min(stats.RoundTime, 120), 120)
                )
        )
        {
            Header = new PanelHeader("[bold]Player Status[/]"),
            Border = BoxBorder.Rounded
        };

        AnsiConsole.Write(panel);
    }

    private IRenderable CreateProgressBar(int value, int maxValue)
    {
        var percentage = Math.Max(0, Math.Min(100, (int)((double)value / maxValue * 100)));
        var filled = (int)(percentage / 10);
        var empty = 10 - filled;

        var bar = new string('█', filled) + new string('░', empty);
        return new Markup($"[blue]{bar}[/] {percentage}%");
    }

    private Table CreateGameInfoTable()
    {
        var table = new Table()
            .Title("[bold blue]Game Information[/]")
            .Border(TableBorder.Rounded)
            .AddColumn("Property")
            .AddColumn("Value");

        table.AddRow("Map", "");
        table.AddRow("Mode", "");
        table.AddRow("Round", "");
        table.AddRow("Phase", "");
        table.AddRow("Score", "");

        return table;
    }

    private Table CreatePlayerStatsTable()
    {
        var table = new Table()
            .Title("[bold green]Player Statistics[/]")
            .Border(TableBorder.Rounded)
            .AddColumn("Stat")
            .AddColumn("Value");

        table.AddRow("Kills", "");
        table.AddRow("Deaths", "");
        table.AddRow("Assists", "");
        table.AddRow("MVPs", "");
        table.AddRow("Score", "");
        table.AddRow("Money", "");
        table.AddRow("Weapon", "");
        table.AddRow("Team", "");

        return table;
    }

    private Table CreateSessionStatsTable()
    {
        var table = new Table()
            .Title("[bold yellow]Session Statistics[/]")
            .Border(TableBorder.Rounded)
            .AddColumn("Stat")
            .AddColumn("Value");

        table.AddRow("Duration", "");
        table.AddRow("Total Rounds", "");
        table.AddRow("Wins", "");
        table.AddRow("Losses", "");
        table.AddRow("Win Rate", "");

        return table;
    }

    private Table CreateDebugInfoTable()
    {
        var table = new Table()
            .Title("[bold magenta]Debug Details[/]")
            .Border(TableBorder.Rounded)
            .AddColumn("Property")
            .AddColumn("Value");

        table.AddRow("Last Message", "");
        table.AddRow("Connection", "");
        table.AddRow("Message Count", "");
        table.AddRow("Last Update", "");

        return table;
    }

    private Panel CreateProgressPanel()
    {
        return new Panel("")
            .Header("[bold cyan]Progress Bars[/]")
            .Border(BoxBorder.Rounded);
    }

    private void UpdateTables(GameStats stats)
    {
        // Update Game Info Table
        _gameInfoTable.UpdateCell(0, 1, stats.MapName ?? "Unknown");
        _gameInfoTable.UpdateCell(1, 1, stats.GameMode ?? "Unknown");
        _gameInfoTable.UpdateCell(2, 1, stats.RoundNumber.ToString());
        _gameInfoTable.UpdateCell(3, 1, GetPhaseColor(stats.RoundPhase).ToString());
        _gameInfoTable.UpdateCell(4, 1, $"[bold]{stats.GetScoreDisplay()}[/]");

        // Update Player Stats Table
        _playerStatsTable.UpdateCell(0, 1, $"[green]{stats.PlayerKills}[/]");
        _playerStatsTable.UpdateCell(1, 1, $"[red]{stats.PlayerDeaths}[/]");
        _playerStatsTable.UpdateCell(2, 1, $"[blue]{stats.PlayerAssists}[/]");
        _playerStatsTable.UpdateCell(3, 1, $"[yellow]{stats.PlayerMVPs}[/]");
        _playerStatsTable.UpdateCell(4, 1, $"[bold]{stats.PlayerScore}[/]");
        _playerStatsTable.UpdateCell(5, 1, $"${stats.PlayerMoney:N0}");
        _playerStatsTable.UpdateCell(6, 1, stats.ActiveWeapon ?? "Unknown");
        _playerStatsTable.UpdateCell(7, 1, GetTeamColor(stats.PlayerTeam).ToString());

        // Update Session Stats Table
        _sessionStatsTable.UpdateCell(0, 1, stats.GetSessionDurationDisplay());
        _sessionStatsTable.UpdateCell(1, 1, stats.TotalRounds.ToString());
        _sessionStatsTable.UpdateCell(2, 1, $"[green]{stats.RoundsWon}[/]");
        _sessionStatsTable.UpdateCell(3, 1, $"[red]{stats.RoundsLost}[/]");
        _sessionStatsTable.UpdateCell(4, 1, $"[bold]{stats.WinRate:F1}%[/]");

        // Update Debug Info Table
        _debugInfoTable.UpdateCell(0, 1, stats.LastMessageContent.Length > 50 ? stats.LastMessageContent.Substring(0, 50) + "..." : stats.LastMessageContent);
        _debugInfoTable.UpdateCell(1, 1, stats.IsConnected ? "[green]Connected[/]" : "[red]Disconnected[/]");
        _debugInfoTable.UpdateCell(2, 1, stats.MessagesReceived.ToString());
        _debugInfoTable.UpdateCell(3, 1, stats.LastMessageTime?.ToString("HH:mm:ss") ?? "Never");
    }

    private void DisplayFullLayout(GameStats stats)
    {
        AnsiConsole.Clear();
        DisplayHeader();
        DisplayDebugInfo(stats);
        AnsiConsole.WriteLine();

        var grid = new Grid()
            .AddColumn(new GridColumn().NoWrap().PadRight(2))
            .AddColumn(new GridColumn().NoWrap().PadRight(2));

        grid.AddRow(_gameInfoTable, _playerStatsTable);
        AnsiConsole.Write(grid);
        AnsiConsole.WriteLine();

        DisplayProgressBars(stats);
        AnsiConsole.WriteLine();

        var bottomGrid = new Grid()
            .AddColumn(new GridColumn().NoWrap().PadRight(2))
            .AddColumn(new GridColumn().NoWrap().PadRight(2));
        bottomGrid.AddRow(_sessionStatsTable, _debugInfoTable);
        AnsiConsole.Write(bottomGrid);

        // Update tables with current data
        UpdateTables(stats);
    }

    private IRenderable GetPhaseColor(string phase)
    {
        if (string.IsNullOrEmpty(phase)) return new Markup("[dim]Unknown[/]");

        return phase.ToLower() switch
        {
            "live" => new Markup("[bold green]Live[/]"),
            "freezetime" => new Markup("[bold yellow]Freeze Time[/]"),
            "over" => new Markup("[bold red]Round Over[/]"),
            "warmup" => new Markup("[bold blue]Warmup[/]"),
            _ => new Markup($"[dim]{phase}[/]")
        };
    }

    private IRenderable GetTeamColor(string team)
    {
        if (string.IsNullOrEmpty(team)) return new Markup("[dim]Unknown[/]");

        return team.ToLower() switch
        {
            "t" => new Markup("[bold red]Terrorist[/]"),
            "ct" => new Markup("[bold blue]Counter-Terrorist[/]"),
            _ => new Markup($"[dim]{team}[/]")
        };
    }
}
