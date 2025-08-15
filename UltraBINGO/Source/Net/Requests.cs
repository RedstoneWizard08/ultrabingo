using System.Threading.Tasks;
using UltraBINGO.Packets;
using UltraBINGO.Types;
using UltraBINGO.Util;

namespace UltraBINGO.Net;

public static class Requests {
    public static async Task RegisterConnection() {
        Logging.Message("Registering connection with server");

        await Main.NetworkManager.Socket.Send(RegisterTicket.Create());
    }

    public static void SendModCheck(VerifyModRequest vmr) {
        ActionQueue.PendingAction = AsyncAction.ModCheck;
        ActionQueue.PendingVerifyModRequest = vmr;

        Main.NetworkManager.Socket.Connect();
    }

    /// <summary>
    /// Create a new bingo game room.
    /// </summary>
    public static async Task CreateRoom() {
        await Main.NetworkManager.Socket.Send(
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

    public static async Task JoinGame(string password) {
        await Main.NetworkManager.Socket.Send(
            new JoinRoomRequest {
                Password = password,
                Username = CommonFunctions.SanitiseUsername(Steamworks.SteamClient.Name),
                SteamId = Steamworks.SteamClient.SteamId.ToString(),
                Rank = GameManager.hasRankAccess ? GameManager.RequestedRank ?? "" : ""
            }
        );
    }

    public static async Task StartGame(int roomId) {
        await Main.NetworkManager.Socket.Send(
            new StartGameRequest {
                RoomId = roomId,
                Ticket = RegisterTicket.Create()
            }
        );
    }

    public static async Task SubmitRun(SubmitRunRequest srr) {
        await Main.NetworkManager.Socket.Send(srr);
    }

    public static async Task LeaveGame(int roomId) {
        await Main.NetworkManager.Socket.Send(
            new LeaveGameRequest {
                Username = CommonFunctions.SanitiseUsername(Steamworks.SteamClient.Name),
                SteamId = Steamworks.SteamClient.SteamId.ToString(),
                RoomId = roomId
            }
        );
    }

    public static async Task KickPlayer(string steamId) {
        await Main.NetworkManager.Socket.Send(
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