using FragifyTracker.Models;

namespace FragifyTracker.Services;

public class WebMapThemeService
{
    private readonly Dictionary<string, WebMapTheme> _mapThemes;

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
        return new Dictionary<string, WebMapTheme>
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
            },
            ["de_dust2"] = new WebMapTheme
            {
                Name = "Dust II",
                Description = "Classic desert map theme",
                PrimaryColor = "#d4af37",
                SecondaryColor = "#b8860b",
                BackgroundGradient = "linear-gradient(135deg, #8B4513 0%, #D4AF37 50%, #F4A460 100%)",
                CardBackground = "rgba(139, 69, 19, 0.2)",
                CardBorder = "rgba(212, 175, 55, 0.3)",
                TextColor = "#F5DEB3",
                AccentColor = "#CD853F",
                DangerColor = "#DC143C",
                SuccessColor = "#32CD32",
                WarningColor = "#FFD700",
                MinimapUrl = "https://raw.githubusercontent.com/SteamDatabase/GameTracking-CSGO/master/game/csgo/resource/overviews/de_dust2_radar.png"
            },
            ["de_mirage"] = new WebMapTheme
            {
                Name = "Mirage",
                Description = "Middle Eastern desert theme",
                PrimaryColor = "#8B4513",
                SecondaryColor = "#CD853F",
                BackgroundGradient = "linear-gradient(135deg, #2F1B14 0%, #8B4513 50%, #DAA520 100%)",
                CardBackground = "rgba(47, 27, 20, 0.2)",
                CardBorder = "rgba(139, 69, 19, 0.3)",
                TextColor = "#F5DEB3",
                AccentColor = "#B8860B",
                DangerColor = "#B22222",
                SuccessColor = "#228B22",
                WarningColor = "#FF8C00",
                MinimapUrl = "https://raw.githubusercontent.com/SteamDatabase/GameTracking-CSGO/master/game/csgo/resource/overviews/de_mirage_radar.png"
            },
            ["de_inferno"] = new WebMapTheme
            {
                Name = "Inferno",
                Description = "Fiery Italian village theme",
                PrimaryColor = "#FF4500",
                SecondaryColor = "#FF6347",
                BackgroundGradient = "linear-gradient(135deg, #1A0F0F 0%, #8B0000 50%, #FF4500 100%)",
                CardBackground = "rgba(26, 15, 15, 0.2)",
                CardBorder = "rgba(255, 69, 0, 0.3)",
                TextColor = "#FFE4E1",
                AccentColor = "#FF8C00",
                DangerColor = "#DC143C",
                SuccessColor = "#00FF7F",
                WarningColor = "#FFD700",
                MinimapUrl = "https://raw.githubusercontent.com/SteamDatabase/GameTracking-CSGO/master/game/csgo/resource/overviews/de_inferno_radar.png"
            },
            ["de_cache"] = new WebMapTheme
            {
                Name = "Cache",
                Description = "Industrial urban theme",
                PrimaryColor = "#708090",
                SecondaryColor = "#4682B4",
                BackgroundGradient = "linear-gradient(135deg, #1C1C1C 0%, #2F4F4F 50%, #708090 100%)",
                CardBackground = "rgba(28, 28, 28, 0.2)",
                CardBorder = "rgba(112, 128, 144, 0.3)",
                TextColor = "#E0E0E0",
                AccentColor = "#87CEEB",
                DangerColor = "#FF6B6B",
                SuccessColor = "#98FB98",
                WarningColor = "#FFB6C1",
                MinimapUrl = "https://raw.githubusercontent.com/GameTracking-CSGO/master/game/csgo/resource/overviews/de_cache_radar.png"
            },
            ["de_overpass"] = new WebMapTheme
            {
                Name = "Overpass",
                Description = "Modern urban infrastructure theme",
                PrimaryColor = "#2E8B57",
                SecondaryColor = "#20B2AA",
                BackgroundGradient = "linear-gradient(135deg, #0F1410 0%, #2E8B57 50%, #20B2AA 100%)",
                CardBackground = "rgba(15, 20, 16, 0.2)",
                CardBorder = "rgba(46, 139, 87, 0.3)",
                TextColor = "#E0F6FF",
                AccentColor = "#00CED1",
                DangerColor = "#FF4500",
                SuccessColor = "#00FF7F",
                WarningColor = "#FFD700",
                MinimapUrl = "https://raw.githubusercontent.com/SteamDatabase/GameTracking-CSGO/master/game/csgo/resource/overviews/de_overpass_radar.png"
            },
            ["de_nuke"] = new WebMapTheme
            {
                Name = "Nuke",
                Description = "Nuclear facility theme",
                PrimaryColor = "#32CD32",
                SecondaryColor = "#00FF00",
                BackgroundGradient = "linear-gradient(135deg, #0F1A0F 0%, #228B22 50%, #32CD32 100%)",
                CardBackground = "rgba(15, 26, 15, 0.2)",
                CardBorder = "rgba(50, 205, 50, 0.3)",
                TextColor = "#E0FFE0",
                AccentColor = "#00FF7F",
                DangerColor = "#FF0000",
                SuccessColor = "#00FF00",
                WarningColor = "#FFFF00",
                MinimapUrl = "https://raw.githubusercontent.com/SteamDatabase/GameTracking-CSGO/master/game/csgo/resource/overviews/de_nuke_radar.png"
            },
            ["de_ancient"] = new WebMapTheme
            {
                Name = "Ancient",
                Description = "Mystical ancient ruins theme",
                PrimaryColor = "#DAA520",
                SecondaryColor = "#B8860B",
                BackgroundGradient = "linear-gradient(135deg, #2F1B14 0%, #8B4513 50%, #DAA520 100%)",
                CardBackground = "rgba(47, 27, 20, 0.2)",
                CardBorder = "rgba(218, 165, 32, 0.3)",
                TextColor = "#F5DEB3",
                AccentColor = "#CD853F",
                DangerColor = "#B22222",
                SuccessColor = "#228B22",
                WarningColor = "#FF8C00",
                MinimapUrl = "https://raw.githubusercontent.com/SteamDatabase/GameTracking-CSGO/master/game/csgo/resource/overviews/de_ancient_radar.png"
            },
            ["de_vertigo"] = new WebMapTheme
            {
                Name = "Vertigo",
                Description = "High-rise skyscraper theme",
                PrimaryColor = "#87CEEB",
                SecondaryColor = "#4682B4",
                BackgroundGradient = "linear-gradient(135deg, #1C1C1C 0%, #2F4F4F 50%, #87CEEB 100%)",
                CardBackground = "rgba(28, 28, 28, 0.2)",
                CardBorder = "rgba(135, 206, 235, 0.3)",
                TextColor = "#E0F6FF",
                AccentColor = "#00CED1",
                DangerColor = "#FF6B6B",
                SuccessColor = "#98FB98",
                WarningColor = "#FFB6C1",
                MinimapUrl = "https://raw.githubusercontent.com/SteamDatabase/GameTracking-CSGO/master/game/csgo/resource/overviews/de_vertigo_radar.png"
            }
        };
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
