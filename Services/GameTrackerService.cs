using CSGSI;
using CSGSI.Nodes;
using FragifyTracker.Models;

namespace FragifyTracker.Services;

public class GameTrackerService
{
    private GameStats _currentStats;
    private GameState? _previousGameState;
    private DateTime _sessionStartTime;
    private int _totalRounds;
    private int _roundsWon;
    private int _roundsLost;

    public GameTrackerService()
    {
        _currentStats = new GameStats();
        _sessionStartTime = DateTime.Now;
        _totalRounds = 0;
        _roundsWon = 0;
        _roundsLost = 0;
    }

    public void UpdateGameState(GameState gameState)
    {
        if (gameState == null) return;

        // Update debug information
        _currentStats.MessagesReceived++;
        _currentStats.LastMessageTime = DateTime.Now;
        _currentStats.IsConnected = true;
        _currentStats.ConnectionStatus = "Connected - Receiving data";

        // Store a summary of the last message for debugging
        _currentStats.LastMessageContent = $"Map: {gameState.Map.Name}, Phase: {gameState.Round.Phase}, Player Health: {gameState.Player?.State.Health ?? 0}";

        // Update basic game info
        _currentStats.MapName = gameState.Map.Name;
        _currentStats.GameMode = gameState.Map.Mode.ToString();
        _currentStats.RoundPhase = gameState.Round.Phase.ToString();
        _currentStats.RoundNumber = 0; // CSGSI doesn't provide round number
        _currentStats.ScoreT = gameState.Map.TeamT.Score;
        _currentStats.ScoreCT = gameState.Map.TeamCT.Score;

        // Update player stats if available
        if (gameState.Player != null)
        {
            UpdatePlayerStats(gameState.Player);
        }

        // Update bomb state
        _currentStats.BombState = gameState.Bomb.State.ToString();

        // Update round timer - CSGSI doesn't provide round time
        _currentStats.RoundTime = 0;

        // Check for round phase changes
        if (_previousGameState != null)
        {
            CheckForRoundPhaseChanges(_previousGameState, gameState);
        }

        _previousGameState = gameState;
    }

    public void OnConnectionLost()
    {
        _currentStats.IsConnected = false;
        _currentStats.ConnectionStatus = "Connection lost - Waiting for reconnection...";
    }

    public void OnConnectionEstablished()
    {
        _currentStats.IsConnected = true;
        _currentStats.ConnectionStatus = "Connected - Waiting for game data...";
    }

    private void UpdatePlayerStats(PlayerNode player)
    {
        _currentStats.PlayerHealth = player.State.Health;
        _currentStats.PlayerArmor = player.State.Armor;
        _currentStats.PlayerMoney = player.State.Money;
        _currentStats.PlayerKills = player.MatchStats.Kills;
        _currentStats.PlayerDeaths = player.MatchStats.Deaths;
        _currentStats.PlayerAssists = player.MatchStats.Assists;
        _currentStats.PlayerMVPs = player.MatchStats.MVPs;
        _currentStats.PlayerScore = player.MatchStats.Score;

        // Update active weapon
        if (player.Weapons?.ActiveWeapon != null)
        {
            _currentStats.ActiveWeapon = player.Weapons.ActiveWeapon.Name;
        }

        // Update team
        _currentStats.PlayerTeam = player.Team.ToString();
    }

    private void CheckForRoundPhaseChanges(GameState previous, GameState current)
    {
        // Check if round just started
        if (previous.Round.Phase == RoundPhase.FreezeTime &&
            current.Round.Phase == RoundPhase.Live)
        {
            OnRoundBegin();
        }

        // Check if round ended
        if (previous.Round.Phase == RoundPhase.Live &&
            current.Round.Phase == RoundPhase.Over)
        {
            OnRoundEnd();
        }
    }

    public void OnRoundBegin()
    {
        _totalRounds++;
        _currentStats.RoundStartTime = DateTime.Now;
    }

    public void OnRoundEnd()
    {
        // Determine round winner based on score changes
        if (_previousGameState != null)
        {
            var scoreDiffT = _currentStats.ScoreT - _previousGameState.Map.TeamT.Score;
            var scoreDiffCT = _currentStats.ScoreCT - _previousGameState.Map.TeamCT.Score;

            if (scoreDiffT > 0)
            {
                _roundsWon = _currentStats.PlayerTeam == "T" ? _roundsWon + 1 : _roundsWon;
                _roundsLost = _currentStats.PlayerTeam == "CT" ? _roundsLost + 1 : _roundsLost;
            }
            else if (scoreDiffCT > 0)
            {
                _roundsWon = _currentStats.PlayerTeam == "CT" ? _roundsWon + 1 : _roundsWon;
                _roundsLost = _currentStats.PlayerTeam == "T" ? _roundsLost + 1 : _roundsLost;
            }
        }
    }

    public void OnBombPlanted()
    {
        _currentStats.BombPlantedTime = DateTime.Now;
    }

    public void OnBombDefused()
    {
        _currentStats.BombDefusedTime = DateTime.Now;
    }

    public void OnBombExploded()
    {
        _currentStats.BombExplodedTime = DateTime.Now;
    }

    public void OnPlayerFlashed(int flashDuration)
    {
        _currentStats.LastFlashDuration = flashDuration;
        _currentStats.LastFlashTime = DateTime.Now;
    }

    public GameStats GetCurrentStats()
    {
        _currentStats.SessionDuration = DateTime.Now - _sessionStartTime;
        _currentStats.TotalRounds = _totalRounds;
        _currentStats.RoundsWon = _roundsWon;
        _currentStats.RoundsLost = _roundsLost;
        _currentStats.WinRate = _totalRounds > 0 ? (double)_roundsWon / _totalRounds * 100 : 0;

        return _currentStats;
    }

    public void ResetSession()
    {
        _currentStats = new GameStats();
        _sessionStartTime = DateTime.Now;
        _totalRounds = 0;
        _roundsWon = 0;
        _roundsLost = 0;
    }
}
