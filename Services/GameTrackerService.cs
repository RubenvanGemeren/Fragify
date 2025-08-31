using FragifyTracker.Models;
using CounterStrike2GSI;

namespace FragifyTracker.Services;

public class GameTrackerService
{
    private GameStats _currentStats;
    private readonly SessionDataManager _sessionManager;
    private readonly GameSessionService _gameSessionService;
    private readonly MapThemeService _mapThemeService;
    private readonly WeaponImageService _weaponImageService;
    private readonly PlayerConfigService _playerConfigService;
    private string? _mainPlayerSteamId;
    private GameStateInfo _gameStateInfo;

            public GameTrackerService()
    {
        _currentStats = new GameStats();
        _sessionManager = new SessionDataManager();
        _playerConfigService = new PlayerConfigService();
        _gameSessionService = new GameSessionService(_playerConfigService);
        _mapThemeService = new MapThemeService();
        _weaponImageService = new WeaponImageService();
        _gameStateInfo = new GameStateInfo();

        _sessionManager.StartNewSession();

        // Set initial connection status for test mode
        SetInitialConnectionStatus();
    }

    private void SetInitialConnectionStatus()
    {
        _currentStats.IsConnected = true;
        _currentStats.ConnectionStatus = "Connected - Test Mode";
        _currentStats.MessagesReceived = 0;
        _currentStats.LastMessageTime = DateTime.Now;
        _currentStats.LastMessageContent = "Test mode initialized - waiting for events";
    }

    public void UpdateGameState(GameState gameState)
    {
        try
        {
            // Debug: Log all available GameState properties to find where scores are stored
            Console.WriteLine($"🔍 GameState properties available:");
            var gameStateType = gameState.GetType();
            var gameStateProperties = gameStateType.GetProperties();
            Console.WriteLine($"🔍 GameState property names: {string.Join(", ", gameStateProperties.Select(p => p.Name))}");

            // Check if this message is from the configured player
            if (gameState.Player?.SteamID != null)
            {
                var configuredSteamId = _playerConfigService.GetSteamId();
                if (gameState.Player.SteamID != configuredSteamId)
                {
                    // Skip messages from other players
                    return;
                }
            }

            _currentStats.MapName = gameState.Map.Name;
            _currentStats.GameMode = gameState.Map.Mode.ToString();
            _currentStats.RoundPhase = gameState.Round.Phase.ToString();
            _currentStats.RoundNumber = 0; // TODO: Fix for new library structure - gameState.Map.Round;
            _currentStats.RoundTime = 0; // TODO: Calculate from phase countdowns

            // Debug: Log Round properties to find where scores are stored
            if (gameState.Round != null)
            {
                Console.WriteLine($"🔍 Round properties available:");
                Console.WriteLine($"   Round.Phase: {gameState.Round.Phase}");

                var roundType = gameState.Round.GetType();
                var roundProperties = roundType.GetProperties();
                Console.WriteLine($"🔍 Round property names: {string.Join(", ", roundProperties.Select(p => p.Name))}");
            }



            if (gameState.Player != null)
            {
                // Track the main player's Steam ID for persistent identification
                if (string.IsNullOrEmpty(_mainPlayerSteamId))
                {
                    _mainPlayerSteamId = gameState.Player.SteamID;
                    Console.WriteLine($"🎯 Main player identified: {_mainPlayerSteamId}");

                    // Start new game session if this is the first time we see this player
                    if (!string.IsNullOrEmpty(_currentStats.MapName) && !string.IsNullOrEmpty(_currentStats.PlayerTeam))
                    {
                        _gameSessionService.StartNewSession(_currentStats.MapName, _currentStats.PlayerTeam, _mainPlayerSteamId);
                    }
                }

                _currentStats.PlayerSteamId = gameState.Player.SteamID;
                _currentStats.PlayerHealth = gameState.Player.State.Health;
                _currentStats.PlayerArmor = gameState.Player.State.Armor;
                _currentStats.PlayerMoney = gameState.Player.State.Money;
                _currentStats.PlayerKills = gameState.Player.MatchStats.Kills;
                _currentStats.PlayerDeaths = gameState.Player.MatchStats.Deaths;
                _currentStats.PlayerAssists = gameState.Player.MatchStats.Assists;
                _currentStats.PlayerMVPs = gameState.Player.MatchStats.MVPs;
                _currentStats.PlayerScore = gameState.Player.MatchStats.Score;
                _currentStats.PlayerTeam = gameState.Player.Team.ToString();
                _currentStats.ActiveWeapon = "Unknown"; // TODO: Fix for new library structure

                // Update game session stats
                _gameSessionService.UpdatePlayerStats(
                    _mainPlayerSteamId,
                    _currentStats.PlayerKills,
                    _currentStats.PlayerDeaths,
                    _currentStats.PlayerAssists,
                    _currentStats.PlayerMoney,
                    _currentStats.PlayerScore,
                    _currentStats.PlayerMVPs
                );
            }

            if (gameState.Map != null)
            {
                // Debug: Log all available properties to find where scores are stored
                Console.WriteLine($"🔍 Map properties available:");
                Console.WriteLine($"   Map.Name: {gameState.Map.Name}");
                Console.WriteLine($"   Map.Mode: {gameState.Map.Mode}");

                // Try to find score properties
                var mapType = gameState.Map.GetType();
                var mapProperties = mapType.GetProperties();
                Console.WriteLine($"🔍 Map property names: {string.Join(", ", mapProperties.Select(p => p.Name))}");

                // Try to extract team scores from different possible locations
                try
                {
                    // Check if TeamT and TeamCT properties exist
                    var teamTProperty = gameStateType.GetProperty("TeamT");
                    var teamCTProperty = gameStateType.GetProperty("TeamCT");

                    if (teamTProperty != null && teamCTProperty != null)
                    {
                        var teamT = teamTProperty.GetValue(gameState);
                        var teamCT = teamCTProperty.GetValue(gameState);

                        if (teamT != null && teamCT != null)
                        {
                            var teamTScoreProperty = teamT.GetType().GetProperty("Score");
                            var teamCTScoreProperty = teamCT.GetType().GetProperty("Score");

                            if (teamTScoreProperty != null && teamCTScoreProperty != null)
                            {
                                _currentStats.ScoreT = (int)teamTScoreProperty.GetValue(teamT);
                                _currentStats.ScoreCT = (int)teamCTScoreProperty.GetValue(teamCT);
                                Console.WriteLine($"✅ Team scores extracted: T={_currentStats.ScoreT}, CT={_currentStats.ScoreCT}");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ Could not extract team scores: {ex.Message}");
                    _currentStats.ScoreT = 0;
                    _currentStats.ScoreCT = 0;
                }
            }

            _currentStats.MessagesReceived++;
            _currentStats.LastMessageTime = DateTime.Now;

            // Store only the essential GameState properties to avoid circular references
            try
            {
                var essentialData = new
                {
                    Map = new
                    {
                        Name = gameState.Map?.Name,
                        Mode = gameState.Map?.Mode.ToString()
                    },
                    Round = new
                    {
                        Phase = gameState.Round?.Phase.ToString()
                    },
                    Player = new
                    {
                        Name = gameState.Player?.Name,
                        SteamID = gameState.Player?.SteamID,
                        Team = gameState.Player?.Team.ToString(),
                        Health = gameState.Player?.State?.Health,
                        Armor = gameState.Player?.State?.Armor,
                        Money = gameState.Player?.State?.Money,
                        Kills = gameState.Player?.MatchStats?.Kills,
                        Deaths = gameState.Player?.MatchStats?.Deaths,
                        Assists = gameState.Player?.MatchStats?.Assists,
                        MVPs = gameState.Player?.MatchStats?.MVPs,
                        Score = gameState.Player?.MatchStats?.Score
                    },
                    Timestamp = DateTime.Now
                };

                var jsonOptions = new System.Text.Json.JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
                };

                _currentStats.LastMessageContent = System.Text.Json.JsonSerializer.Serialize(essentialData, jsonOptions);
            }
            catch (Exception ex)
            {
                // Fallback to simple format if JSON serialization fails
                _currentStats.LastMessageContent = $"Map: {_currentStats.MapName}, Phase: {_currentStats.RoundPhase}, Player Health: {_currentStats.PlayerHealth}";
                Console.WriteLine($"[WARNING] Failed to serialize essential GameState data: {ex.Message}");
            }

            _currentStats.IsConnected = true;
            _currentStats.ConnectionStatus = "Connected - Receiving data";

            // Update session data
            UpdateSessionData(gameState);

            // Check for round phase changes
            CheckForRoundPhaseChanges(gameState);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating game state: {ex.Message}");
        }
    }

    private void UpdateSessionData(GameState gameState)
    {
        try
        {
            // Update current round info
            _sessionManager.UpdateCurrentRound(
                _currentStats.RoundNumber,
                _currentStats.RoundPhase,
                _currentStats.RoundTime
            );

            // Update main player stats using their Steam ID for persistent tracking
            var playerId = _mainPlayerSteamId ?? "unknown_player";
            var playerName = gameState.Player?.Name ?? "FragifyPlayer";
            var playerTeam = _currentStats.PlayerTeam ?? "Unknown";

            _sessionManager.UpdatePlayerStats(
                playerId,
                playerName,
                playerTeam,
                _currentStats
            );

            // Update session metadata
            var session = _sessionManager.GetCurrentSession();
            session.MapName = _currentStats.MapName;
            session.GameMode = _currentStats.GameMode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating session data: {ex.Message}");
        }
    }

    private void CheckForRoundPhaseChanges(GameState gameState)
    {
        var currentPhase = gameState.Round.Phase.ToString();

        // Update game state info for visual effects
        UpdateGameStateInfo(currentPhase);

        if (currentPhase == "Live" && _currentStats.RoundPhase != "Live")
        {
            OnRoundBegin();
        }
        else if (currentPhase == "Over" && _currentStats.RoundPhase != "Over")
        {
        }
    }

    private void UpdateGameStateInfo(string phase)
    {
        _gameStateInfo.CurrentPhase = phase;


        // Update visual effects based on game state
        switch (phase)
        {
            case "Freezetime":
                _gameStateInfo.BorderColor = "#87CEEB"; // Icey blue
                _gameStateInfo.BorderEffect = "Glow";
                break;
            case "Live":
                _gameStateInfo.BorderColor = "#4A90E2"; // Normal blue
                _gameStateInfo.BorderEffect = "None";
                break;
            case "Over":
                // Border color will be set by round result
                break;
        }


    }

    public void OnRoundBegin()
    {
        _currentStats.TotalRounds++;
        _currentStats.RoundTime = 0;

        var roundData = new RoundData
        {
            RoundNumber = _currentStats.TotalRounds,
            Phase = "Live",
            StartTime = DateTime.Now
        };

        _sessionManager.AddRoundData(roundData);

        var gameEvent = new GameEvent
        {
            EventType = "RoundBegin",
            PlayerId = _mainPlayerSteamId ?? "unknown_player",
            PlayerName = "FragifyPlayer",
            Description = $"Round {_currentStats.TotalRounds} started",
            Timestamp = DateTime.Now,
            RoundNumber = _currentStats.TotalRounds
        };

        _sessionManager.AddGameEvent(gameEvent);
        _gameSessionService.AddGameEvent(gameEvent);
    }

    public void OnRoundEnd()
    {
        var roundData = _sessionManager.GetCurrentSession().Rounds.LastOrDefault();
        if (roundData != null)
        {
            roundData.EndTime = DateTime.Now;
            roundData.Phase = "Over";

            // Determine winner based on current score
            if (_currentStats.ScoreT > _currentStats.ScoreCT)
            {
                roundData.Winner = "T";
                _currentStats.RoundsWon++;
            }
            else if (_currentStats.ScoreCT > _currentStats.ScoreT)
            {
                roundData.Winner = "CT";
                _currentStats.RoundsLost++;
            }
        }

        // Update visual effects based on round result
        if (roundData?.Winner == _currentStats.PlayerTeam)
        {
            _gameStateInfo.IsRoundWon = true;
            _gameStateInfo.IsRoundLost = false;
            _gameStateInfo.RoundResult = "Won";
            _gameStateInfo.BorderColor = "#00FF00"; // Green for won
            _gameStateInfo.BorderEffect = "Pulse";
        }
        else if (!string.IsNullOrEmpty(roundData?.Winner))
        {
            _gameStateInfo.IsRoundWon = false;
            _gameStateInfo.IsRoundLost = true;
            _gameStateInfo.RoundResult = "Lost";
            _gameStateInfo.BorderColor = "#FF0000"; // Red for lost
            _gameStateInfo.BorderEffect = "Shake";
        }

        var gameEvent = new GameEvent
        {
            EventType = "RoundEnd",
            PlayerId = _mainPlayerSteamId ?? "unknown_player",
            PlayerName = "FragifyPlayer",
            Description = $"Round {_currentStats.TotalRounds} ended - Winner: {roundData?.Winner ?? "Unknown"}",
            Timestamp = DateTime.Now,
            RoundNumber = _currentStats.TotalRounds
        };

        _sessionManager.AddGameEvent(gameEvent);

        // Also add to game session
        if (roundData != null)
        {
            _gameSessionService.AddRoundData(roundData);
        }
    }

    public void OnRoundStarted()
    {
        _currentStats.TotalRounds++;
        _currentStats.RoundNumber = _currentStats.TotalRounds;

        var gameEvent = new GameEvent
        {
            EventType = "RoundStarted",
            PlayerId = _mainPlayerSteamId ?? "unknown_player",
            PlayerName = "FragifyPlayer",
            Description = $"Round {_currentStats.TotalRounds} started",
            Timestamp = DateTime.Now,
            RoundNumber = _currentStats.TotalRounds
        };

        _sessionManager.AddGameEvent(gameEvent);
        _gameSessionService.AddGameEvent(gameEvent);
    }

    public void OnRoundEnded(string winningTeam)
    {
        var roundData = new RoundData
        {
            RoundNumber = _currentStats.TotalRounds,
            Winner = winningTeam,
            EndTime = DateTime.Now,
                            WinCondition = "Elimination"
        };

        _sessionManager.AddRoundData(roundData);

        var gameEvent = new GameEvent
        {
            EventType = "RoundEnded",
            PlayerId = _mainPlayerSteamId ?? "unknown_player",
            PlayerName = "FragifyPlayer",
            Description = $"Round {_currentStats.TotalRounds} ended - Winner: {winningTeam}",
            Timestamp = DateTime.Now,
            RoundNumber = _currentStats.TotalRounds
        };

        _sessionManager.AddGameEvent(gameEvent);

        // Also add to game session
        if (roundData != null)
        {
            _gameSessionService.AddRoundData(roundData);
        }
    }

    public void OnPlayerDied(string playerName)
    {
        var gameEvent = new GameEvent
        {
            EventType = "PlayerDied",
            PlayerId = _mainPlayerSteamId ?? "unknown_player",
            PlayerName = playerName,
            Description = $"{playerName} died",
            Timestamp = DateTime.Now,
            RoundNumber = _currentStats.TotalRounds
        };

        _sessionManager.AddGameEvent(gameEvent);
        _gameSessionService.AddGameEvent(gameEvent);
    }

    public void OnPlayerTookDamage(string playerName, int damage)
    {
        var gameEvent = new GameEvent
        {
            EventType = "PlayerTookDamage",
            PlayerId = _mainPlayerSteamId ?? "unknown_player",
            PlayerName = playerName,
            Description = $"{playerName} took {damage} damage",
            Timestamp = DateTime.Now,
            RoundNumber = _currentStats.TotalRounds
        };

        _sessionManager.AddGameEvent(gameEvent);
        _gameSessionService.AddGameEvent(gameEvent);
    }







    public void OnConnectionEstablished()
    {
        _currentStats.IsConnected = true;
        _currentStats.ConnectionStatus = "Connected - Receiving data";
    }

    public void OnConnectionLost()
    {
        _currentStats.IsConnected = false;
        _currentStats.ConnectionStatus = "Disconnected - No recent messages";
    }

    public GameStats GetCurrentStats()
    {
        return _currentStats.Clone();
    }

    public SessionData GetCurrentSession()
    {
        return _sessionManager.GetCurrentSession();
    }

    public List<PlayerSessionData> GetAllPlayers()
    {
        return _sessionManager.GetAllPlayers();
    }

    public List<RoundData> GetAllRounds()
    {
        return _sessionManager.GetAllRounds();
    }

    public string? GetMainPlayerSteamId()
    {
        return _mainPlayerSteamId;
    }

    public MapInfo? GetCurrentMapTheme()
    {
        return _mapThemeService.GetMapTheme(_currentStats.MapName);
    }

    public string GetWeaponImageUrl(string weaponName)
    {
        return _weaponImageService.GetWeaponImageUrl(weaponName);
    }

    public string GetAgentImageUrl(string team)
    {
        return _weaponImageService.GetAgentImageUrl(team);
    }

    public GameStateInfo GetGameStateInfo()
    {
        return _gameStateInfo;
    }

    public GameSession? GetCurrentGameSession()
    {
        return _gameSessionService.GetCurrentSession();
    }

    public List<GameSession> GetAllGameSessions()
    {
        return _gameSessionService.GetAllSessions();
    }

    public void ResetSession()
    {
        _currentStats = new GameStats();
        _sessionManager.StartNewSession();
        SetInitialConnectionStatus();
    }
}
