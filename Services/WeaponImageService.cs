namespace FragifyTracker.Services;

public class WeaponImageService
{
    private readonly Dictionary<string, string> _weaponImages;
    private readonly Dictionary<string, string> _agentImages;

    // Base URLs for weapon and agent images
            private const string WEAPON_BASE_URL = "https://totalcsgo.com/weapons/";
        private const string AGENT_BASE_URL = "https://totalcsgo.com/agents/";

    public WeaponImageService()
    {
        _weaponImages = new Dictionary<string, string>();
        _agentImages = new Dictionary<string, string>();
        InitializeWeaponImages();
        InitializeAgentImages();
    }

    private void InitializeWeaponImages()
    {
        // CS:GO weapon names and their corresponding image files
        var weapons = new Dictionary<string, string>
        {
            // Pistols
            { "weapon_deagle", "deagle.png" },
            { "weapon_elite", "elite.png" },
            { "weapon_fiveseven", "fiveseven.png" },
            { "weapon_glock", "glock.png" },
            { "weapon_p250", "p250.png" },
            { "weapon_tec9", "tec9.png" },
            { "weapon_usp_silencer", "usp_silencer.png" },
            { "weapon_cz75a", "cz75a.png" },
            { "weapon_revolver", "revolver.png" },
            { "weapon_p2000", "p2000.png" },

            // Rifles
            { "weapon_ak47", "ak47.png" },
            { "weapon_aug", "aug.png" },
            { "weapon_awp", "awp.png" },
            { "weapon_famas", "famas.png" },
            { "weapon_g3sg1", "g3sg1.png" },
            { "weapon_galilar", "galilar.png" },
            { "weapon_m249", "m249.png" },
            { "weapon_m4a1", "m4a1.png" },
            { "weapon_m4a1_silencer", "m4a1_silencer.png" },
            { "weapon_mac10", "mac10.png" },
            { "weapon_mp5sd", "mp5sd.png" },
            { "weapon_mp7", "mp7.png" },
            { "weapon_mp9", "mp9.png" },
            { "weapon_negev", "negev.png" },
            { "weapon_nova", "nova.png" },
            { "weapon_p90", "p90.png" },
            { "weapon_sawedoff", "sawedoff.png" },
            { "weapon_scar20", "scar20.png" },
            { "weapon_sg556", "sg556.png" },
            { "weapon_ssg08", "ssg08.png" },
            { "weapon_ump45", "ump45.png" },
            { "weapon_xm1014", "xm1014.png" },

            // Knives and other
            { "weapon_knife", "knife_default_ct.png" },
            { "weapon_knife_t", "knife_default_t.png" },
            { "weapon_knife_butterfly", "knife_butterfly.png" },
            { "weapon_knife_falchion", "knife_falchion.png" },
            { "weapon_knife_flip", "knife_flip.png" },
            { "weapon_knife_gut", "knife_gut.png" },
            { "weapon_knife_karambit", "knife_karambit.png" },
            { "weapon_knife_m9_bayonet", "knife_m9_bayonet.png" },
            { "weapon_knife_tactical", "knife_tactical.png" },
            { "weapon_knife_ursus", "knife_ursus.png" },
            { "weapon_knife_widowmaker", "knife_widowmaker.png" },

            // Grenades
            { "weapon_hegrenade", "hegrenade.png" },
            { "weapon_flashbang", "flashbang.png" },
            { "weapon_smokegrenade", "smokegrenade.png" },
            { "weapon_molotov", "molotov.png" },
            { "weapon_decoy", "decoy.png" },
            { "weapon_incgrenade", "incgrenade.png" },

            // Equipment
            { "weapon_c4", "c4.png" },
            { "weapon_taser", "taser.png" },
            { "weapon_healthshot", "healthshot.png" }
        };

        foreach (var weapon in weapons)
        {
            _weaponImages[weapon.Key] = $"{WEAPON_BASE_URL}{weapon.Value}";
        }
    }

    private void InitializeAgentImages()
    {
        // CS:GO agent models and their corresponding image files
        var agents = new Dictionary<string, string>
        {
            // T Agents
            { "T", "t_phoenix.png" },
            { "CT", "ct_sas.png" },

            // Specific T agents
            { "t_phoenix", "t_phoenix.png" },
            { "t_leet", "t_leet.png" },
            { "t_separatist", "t_separatist.png" },
            { "t_balkan", "t_balkan.png" },
            { "t_professional", "t_professional.png" },
            { "t_anarchist", "t_anarchist.png" },
            { "t_pirate", "t_pirate.png" },
            { "t_guerilla", "t_guerilla.png" },

            // Specific CT agents
            { "ct_sas", "ct_sas.png" },
            { "ct_gign", "ct_gign.png" },
            { "ct_gsg9", "ct_gsg9.png" },
            { "ct_idf", "ct_idf.png" },
            { "ct_swat", "ct_swat.png" },
            { "ct_fbi", "ct_fbi.png" },
            { "ct_st6", "ct_st6.png" },
            { "ct_2", "ct_2.png" }
        };

        foreach (var agent in agents)
        {
            _agentImages[agent.Key] = $"{AGENT_BASE_URL}{agent.Value}";
        }
    }

    public string GetWeaponImageUrl(string weaponName)
    {
        if (string.IsNullOrEmpty(weaponName))
            return GetDefaultWeaponImage();

        var normalizedName = weaponName.ToLower();

        // Try exact match first
        if (_weaponImages.TryGetValue(normalizedName, out var imageUrl))
            return imageUrl;

        // Try partial match
        foreach (var weapon in _weaponImages.Keys)
        {
            if (weapon.Contains(normalizedName) || normalizedName.Contains(weapon))
                return _weaponImages[weapon];
        }

        return GetDefaultWeaponImage();
    }

    public string GetAgentImageUrl(string team)
    {
        if (string.IsNullOrEmpty(team))
            return GetDefaultAgentImage();

        var normalizedTeam = team.ToUpper();

        if (_agentImages.TryGetValue(normalizedTeam, out var imageUrl))
            return imageUrl;

        // Fallback to team-based default
        return normalizedTeam == "T" ? _agentImages["T"] : _agentImages["CT"];
    }

    private string GetDefaultWeaponImage()
    {
        return $"{WEAPON_BASE_URL}knife_default_ct.png";
    }

    private string GetDefaultAgentImage()
    {
        return _agentImages["CT"];
    }

    public List<string> GetAllWeaponNames()
    {
        return _weaponImages.Keys.ToList();
    }

    public List<string> GetAllAgentNames()
    {
        return _agentImages.Keys.ToList();
    }
}
