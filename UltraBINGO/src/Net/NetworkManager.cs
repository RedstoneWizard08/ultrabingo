using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Timers;
using BepInEx.Configuration;
using Newtonsoft.Json;
using UltraBINGO.API;
using UltraBINGO.Packets;
using UltraBINGO.Types;
using UltraBINGO.UI;
using UltraBINGO.Util;
using WebSocketSharp;
using static UltraBINGO.CommonFunctions;

// ReSharper disable InconsistentNaming

namespace UltraBINGO.Net;

public static class NetworkManager {
    public static AsyncAction pendingAction = AsyncAction.None;
    public static string pendingPassword = "";
    private static VerifyModRequest? pendingVmr;
    private static BasePacket? QueuedMessage;
    private static Types.State currentState = Types.State.Normal;
    public static ConfigEntry<string>? serverURLConfig;
    public static ConfigEntry<string>? serverPortConfig;
    public static ConfigEntry<string>? lastRankUsedConfig;
    private static string? _serverURL;
    private static readonly HttpClient Client = new();
    public static string? serverMapPoolCatalogURL;
    public static bool modlistCheckDone = false;
    public static bool modlistCheckPassed = false;
    private static string? _steamTicket;
    public static string? requestedRank = "";
    private static WebSocket? _ws;
    private static Timer? _heartbeatTimer;
    private const int MaxReconnectionAttempts = 3;
    private static int _currentReconnection;


    public static void SetState(Types.State newState) {
        currentState = newState;
    }

    private static async Task HandleAsyncConnect() {
        SetupHeartbeat();

        switch (pendingAction) {
            case AsyncAction.Host:
                await CreateRoom();
                break;

            case AsyncAction.Join:
                await JoinGame(pendingPassword);
                break;

            case AsyncAction.ModCheck:
                if (pendingVmr != null) await SendEncodedMessage(pendingVmr);
                break;

            case AsyncAction.RetrySend:
                if (QueuedMessage != null) await SendEncodedMessage(QueuedMessage);
                QueuedMessage = null;
                break;

            case AsyncAction.ReconnectGame:
                Logging.Warn("Requesting to reconnect");

                await SendEncodedMessage(
                    new ReconnectRequest {
                        SteamId = Steamworks.SteamClient.SteamId.ToString(),
                        RoomId = GameManager.CurrentGame.GameId,
                        Ticket = CreateRegisterTicket()
                    }
                );

                break;

            case AsyncAction.FetchGames:
                await SendEncodedMessage(new FetchGamesRequest());
                break;

            case AsyncAction.None:
            default:
                break;
        }
    }

    public static void SendModCheck(VerifyModRequest vmr) {
        pendingAction = AsyncAction.ModCheck;
        pendingVmr = vmr;

        ConnectWebSocket();
    }

    private static string GetSteamTicket() {
        return _steamTicket ?? throw new NullReferenceException("Steam ticket was not set!");
    }

    public static void SetSteamTicket(string ticket) {
        _steamTicket = ticket;
    }

    /// <summary>
    /// Fetch the bingo map catalog from the server.
    /// </summary>
    public static async Task<string?> FetchCatalog(string urlToRequest) {
        try {
            var responseTomlRaw = await Client.GetStringAsync(urlToRequest);
            return responseTomlRaw;
        } catch (Exception e) {
            Logging.Error("Something went wrong while fetching from the URL");
            Logging.Error(e.Message);
            return null;
        }
    }

    /// <summary>
    /// Check if the WebSocket connection to the server is active and alive.
    /// </summary>
    public static bool IsConnectionUp() {
        return _ws?.IsAlive ?? false;
    }

    /// <summary>
    /// Init and set up the WebSocket connection.
    /// </summary>
    public static void Initialise(string url, string port) {
        _serverURL = $"ws://{url}:{port}";
        serverMapPoolCatalogURL = $"http://{url}:{port}/bingoMapPool.toml";

        _ws = new WebSocket(_serverURL);
        _ws.EnableRedirection = true;
        _ws.WaitTime = TimeSpan.FromSeconds(15);

        _ws.OnOpen += (_, _) => { HandleAsyncConnect().Wait(); };
        _ws.OnMessage += OnMessageReceived;
        _ws.OnError += ActuallyHandleError;

        _ws.OnClose += (_, e) => {
            if (e.WasClean) {
                Logging.Message("Disconnected cleanly from server");
            } else {
                Logging.Error("Network connection error.");
                Logging.Error(e.Reason);
            }
        };
    }

    private static void TryReconnect() {
        pendingAction = AsyncAction.ReconnectGame;
        ConnectWebSocket();
    }

    private static void ActuallyHandleError(object sender, ErrorEventArgs e) {
        HandleError(sender, e).Wait();
    }

    /// <summary>
    /// Handle any errors that happen with the WebSocket connection.
    /// </summary>
    private static async Task HandleError(object _, ErrorEventArgs e) {
        Logging.Error("Network error happened");
        Logging.Error(e.Message);
        Logging.Error(e.Exception?.ToString());
        if (IsConnectionUp())
            _ws?.CloseAsync();
        else
            // If player is on main menu
            switch (currentState) {
                case Types.State.InMenu:
                    MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage("Failed to contact server.");
                    BingoMainMenu.UnlockUI();
                    break;

                case Types.State.InLobby:
                    MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage(
                        "Connection to the lobby was lost. Returning to menu."
                    );

                    GameManager.ClearGameVariables();
                    SetState(Types.State.InMenu);

                    BingoEncapsulator.BingoCardScreen?.SetActive(false);
                    BingoEncapsulator.BingoLobbyScreen?.SetActive(false);
                    BingoEncapsulator.BingoEndScreen?.SetActive(false);
                    BingoEncapsulator.BingoMenu?.SetActive(true);

                    break;

                case Types.State.InGame:
                    // Handle in-game reconnection attempts here
                    if (GameManager.IsInBingoLevel) {
                        _currentReconnection++;

                        if (_currentReconnection > MaxReconnectionAttempts) {
                            _currentReconnection = 0;

                            GameManager.ClearGameVariables();
                            MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage(
                                "Failed to reconnect. Exitting game in 5 seconds."
                            );
                            SetState(Types.State.Normal);

                            await Task.Delay(5000);
                            SceneHelper.LoadScene("Main Menu");
                        } else {
                            MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage(
                                $"Connection to the game was lost.\nAttempting reconnection... <color=orange>({_currentReconnection}/{MaxReconnectionAttempts})</color>"
                            );
                            await Task.Delay(2000);

                            TryReconnect();
                        }
                    }

                    break;

                case Types.State.InBrowser:
                    if (!BingoBrowser.fetchDone)
                        BingoBrowser.DisplayError();
                    else
                        BingoBrowser.UnlockUI();
                    break;

                case Types.State.Normal:
                default: break;
            }
    }

    /// <summary>
    /// Decode base64 messages received from the server.
    /// </summary>
    private static string DecodeMessage(string encodedMessage) {
        var base64EncodedBytes = Convert.FromBase64String(encodedMessage);

        return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
    }

    /// <summary>
    /// Encode and send base64 messages to the server.
    /// </summary>
    public static async Task SendEncodedMessage(BasePacket packet) {
        if (!IsConnectionUp()) {
            Logging.Warn("Queuing message & retrying connection");

            QueuedMessage = packet;
            pendingAction = AsyncAction.RetrySend;

            ConnectWebSocket();

            return;
        }

        if (!PacketManager.Packets.ContainsKey(packet.GetType())) {
            Logging.Error($"Unregistered packet type: {packet.GetType()}. Failed to send!");
            return;
        }

        var packetInfo = PacketManager.Packets[packet.GetType()];

        var message = new EncapsulatedMessage {
            Contents = packetInfo.Serialize(packet),
            Header = packetInfo.Name
        };

        var encodedBytes = System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));
        var encodedJson = Convert.ToBase64String(encodedBytes);
        var source = new TaskCompletionSource<bool>();

        _ws?.SendAsync(
            encodedJson,
            completed => {
                if (!completed) Logging.Warn("Async not sent");

                source.TrySetResult(completed);
            }
        );

        await source.Task;
    }

    public static RegisterTicket CreateRegisterTicket() {
        var rt = new RegisterTicket {
            SteamId = Steamworks.SteamClient.SteamId.ToString(),
            SteamTicket = GetSteamTicket(),
            SteamUsername = Steamworks.SteamClient.Name,
            GameId = GameManager.CurrentGame.GameId
        };

        return rt;
    }

    public static async Task RegisterConnection() {
        Logging.Message("Registering connection with server");

        await SendEncodedMessage(CreateRegisterTicket());
    }

    /// <summary>
    /// Connect the WebSocket to the server.
    /// </summary>
    public static void ConnectWebSocket() {
        if (IsConnectionUp()) _ws?.Close();
        _ws?.ConnectAsync();
    }

    /// <summary>
    /// Disconnect WebSocket.
    /// </summary>
    public static void DisconnectWebSocket(ushort code = 1000, string reason = "Disconnect reason not specified") {
        _ws?.Close(code, reason);
    }

    /// <summary>
    /// Setup WebSocket heartbeat.
    /// </summary>
    private static void SetupHeartbeat() {
        _heartbeatTimer = new Timer(10 * 1000); //Ping once every 10 seconds
        _heartbeatTimer.Elapsed += SendPing;
        _heartbeatTimer.AutoReset = true;
        _heartbeatTimer.Enabled = true;
    }

    /// <summary>
    /// Ping the WebSocket server to keep the connection alive.
    /// </summary>
    private static void SendPing(object source, ElapsedEventArgs e) {
        _ws?.Ping();
    }

    /// <summary>
    /// Create a new bingo game room.
    /// </summary>
    private static async Task CreateRoom() {
        await SendEncodedMessage(
            new CreateRoomRequest {
                RoomName = "TestRoom",
                RoomPassword = "password",
                MaxPlayers = 8,
                PRankRequired = false,
                HostSteamName = SanitiseUsername(Steamworks.SteamClient.Name),
                HostSteamId = Steamworks.SteamClient.SteamId.ToString(),
                Rank = GameManager.hasRankAccess ? requestedRank ?? "" : ""
            }
        );
    }

    private static async Task JoinGame(string password) {
        await SendEncodedMessage(
            new JoinRoomRequest {
                Password = password,
                Username = SanitiseUsername(Steamworks.SteamClient.Name),
                SteamId = Steamworks.SteamClient.SteamId.ToString(),
                Rank = GameManager.hasRankAccess ? requestedRank ?? "" : ""
            }
        );
    }

    public static async Task SendStartGameSignal(int roomId) {
        await SendEncodedMessage(
            new StartGameRequest {
                RoomId = roomId,
                Ticket = CreateRegisterTicket()
            }
        );
    }

    public static async Task SubmitRun(SubmitRunRequest srr) {
        await SendEncodedMessage(srr);
    }

    public static async Task SendLeaveGameRequest(int roomId) {
        await SendEncodedMessage(
            new LeaveGameRequest {
                Username = SanitiseUsername(Steamworks.SteamClient.Name),
                SteamId = Steamworks.SteamClient.SteamId.ToString(),
                RoomId = roomId
            }
        );
    }

    public static async Task KickPlayer(string steamId) {
        await SendEncodedMessage(
            new KickPlayer {
                GameId = GameManager.CurrentGame.GameId,
                PlayerToKick = steamId,
                Ticket = CreateRegisterTicket()
            }
        );
    }

    public static void RequestGames() {
        pendingAction = AsyncAction.FetchGames;

        ConnectWebSocket();
    }


    /// <summary>
    /// Handle all incoming messages received from the server.
    /// </summary>
    private static void OnMessageReceived(object _, MessageEventArgs e) {
        Logging.Info("Message received!");
        Logging.Info($"Contents: {e.Data}");

        try {
            ProcessMessage(e.Data).Wait();
        } catch (Exception ex) {
            Logging.Error("Failed to process message!");
            Logging.Error($"Error: {ex}");
        }
    }

    private static async Task ProcessMessage(string data) {
        var em = JsonConvert.DeserializeObject<EncapsulatedMessage>(DecodeMessage(data));

        if (em == null) {
            Logging.Error("EncapsulatedMessage failed to decode! Got a null result!");
            return;
        }

        if (!PacketManager.PacketsByName.ContainsKey(em.Header)) {
            Logging.Error($"Packet with name '{em.Header}' does not exist!");
            return;
        }

        var err = await PacketManager.PacketsByName[em.Header].Handle(em.Contents);

        if (err != null) {
            Logging.Error($"An error occured while handling packet '{em.Header}':");
            Logging.Error(err.Message);
            Logging.Error(err.StackTrace);
        }
    }
}