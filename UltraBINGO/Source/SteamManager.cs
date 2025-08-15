using System;

namespace UltraBINGO;

public static class SteamManager {
    private static string? _steamTicket;
    
    public static string GetSteamTicket() {
        return _steamTicket ?? throw new NullReferenceException("Steam ticket was not set!");
    }

    public static void SetSteamTicket(string ticket) {
        _steamTicket = ticket;
    }
}