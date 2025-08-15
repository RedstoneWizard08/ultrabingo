using System.Threading.Tasks;
using UltraBINGO.Packets;
using UltraBINGO.Types;
using UltraBINGO.Util;

namespace UltraBINGO.Net;

public class ActionQueue {
    public static AsyncAction PendingAction = AsyncAction.None;
    public static string? PendingPassword = null;
    public static VerifyModRequest? PendingVerifyModRequest;

    public static async Task Process() {
        switch (PendingAction) {
            case AsyncAction.Host:
                await Requests.CreateRoom();
                break;

            case AsyncAction.Join:
                if (PendingPassword != null) await Requests.JoinGame(PendingPassword);
                break;

            case AsyncAction.ModCheck:
                if (PendingVerifyModRequest != null) await Main.NetworkManager.Socket.Send(PendingVerifyModRequest);
                break;

            case AsyncAction.ReconnectGame:
                Logging.Warn("Requesting to reconnect");

                await Main.NetworkManager.Socket.Send(
                    new ReconnectRequest {
                        SteamId = Steamworks.SteamClient.SteamId.ToString(),
                        RoomId = GameManager.CurrentGame.GameId,
                        Ticket = RegisterTicket.Create()
                    }
                );

                break;

            case AsyncAction.FetchGames:
                await Main.NetworkManager.Socket.Send(new FetchGamesRequest());
                break;

            case AsyncAction.None:
            default:
                break;
        }
    }
}