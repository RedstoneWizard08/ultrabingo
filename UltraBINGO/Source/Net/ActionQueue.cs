using System.Threading.Tasks;
using UltraBINGO.Packets;
using UltraBINGO.Types;
using UltraBINGO.Util;

namespace UltraBINGO.Net;

public class ActionQueue {
    public static AsyncAction PendingAction = AsyncAction.None;
    public static string? PendingPassword = null;
    public static VerifyModList? PendingVerifyModRequest;

    public static void Process() {
        switch (PendingAction) {
            case AsyncAction.Host:
                Requests.CreateRoom();
                break;

            case AsyncAction.Join:
                if (PendingPassword != null) Requests.JoinGame(PendingPassword);
                break;

            case AsyncAction.ModCheck:
                if (PendingVerifyModRequest != null) Main.NetworkManager.Socket.Send(PendingVerifyModRequest);
                break;

            case AsyncAction.ReconnectGame:
                Logging.Warn("Requesting to reconnect");

                Main.NetworkManager.Socket.Send(
                    new ReconnectRequest {
                        SteamId = Steamworks.SteamClient.SteamId.ToString(),
                        RoomId = GameManager.CurrentGame.GameId,
                        Ticket = RegisterTicket.Create()
                    }
                );

                break;

            case AsyncAction.FetchGames:
                Main.NetworkManager.Socket.Send(new FetchGames());
                break;

            case AsyncAction.None:
            default:
                break;
        }
    }
}