using UnityEngine;

namespace UltraBINGO.Util;

public static class Misc {
    private const string DiscordUrl = "https://discord.gg/VyzFJwEWtJ";

    public static void OpenDiscordLink() {
        Application.OpenURL(DiscordUrl);
    }
}