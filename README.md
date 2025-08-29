# Fragify - CS:GO Live Game Tracker

A beautiful command-line application that provides real-time statistics and game state information while playing Counter-Strike: Global Offensive. Built with C# and the CSGSI library for Game State Integration.

## Features

- **Real-time Game Tracking**: Live updates of game state, player statistics, and round information
- **Beautiful CLI Interface**: Rich, colorful terminal interface using Spectre.Console
- **Comprehensive Statistics**: Track kills, deaths, assists, MVPs, money, health, armor, and more
- **Round Analysis**: Monitor round phases, bomb states, and team scores
- **Session Tracking**: Track your performance across multiple rounds and matches
- **Event Notifications**: Get notified of important game events (bomb planted, round end, etc.)

## Screenshots

The application displays:
- Game information (map, mode, round, phase, score)
- Player statistics (K/D/A, MVPs, money, weapon, team)
- Session statistics (duration, rounds, win rate)
- Bomb information (state, timestamps)
- Progress bars for health, armor, and round timer

## Prerequisites

- .NET 8.0 SDK or Runtime
- Counter-Strike: Global Offensive
- Administrator privileges (if using non-localhost URIs)

## Installation

1. **Clone the repository**:
   ```bash
   git clone <your-repo-url>
   cd FragifyTracker
   ```

2. **Restore dependencies**:
   ```bash
   dotnet restore
   ```

3. **Build the application**:
   ```bash
   dotnet build
   ```

## Setup

### 1. CS:GO Configuration

1. Copy the `gamestate_integration_fragify.cfg` file to your CS:GO cfg folder:
   - **Windows**: `C:\Program Files (x86)\Steam\steamapps\common\Counter-Strike Global Offensive\csgo\cfg\`
   - **Linux**: `~/.steam/steam/steamapps/common/Counter-Strike Global Offensive/csgo/cfg/`

2. Restart CS:GO or execute the config in console:
   ```
   exec gamestate_integration_fragify
   ```

### 2. Run the Application

```bash
dotnet run
```

Or specify a custom port:
```bash
dotnet run -- --port 3001
```

## Usage

1. **Start the application** before or during a CS:GO match
2. **Join a game** - the tracker will automatically detect and start monitoring
3. **View real-time statistics** in the beautiful CLI interface
4. **Monitor important events** like bomb plants, round ends, and player flashes
5. **Track your performance** across multiple rounds and matches

## Command Line Options

- `--port <port>` or `-p <port>`: Specify the port to listen on (default: 3000)

## Architecture

The application is built with a clean, modular architecture:

- **Program.cs**: Main entry point and event handling
- **Services/GameTrackerService.cs**: Core game state tracking logic
- **Models/GameStats.cs**: Data model for game statistics
- **UI/DisplayManager.cs**: Rich terminal interface using Spectre.Console

## Dependencies

- **CSGSI**: Counter-Strike Game State Integration library
- **Spectre.Console**: Beautiful command-line interface library
- **Newtonsoft.Json**: JSON parsing and manipulation

## Troubleshooting

### Common Issues

1. **"Failed to start listener" error**:
   - Ensure you have administrator privileges
   - Check if the port is already in use
   - Try a different port number

2. **No data received**:
   - Verify the config file is in the correct CS:GO cfg folder
   - Restart CS:GO after adding the config
   - Check if the URI in the config matches your application port

3. **Permission denied**:
   - Run the application as administrator
   - Use `http://localhost:<port>` instead of external IPs

### Port Configuration

If you need to use a different port, update both:
1. The command line argument when running the application
2. The `gamestate_integration_fragify.cfg` file

## Development

### Building from Source

```bash
dotnet build --configuration Release
```

### Running Tests

```bash
dotnet test
```

### Code Style

The project follows C# coding conventions and uses nullable reference types for better type safety.

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests if applicable
5. Submit a pull request

## License

This project is licensed under the same license as the original Fragify project.

## Acknowledgments

- [CSGSI](https://github.com/rakijah/CSGSI) - The excellent C# library for CS:GO Game State Integration
- [Spectre.Console](https://spectreconsole.net/) - Beautiful .NET console library
- Valve Corporation - For creating the Game State Integration system

## Support

If you encounter any issues or have questions:
1. Check the troubleshooting section above
2. Review the CSGSI documentation
3. Open an issue on the GitHub repository

---

**Happy fragging! 🎯**