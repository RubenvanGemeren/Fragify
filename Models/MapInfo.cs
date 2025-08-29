namespace FragifyTracker.Models;

public class MapInfo
{
    public string MapName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Theme { get; set; } = string.Empty;

    // Color palette for the map theme
    public MapColorPalette Colors { get; set; } = new();

    // Map information
    public string MinimapUrl { get; set; } = string.Empty;
    public List<MapLocation> CommonLocations { get; set; } = new();
    public string Description { get; set; } = string.Empty;
}

public class MapColorPalette
{
    public string Primary { get; set; } = "#4A90E2";      // Main theme color
    public string Secondary { get; set; } = "#2C3E50";    // Secondary color
    public string Accent { get; set; } = "#E74C3C";       // Accent color
    public string Background { get; set; } = "#1A1A1A";   // Background color
    public string Surface { get; set; } = "#2D2D2D";      // Surface color
    public string Text { get; set; } = "#FFFFFF";         // Text color
    public string Border { get; set; } = "#4A90E2";       // Border color
}

public class MapLocation
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // "Bombsite", "Spawn", "Mid", "Common", etc.
}
