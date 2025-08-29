using FragifyTracker.Models;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace FragifyTracker.UI;

public class DisplayManager
{
    private readonly Table _gameInfoTable;
    private readonly Table _playerStatsTable;
    private readonly Table _sessionStatsTable;
    private readonly Table _bombInfoTable;
    private readonly Table _debugInfoTable;
    private DateTime _lastUpdate = DateTime.MinValue;

    public DisplayManager()
    {
        _gameInfoTable = CreateGameInfoTable();
        _playerStatsTable = CreatePlayerStatsTable();
        _sessionStatsTable = CreateSessionStatsTable();
        _bombInfoTable = CreateBombInfoTable();
        _debugInfoTable = CreateDebugInfoTable();
    }

    public void UpdateDisplay(GameStats? stats)
    {
        if (stats == null) return;

        // Only update display every 500ms to avoid flickering
        if ((DateTime.Now - _lastUpdate).TotalMilliseconds < 500) return;
        _lastUpdate = DateTime.Now;

        AnsiConsole.Clear();

        // Display header
        DisplayHeader();

        // Display debug info prominently at the top
        DisplayDebugInfo(stats);
        AnsiConsole.WriteLine();

        // Display main content in a grid layout
        var grid = new Grid()
            .AddColumn(new GridColumn().NoWrap().PadRight(2))
            .AddColumn(new GridColumn().NoWrap().PadRight(2));

        grid.AddRow(_gameInfoTable, _playerStatsTable);
        AnsiConsole.Write(grid);
        AnsiConsole.WriteLine();

        // Display progress bars
        DisplayProgressBars(stats);
        AnsiConsole.WriteLine();

        // Display session and bomb info
        var bottomGrid = new Grid()
            .AddColumn(new GridColumn().NoWrap().PadRight(2))
            .AddColumn(new GridColumn().NoWrap().PadRight(2));

        bottomGrid.AddRow(_sessionStatsTable, _bombInfoTable);
        AnsiConsole.Write(bottomGrid);

        // Update tables with current data
        UpdateTables(stats);
    }

    private void DisplayHeader()
    {
        AnsiConsole.Write(
            new Rule("[bold blue]Fragify - CS:GO Live Tracker[/]")
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
        var table = new Table();
        table.Title = new TableTitle("[bold blue]Game Information[/]");
        table.Border = TableBorder.Rounded;
        table.AddColumn("Property");
        table.AddColumn("Value");

        table.AddRow("Map", "");
        table.AddRow("Mode", "");
        table.AddRow("Round", "");
        table.AddRow("Phase", "");
        table.AddRow("Score", "");

        return table;
    }

    private Table CreatePlayerStatsTable()
    {
        var table = new Table();
        table.Title = new TableTitle("[bold green]Player Statistics[/]");
        table.Border = TableBorder.Rounded;
        table.AddColumn("Stat");
        table.AddColumn("Value");

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
        var table = new Table();
        table.Title = new TableTitle("[bold yellow]Session Statistics[/]");
        table.Border = TableBorder.Rounded;
        table.AddColumn("Stat");
        table.AddColumn("Value");

        table.AddRow("Duration", "");
        table.AddRow("Total Rounds", "");
        table.AddRow("Wins", "");
        table.AddRow("Losses", "");
        table.AddRow("Win Rate", "");

        return table;
    }

    private Table CreateBombInfoTable()
    {
        var table = new Table();
        table.Title = new TableTitle("[bold red]Bomb Information[/]");
        table.Border = TableBorder.Rounded;
        table.AddColumn("Property");
        table.AddColumn("Value");

        table.AddRow("State", "");
        table.AddRow("Planted", "");
        table.AddRow("Defused", "");
        table.AddRow("Exploded", "");

        return table;
    }

    private Table CreateDebugInfoTable()
    {
        var table = new Table();
        table.Title = new TableTitle("[bold magenta]Debug Details[/]");
        table.Border = TableBorder.Rounded;
        table.AddColumn("Property");
        table.AddColumn("Value");

        table.AddRow("Last Message", "");
        table.AddRow("Connection", "");
        table.AddRow("Message Count", "");
        table.AddRow("Last Update", "");

        return table;
    }

    private void UpdateTables(GameStats stats)
    {
        // Update Game Info Table
        _gameInfoTable.UpdateCell(0, 1, stats.MapName);
        _gameInfoTable.UpdateCell(1, 1, stats.GameMode);
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
        _playerStatsTable.UpdateCell(6, 1, stats.ActiveWeapon);
        _playerStatsTable.UpdateCell(7, 1, GetTeamColor(stats.PlayerTeam).ToString());

        // Update Session Stats Table
        _sessionStatsTable.UpdateCell(0, 1, stats.GetSessionDurationDisplay());
        _sessionStatsTable.UpdateCell(1, 1, stats.TotalRounds.ToString());
        _sessionStatsTable.UpdateCell(2, 1, $"[green]{stats.RoundsWon}[/]");
        _sessionStatsTable.UpdateCell(3, 1, $"[red]{stats.RoundsLost}[/]");
        _sessionStatsTable.UpdateCell(4, 1, $"[bold]{stats.WinRate:F1}%[/]");

        // Update Bomb Info Table
        _bombInfoTable.UpdateCell(0, 1, GetBombStateColor(stats.BombState).ToString());
        _bombInfoTable.UpdateCell(1, 1, stats.BombPlantedTime?.ToString("HH:mm:ss") ?? "N/A");
        _bombInfoTable.UpdateCell(2, 1, stats.BombDefusedTime?.ToString("HH:mm:ss") ?? "N/A");
        _bombInfoTable.UpdateCell(3, 1, stats.BombExplodedTime?.ToString("HH:mm:ss") ?? "N/A");

        // Update Debug Info Table
        _debugInfoTable.UpdateCell(0, 1, stats.LastMessageContent.Length > 50 ? stats.LastMessageContent.Substring(0, 50) + "..." : stats.LastMessageContent);
        _debugInfoTable.UpdateCell(1, 1, stats.IsConnected ? "[green]Connected[/]" : "[red]Disconnected[/]");
        _debugInfoTable.UpdateCell(2, 1, stats.MessagesReceived.ToString());
        _debugInfoTable.UpdateCell(3, 1, stats.LastMessageTime?.ToString("HH:mm:ss") ?? "Never");
    }

    private IRenderable GetPhaseColor(string phase)
    {
        return phase switch
        {
            "Live" => new Markup("[bold green]Live[/]"),
            "FreezeTime" => new Markup("[bold yellow]Freeze Time[/]"),
            "Over" => new Markup("[bold red]Round Over[/]"),
            "Warmup" => new Markup("[bold blue]Warmup[/]"),
            _ => new Markup(phase)
        };
    }

    private IRenderable GetTeamColor(string team)
    {
        return team switch
        {
            "T" => new Markup("[bold red]Terrorist[/]"),
            "CT" => new Markup("[bold blue]Counter-Terrorist[/]"),
            _ => new Markup(team)
        };
    }

    private IRenderable GetBombStateColor(string state)
    {
        return state switch
        {
            "Planted" => new Markup("[bold red]Planted[/]"),
            "Defused" => new Markup("[bold green]Defused[/]"),
            "Exploded" => new Markup("[bold red]Exploded[/]"),
            "Planting" => new Markup("[bold yellow]Planting...[/]"),
            "Defusing" => new Markup("[bold yellow]Defusing...[/]"),
            _ => new Markup(state)
        };
    }
}
