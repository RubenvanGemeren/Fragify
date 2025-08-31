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

            // Note: Bomb state is not available to players in competitive games
            // Only available to spectators, so we'll set it to "Unknown"
            _currentStats.BombState = "Unknown";

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
                // TODO: Fix for new library structure - need to find where team scores are stored
                _currentStats.ScoreT = 0; // gameState.Map.TeamT.Score;
                _currentStats.ScoreCT = 0; // gameState.Map.TeamCT.Score;
            }

            _currentStats.MessagesReceived++;
            _currentStats.LastMessageTime = DateTime.Now;

            // Store the full GameState as JSON to see all available data
            try
            {
                var jsonOptions = new System.Text.Json.JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
                    MaxDepth = 64, // Increase max depth for complex objects
                    ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles // Handle circular references
                };
                _currentStats.LastMessageContent = System.Text.Json.JsonSerializer.Serialize(gameState, jsonOptions);
            }
            catch (Exception ex)
            {
                // Fallback to simple format if JSON serialization fails
                _currentStats.LastMessageContent = $"Map: {_currentStats.MapName}, Phase: {_currentStats.RoundPhase}, Player Health: {_currentStats.PlayerHealth}";
                Console.WriteLine($"[WARNING] Failed to serialize GameState to JSON: {ex.Message}");
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
        _gameStateInfo.BombState = "N/A"; // Bomb state is not available to players

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

        // Bomb state visual effects are removed as bomb state is not available to players
        _gameStateInfo.ShowBombIcon = false;
        _gameStateInfo.ShowBombTimer = false;
        _gameStateInfo.BombIconColor = "#000000"; // Default color
        _gameStateInfo.BombTimeRemaining = 0;
        _gameStateInfo.BombPlantedTime = null;
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
            WinCondition = "Bomb"
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
