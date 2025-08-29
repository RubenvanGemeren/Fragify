using System.Text.Json;

namespace FragifyTracker.Services;

public class PlayerConfigService
{
    private readonly string _configPath = "config/player.json";
    private PlayerConfig _config;

    public PlayerConfigService()
    {
        _config = LoadConfig();
    }

    private PlayerConfig LoadConfig()
    {
        try
        {
            // Create config directory if it doesn't exist
            var configDir = Path.GetDirectoryName(_configPath);
            if (!Directory.Exists(configDir))
            {
                Directory.CreateDirectory(configDir!);
            }

            // If config doesn't exist, create default
            if (!File.Exists(_configPath))
            {
                var defaultConfig = new PlayerConfig
                {
                    SteamId = "",
                    PlayerName = "FragifyPlayer",
                    AutoDetectSteamId = true,
                    Theme = "Dark",
                    Language = "en"
                };

                SaveConfig(defaultConfig);
                return defaultConfig;
            }

            var jsonContent = File.ReadAllText(_configPath);
            var config = JsonSerializer.Deserialize<PlayerConfig>(jsonContent);
            return config ?? new PlayerConfig();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading player config: {ex.Message}");
            return new PlayerConfig();
        }
    }

    private void SaveConfig(PlayerConfig config)
    {
        try
        {
            var jsonContent = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_configPath, jsonContent);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving player config: {ex.Message}");
        }
    }

    public string GetSteamId()
    {
        return _config.SteamId;
    }

    public void SetSteamId(string steamId)
    {
        _config.SteamId = steamId;
        SaveConfig(_config);
    }

    public string GetPlayerName()
    {
        return _config.PlayerName;
    }

    public void SetPlayerName(string playerName)
    {
        _config.PlayerName = playerName;
        SaveConfig(_config);
    }

    public bool GetAutoDetectSteamId()
    {
        return _config.AutoDetectSteamId;
    }

    public void SetAutoDetectSteamId(bool autoDetect)
    {
        _config.AutoDetectSteamId = autoDetect;
        SaveConfig(_config);
    }

    public string GetTheme()
    {
        return _config.Theme;
    }

    public void SetTheme(string theme)
    {
        _config.Theme = theme;
        SaveConfig(_config);
    }

    public PlayerConfig GetFullConfig()
    {
        return _config;
    }

    public void UpdateConfig(PlayerConfig newConfig)
    {
        _config = newConfig;
        SaveConfig(_config);
    }
}

public class PlayerConfig
{
    public string SteamId { get; set; } = string.Empty;
    public string PlayerName { get; set; } = "FragifyPlayer";
    public bool AutoDetectSteamId { get; set; } = true;
    public string Theme { get; set; } = "Dark";
    public string Language { get; set; } = "en";
}
