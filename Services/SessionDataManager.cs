using FragifyTracker.Models;
using Newtonsoft.Json;
using System.Text;

namespace FragifyTracker.Services;

public class SessionDataManager
{
    private readonly string _sessionDataPath;
    private readonly object _lockObject = new object();
    private SessionData _currentSession;

    public SessionDataManager()
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var fragifyPath = Path.Combine(appDataPath, "Fragify");

        if (!Directory.Exists(fragifyPath))
        {
            Directory.CreateDirectory(fragifyPath);
        }

        _sessionDataPath = Path.Combine(fragifyPath, "session_data.json");
        _currentSession = new SessionData();
        LoadSessionData();
    }

    public void StartNewSession()
    {
        lock (_lockObject)
        {
            _currentSession = new SessionData
            {
                SessionId = Guid.NewGuid().ToString(),
                StartTime = DateTime.Now,
                Players = new Dictionary<string, PlayerSessionData>(),
                Rounds = new List<RoundData>(),
                GameEvents = new List<GameEvent>()
            };
            SaveSessionData();
        }
    }

    public void UpdatePlayerStats(string playerId, string playerName, string team, GameStats stats)
    {
        lock (_lockObject)
        {
            if (!_currentSession.Players.ContainsKey(playerId))
            {
                _currentSession.Players[playerId] = new PlayerSessionData
                {
                    PlayerId = playerId,
                    PlayerName = playerName,
                    Team = team,
                    RoundStats = new List<PlayerRoundStats>(),
                    TotalStats = new PlayerTotalStats()
                };
            }

            var player = _currentSession.Players[playerId];
            player.LastUpdate = DateTime.Now;
            player.CurrentStats = stats.Clone();

            // Update total stats
            player.TotalStats.TotalKills = stats.PlayerKills;
            player.TotalStats.TotalDeaths = stats.PlayerDeaths;
            player.TotalStats.TotalAssists = stats.PlayerAssists;
            player.TotalStats.TotalMVPs = stats.PlayerMVPs;
            player.TotalStats.TotalScore = stats.PlayerScore;
            player.TotalStats.TotalMoney = stats.PlayerMoney;
            player.TotalStats.TotalRounds = stats.TotalRounds;
            player.TotalStats.RoundsWon = stats.RoundsWon;
            player.TotalStats.RoundsLost = stats.RoundsLost;

            SaveSessionData();
        }
    }

    public void AddRoundData(RoundData roundData)
    {
        lock (_lockObject)
        {
            _currentSession.Rounds.Add(roundData);
            SaveSessionData();
        }
    }

    public void AddGameEvent(GameEvent gameEvent)
    {
        lock (_lockObject)
        {
            _currentSession.GameEvents.Add(gameEvent);
            SaveSessionData();
        }
    }

    public void UpdateCurrentRound(int roundNumber, string phase, int roundTime)
    {
        lock (_lockObject)
        {
            _currentSession.CurrentRound = roundNumber;
            _currentSession.CurrentPhase = phase;
            _currentSession.RoundTime = roundTime;
            SaveSessionData();
        }
    }

    public SessionData GetCurrentSession()
    {
        lock (_lockObject)
        {
            return _currentSession.Clone();
        }
    }

    public List<PlayerSessionData> GetAllPlayers()
    {
        lock (_lockObject)
        {
            return _currentSession.Players.Values.ToList();
        }
    }

    public List<RoundData> GetAllRounds()
    {
        lock (_lockObject)
        {
            return _currentSession.Rounds.ToList();
        }
    }

    private void LoadSessionData()
    {
        try
        {
            if (File.Exists(_sessionDataPath))
            {
                var json = File.ReadAllText(_sessionDataPath);
                var session = JsonConvert.DeserializeObject<SessionData>(json);
                if (session != null)
                {
                    _currentSession = session;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to load session data: {ex.Message}");
        }
    }

    private void SaveSessionData()
    {
        try
        {
            var json = JsonConvert.SerializeObject(_currentSession, Formatting.Indented);
            File.WriteAllText(_sessionDataPath, json, Encoding.UTF8);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to save session data: {ex.Message}");
        }
    }
}

public class SessionData
{
    public string SessionId { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public Dictionary<string, PlayerSessionData> Players { get; set; } = new();
    public List<RoundData> Rounds { get; set; } = new();
    public List<GameEvent> GameEvents { get; set; } = new();
    public int CurrentRound { get; set; }
    public string CurrentPhase { get; set; } = string.Empty;
    public int RoundTime { get; set; }
    public string MapName { get; set; } = string.Empty;
    public string GameMode { get; set; } = string.Empty;

    public SessionData Clone()
    {
        return new SessionData
        {
            SessionId = SessionId,
            StartTime = StartTime,
            EndTime = EndTime,
            Players = Players.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Clone()),
            Rounds = Rounds.Select(r => r.Clone()).ToList(),
            GameEvents = GameEvents.Select(e => e.Clone()).ToList(),
            CurrentRound = CurrentRound,
            CurrentPhase = CurrentPhase,
            RoundTime = RoundTime,
            MapName = MapName,
            GameMode = GameMode
        };
    }
}

public class PlayerSessionData
{
    public string PlayerId { get; set; } = string.Empty;
    public string PlayerName { get; set; } = string.Empty;
    public string Team { get; set; } = string.Empty;
    public DateTime LastUpdate { get; set; }
    public GameStats? CurrentStats { get; set; }
    public List<PlayerRoundStats> RoundStats { get; set; } = new();
    public PlayerTotalStats TotalStats { get; set; } = new();

    public PlayerSessionData Clone()
    {
        return new PlayerSessionData
        {
            PlayerId = PlayerId,
            PlayerName = PlayerName,
            Team = Team,
            LastUpdate = LastUpdate,
            CurrentStats = CurrentStats?.Clone(),
            RoundStats = RoundStats.Select(r => r.Clone()).ToList(),
            TotalStats = TotalStats.Clone()
        };
    }
}

public class PlayerRoundStats
{
    public int RoundNumber { get; set; }
    public int Kills { get; set; }
    public int Deaths { get; set; }
    public int Assists { get; set; }
    public int Score { get; set; }
    public int Money { get; set; }
    public string Weapon { get; set; } = string.Empty;
    public bool Won { get; set; }
    public DateTime Timestamp { get; set; }

    public PlayerRoundStats Clone()
    {
        return new PlayerRoundStats
        {
            RoundNumber = RoundNumber,
            Kills = Kills,
            Deaths = Deaths,
            Assists = Assists,
            Score = Score,
            Money = Money,
            Weapon = Weapon,
            Won = Won,
            Timestamp = Timestamp
        };
    }
}

public class PlayerTotalStats
{
    public int TotalKills { get; set; }
    public int TotalDeaths { get; set; }
    public int TotalAssists { get; set; }
    public int TotalMVPs { get; set; }
    public int TotalScore { get; set; }
    public int TotalMoney { get; set; }
    public int TotalRounds { get; set; }
    public int RoundsWon { get; set; }
    public int RoundsLost { get; set; }
    public double KDRatio => TotalDeaths > 0 ? (double)TotalKills / TotalDeaths : TotalKills;
    public double WinRate => TotalRounds > 0 ? (double)RoundsWon / TotalRounds * 100 : 0;

    public PlayerTotalStats Clone()
    {
        return new PlayerTotalStats
        {
            TotalKills = TotalKills,
            TotalDeaths = TotalDeaths,
            TotalAssists = TotalAssists,
            TotalMVPs = TotalMVPs,
            TotalScore = TotalScore,
            TotalMoney = TotalMoney,
            TotalRounds = TotalRounds,
            RoundsWon = RoundsWon,
            RoundsLost = RoundsLost
        };
    }
}

public class RoundData
{
    public int RoundNumber { get; set; }
    public string Phase { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public string Winner { get; set; } = string.Empty;
    public string WinCondition { get; set; } = string.Empty;
    public Dictionary<string, PlayerRoundStats> PlayerStats { get; set; } = new();
    public List<GameEvent> Events { get; set; } = new();

    public RoundData Clone()
    {
        return new RoundData
        {
            RoundNumber = RoundNumber,
            Phase = Phase,
            StartTime = StartTime,
            EndTime = EndTime,
            Winner = Winner,
            WinCondition = WinCondition,
            PlayerStats = PlayerStats.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Clone()),
            Events = Events.Select(e => e.Clone()).ToList()
        };
    }
}

public class GameEvent
{
    public string EventType { get; set; } = string.Empty;
    public string PlayerId { get; set; } = string.Empty;
    public string PlayerName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public int RoundNumber { get; set; }
    public Dictionary<string, object> AdditionalData { get; set; } = new();

    public GameEvent Clone()
    {
        return new GameEvent
        {
            EventType = EventType,
            PlayerId = PlayerId,
            PlayerName = PlayerName,
            Description = Description,
            Timestamp = Timestamp,
            RoundNumber = RoundNumber,
            AdditionalData = new Dictionary<string, object>(AdditionalData)
        };
    }
}
