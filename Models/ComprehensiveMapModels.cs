namespace FragifyTracker.Models;

// Helper classes for deserializing the comprehensive maps data
public class ComprehensiveMapData
{
    public string displayName { get; set; } = string.Empty;
    public string imageUrl { get; set; } = string.Empty;
    public string totalCsUrl { get; set; } = string.Empty;
    public string fallbackUrl { get; set; } = string.Empty;
    public string theme { get; set; } = string.Empty;
    public string description { get; set; } = string.Empty;
    public ComprehensiveMapColors colors { get; set; } = new();
    public List<ComprehensiveMapLocation> commonLocations { get; set; } = new();
    public List<ComprehensiveMapCallout> callouts { get; set; } = new();
}

public class ComprehensiveMapColors
{
    public string primary { get; set; } = string.Empty;
    public string secondary { get; set; } = string.Empty;
    public string accent { get; set; } = string.Empty;
    public string background { get; set; } = string.Empty;
    public string surface { get; set; } = string.Empty;
    public string text { get; set; } = string.Empty;
    public string border { get; set; } = string.Empty;
}

public class ComprehensiveMapLocation
{
    public string name { get; set; } = string.Empty;
    public string description { get; set; } = string.Empty;
    public string type { get; set; } = string.Empty;
}

public class ComprehensiveMapCallout
{
    public string points { get; set; } = string.Empty;
    public string name { get; set; } = string.Empty;
    public string description { get; set; } = string.Empty;
}
