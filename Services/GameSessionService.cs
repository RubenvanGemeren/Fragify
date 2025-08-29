using FragifyTracker.Models;
using System.Text.Json;

namespace FragifyTracker.Services;

public class GameSessionService
{
    private readonly string _sessionsPath = "Data/sessions/";
    private readonly PlayerConfigService _playerConfig;
    private GameSession? _currentSession;
    private readonly object _sessionLock = new object();

    public GameSessionService(PlayerConfigService playerConfig)
    {
        _playerConfig = playerConfig;
        Directory.CreateDirectory(_sessionsPath);
    }

    public string StartNewSession(string mapName, string playerTeam, string steamId)
    {
        lock (_sessionLock)
        {
            var gameId = GenerateGameId(mapName, steamId, playerTeam);
            var session = new GameSession
            {
                GameId = gameId,
                MapName = mapName,
                PlayerTeam = playerTeam,
                PlayerSteamId = steamId,
                StartTime = DateTime.Now,
                Status = "Active",
                Rounds = new List<RoundData>(),
                Events = new List<GameEvent>(),
                PlayerStats = new PlayerSessionStats
                {
                    SteamId = steamId,
                    Team = playerTeam,
                    Kills = 0,
                    Deaths = 0,
                    Assists = 0,
                    Money = 800,
                    Score = 0,
                    MVPs = 0,
                    RoundsWon = 0,
                    RoundsLost = 0
                }
            };

            _currentSession = session;
            SaveSession(session);

            Console.WriteLine($"🎮 New game session started: {gameId}");
            return gameId;
        }
    }

    public void UpdatePlayerStats(string steamId, int kills, int deaths, int assists, int money, int score, int mvps)
    {
        lock (_sessionLock)
        {
            if (_currentSession?.PlayerStats != null && _currentSession.PlayerStats.SteamId == steamId)
            {
                _currentSession.PlayerStats.Kills = kills;
                _currentSession.PlayerStats.Deaths = deaths;
                _currentSession.PlayerStats.Assists = assists;
                _currentSession.PlayerStats.Money = money;
                _currentSession.PlayerStats.Score = score;
                _currentSession.PlayerStats.MVPs = mvps;

                SaveSession(_currentSession);
            }
        }
    }

    public void AddRoundData(RoundData roundData)
    {
        lock (_sessionLock)
        {
            if (_currentSession != null)
            {
                _currentSession.Rounds.Add(roundData);

                // Update player stats based on round result
                if (_currentSession.PlayerStats != null)
                {
                    if (roundData.Winner == _currentSession.PlayerTeam)
                    {
                        _currentSession.PlayerStats.RoundsWon++;
                    }
                    else if (!string.IsNullOrEmpty(roundData.Winner))
                    {
                        _currentSession.PlayerStats.RoundsLost++;
                    }
                }

                SaveSession(_currentSession);
            }
        }
    }

    public void AddGameEvent(GameEvent gameEvent)
    {
        lock (_sessionLock)
        {
            if (_currentSession != null)
            {
                _currentSession.Events.Add(gameEvent);
                SaveSession(_currentSession);
            }
        }
    }

        public void EndSession(string result = "Completed")
    {
        lock (_sessionLock)
        {
            if (_currentSession != null)
            {
                _currentSession.EndTime = DateTime.Now;
                _currentSession.Status = result;
                _currentSession.Duration = (_currentSession.EndTime - _currentSession.StartTime) ?? TimeSpan.Zero;

                SaveSession(_currentSession);
                Console.WriteLine($"🏁 Game session ended: {_currentSession.GameId} - {result}");

                _currentSession = null;
            }
        }
    }

    public GameSession? GetCurrentSession()
    {
        lock (_sessionLock)
        {
            return _currentSession;
        }
    }

    public List<GameSession> GetAllSessions()
    {
        var sessions = new List<GameSession>();
        var sessionFiles = Directory.GetFiles(_sessionsPath, "*.json");

        foreach (var file in sessionFiles)
        {
            try
            {
                var jsonContent = File.ReadAllText(file);
                var session = JsonSerializer.Deserialize<GameSession>(jsonContent);
                if (session != null)
                {
                    sessions.Add(session);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading session from {file}: {ex.Message}");
            }
        }

        return sessions.OrderByDescending(s => s.StartTime).ToList();
    }

    public List<GameSession> GetSessionsBySteamId(string steamId)
    {
        return GetAllSessions().Where(s => s.PlayerSteamId == steamId).ToList();
    }

    public List<GameSession> GetSessionsByMap(string mapName)
    {
        return GetAllSessions().Where(s => s.MapName.Equals(mapName, StringComparison.OrdinalIgnoreCase)).ToList();
    }

    private string GenerateGameId(string mapName, string steamId, string team)
    {
        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var shortSteamId = steamId.Length > 8 ? steamId.Substring(steamId.Length - 8) : steamId;
        var shortMapName = mapName.Replace("de_", "").Replace("cs_", "").Replace("as_", "");

        return $"{shortMapName}_{timestamp}_{shortSteamId}_{team}";
    }

    private void SaveSession(GameSession session)
    {
        try
        {
            var fileName = $"{session.GameId}.json";
            var filePath = Path.Combine(_sessionsPath, fileName);

            var jsonContent = JsonSerializer.Serialize(session, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(filePath, jsonContent);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving session: {ex.Message}");
        }
    }

    public void DeleteSession(string gameId)
    {
        try
        {
            var fileName = $"{gameId}.json";
            var filePath = Path.Combine(_sessionsPath, fileName);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                Console.WriteLine($"🗑️ Session deleted: {gameId}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting session {gameId}: {ex.Message}");
        }
    }
}

public class GameSession
{
    public string GameId { get; set; } = string.Empty;
    public string MapName { get; set; } = string.Empty;
    public string PlayerTeam { get; set; } = string.Empty;
    public string PlayerSteamId { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public TimeSpan Duration { get; set; }
    public string Status { get; set; } = "Active";
    public List<RoundData> Rounds { get; set; } = new();
    public List<GameEvent> Events { get; set; } = new();
    public PlayerSessionStats PlayerStats { get; set; } = new();
}

public class PlayerSessionStats
{
    public string SteamId { get; set; } = string.Empty;
    public string Team { get; set; } = string.Empty;
    public int Kills { get; set; }
    public int Deaths { get; set; }
    public int Assists { get; set; }
    public int Money { get; set; }
    public int Score { get; set; }
    public int MVPs { get; set; }
    public int RoundsWon { get; set; }
    public int RoundsLost { get; set; }

    public double KDRatio => Deaths > 0 ? (double)Kills / Deaths : Kills;
    public double WinRate => (RoundsWon + RoundsLost) > 0 ? (double)RoundsWon / (RoundsWon + RoundsLost) : 0;
}
