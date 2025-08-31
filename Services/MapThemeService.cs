using FragifyTracker.Models;
using System.Text.Json;

namespace FragifyTracker.Services;

public class MapThemeService
{
    private readonly Dictionary<string, MapInfo> _mapThemes;
    private readonly string _mapsDataPath = "Data/comprehensive_maps.json";

    public MapThemeService()
    {
        _mapThemes = new Dictionary<string, MapInfo>();
        LoadMapThemes();
    }

    private void LoadMapThemes()
    {
        try
        {
            // Create Data directory if it doesn't exist
            var dataDir = Path.GetDirectoryName(_mapsDataPath);
            if (!Directory.Exists(dataDir))
            {
                Directory.CreateDirectory(dataDir!);
            }

            // If comprehensive_maps.json doesn't exist, create default themes
            if (!File.Exists(_mapsDataPath))
            {
                CreateDefaultMapThemes();
            }

            var jsonContent = File.ReadAllText(_mapsDataPath);
            var mapsData = JsonSerializer.Deserialize<Dictionary<string, ComprehensiveMapData>>(jsonContent);

            if (mapsData != null)
            {
                foreach (var kvp in mapsData)
                {
                    var mapData = kvp.Value;
                    var mapInfo = new MapInfo
                    {
                        MapName = kvp.Key,
                        DisplayName = mapData.displayName,
                        Theme = mapData.theme,
                        Colors = new MapColorPalette
                        {
                            Primary = mapData.colors.primary,
                            Secondary = mapData.colors.secondary,
                            Accent = mapData.colors.accent,
                            Background = mapData.colors.background,
                            Surface = mapData.colors.surface,
                            Text = mapData.colors.text,
                            Border = mapData.colors.border
                        },
                        MinimapUrl = mapData.fallbackUrl,
                        Description = mapData.description,
                        CommonLocations = mapData.commonLocations.Select(loc => new MapLocation
                        {
                            Name = loc.name,
                            Type = loc.type,
                            Description = loc.description
                        }).ToList()
                    };

                    _mapThemes[mapInfo.MapName.ToLower()] = mapInfo;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading map themes: {ex.Message}");
            CreateDefaultMapThemes();
        }
    }

    private void CreateDefaultMapThemes()
    {
        var defaultMaps = new List<MapInfo>
        {
            new MapInfo
            {
                MapName = "de_dust2",
                DisplayName = "Dust II",
                Theme = "Desert",
                Colors = new MapColorPalette
                {
                    Primary = "#D4AF37",      // Gold
                    Secondary = "#8B4513",    // Saddle Brown
                    Accent = "#CD853F",       // Peru
                    Background = "#2F1B14",   // Dark Brown
                    Surface = "#3D2817",      // Medium Brown
                    Text = "#F5DEB3",         // Wheat
                    Border = "#D4AF37"        // Gold
                },
                MinimapUrl = "https://totalcsgo.com/maps/de_dust2.png",
                Description = "Classic desert map with long sightlines and strategic positions",
                CommonLocations = new List<MapLocation>
                {
                    new() { Name = "A Long", Type = "Common", Description = "Long corridor to A site" },
                    new() { Name = "B Site", Type = "Bombsite", Description = "B bombsite with multiple entry points" },
                    new() { Name = "Mid", Type = "Common", Description = "Central area connecting both sites" },
                    new() { Name = "A Site", Type = "Bombsite", Description = "A bombsite with elevated positions" }
                }
            },
            new MapInfo
            {
                MapName = "de_mirage",
                DisplayName = "Mirage",
                Theme = "Middle Eastern",
                Colors = new MapColorPalette
                {
                    Primary = "#8B4513",      // Saddle Brown
                    Secondary = "#CD853F",    // Peru
                    Accent = "#DAA520",       // Goldenrod
                    Background = "#2F1B14",   // Dark Brown
                    Surface = "#3D2817",      // Medium Brown
                    Text = "#F5DEB3",         // Wheat
                    Border = "#8B4513"        // Saddle Brown
                },
                MinimapUrl = "https://totalcsgo.com/maps/de_mirage.png",
                Description = "Middle Eastern themed map with complex angles and tight corridors",
                CommonLocations = new List<MapLocation>
                {
                    new() { Name = "A Site", Type = "Bombsite", Description = "A bombsite with multiple angles" },
                    new() { Name = "B Site", Type = "Bombsite", Description = "B bombsite with window positions" },
                    new() { Name = "Mid", Type = "Common", Description = "Central area with multiple routes" },
                    new() { Name = "Palace", Type = "Common", Description = "Palace area leading to A site" }
                }
            },
            new MapInfo
            {
                MapName = "de_inferno",
                DisplayName = "Inferno",
                Theme = "Italian Village",
                Colors = new MapColorPalette
                {
                    Primary = "#FF4500",      // Orange Red
                    Secondary = "#8B0000",    // Dark Red
                    Accent = "#FF6347",       // Tomato
                    Background = "#1A0F0F",   // Very Dark Red
                    Surface = "#2D1B1B",      // Dark Red
                    Text = "#FFE4E1",         // Misty Rose
                    Border = "#FF4500"        // Orange Red
                },
                MinimapUrl = "https://totalcsgo.com/maps/de_inferno.png",
                Description = "Italian village map with narrow streets and close combat",
                CommonLocations = new List<MapLocation>
                {
                    new() { Name = "A Site", Type = "Bombsite", Description = "A bombsite in the church area" },
                    new() { Name = "B Site", Type = "Bombsite", Description = "B bombsite near banana" },
                    new() { Name = "Banana", Type = "Common", Description = "Long corridor to B site" },
                    new() { Name = "Mid", Type = "Common", Description = "Central area connecting both sites" }
                }
            },
            new MapInfo
            {
                MapName = "de_cache",
                DisplayName = "Cache",
                Theme = "Industrial",
                Colors = new MapColorPalette
                {
                    Primary = "#708090",      // Slate Gray
                    Secondary = "#2F4F4F",    // Dark Slate Gray
                    Accent = "#4682B4",       // Steel Blue
                    Background = "#1C1C1C",   // Dark Gray
                    Surface = "#2F2F2F",      // Medium Gray
                    Text = "#E0E0E0",         // Light Gray
                    Border = "#708090"        // Slate Gray
                },
                MinimapUrl = "https://totalcsgo.com/maps/de_cache.png",
                Description = "Industrial map with clean angles and strategic positions",
                CommonLocations = new List<MapLocation>
                {
                    new() { Name = "A Site", Type = "Bombsite", Description = "A bombsite with multiple entry points" },
                    new() { Name = "B Site", Type = "Bombsite", Description = "B bombsite with window positions" },
                    new() { Name = "Mid", Type = "Common", Description = "Central area with multiple routes" },
                    new() { Name = "Vent", Type = "Common", Description = "Vent area connecting sites" }
                }
            }
        };

        try
        {
            var jsonContent = JsonSerializer.Serialize(defaultMaps, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_mapsDataPath, jsonContent);

            foreach (var map in defaultMaps)
            {
                _mapThemes[map.MapName.ToLower()] = map;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating default map themes: {ex.Message}");
        }
    }

    public MapInfo? GetMapTheme(string mapName)
    {
        if (string.IsNullOrEmpty(mapName))
            return null;

        var normalizedName = mapName.ToLower();
        return _mapThemes.TryGetValue(normalizedName, out var theme) ? theme : null;
    }

    public List<MapInfo> GetAllMaps()
    {
        return _mapThemes.Values.ToList();
    }

    public void UpdateMapTheme(MapInfo mapInfo)
    {
        if (mapInfo == null || string.IsNullOrEmpty(mapInfo.MapName))
            return;

        _mapThemes[mapInfo.MapName.ToLower()] = mapInfo;
        SaveMapThemes();
    }

    private void SaveMapThemes()
    {
        try
        {
            var maps = _mapThemes.Values.ToList();
            var jsonContent = JsonSerializer.Serialize(maps, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_mapsDataPath, jsonContent);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving map themes: {ex.Message}");
        }
    }
}


