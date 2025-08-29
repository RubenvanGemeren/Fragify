using FragifyTracker.Models;
using CSGSI;

namespace FragifyTracker.Services;

public class TestDataGenerator
{
    private readonly GameTrackerService _trackerService;
    private bool _autoSimulationEnabled = false;
    private DateTime _lastAutoUpdate = DateTime.Now;
    private int _currentRound = 0;
    private readonly Random _random = new Random();

    // Game simulation state
    private bool _gameInProgress = false;
    private int _roundPhase = 0; // 0=freezetime, 1=live, 2=over
    private int _roundTimer = 0;
    private int _maxRounds = 24;
    private int _scoreT = 0;
    private int _scoreCT = 0;

    public bool IsAutoSimulationEnabled => _autoSimulationEnabled;

    public TestDataGenerator(GameTrackerService trackerService)
    {
        _trackerService = trackerService;
    }

    public void Update()
    {
        if (!_autoSimulationEnabled) return;

        var now = DateTime.Now;
        if ((now - _lastAutoUpdate).TotalSeconds >= 3)
        {
            SimulateGameProgression();
            _lastAutoUpdate = now;
        }
    }

    public void SimulateRoundStart()
    {
        if (_currentRound >= _maxRounds)
        {
            Console.WriteLine("Game already completed! Reset session to start new game.");
            return;
        }

        _currentRound++;
        _roundPhase = 0; // Freeze time
        _roundTimer = 0;

        var gameState = CreateTestGameState();
        _trackerService.UpdateGameState(gameState);

        Console.WriteLine($"🎯 Round {_currentRound} started (Freeze Time)");
    }

    public void SimulateBombPlanted()
    {
        if (_roundPhase != 1) return;

        _roundPhase = 1; // Keep live phase
        var gameState = CreateTestGameState();
        gameState.Bomb.State = CSGSI.Nodes.BombState.Planted;
        _trackerService.UpdateGameState(gameState);

        Console.WriteLine("💣 Bomb planted!");
    }

    public void SimulateBombDefused()
    {
        if (_roundPhase != 1) return;

        _scoreCT++;
        _roundPhase = 2; // Round over
        var gameState = CreateTestGameState();
        gameState.Bomb.State = CSGSI.Nodes.BombState.Defused;
        _trackerService.UpdateGameState(gameState);

        Console.WriteLine("✅ Bomb defused! CT wins round!");
    }

    public void SimulateBombExploded()
    {
        if (_roundPhase != 1) return;

        _scoreT++;
        _roundPhase = 2; // Round over
        var gameState = CreateTestGameState();
        gameState.Bomb.State = CSGSI.Nodes.BombState.Exploded;
        _trackerService.UpdateGameState(gameState);

        Console.WriteLine("💥 Bomb exploded! T wins round!");
    }

    public void SimulateRoundEnd()
    {
        if (_roundPhase != 1) return;

        // Randomly determine winner
        if (_random.Next(2) == 0)
        {
            _scoreT++;
            Console.WriteLine("🏆 T wins round!");
        }
        else
        {
            _scoreCT++;
            Console.WriteLine("🏆 CT wins round!");
        }

        _roundPhase = 2; // Round over
        var gameState = CreateTestGameState();
        _trackerService.UpdateGameState(gameState);

        Console.WriteLine($"Round {_currentRound} ended! Score: T {_scoreT} - CT {_scoreCT}");
    }

    public void SimulatePlayerFlash()
    {
        var gameState = CreateTestGameState();
        _trackerService.UpdateGameState(gameState);

        Console.WriteLine("😵 Player flashed!");
    }

    public void GenerateInitialData()
    {
        Console.WriteLine("🎮 Generating initial test data...");

        // Create a realistic initial game state with specific values
        var initialJson = @"{
            ""map"": {
                ""name"": ""de_dust2"",
                ""mode"": ""competitive"",
                ""team_t"": {
                    ""score"": 0
                },
                ""team_ct"": {
                    ""score"": 0
                }
            },
            ""round"": {
                ""phase"": ""freezetime"",
                ""score_t"": 0,
                ""score_ct"": 0
            },
            ""player"": {
                ""steamid"": ""76561198012345678"",
                ""name"": ""FragifyPlayer"",
                ""team"": ""T"",
                ""state"": {
                    ""health"": 100,
                    ""armor"": 100,
                    ""money"": 800,
                    ""round_kills"": 0,
                    ""round_totaldmg"": 0
                },
                ""match_stats"": {
                    ""kills"": 0,
                    ""assists"": 0,
                    ""deaths"": 0,
                    ""mvps"": 0,
                    ""score"": 0
                },
                ""weapons"": {
                    ""active_weapon"": {
                        ""name"": ""weapon_knife"",
                        ""paintkit"": ""default"",
                        ""type"": ""rifle"",
                        ""state"": ""active""
                    }
                }
            },
            ""bomb"": {
                ""state"": ""carried"",
                ""countdown"": ""0.0""
            },
            ""grenades"": {
                ""hegrenade"": {
                    ""owner"": ""76561198012345678"",
                    ""position"": ""0.0, 0.0, 0.0"",
                    ""velocity"": ""0.0, 0.0, 0.0"",
                    ""lifetime"": ""0.0"",
                    ""type"": ""weapon_hegrenade""
                }
            },
            ""phase_countdowns"": {
                ""phase_ends_in"": ""3""
            }
        }";

        try
        {
            Console.WriteLine($"🔍 Creating GameState from JSON...");
            var gameState = new GameState(initialJson);
            Console.WriteLine($"✅ GameState created successfully");
            Console.WriteLine($"   Map: {gameState.Map?.Name ?? "NULL"}");
            Console.WriteLine($"   Player: {gameState.Player?.Name ?? "NULL"}");
            Console.WriteLine($"   Round Phase: {gameState.Round?.Phase.ToString() ?? "NULL"}");

            _trackerService.UpdateGameState(gameState);
            Console.WriteLine("✅ Initial test data generated!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error creating GameState: {ex.Message}");
            Console.WriteLine($"❌ Stack trace: {ex.StackTrace}");
        }
    }

    public void SimulateFullGame()
    {
        Console.WriteLine("🎮 Starting full game simulation...");
        _gameInProgress = true;
        _currentRound = 0;
        _scoreT = 0;
        _scoreCT = 0;

        // Start first round
        SimulateRoundStart();
    }

    public void SimulateNextRound()
    {
        if (_currentRound >= _maxRounds)
        {
            Console.WriteLine("Game completed! All rounds played.");
            return;
        }

        SimulateRoundStart();
    }

    public void ToggleAutoSimulation()
    {
        _autoSimulationEnabled = !_autoSimulationEnabled;
        if (_autoSimulationEnabled)
        {
            Console.WriteLine("🔄 Auto-simulation enabled - will progress through rounds automatically");
        }
        else
        {
            Console.WriteLine("⏸️ Auto-simulation disabled");
        }
    }

    private void SimulateGameProgression()
    {
        if (!_gameInProgress) return;

        switch (_roundPhase)
        {
            case 0: // Freeze time
                _roundTimer++;
                if (_roundTimer >= 3) // 3 seconds freeze time
                {
                    _roundPhase = 1; // Live
                    _roundTimer = 0;
                    Console.WriteLine($"🎯 Round {_currentRound} is now LIVE!");

                    var liveGameState = CreateTestGameState();
                    liveGameState.Round.Phase = CSGSI.Nodes.RoundPhase.Live;
                    _trackerService.UpdateGameState(liveGameState);
                }
                break;

            case 1: // Live
                _roundTimer++;
                if (_roundTimer >= 10) // 10 seconds live
                {
                    // Randomly determine round outcome
                    if (_random.Next(3) == 0)
                    {
                        SimulateBombPlanted();
                        // Wait a bit then explode/defuse
                        Task.Delay(2000).ContinueWith(_ =>
                        {
                            if (_random.Next(2) == 0)
                                SimulateBombExploded();
                            else
                                SimulateBombDefused();
                        });
                    }
                    else
                    {
                        SimulateRoundEnd();
                    }
                }
                break;

            case 2: // Round over
                _roundTimer++;
                if (_roundTimer >= 3) // 3 seconds between rounds
                {
                    if (_currentRound < _maxRounds)
                    {
                        SimulateNextRound();
                    }
                    else
                    {
                        _gameInProgress = false;
                        Console.WriteLine("🏁 Game completed! All rounds played.");
                    }
                }
                break;
        }
    }

    private GameState CreateTestGameState()
    {
        var json = CreateTestGameStateJson();
        return new GameState(json);
    }

    private string CreateTestGameStateJson()
    {
        var roundPhase = _roundPhase switch
        {
            0 => "freezetime",
            1 => "live",
            2 => "over",
            _ => "freezetime"
        };

        var bombState = _roundPhase == 1 && _roundTimer > 5 ? "planted" : "carried";

        return $@"{{
            ""map"": {{
                ""name"": ""{GetRandomMap()}"",
                ""mode"": ""competitive"",
                ""team_t"": {{
                    ""score"": {_scoreT}
                }},
                ""team_ct"": {{
                    ""score"": {_scoreCT}
                }}
            }},
            ""round"": {{
                ""phase"": ""{roundPhase}"",
                ""score_t"": {_scoreT},
                ""score_ct"": {_scoreCT}
            }},
            ""player"": {{
                ""steamid"": ""76561198012345678"",
                ""name"": ""FragifyPlayer"",
                ""team"": ""{(_random.Next(2) == 0 ? "T" : "CT")}"",
                ""state"": {{
                    ""health"": {_random.Next(1, 101)},
                    ""armor"": {_random.Next(0, 101)},
                    ""money"": {_random.Next(800, 16001)},
                    ""round_kills"": {_random.Next(0, 6)},
                    ""round_totaldmg"": {_random.Next(0, 500)}
                }},
                ""match_stats"": {{
                    ""kills"": {_random.Next(0, 31)},
                    ""assists"": {_random.Next(0, 21)},
                    ""deaths"": {_random.Next(0, 31)},
                    ""mvps"": {_random.Next(0, 11)},
                    ""score"": {_random.Next(0, 1001)}
                }},
                ""weapons"": {{
                    ""active_weapon"": {{
                        ""name"": ""{GetRandomWeapon()}"",
                        ""paintkit"": ""default"",
                        ""type"": ""rifle"",
                        ""state"": ""active""
                    }}
                }}
            }},
            ""bomb"": {{
                ""state"": ""{bombState}"",
                ""countdown"": ""0.0""
            }},
            ""grenades"": {{
                ""hegrenade"": {{
                    ""owner"": ""76561198012345678"",
                    ""position"": ""0.0, 0.0, 0.0"",
                    ""velocity"": ""0.0, 0.0, 0.0"",
                    ""lifetime"": ""0.0"",
                    ""type"": ""weapon_hegrenade""
                }}
            }},
            ""phase_countdowns"": {{
                ""phase_ends_in"": ""{(_roundPhase == 0 ? 3 - _roundTimer : _roundPhase == 1 ? 10 - _roundTimer : 0)}""
            }}
        }}";
    }

    private string GetRandomMap()
    {
        var maps = new[] { "de_dust2", "de_mirage", "de_inferno", "de_overpass", "de_nuke", "de_ancient", "de_vertigo" };
        return maps[_random.Next(maps.Length)];
    }

    private string GetRandomWeapon()
    {
        var weapons = new[] { "weapon_ak47", "weapon_m4a1", "weapon_awp", "weapon_deagle", "weapon_usp_silencer", "weapon_glock" };
        return weapons[_random.Next(weapons.Length)];
    }

    public void ResetGame()
    {
        _gameInProgress = false;
        _currentRound = 0;
        _roundPhase = 0;
        _roundTimer = 0;
        _scoreT = 0;
        _scoreCT = 0;
        _trackerService.ResetSession();
        Console.WriteLine("🔄 Game reset - ready for new session");
    }

    public void ShowGameStatus()
    {
        Console.WriteLine($"🎮 Game Status:");
        Console.WriteLine($"   Round: {_currentRound}/{_maxRounds}");
        Console.WriteLine($"   Phase: {(_roundPhase switch { 0 => "Freeze Time", 1 => "Live", 2 => "Round Over", _ => "Unknown" })}");
        Console.WriteLine($"   Score: T {_scoreT} - CT {_scoreCT}");
        Console.WriteLine($"   Timer: {_roundTimer}s");
        Console.WriteLine($"   Auto-sim: {(_autoSimulationEnabled ? "ON" : "OFF")}");
    }
}
