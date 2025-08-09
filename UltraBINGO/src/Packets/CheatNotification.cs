using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using UltraBINGO.API;

namespace UltraBINGO.Packets;

[Packet(PacketDirection.ServerToClient)]
public class CheatNotification : IncomingPacket {
    [UsedImplicitly] public required string PlayerToHumiliate;

    private static readonly List<string> Messages = [
        "Unfortunately, I ate them all.",
        "Their punishment is one ranked match of League.",
        "What a silly person.",
        "They may have spontaneously imploded.",
        "Their death was a canon event.",
        "Jakito does not approve.",
        "Their punishment is 1 hour of grinding 1-4.",
        "Their attempt was a futile struggle, doomed from the very start.",
        "Hakita told them to shut the fuck up.",
        "They were misinformed on the internet.",
        "Point and laugh at this person."
    ];

    public override Task Handle() {
        var random = new Random();
        var msg = $"{PlayerToHumiliate} tried to enable cheats.\n{Messages[random.Next(Messages.Count)]}";

        MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage(msg);

        return Task.CompletedTask;
    }
}