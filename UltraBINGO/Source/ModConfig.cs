using BepInEx.Configuration;

namespace UltraBINGO;

public class ModConfig(ConfigFile config) {
    private const string DefaultHost = "clearwaterbirb.uk";
    private const int DefaultPort = 2052;

    public readonly ConfigEntry<string> ServerHost = config.Bind(
        "General",
        nameof(ServerHost),
        DefaultHost,
        "Server URL"
    );

    public readonly ConfigEntry<int> ServerPort = config.Bind(
        "General",
        nameof(ServerPort),
        DefaultPort,
        "Server Port"
    );

    public readonly ConfigEntry<string> LastRankUsed = config.Bind(
        "General",
        nameof(LastRankUsed),
        "None",
        "Last Rank Used (Only works if your SteamID has access to this rank)"
    );
}