# Fragify Program - Complete Architecture Guide

## Overview
Fragify is a real-time Counter-Strike 2 (CS2) game tracker that monitors your gameplay and displays live statistics. It's built using C# (.NET 9.0) and integrates with CS2's Game State Integration (GSI) system to receive live game data.

**Important Note**: This application is designed to work with **player data only** (not spectator data), making it suitable for competitive gameplay where you're actively playing.

## Table of Contents
1. [Program Architecture](#program-architecture)
2. [Core Components](#core-components)
3. [Data Models](#data-models)
4. [Services Layer](#services-layer)
5. [User Interface Layer](#user-interface-layer)
6. [Data Flow](#data-flow)
7. [Configuration](#configuration)
8. [Testing & Development](#testing--development)
9. [File Structure](#file-structure)
10. [Player vs Spectator Data](#player-vs-spectator-data)

---

## Program Architecture

### High-Level Architecture
```
CS2 Game → GSI Integration → Fragify Listener → Services → UI Display
```

The program follows a **layered architecture** pattern:
- **Entry Point**: `Program.cs` - Main application logic and event handling
- **Data Layer**: `Models/` - Data structures and enums
- **Business Logic**: `Services/` - Core functionality and game tracking
- **Presentation**: `UI/` - User interface implementations
- **Configuration**: External config files for CS2 integration

### Key Design Patterns
- **Event-Driven Architecture**: Uses CS2 GSI events to trigger updates
- **Service-Oriented Design**: Modular services for different functionalities
- **Factory Pattern**: `UIFactory` creates different UI implementations
- **Observer Pattern**: Event handlers respond to game state changes
- **Player Filtering**: Only processes messages from the configured player's Steam ID

---

## Core Components

### 1. Program.cs (Main Entry Point)
**Location**: Root directory
**Purpose**: Application bootstrap, event handling, and main loop

**Key Responsibilities**:
- Parse command-line arguments
- Initialize game tracker and UI
- Set up CS2 GSI event listeners (player data only)
- Handle test mode functionality
- Manage application lifecycle

**Important Methods**:
- `Main()`: Application entry point
- `InitializeGameTracker()`: Sets up game tracking services
- `OnNewGameState()`: Main event handler for game updates
- `RunMainLoop()`: Main application loop

**Event Handlers**:
```csharp
_listener.NewGameState += OnNewGameState;        // Main game updates (player data only)
```

**Note**: Only the main game state event is used, as other events (bomb, round, player events) are not available to players in competitive games.

### 2. Game State Listener
**Library**: `CounterStrike2GSI` (v1.0.3)
**Purpose**: Receives real-time data from CS2

**What It Listens For** (Player-Available Data Only):
- Player position, health, armor, money
- Weapon information and match statistics
- Round phases and team scores
- Map information and game mode

**What It Does NOT Listen For** (Spectator-Only Data):
- Bomb state and countdowns
- All players' information
- Grenade positions and states
- Phase countdowns
- Tournament draft information

---

## Data Models

### 1. GameStats.cs
**Location**: `Models/GameStats.cs`
**Purpose**: Central data structure for all game information

**Key Properties**:
```csharp
// Game Information
public string MapName { get; set; }
public string GameMode { get; set; }
public int RoundNumber { get; set; }
public int ScoreT { get; set; }        // Terrorist score
public int ScoreCT { get; set; }       // Counter-Terrorist score

// Player Information
public int PlayerHealth { get; set; }
public int PlayerArmor { get; set; }
public int PlayerMoney { get; set; }
public int PlayerKills { get; set; }
public int PlayerDeaths { get; set; }
public double PlayerKDRatio => PlayerDeaths > 0 ? (double)PlayerKills / PlayerDeaths : PlayerKills;

// Session Information
public TimeSpan SessionDuration { get; set; }
public int TotalRounds { get; set; }
public double WinRate { get; set; }

// Bomb Information (Limited)
public string BombState { get; set; } = "Unknown"; // Not available to players
```

**Computed Properties**:
- `IsAlive`: Checks if player health > 0
- `HasArmor`: Checks if player has armor
- `IsBombPlanted`: Always false (bomb state not available to players)

### 2. GameStateInfo.cs
**Location**: `Models/GameStateInfo.cs`
**Purpose**: Visual and UI state information

**Key Properties**:
```csharp
public string CurrentPhase { get; set; }        // Warmup, Live, Over, etc.
public string BombState { get; set; } = "N/A"; // Not available to players
public string BorderColor { get; set; }         // UI theme color
public bool ShowBombTimer { get; set; } = false; // Always false
```

**Enums**:
- `GamePhase`: Game state phases
- `BombState`: Bomb status (not used in player mode)
- `RoundResult`: Round outcomes

---

## Services Layer

### 1. GameTrackerService.cs
**Location**: `Services/GameTrackerService.cs`
**Purpose**: Core business logic for game tracking

**Key Responsibilities**:
- Process incoming game state data
- Filter messages by configured Steam ID
- Update player statistics
- Manage game sessions
- Handle round phase changes

**Main Methods**:
```csharp
public void UpdateGameState(GameState gameState)    // Process CS2 data
public void OnRoundStarted()                       // Handle round events
public GameStats GetCurrentStats()                 // Get current statistics
```

**Player Filtering**:
```csharp
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
```

### 2. SessionDataManager.cs
**Location**: `Services/SessionDataManager.cs`
**Purpose**: Manage game session data and persistence

**Features**:
- Track multiple game sessions
- Save session data to JSON files
- Calculate session statistics
- Manage data retention

### 3. GameSessionService.cs
**Location**: `Services/GameSessionService.cs`
**Purpose**: Handle individual game sessions

**Capabilities**:
- Start new game sessions
- Track round-by-round performance
- Calculate win rates and statistics
- Manage player identification

### 4. PlayerConfigService.cs
**Location**: `Services/PlayerConfigService.cs`
**Purpose**: Manage player configuration and Steam ID filtering

**Key Methods**:
```csharp
public string GetSteamId()                    // Get configured Steam ID
public void SetSteamId(string steamId)        // Set Steam ID
public bool GetAutoDetectSteamId()            // Get auto-detection setting
```

**Configuration File**: `config/player.json`
```json
{
  "SteamId": "76561198126180159",
  "PlayerName": "Chungle Ruby",
  "AutoDetectSteamId": true,
  "Theme": "Dark",
  "Language": "en"
}
```

---

## Player vs Spectator Data

### Available to Players (What We Use)
- **Map Information**: Name, mode, team scores
- **Round Information**: Phase, round number
- **Player Information**: Health, armor, money, weapons, match stats
- **Basic Game State**: Current phase, round status

### Not Available to Players (What We Don't Use)
- **Bomb State**: Planted, defused, exploded, countdown
- **All Players Data**: Other players' positions, stats, weapons
- **Grenade Information**: Positions, velocities, lifetimes
- **Phase Countdowns**: Exact timing for round phases
- **Tournament Draft**: Competitive draft information

### Why This Matters
In competitive CS2 games, players only have access to their own information and basic game state. Spectator-only features like bomb state tracking and all players' data are not available, so the application focuses on what players can actually see and track.

---

## User Interface Layer

### 1. UIFactory.cs
**Location**: `UI/UIFactory.cs`
**Purpose**: Factory for creating different UI implementations

**Supported UI Types**:
- `cli` (Console): Command-line interface using Spectre.Console
- `web` (Browser): Web-based dashboard

**Usage**:
```bash
dotnet run -- --ui cli          # Command-line interface
dotnet run -- --ui web          # Web interface
dotnet run -- --ui web --web-port 8080  # Custom port
```

### 2. CliInterface.cs
**Location**: `UI/CliInterface.cs`
**Purpose**: Rich terminal interface using Spectre.Console

**Features**:
- Colorful, formatted output
- Progress bars for health/armor
- Real-time statistics display
- Event notifications (round changes only)

### 3. WebInterface.cs
**Location**: `UI/WebInterface.cs`
**Purpose**: Web-based dashboard interface

**Features**:
- HTML/CSS/JavaScript dashboard
- Real-time updates via HTTP
- Responsive design
- Interactive elements

---

## Data Flow

### 1. Data Reception Flow
```
CS2 Game → GSI Config → HTTP POST → GameStateListener → Steam ID Filter → Event Handlers
```

1. **CS2 sends data** to configured endpoint (localhost:3000)
2. **GameStateListener** receives HTTP POST requests
3. **Steam ID filter** checks if message is from configured player
4. **Event handlers** process player data only
5. **GameTrackerService** updates internal state
6. **UI components** refresh with new data

### 2. Event Processing Flow
```
Game Event → Steam ID Check → Service Method → State Update → UI Refresh
```

**Example - Round Phase Change**:
1. CS2 sends round phase update
2. Steam ID filter validates sender
3. `GameTrackerService.UpdateGameState()` called
4. Game state updated with round information
5. UI refreshes to show new round phase

### 3. UI Update Flow
```
State Change → Service Update → UI Interface → Display Refresh
```

1. Game state changes in service
2. UI interface calls `UpdateDisplay()`
3. Display refreshes with new data
4. User sees updated information

---

## Configuration

### 1. CS2 GSI Configuration
**File**: `gamestate_integration_fragify.cfg`
**Purpose**: Tell CS2 where to send game data

**Key Settings**:
```json
"uri" "http://localhost:3000"           // Where to send data
"timeout" "5.0"                         // Connection timeout
"token" "FragifyTracker"                // Authentication token
```

**Data Streams Enabled** (Player-Available Only):
- `"map" "1"`                           // Map information
- `"round" "1"`                         // Round data
- `"player_id" "1"`                     // Player identification
- `"player_weapons" "1"`                // Player weapons
- `"player_match_stats" "1"`            // Player statistics
- `"player_state" "1"`                  // Player status
- `"player_position" "1"`               // Player position
- `"player_forward" "1"`                // Player direction

**Data Streams Disabled** (Spectator-Only):
- `"allplayers_*"`                      // Other players' data
- `"bomb"`                              // Bomb state
- `"grenades"`                          // Grenade information
- `"phase_countdowns"`                  // Phase timing
- `"tournament_draft"`                  // Tournament info

### 2. Application Configuration
**Command Line Options**:
```bash
--ui <type>           # UI type (cli/web)
--web-port <port>     # Web interface port
--test                # Enable test mode
--help                # Show usage information
```

### 3. Player Configuration
**File**: `config/player.json`
```json
{
  "SteamId": "YOUR_STEAM_ID_HERE",
  "PlayerName": "Your Player Name",
  "AutoDetectSteamId": true,
  "Theme": "Dark",
  "Language": "en"
}
```

**Steam ID Format**: 17-digit Steam ID (e.g., "76561198126180159")

---

## Testing & Development

### 1. Test Mode
**Enable with**: `dotnet run -- --test`

**Test Commands** (Updated for Player-Only Data):
- `1` - Start Round
- `2` - End Round
- `3` - Flash Player
- `4` - Toggle Auto-simulation
- `5` - Full Game Simulation
- `6` - Next Round
- `7` - Game Status
- `0` - Reset Game
- `r` - Reset Session

**Features**:
- Simulates real game events (player-available only)
- Generates test data without bomb state
- Auto-simulation mode
- Debug information display

**Removed Commands**:
- Plant Bomb (not available to players)
- Defuse Bomb (not available to players)

### 2. Development Features
- **Error Handling**: Comprehensive exception handling
- **Logging**: Debug output for troubleshooting
- **Connection Monitoring**: Tracks connection status
- **Steam ID Filtering**: Only processes configured player's data
- **Graceful Shutdown**: Proper cleanup on exit

---

## File Structure

### Root Directory
```
Fragify/
├── Program.cs                              # Main entry point
├── FragifyTracker.csproj                   # Project file
├── gamestate_integration_fragify.cfg       # CS2 GSI config (player-only)
├── build.bat                               # Build script
└── README.md                               # Project documentation
```

### Models Directory
```
Models/
├── GameStateInfo.cs                        # UI state information
├── GameStats.cs                            # Main game statistics
└── MapInfo.cs                              # Map-related data
```

### Services Directory
```
Services/
├── GameTrackerService.cs                   # Core tracking logic (player-filtered)
├── GameSessionService.cs                   # Session management
├── SessionDataManager.cs                   # Data persistence
├── MapThemeService.cs                      # Map theming
├── WeaponImageService.cs                   # Weapon handling
├── PlayerConfigService.cs                  # Player configuration & Steam ID filtering
├── MinimapImageService.cs                  # Minimap handling
├── WebMapThemeService.cs                   # Web map themes
└── TestDataGenerator.cs                    # Test data generation (player-only)
```

### UI Directory
```
UI/
├── UIFactory.cs                            # UI creation factory
├── IUserInterface.cs                       # UI interface contract
├── CliInterface.cs                         # Command-line interface
└── WebInterface.cs                         # Web dashboard
```

### Web Assets
```
wwwroot/
├── index.html                              # Main dashboard
├── css/
│   ├── dashboard.css                       # Dashboard styling
│   └── test.css                           # Test page styling
└── js/
    ├── dashboard.js                        # Dashboard logic
    └── test.js                            # Test page logic
```

---

## How Everything Works Together

### 1. Startup Sequence
1. **Program.cs** parses command-line arguments
2. **UIFactory** creates appropriate UI interface
3. **GameTrackerService** initializes tracking services
4. **PlayerConfigService** loads configured Steam ID
5. **GameStateListener** starts listening on port 3000
6. **Event handlers** are registered for CS2 events
7. **Main loop** begins running

### 2. Runtime Operation
1. **CS2 sends data** to localhost:3000
2. **Steam ID filter** validates sender is configured player
3. **Event handlers** process player data only
4. **Services** update internal state and statistics
5. **UI interfaces** refresh with new information
6. **User sees** real-time game updates

### 3. Data Persistence
1. **Session data** saved to JSON files
2. **Game statistics** tracked across sessions
3. **Player performance** history maintained
4. **Map-specific data** stored and retrieved

---

## Key Technologies Used

### 1. .NET 9.0
- Modern C# features
- High-performance runtime
- Cross-platform compatibility

### 2. CounterStrike2GSI
- Official CS2 integration library
- Real-time game state data
- Event-driven architecture
- **Player data focus** (not spectator)

### 3. Spectre.Console
- Beautiful terminal output
- Rich formatting and colors
- Progress bars and tables

### 4. ASP.NET Core
- Web interface hosting
- HTTP endpoint management
- Static file serving

---

## Common Use Cases

### 1. Live Game Tracking
- Monitor your performance in real-time
- Track round-by-round statistics
- Get notified of round phase changes
- **Note**: No bomb state tracking (not available to players)

### 2. Performance Analysis
- Review session statistics
- Analyze win rates and K/D ratios
- Track improvement over time
- Focus on player-available metrics

### 3. Game Development
- Test game integration
- Debug GSI data
- Develop custom features
- Understand player vs spectator data limitations

---

## Troubleshooting

### 1. No Data Received
- Check CS2 config file location
- Verify port 3000 is available
- Restart CS2 after config changes
- Ensure Steam ID in config matches your Steam ID

### 2. Permission Issues
- Run as administrator
- Use localhost instead of external IPs
- Check firewall settings

### 3. Steam ID Filtering Issues
- Verify Steam ID in `config/player.json`
- Check Steam ID format (17 digits)
- Ensure `AutoDetectSteamId` is set correctly

### 4. Build Issues
- Ensure .NET 9.0 SDK installed
- Run `dotnet restore` before building
- Check for missing dependencies

---

## Important Limitations

### 1. Player Data Only
- **Bomb state tracking**: Not available to players
- **All players data**: Not available to players
- **Grenade information**: Not available to players
- **Phase countdowns**: Not available to players

### 2. Competitive Game Focus
- Designed for active gameplay, not spectating
- Focuses on personal performance metrics
- Round tracking based on phase changes
- Team score tracking from map data

### 3. Steam ID Requirement
- Must configure your Steam ID in `config/player.json`
- Only processes messages from configured player
- Prevents data from other players from interfering

---

This guide covers all the major components and how they work together in your Fragify program. The architecture is designed to be modular, maintainable, and focused on player-available data for competitive CS2 gameplay.
