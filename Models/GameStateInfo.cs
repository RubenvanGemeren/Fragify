namespace FragifyTracker.Models;

public class GameStateInfo
{
    public string CurrentPhase { get; set; } = "Unknown";
    public string BombState { get; set; } = "None";
    public DateTime? BombPlantedTime { get; set; }
    public int BombTimeRemaining { get; set; } = 0;
    public bool IsRoundWon { get; set; } = false;
    public bool IsRoundLost { get; set; } = false;
    public string RoundResult { get; set; } = "Unknown";

    // Visual effect states
    public string BorderColor { get; set; } = "#4A90E2";
    public string BorderEffect { get; set; } = "None";
    public bool ShowBombTimer { get; set; } = false;
    public bool ShowBombIcon { get; set; } = false;
    public string BombIconColor { get; set; } = "#FF0000";
}

public enum GamePhase
{
    Unknown,
    Warmup,
    Freezetime,
    Live,
    Over,
    Intermission
}

public enum BombState
{
    None,
    Carried,
    Dropped,
    Planted,
    Defused,
    Exploded
}

public enum RoundResult
{
    Unknown,
    Won,
    Lost,
    Draw
}
