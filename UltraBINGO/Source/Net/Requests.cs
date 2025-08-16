using System.Threading.Tasks;
using UltraBINGO.Packets;
using UltraBINGO.Types;
using UltraBINGO.Util;

namespace UltraBINGO.Net;

public static class Requests {
    public static void RegisterConnection() {
        Logging.Message("Registering connection with server");

        Main.NetworkManager.Socket.Send(RegisterTicket.Create());
    }

    public static void SendModCheck(VerifyModList vmr) {
        ActionQueue.PendingAction = AsyncAction.ModCheck;
        ActionQueue.PendingVerifyModRequest = vmr;

        Main.NetworkManager.Socket.Connect();
    }

    /// <summary>
    /// Create a new bingo game room.
    /// </summary>
    public static void CreateRoom() {
        Main.NetworkManager.Socket.Send(
            new CreateRoomRequest {
                RoomName = "TestRoom",
                RoomPassword = "password",
                MaxPlayers = 8,
                PRankRequired = false,
                HostSteamName = CommonFunctions.SanitiseUsername(Steamworks.SteamClient.Name),
                HostSteamId = Steamworks.SteamClient.SteamId.ToString(),
                Rank = GameManager.hasRankAccess ? GameManager.RequestedRank ?? "" : ""
            }
        );
    }

    public static void JoinGame(string password) {
        Main.NetworkManager.Socket.Send(
            new JoinRoom {
                Password = password,
                Username = CommonFunctions.SanitiseUsername(Steamworks.SteamClient.Name),
                SteamId = Steamworks.SteamClient.SteamId.ToString(),
                Rank = GameManager.hasRankAccess ? GameManager.RequestedRank ?? "" : ""
            }
        );
    }

    public static void StartGame(int roomId) {
        Main.NetworkManager.Socket.Send(
            new StartGameRequest {
                RoomId = roomId,
                Ticket = RegisterTicket.Create()
            }
        );
    }

    public static void SubmitRun(SubmitRunRequest srr) {
        Main.NetworkManager.Socket.Send(srr);
    }

    public static void LeaveGame(int roomId) {
        Main.NetworkManager.Socket.Send(
            new LeaveGame {
                Username = CommonFunctions.SanitiseUsername(Steamworks.SteamClient.Name),
                SteamId = Steamworks.SteamClient.SteamId.ToString(),
                RoomId = roomId
            }
        );
    }

    public static void KickPlayer(string steamId) {
        Main.NetworkManager.Socket.Send(
            new KickPlayer {
                GameId = GameManager.CurrentGame.GameId,
                PlayerToKick = steamId,
                Ticket = RegisterTicket.Create()
            }
        );
    }

    public static void RequestGames() {
        ActionQueue.PendingAction = AsyncAction.FetchGames;
        Main.NetworkManager.Socket.Connect();
    }
}