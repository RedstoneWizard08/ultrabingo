using System;
using System.Threading.Tasks;
using System.Timers;
using UltraBINGO.API;
using UltraBINGO.Util;
using WebSocketSharp;

namespace UltraBINGO.Net;

public class SocketManager {
    private readonly WebSocket _ws;
    private Timer? _heartbeatTimer;
    private BasePacket? _queuedMessage;

    public event Func<Task> OnConnect = () => Task.CompletedTask;
    public event Func<Task> OnError = () => Task.CompletedTask;

    /// <summary>
    /// Check if the WebSocket connection to the server is active and alive.
    /// </summary>
    public bool IsAlive => _ws.IsAlive;

    /// <summary>
    /// Init and set up the WebSocket connection.
    /// </summary>
    public SocketManager(string host, int port) {
        _ws = new WebSocket($"ws://{host}:{port}");
        _ws.EnableRedirection = true;
        _ws.WaitTime = TimeSpan.FromSeconds(15);

        _ws.OnOpen += (_, _) => HandleConnect().Wait();
        _ws.OnMessage += OnMessageReceived;
        _ws.OnError += (_, e) => HandleError(e).Wait();

        _ws.OnClose += (_, e) => {
            if (e.WasClean) {
                Logging.Message("Disconnected cleanly from server");
            } else {
                Logging.Error("Network connection error.");
                Logging.Error(e.Reason);
            }
        };
    }

    /// <summary>
    /// Connect the WebSocket to the server.
    /// </summary>
    public void Connect() {
        if (IsAlive) _ws.Close();
        _ws.ConnectAsync();
    }

    /// <summary>
    /// Disconnect WebSocket.
    /// </summary>
    public void Disconnect(ushort code = 1000, string reason = "Disconnect reason not specified") => _ws.Close(code, reason);

    /// <summary>
    /// Encode and send base64 messages to the server.
    /// </summary>
    public async Task Send(BasePacket packet) {
        if (!IsAlive) {
            Logging.Warn("Queuing message & retrying connection");
            _queuedMessage = packet;
            Connect();
            return;
        }

        var encodedJson = Messaging.EncodePacket(packet);

        if (encodedJson == null) return;

        var source = new TaskCompletionSource<bool>();

        _ws.SendAsync(
            encodedJson,
            completed => {
                if (!completed) Logging.Warn("Async not sent");

                source.TrySetResult(completed);
            }
        );

        await source.Task;
    }

    /// <summary>
    /// Handle all incoming messages received from the server.
    /// </summary>
    private void OnMessageReceived(object _, MessageEventArgs e) {
        Logging.Info("Message received!");
        Logging.Info($"Contents: {e.Data}");

        try {
            ProcessMessage(e.Data).Wait();
        } catch (Exception ex) {
            Logging.Error("Failed to process message!");
            Logging.Error($"Error: {ex}");
        }
    }

    private async Task ProcessMessage(string data) {
        var em = Messaging.DecodeRawPacket(data);

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

    /// <summary>
    /// Setup WebSocket heartbeat.
    /// </summary>
    private void SetupHeartbeat() {
        _heartbeatTimer = new Timer(10 * 1000); // Ping once every 10 seconds

        _heartbeatTimer.Elapsed += Ping;
        _heartbeatTimer.AutoReset = true;
        _heartbeatTimer.Enabled = true;
    }

    /// <summary>
    /// Ping the WebSocket server to keep the connection alive.
    /// </summary>
    private void Ping(object source, ElapsedEventArgs e) => _ws?.Ping();

    /// <summary>
    /// Handle any errors that happen with the WebSocket connection.
    /// </summary>
    private async Task HandleError(ErrorEventArgs e) {
        Logging.Error("Network error happened");
        Logging.Error(e.Message);
        Logging.Error(e.Exception?.ToString());

        if (IsAlive) _ws.CloseAsync();
        else await OnError.Invoke();
    }

    private async Task HandleConnect() {
        SetupHeartbeat();

        if (_queuedMessage != null) await Send(_queuedMessage);
        _queuedMessage = null;

        await OnConnect.Invoke();
    }
}