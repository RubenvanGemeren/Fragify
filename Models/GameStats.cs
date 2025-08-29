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
    public int PlayerKDRatio => PlayerDeaths > 0 ? PlayerKills / PlayerDeaths : PlayerKills;
    public bool IsAlive => PlayerHealth > 0;
    public bool HasArmor => PlayerArmor > 0;
    public bool IsBombPlanted => BombState == "Planted";
    public bool IsBombDefused => BombState == "Defused";
    public bool IsBombExploded => BombState == "Exploded";

    public string GetScoreDisplay()
    {
        return $"{ScoreT} - {ScoreCT}";
    }

    public string GetPlayerStatus()
    {
        if (!IsAlive) return "Dead";
        if (HasArmor) return $"Alive ({PlayerHealth}HP, {PlayerArmor}AP)";
        return $"Alive ({PlayerHealth}HP)";
    }

    public string GetRoundTimeDisplay()
    {
        var minutes = RoundTime / 60;
        var seconds = RoundTime % 60;
        return $"{minutes:D2}:{seconds:D2}";
    }

    public string GetSessionDurationDisplay()
    {
        return $"{SessionDuration.Hours:D2}:{SessionDuration.Minutes:D2}:{SessionDuration.Seconds:D2}";
    }

    public string GetLastMessageTimeDisplay()
    {
        if (!LastMessageTime.HasValue) return "Never";
        var timeSince = DateTime.Now - LastMessageTime.Value;
        if (timeSince.TotalSeconds < 60)
            return $"{timeSince.TotalSeconds:F0}s ago";
        if (timeSince.TotalMinutes < 60)
            return $"{timeSince.TotalMinutes:F0}m ago";
        return $"{timeSince.TotalHours:F0}h ago";
    }
}
