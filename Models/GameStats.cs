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
    public string PlayerSteamId { get; set; } = string.Empty;
    public int PlayerKills { get; set; } = 0;
    public int PlayerDeaths { get; set; } = 0;
    public int PlayerAssists { get; set; } = 0;
    public int PlayerMVPs { get; set; } = 0;
    public int PlayerScore { get; set; } = 0;
    public int PlayerHealth { get; set; } = 100;
    public int PlayerArmor { get; set; } = 100;
    public int PlayerMoney { get; set; } = 800;
    public string ActiveWeapon { get; set; } = string.Empty;
    public int LastFlashDuration { get; set; } = 0;

    // Session Information
    public DateTime SessionStartTime { get; set; } = DateTime.Now;
    public DateTime? LastMessageTime { get; set; }
    public int TotalRounds { get; set; } = 0;
    public int RoundsWon { get; set; } = 0;
    public int RoundsLost { get; set; } = 0;
    public double WinRate => TotalRounds > 0 ? (double)RoundsWon / TotalRounds * 100 : 0;

    // Connection Status
    public bool IsConnected { get; set; } = false;
    public string ConnectionStatus { get; set; } = "Disconnected";
    public int MessagesReceived { get; set; } = 0;
    public string LastMessageContent { get; set; } = string.Empty;

    // Round Information
    public string WinCondition { get; set; } = string.Empty;
    public string WinTeam { get; set; } = string.Empty;
    public int RoundTime { get; set; } = 0;

    // Utility Methods
    public string SessionDuration
    {
        get
        {
            var duration = DateTime.Now - SessionStartTime;
            return $"{duration.Hours:D2}:{duration.Minutes:D2}:{duration.Seconds:D2}";
        }
    }

    public string GetSessionDurationDisplay()
    {
        var duration = DateTime.Now - SessionStartTime;
        return $"{duration.Hours:D2}:{duration.Minutes:D2}:{duration.Seconds:D2}";
    }

    public string GetRoundTimeDisplay()
    {
        var minutes = RoundTime / 60;
        var seconds = RoundTime % 60;
        return $"{minutes:D2}:{seconds:D2}";
    }

    public string GetScoreDisplay()
    {
        return $"{ScoreT} - {ScoreCT}";
    }

    public string GetLastMessageTimeDisplay()
    {
        if (LastMessageTime.HasValue)
        {
            var timeSince = DateTime.Now - LastMessageTime.Value;
            if (timeSince.TotalMinutes < 1)
                return "Just now";
            if (timeSince.TotalMinutes < 60)
                return $"{(int)timeSince.TotalMinutes}m ago";
            if (timeSince.TotalHours < 24)
                return $"{(int)timeSince.TotalHours}h ago";
            return $"{(int)timeSince.TotalDays}d ago";
        }
        return "Never";
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
            PlayerSteamId = this.PlayerSteamId,
            PlayerKills = this.PlayerKills,
            PlayerDeaths = this.PlayerDeaths,
            PlayerAssists = this.PlayerAssists,
            PlayerMVPs = this.PlayerMVPs,
            PlayerScore = this.PlayerScore,
            PlayerHealth = this.PlayerHealth,
            PlayerArmor = this.PlayerArmor,
            PlayerMoney = this.PlayerMoney,
            ActiveWeapon = this.ActiveWeapon,
            LastFlashDuration = this.LastFlashDuration,
            SessionStartTime = this.SessionStartTime,
            LastMessageTime = this.LastMessageTime,
            TotalRounds = this.TotalRounds,
            RoundsWon = this.RoundsWon,
            RoundsLost = this.RoundsLost,
            IsConnected = this.IsConnected,
            ConnectionStatus = this.ConnectionStatus,
            MessagesReceived = this.MessagesReceived,
            LastMessageContent = this.LastMessageContent,
            WinCondition = this.WinCondition,
            WinTeam = this.WinTeam,
            RoundTime = this.RoundTime
        };
    }
}
