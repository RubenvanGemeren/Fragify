using CSGSI;
using CSGSI.Nodes;
using FragifyTracker.Services;

namespace FragifyTracker.Services;

public class TestDataGenerator
{
    private readonly GameTrackerService _trackerService;
    private bool _autoSimulationEnabled = true;
    private DateTime _lastAutoUpdate = DateTime.Now;
    private int _simulationStep = 0;
    private readonly Random _random = new Random();

    public bool IsAutoSimulationEnabled => _autoSimulationEnabled;

    public TestDataGenerator(GameTrackerService trackerService)
    {
        _trackerService = trackerService;
    }

    public void Update()
    {
        if (!_autoSimulationEnabled) return;

        var timeSinceLastUpdate = DateTime.Now - _lastAutoUpdate;
        if (timeSinceLastUpdate.TotalSeconds >= 3) // Update every 3 seconds
        {
            SimulateGameStateUpdate();
            _lastAutoUpdate = DateTime.Now;
        }
    }

    public void SimulateRoundStart()
    {
        var gameState = CreateTestGameState();
        gameState.Round.Phase = RoundPhase.Live;

        _trackerService.UpdateGameState(gameState);
        _trackerService.OnRoundBegin();

        _simulationStep = 1;
    }

    public void SimulateBombPlanted()
    {
        var gameState = CreateTestGameState();
        gameState.Round.Phase = RoundPhase.Live;
        gameState.Bomb.State = BombState.Planted;

        _trackerService.UpdateGameState(gameState);
        _trackerService.OnBombPlanted();

        _simulationStep = 2;
    }

    public void SimulateBombDefused()
    {
        var gameState = CreateTestGameState();
        gameState.Round.Phase = RoundPhase.Live;
        gameState.Bomb.State = BombState.Defused;

        _trackerService.UpdateGameState(gameState);
        _trackerService.OnBombDefused();

        _simulationStep = 3;
    }

    public void SimulateRoundEnd()
    {
        var gameState = CreateTestGameState();
        gameState.Round.Phase = RoundPhase.Over;

        // Simulate score changes
        if (_random.Next(2) == 0)
        {
            gameState.Map.TeamT.Score++;
        }
        else
        {
            gameState.Map.TeamCT.Score++;
        }

        _trackerService.UpdateGameState(gameState);
        _trackerService.OnRoundEnd();

        _simulationStep = 0;
    }

    public void SimulatePlayerFlash()
    {
        var gameState = CreateTestGameState();
        gameState.Round.Phase = RoundPhase.Live;

        _trackerService.UpdateGameState(gameState);
        _trackerService.OnPlayerFlashed(_random.Next(50, 200));

        _simulationStep = 4;
    }

    public void ToggleAutoSimulation()
    {
        _autoSimulationEnabled = !_autoSimulationEnabled;
    }

    private void SimulateGameStateUpdate()
    {
        var gameState = CreateTestGameState();

        // Simulate different game phases
        switch (_simulationStep)
        {
            case 0: // Round start
                gameState.Round.Phase = RoundPhase.FreezeTime;
                break;
            case 1: // Round live
                gameState.Round.Phase = RoundPhase.Live;
                // Simulate player actions
                SimulatePlayerActions(gameState);
                break;
            case 2: // Bomb planted
                gameState.Round.Phase = RoundPhase.Live;
                gameState.Bomb.State = BombState.Planted;
                break;
            case 3: // Bomb defused
                gameState.Round.Phase = RoundPhase.Live;
                gameState.Bomb.State = BombState.Defused;
                break;
            case 4: // Round end
                gameState.Round.Phase = RoundPhase.Over;
                break;
        }

        _trackerService.UpdateGameState(gameState);

        // Auto-advance simulation
        if (_autoSimulationEnabled)
        {
            _simulationStep = (_simulationStep + 1) % 5;
        }
    }

    private void SimulatePlayerActions(GameState gameState)
    {
        if (gameState.Player != null)
        {
            // Simulate health changes
            if (_random.Next(100) < 30) // 30% chance to take damage
            {
                gameState.Player.State.Health = Math.Max(0, gameState.Player.State.Health - _random.Next(10, 50));
            }

            // Simulate money changes
            if (_random.Next(100) < 20) // 20% chance to get money
            {
                gameState.Player.State.Money += _random.Next(100, 500);
            }

            // Simulate kills/deaths
            if (_random.Next(100) < 10) // 10% chance to get a kill
            {
                gameState.Player.MatchStats.Kills++;
                gameState.Player.MatchStats.Score += 100;
            }

            if (_random.Next(100) < 5) // 5% chance to die
            {
                gameState.Player.State.Health = 0;
                gameState.Player.MatchStats.Deaths++;
            }
        }
    }

    private GameState CreateTestGameState()
    {
        // Create a minimal test game state using JSON string
        var jsonData = CreateTestGameStateJson();
        var gameState = new GameState(jsonData);

        return gameState;
    }

    private string CreateTestGameStateJson()
    {
        var mapName = GetRandomMap();
        var weaponName = GetRandomWeapon();
        var health = _random.Next(0, 101);
        var armor = _random.Next(0, 101);
        var money = _random.Next(0, 16001);
        var kills = _random.Next(0, 30);
        var deaths = _random.Next(0, 20);
        var assists = _random.Next(0, 15);
        var mvps = _random.Next(0, 10);
        var score = kills * 100 + assists * 50;
        var teamTScore = _random.Next(0, 16);
        var teamCTScore = _random.Next(0, 16);
        var roundPhase = GetRoundPhaseString(_simulationStep);
        var bombState = GetBombStateString(_simulationStep);
        var playerTeam = _random.Next(2) == 0 ? "T" : "CT";

        return $@"{{
            ""map"": {{
                ""name"": ""{mapName}"",
                ""mode"": ""Competitive"",
                ""team_t"": {{
                    ""score"": {teamTScore}
                }},
                ""team_ct"": {{
                    ""score"": {teamCTScore}
                }}
            }},
            ""round"": {{
                ""phase"": ""{roundPhase}""
            }},
            ""player"": {{
                ""state"": {{
                    ""health"": {health},
                    ""armor"": {armor},
                    ""money"": {money}
                }},
                ""match_stats"": {{
                    ""kills"": {kills},
                    ""deaths"": {deaths},
                    ""assists"": {assists},
                    ""mvps"": {mvps},
                    ""score"": {score}
                }},
                ""team"": ""{playerTeam}"",
                ""weapons"": {{
                    ""active_weapon"": {{
                        ""name"": ""{weaponName}""
                    }}
                }}
            }},
            ""bomb"": {{
                ""state"": ""{bombState}""
            }}
        }}";
    }

    private string GetRoundPhaseString(int step)
    {
        return step switch
        {
            0 => "FreezeTime",
            1 => "Live",
            2 => "Live",
            3 => "Live",
            4 => "Over",
            _ => "Live"
        };
    }

    private string GetBombStateString(int step)
    {
        return step switch
        {
            2 => "Planted",
            3 => "Defused",
            _ => "Carried"
        };
    }

    private string GetRandomMap()
    {
        var maps = new[] { "de_dust2", "de_mirage", "de_inferno", "de_overpass", "de_nuke", "de_ancient", "de_vertigo" };
        return maps[_random.Next(maps.Length)];
    }

    private string GetRandomWeapon()
    {
        var weapons = new[] {
            "weapon_ak47", "weapon_m4a1", "weapon_awp", "weapon_deagle", "weapon_usp_silencer",
            "weapon_glock", "weapon_p250", "weapon_famas", "weapon_galilar", "weapon_knife"
        };
        return weapons[_random.Next(weapons.Length)];
    }
}
