using FragifyTracker.Models;

namespace FragifyTracker.Services;

public class WebMapThemeService
{
    private readonly Dictionary<string, WebMapTheme> _mapThemes;
    private readonly string _mapsDataPath = "Data/comprehensive_maps.json";

    public WebMapThemeService()
    {
        _mapThemes = InitializeMapThemes();
    }

    public WebMapTheme GetMapTheme(string mapName)
    {
        if (string.IsNullOrEmpty(mapName))
            return GetDefaultTheme();

        var normalizedName = mapName.ToLower();
        return _mapThemes.TryGetValue(normalizedName, out var theme) ? theme : GetDefaultTheme();
    }

    public WebMapTheme GetDefaultTheme()
    {
        return _mapThemes["default"];
    }

    private Dictionary<string, WebMapTheme> InitializeMapThemes()
    {
        var themes = new Dictionary<string, WebMapTheme>
        {
            ["default"] = new WebMapTheme
            {
                Name = "Default",
                Description = "Default CS:GO theme",
                PrimaryColor = "#4ade80",
                SecondaryColor = "#22c55e",
                BackgroundGradient = "linear-gradient(135deg, #1e3c72 0%, #2a5298 100%)",
                CardBackground = "rgba(255, 255, 255, 0.1)",
                CardBorder = "rgba(255, 255, 255, 0.2)",
                TextColor = "#ffffff",
                AccentColor = "#fbbf24",
                DangerColor = "#ef4444",
                SuccessColor = "#10b981",
                WarningColor = "#f59e0b",
                MinimapUrl = ""
            }
        };

        // Load themes from comprehensive_maps.json
        try
        {
            if (File.Exists(_mapsDataPath))
            {
                var jsonContent = File.ReadAllText(_mapsDataPath);
                var mapsData = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, ComprehensiveMapData>>(jsonContent);

                if (mapsData != null)
                {
                    foreach (var kvp in mapsData)
                    {
                        var mapData = kvp.Value;
                        var theme = new WebMapTheme
                        {
                            Name = mapData.displayName,
                            Description = mapData.description,
                            PrimaryColor = mapData.colors.primary,
                            SecondaryColor = mapData.colors.secondary,
                            AccentColor = mapData.colors.accent,
                            BackgroundGradient = $"linear-gradient(135deg, {mapData.colors.background} 0%, {mapData.colors.surface} 50%, {mapData.colors.primary} 100%)",
                            CardBackground = $"rgba({HexToRgb(mapData.colors.background)}, 0.2)",
                            CardBorder = $"rgba({mapData.colors.border}, 0.3)",
                            TextColor = mapData.colors.text,
                            DangerColor = "#ef4444",
                            SuccessColor = "#10b981",
                            WarningColor = "#f59e0b",
                            MinimapUrl = mapData.fallbackUrl
                        };

                        themes[kvp.Key] = theme;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading map themes: {ex.Message}");
        }

        return themes;
    }

    private string HexToRgb(string hex)
    {
        if (hex.StartsWith("#"))
            hex = hex.Substring(1);

        if (hex.Length == 6)
        {
            var r = Convert.ToInt32(hex.Substring(0, 2), 16);
            var g = Convert.ToInt32(hex.Substring(2, 2), 16);
            var b = Convert.ToInt32(hex.Substring(4, 2), 16);
            return $"{r}, {g}, {b}";
        }

        return "0, 0, 0";
    }
}

public class WebMapTheme
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string PrimaryColor { get; set; } = "#4ade80";
    public string SecondaryColor { get; set; } = "#22c55e";
    public string BackgroundGradient { get; set; } = "linear-gradient(135deg, #1e3c72 0%, #2a5298 100%)";
    public string CardBackground { get; set; } = "rgba(255, 255, 255, 0.1)";
    public string CardBorder { get; set; } = "rgba(255, 255, 255, 0.2)";
    public string TextColor { get; set; } = "#ffffff";
    public string AccentColor { get; set; } = "#fbbf24";
    public string DangerColor { get; set; } = "#ef4444";
    public string SuccessColor { get; set; } = "#10b981";
    public string WarningColor { get; set; } = "#f59e0b";
    public string MinimapUrl { get; set; } = string.Empty;

    public string GetCssVariables()
    {
        return $@"
            --primary-color: {PrimaryColor};
            --secondary-color: {SecondaryColor};
            --background-gradient: {BackgroundGradient};
            --card-background: {CardBackground};
            --card-border: {CardBorder};
            --text-color: {TextColor};
            --accent-color: {AccentColor};
            --danger-color: {DangerColor};
            --success-color: {SuccessColor};
            --warning-color: {WarningColor};
        ";
    }
}


