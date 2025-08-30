using System.Text.Json;

namespace FragifyTracker.Services;

public class MinimapImageService
{
    private readonly Dictionary<string, List<string>> _minimapUrls;
    private readonly string _minimapDataPath = "Data/minimap_urls.json";

    public MinimapImageService()
    {
        _minimapUrls = new Dictionary<string, List<string>>();
        LoadMinimapUrls();
    }

    private void LoadMinimapUrls()
    {
        try
        {
            // Create Data directory if it doesn't exist
            var dataDir = Path.GetDirectoryName(_minimapDataPath);
            if (!Directory.Exists(dataDir))
            {
                Directory.CreateDirectory(dataDir!);
            }

            // If minimap_urls.json doesn't exist, create default URLs
            if (!File.Exists(_minimapDataPath))
            {
                CreateDefaultMinimapUrls();
            }

            var jsonContent = File.ReadAllText(_minimapDataPath);
            var urls = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(jsonContent);

            if (urls != null)
            {
                foreach (var kvp in urls)
                {
                    _minimapUrls[kvp.Key.ToLower()] = kvp.Value;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading minimap URLs: {ex.Message}");
            CreateDefaultMinimapUrls();
        }
    }

    private void CreateDefaultMinimapUrls()
    {
        var defaultUrls = new Dictionary<string, List<string>>
        {
            ["de_dust2"] = new List<string>
            {
                "https://raw.githubusercontent.com/SteamDatabase/GameTracking-CSGO/master/game/csgo/resource/overviews/de_dust2_radar.png",
                "https://cdn.cloudflare.steamstatic.com/apps/730/icons/econ/default_generated/weapon_knife_t_karambit_gs_radar_1_light_large.2ed1a956da341829f8f71ff6d147c0ad7ddf4d55.png",
                "https://steamcdn-a.akamaihd.net/apps/730/icons/econ/default_generated/weapon_knife_t_karambit_gs_radar_1_light_large.2ed1a956da341829f8f71ff6d147c0ad7ddf4d55.png"
            },
            ["de_mirage"] = new List<string>
            {
                "https://raw.githubusercontent.com/SteamDatabase/GameTracking-CSGO/master/game/csgo/resource/overviews/de_mirage_radar.png",
                "https://cdn.cloudflare.steamstatic.com/apps/730/icons/econ/default_generated/weapon_knife_t_karambit_gs_radar_1_light_large.2ed1a956da341829f8f71ff6d147c0ad7ddf4d55.png"
            },
            ["de_inferno"] = new List<string>
            {
                "https://raw.githubusercontent.com/SteamDatabase/GameTracking-CSGO/master/game/csgo/resource/overviews/de_inferno_radar.png",
                "https://cdn.cloudflare.steamstatic.com/apps/730/icons/econ/default_generated/weapon_knife_t_karambit_gs_radar_1_light_large.2ed1a956da341829f8f71ff6d147c0ad7ddf4d55.png"
            },
            ["de_cache"] = new List<string>
            {
                "https://raw.githubusercontent.com/SteamDatabase/GameTracking-CSGO/master/game/csgo/resource/overviews/de_cache_radar.png",
                "https://cdn.cloudflare.steamstatic.com/apps/730/icons/econ/default_generated/weapon_knife_t_karambit_gs_radar_1_light_large.2ed1a956da341829f8f71ff6d147c0ad7ddf4d55.png"
            },
            ["de_overpass"] = new List<string>
            {
                "https://raw.githubusercontent.com/SteamDatabase/GameTracking-CSGO/master/game/csgo/resource/overviews/de_overpass_radar.png",
                "https://cdn.cloudflare.steamstatic.com/apps/730/icons/econ/default_generated/weapon_knife_t_karambit_gs_radar_1_light_large.2ed1a956da341829f8f71ff6d147c0ad7ddf4d55.png"
            },
            ["de_nuke"] = new List<string>
            {
                "https://raw.githubusercontent.com/SteamDatabase/GameTracking-CSGO/master/game/csgo/resource/overviews/de_nuke_radar.png",
                "https://cdn.cloudflare.steamstatic.com/apps/730/icons/econ/default_generated/weapon_knife_t_karambit_gs_radar_1_light_large.2ed1a956da341829f8f71ff6d147c0ad7ddf4d55.png"
            },
            ["de_ancient"] = new List<string>
            {
                "https://raw.githubusercontent.com/SteamDatabase/GameTracking-CSGO/master/game/csgo/resource/overviews/de_ancient_radar.png",
                "https://cdn.cloudflare.steamstatic.com/apps/730/icons/econ/default_generated/weapon_knife_t_karambit_gs_radar_1_light_large.2ed1a956da341829f8f71ff6d147c0ad7ddf4d55.png"
            },
            ["de_vertigo"] = new List<string>
            {
                "https://raw.githubusercontent.com/SteamDatabase/GameTracking-CSGO/master/game/csgo/resource/overviews/de_vertigo_radar.png",
                "https://cdn.cloudflare.steamstatic.com/apps/730/icons/econ/default_generated/weapon_knife_t_karambit_gs_radar_1_light_large.2ed1a956da341829f8f71ff6d147c0ad7ddf4d55.png"
            }
        };

        try
        {
            var jsonContent = JsonSerializer.Serialize(defaultUrls, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_minimapDataPath, jsonContent);

            foreach (var kvp in defaultUrls)
            {
                _minimapUrls[kvp.Key.ToLower()] = kvp.Value;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating default minimap URLs: {ex.Message}");
        }
    }

    public List<string> GetMinimapUrls(string mapName)
    {
        if (string.IsNullOrEmpty(mapName))
            return new List<string>();

        var normalizedName = mapName.ToLower();
        return _minimapUrls.TryGetValue(normalizedName, out var urls) ? urls : new List<string>();
    }

    public string? GetPrimaryMinimapUrl(string mapName)
    {
        var urls = GetMinimapUrls(mapName);
        return urls.Count > 0 ? urls[0] : null;
    }

    public void AddMinimapUrl(string mapName, string url)
    {
        if (string.IsNullOrEmpty(mapName) || string.IsNullOrEmpty(url))
            return;

        var normalizedName = mapName.ToLower();
        if (!_minimapUrls.ContainsKey(normalizedName))
        {
            _minimapUrls[normalizedName] = new List<string>();
        }

        if (!_minimapUrls[normalizedName].Contains(url))
        {
            _minimapUrls[normalizedName].Add(url);
            SaveMinimapUrls();
        }
    }

    private void SaveMinimapUrls()
    {
        try
        {
            var urls = _minimapUrls.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            var jsonContent = JsonSerializer.Serialize(urls, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_minimapDataPath, jsonContent);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving minimap URLs: {ex.Message}");
        }
    }
}
