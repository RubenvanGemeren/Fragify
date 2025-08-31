using System;
using System.Collections.Generic;

namespace FragifyTracker.Models;

public class GameStats
{
    // Basic Game Information
    public string MapName { get; set; } = string.Empty;
    public string GameMode { get; set; } = string.Empty;
    public int RoundNumber { get; set; } = 0;
    public string RoundPhase { get; set; } = string.Empty;
    public int ScoreT { get; set; } = 0;
    public int ScoreCT { get; set; } = 0;

    // Player Information
    public string PlayerName { get; set; } = string.Empty;
    public string PlayerTeam { get; set; } = string.Empty;
    public int PlayerKills { get; set; } = 0;
    public int PlayerDeaths { get; set; } = 0;
    public int PlayerAssists { get; set; } = 0;
    public int PlayerMvps { get; set; } = 0;
    public int PlayerScore { get; set; } = 0;
    public int PlayerHealth { get; set; } = 100;
    public int PlayerArmor { get; set; } = 100;
    public int PlayerMoney { get; set; } = 800;
    public string ActiveWeapon { get; set; } = string.Empty;

    // Session Information
    public DateTime SessionStartTime { get; set; } = DateTime.Now;
    public DateTime? LastMessageTime { get; set; }
    public int TotalRounds { get; set; } = 0;
    public int RoundsWon { get; set; } = 0;
    public int RoundsLost { get; set; } = 0;
    public double WinRate => TotalRounds > 0 ? (double)RoundsWon / TotalRounds * 100 : 0;

    // Connection Status
    public bool IsConnected { get; set; } = false;
    public int MessagesReceived { get; set; } = 0;

    // Round Information
    public string WinCondition { get; set; } = string.Empty;
    public string WinTeam { get; set; } = string.Empty;

    // Utility Methods
    public string SessionDuration
    {
        get
        {
            var duration = DateTime.Now - SessionStartTime;
            return $"{duration.Hours:D2}:{duration.Minutes:D2}:{duration.Seconds:D2}";
        }
    }

    public GameStats Clone()
    {
        return new GameStats
        {
            MapName = this.MapName,
            GameMode = this.GameMode,
            RoundNumber = this.RoundNumber,
            RoundPhase = this.RoundPhase,
            ScoreT = this.ScoreT,
            ScoreCT = this.ScoreCT,
            PlayerName = this.PlayerName,
            PlayerTeam = this.PlayerTeam,
            PlayerKills = this.PlayerKills,
            PlayerDeaths = this.PlayerDeaths,
            PlayerAssists = this.PlayerAssists,
            PlayerMvps = this.PlayerMvps,
            PlayerScore = this.PlayerScore,
            PlayerHealth = this.PlayerHealth,
            PlayerArmor = this.PlayerArmor,
            PlayerMoney = this.PlayerMoney,
            ActiveWeapon = this.ActiveWeapon,
            SessionStartTime = this.SessionStartTime,
            LastMessageTime = this.LastMessageTime,
            TotalRounds = this.TotalRounds,
            RoundsWon = this.RoundsWon,
            RoundsLost = this.RoundsLost,
            IsConnected = this.IsConnected,
            MessagesReceived = this.MessagesReceived,
            WinCondition = this.WinCondition,
            WinTeam = this.WinTeam
        };
    }
}
