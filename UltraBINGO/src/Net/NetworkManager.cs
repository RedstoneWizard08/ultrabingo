using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Timers;
using BepInEx.Configuration;
using Newtonsoft.Json;
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
    private static string QueuedMessage = "";
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
            case AsyncAction.Host: {
                await CreateRoom();
                break;
            }

            case AsyncAction.Join: {
                await JoinGame(pendingPassword);
                break;
            }

            case AsyncAction.ModCheck: {
                await SendEncodedMessage(JsonConvert.SerializeObject(pendingVmr));
                break;
            }

            case AsyncAction.RetrySend: {
                await SendEncodedMessage(JsonConvert.SerializeObject(QueuedMessage));
                QueuedMessage = "";
                break;
            }

            case AsyncAction.ReconnectGame: {
                Logging.Warn("Requesting to reconnect");

                await SendEncodedMessage(JsonConvert.SerializeObject(new ReconnectRequest {
                    SteamId = Steamworks.SteamClient.SteamId.ToString(),
                    RoomId = GameManager.CurrentGame.GameId,
                    Ticket = CreateRegisterTicket()
                }));

                break;
            }

            case AsyncAction.FetchGames: {
                var fgr = new FetchGamesRequest();
                await SendEncodedMessage(JsonConvert.SerializeObject(fgr));
                break;
            }

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

    //Fetch the bingo map catalog from the server.
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

    //Check if the WebSocket connection to the server is active and alive.
    public static bool IsConnectionUp() {
        return _ws?.IsAlive ?? false;
    }

    //Init and set up the WebSocket connection.
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

    //Handle any errors that happen with the WebSocket connection.
    private static async Task HandleError(object _, ErrorEventArgs e) {
        Logging.Error("Network error happened");
        Logging.Error(e.Message);
        Logging.Error(e.Exception?.ToString());
        if (IsConnectionUp())
            _ws?.CloseAsync();
        else
            // If player is on main menu
            switch (currentState) {
                case Types.State.InMenu: {
                    MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage("Failed to contact server.");
                    BingoMainMenu.UnlockUI();
                    break;
                }
                case Types.State.InLobby: {
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
                }
                case Types.State.InGame: {
                    //Handle in-game reconnection attempts here
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
                }
                case Types.State.InBrowser: {
                    if (!BingoBrowser.fetchDone)
                        BingoBrowser.DisplayError();
                    else
                        BingoBrowser.UnlockUI();

                    break;
                }
                case Types.State.Normal:
                default: break;
            }
    }

    //Decode base64 messages recieved from the server.
    private static string DecodeMessage(string encodedMessage) {
        var base64EncodedBytes = Convert.FromBase64String(encodedMessage);
        return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
    }

    //Encode and send base64 messages to the server.
    public static async Task SendEncodedMessage(string jsonToEncode) {
        if (!IsConnectionUp()) {
            Logging.Warn("Queuing message & retrying connection");
            QueuedMessage = jsonToEncode;
            pendingAction = AsyncAction.RetrySend;
            ConnectWebSocket();
            return;
        }

        var encodedBytes = System.Text.Encoding.UTF8.GetBytes(jsonToEncode);
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
        var rt = CreateRegisterTicket();
        await SendEncodedMessage(JsonConvert.SerializeObject(rt));
    }

    //Connect the WebSocket to the server.
    public static void ConnectWebSocket() {
        if (IsConnectionUp()) _ws?.Close();
        _ws?.ConnectAsync();
    }

    //Disconnect WebSocket.
    public static void DisconnectWebSocket(ushort code = 1000, string reason = "Disconnect reason not specified") {
        _ws?.Close(code, reason);
    }

    //Setup WebSocket heartbeat.
    private static void SetupHeartbeat() {
        _heartbeatTimer = new Timer(10 * 1000); //Ping once every 10 seconds
        _heartbeatTimer.Elapsed += SendPing;
        _heartbeatTimer.AutoReset = true;
        _heartbeatTimer.Enabled = true;
    }

    //Ping the WebSocket server to keep the connection alive.
    private static void SendPing(object source, ElapsedEventArgs e) {
        _ws?.Ping();
    }

    //Create a new bingo game room.
    private static async Task CreateRoom() {
        var crr = new CreateRoomRequest {
            RoomName = "TestRoom",
            RoomPassword = "password",
            MaxPlayers = 8,
            PRankRequired = false,
            HostSteamName = SanitiseUsername(Steamworks.SteamClient.Name),
            HostSteamId = Steamworks.SteamClient.SteamId.ToString(),
            Rank = GameManager.hasRankAccess ? requestedRank ?? "" : ""
        };

        await SendEncodedMessage(JsonConvert.SerializeObject(crr));
    }

    private static async Task JoinGame(string password) {
        var jrr = new JoinRoomRequest {
            Password = password,
            Username = SanitiseUsername(Steamworks.SteamClient.Name),
            SteamId = Steamworks.SteamClient.SteamId.ToString(),
            Rank = GameManager.hasRankAccess ? requestedRank ?? "" : ""
        };
        await SendEncodedMessage(JsonConvert.SerializeObject(jrr));
    }

    public static async Task SendStartGameSignal(int roomId) {
        var gameRequest = new StartGameRequest {
            RoomId = roomId,
            Ticket = CreateRegisterTicket()
        };

        await SendEncodedMessage(JsonConvert.SerializeObject(gameRequest));
    }

    public static async Task SubmitRun(SubmitRunRequest srr) {
        await SendEncodedMessage(JsonConvert.SerializeObject(srr));
    }

    public static async Task SendLeaveGameRequest(int roomId) {
        var leaveRequest = new LeaveGameRequest {
            Username = SanitiseUsername(Steamworks.SteamClient.Name),
            SteamId = Steamworks.SteamClient.SteamId.ToString(),
            RoomId = roomId
        };

        await SendEncodedMessage(JsonConvert.SerializeObject(leaveRequest));
    }

    public static async Task KickPlayer(string steamId) {
        var kp = new KickPlayer {
            GameId = GameManager.CurrentGame.GameId,
            PlayerToKick = steamId,
            Ticket = CreateRegisterTicket()
        };

        await SendEncodedMessage(JsonConvert.SerializeObject(kp));
    }

    public static void RequestGames() {
        pendingAction = AsyncAction.FetchGames;
        ConnectWebSocket();
    }


    //Handle all incoming messages received from the server.
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