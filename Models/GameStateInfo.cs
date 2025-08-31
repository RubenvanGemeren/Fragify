using System;

namespace FragifyTracker.Models;

public class GameStateInfo
{
    // Map Information
    public string MapName { get; set; } = string.Empty;
    public string GameMode { get; set; } = string.Empty;
    public string RoundPhase { get; set; } = string.Empty;
    public int RoundNumber { get; set; } = 0;
    public int ScoreT { get; set; } = 0;
    public int ScoreCT { get; set; } = 0;

    // Player Information
    public string PlayerName { get; set; } = string.Empty;
    public string PlayerTeam { get; set; } = string.Empty;
    public int PlayerHealth { get; set; } = 100;
    public int PlayerArmor { get; set; } = 100;
    public int PlayerMoney { get; set; } = 800;
    public string ActiveWeapon { get; set; } = string.Empty;

    // Session Information
    public string SessionDuration { get; set; } = "00:00:00";
    public int TotalRounds { get; set; } = 0;
    public int RoundsWon { get; set; } = 0;
    public int RoundsLost { get; set; } = 0;
    public double WinRate { get; set; } = 0.0;

    // Connection Status
    public bool IsConnected { get; set; } = false;
    public DateTime? LastMessageTime { get; set; }
    public int MessagesReceived { get; set; } = 0;

    // UI State
    public bool IsUpdating { get; set; } = false;
    public string LastUpdateTime { get; set; } = string.Empty;
    public string CurrentPhase { get; set; } = string.Empty;
    public string BorderColor { get; set; } = string.Empty;
    public string BorderEffect { get; set; } = string.Empty;
    public bool IsRoundWon { get; set; } = false;
    public bool IsRoundLost { get; set; } = false;
    public string RoundResult { get; set; } = string.Empty;
}
