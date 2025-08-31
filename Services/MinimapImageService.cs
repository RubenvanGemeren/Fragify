using System.Text.Json;
using FragifyTracker.Models;

namespace FragifyTracker.Services;

public class MinimapImageService
{
    private readonly Dictionary<string, List<string>> _minimapUrls;
    private readonly string _minimapDataPath = "Data/comprehensive_maps.json";

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

            // If comprehensive_maps.json doesn't exist, create default URLs
            if (!File.Exists(_minimapDataPath))
            {
                CreateDefaultMinimapUrls();
            }

            var jsonContent = File.ReadAllText(_minimapDataPath);
            var mapsData = JsonSerializer.Deserialize<Dictionary<string, ComprehensiveMapData>>(jsonContent);

            if (mapsData != null)
            {
                foreach (var kvp in mapsData)
                {
                    var mapData = kvp.Value;
                    var urls = new List<string>
                    {
                        mapData.imageUrl,
                        mapData.fallbackUrl
                    };

                    _minimapUrls[kvp.Key.ToLower()] = urls;
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
                "https://totalcsgo.com/maps/de_dust2.png",
                "https://totalcsgo.com/maps/de_dust2.png"
            },
            ["de_mirage"] = new List<string>
            {
                "https://totalcsgo.com/maps/de_mirage.png",
                "https://totalcsgo.com/maps/de_mirage.png"
            },
            ["de_inferno"] = new List<string>
            {
                "https://totalcsgo.com/maps/de_inferno.png",
                "https://totalcsgo.com/maps/de_inferno.png"
            },
            ["de_cache"] = new List<string>
            {
                "https://totalcsgo.com/maps/de_cache.png",
                "https://totalcsgo.com/maps/de_cache.png"
            },
            ["de_overpass"] = new List<string>
            {
                "https://totalcsgo.com/maps/de_overpass.png",
                "https://totalcsgo.com/maps/de_overpass.png"
            },
            ["de_nuke"] = new List<string>
            {
                "https://totalcsgo.com/maps/de_nuke.png",
                "https://totalcsgo.com/maps/de_nuke.png"
            },
            ["de_ancient"] = new List<string>
            {
                "https://totalcsgo.com/maps/de_ancient.png",
                "https://totalcsgo.com/maps/de_ancient.png"
            },
            ["de_vertigo"] = new List<string>
            {
                "https://totalcsgo.com/maps/de_vertigo.png",
                "https://totalcsgo.com/maps/de_vertigo.png"
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


