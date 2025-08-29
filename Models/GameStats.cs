namespace FragifyTracker.Models;

public class GameStats
{
    // Game Information
    public string MapName { get; set; } = string.Empty;
    public string GameMode { get; set; } = string.Empty;
    public string RoundPhase { get; set; } = string.Empty;
    public int RoundNumber { get; set; }
    public int ScoreT { get; set; }
    public int ScoreCT { get; set; }
    public int RoundTime { get; set; }

    // Player Information
    public string PlayerSteamId { get; set; } = string.Empty;
    public int PlayerHealth { get; set; }
    public int PlayerArmor { get; set; }
    public int PlayerMoney { get; set; }
    public int PlayerKills { get; set; }
    public int PlayerDeaths { get; set; }
    public int PlayerAssists { get; set; }
    public int PlayerMVPs { get; set; }
    public int PlayerScore { get; set; }
    public string ActiveWeapon { get; set; } = string.Empty;
    public string PlayerTeam { get; set; } = string.Empty;

    // Bomb Information
    public string BombState { get; set; } = string.Empty;
    public DateTime? BombPlantedTime { get; set; }
    public DateTime? BombDefusedTime { get; set; }
    public DateTime? BombExplodedTime { get; set; }

    // Session Information
    public TimeSpan SessionDuration { get; set; }
    public int TotalRounds { get; set; }
    public int RoundsWon { get; set; }
    public int RoundsLost { get; set; }
    public double WinRate { get; set; }

    // Round Information
    public DateTime? RoundStartTime { get; set; }

    // Flash Information
    public int LastFlashDuration { get; set; }
    public DateTime? LastFlashTime { get; set; }

    // Debug Information
    public int MessagesReceived { get; set; }
    public DateTime? LastMessageTime { get; set; }
    public string ConnectionStatus { get; set; } = "Waiting for connection...";
    public bool IsConnected { get; set; }
    public string LastMessageContent { get; set; } = "No messages yet";

    // Computed Properties
    public double PlayerKDRatio => PlayerDeaths > 0 ? (double)PlayerKills / PlayerDeaths : PlayerKills;
    public bool IsAlive => PlayerHealth > 0;
    public bool HasArmor => PlayerArmor > 0;
    public bool IsBombPlanted => BombState == "Planted";
    public bool IsBombDefused => BombState == "Defused";
    public bool IsBombExploded => BombState == "Exploded";

    // Computed properties for display
    public string GetScoreDisplay() => $"{ScoreT} - {ScoreCT}";
    public string GetSessionDurationDisplay() => SessionDuration.ToString(@"hh\:mm\:ss");
    public string GetRoundTimeDisplay() => RoundTime.ToString(@"mm\:ss");
    public string GetLastMessageTimeDisplay() => LastMessageTime?.ToString("HH:mm:ss") ?? "Never";

    public string GetPlayerStatus()
    {
        if (!IsAlive) return "Dead";
        if (HasArmor) return $"Alive ({PlayerHealth}HP, {PlayerArmor}AP)";
        return $"Alive ({PlayerHealth}HP)";
    }

    public GameStats Clone()
    {
        return new GameStats
        {
            // Game Info
            MapName = this.MapName,
            GameMode = this.GameMode,
            RoundPhase = this.RoundPhase,
            RoundNumber = this.RoundNumber,
            RoundTime = this.RoundTime,
            ScoreT = this.ScoreT,
            ScoreCT = this.ScoreCT,
            BombState = this.BombState,
            BombPlantedTime = this.BombPlantedTime,
            BombDefusedTime = this.BombDefusedTime,
            BombExplodedTime = this.BombExplodedTime,

            // Player Info
            PlayerSteamId = this.PlayerSteamId,
            PlayerHealth = this.PlayerHealth,
            PlayerArmor = this.PlayerArmor,
            PlayerMoney = this.PlayerMoney,
            PlayerKills = this.PlayerKills,
            PlayerDeaths = this.PlayerDeaths,
            PlayerAssists = this.PlayerAssists,
            PlayerMVPs = this.PlayerMVPs,
            PlayerScore = this.PlayerScore,
            PlayerTeam = this.PlayerTeam,
            ActiveWeapon = this.ActiveWeapon,

            // Session Info
            SessionDuration = this.SessionDuration,
            TotalRounds = this.TotalRounds,
            RoundsWon = this.RoundsWon,
            RoundsLost = this.RoundsLost,
            WinRate = this.WinRate,
            RoundStartTime = this.RoundStartTime,

            // Flash Info
            LastFlashDuration = this.LastFlashDuration,
            LastFlashTime = this.LastFlashTime,

            // Debug Info
            MessagesReceived = this.MessagesReceived,
            LastMessageTime = this.LastMessageTime,
            IsConnected = this.IsConnected,
            ConnectionStatus = this.ConnectionStatus,
            LastMessageContent = this.LastMessageContent
        };
    }
}
