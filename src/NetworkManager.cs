using System;
using System.Runtime.Remoting.Channels;
using System.Timers;
using Newtonsoft.Json;
using UltraBINGO;
using UltraBINGO.NetworkMessages;
using UltraBINGO.UI_Elements;
using UnityEngine.SceneManagement;
using WebSocketSharp;

namespace UltrakillBingoClient;

public class SendMessage
{
    public string messageType;
}

public class MessageResponse
{
    public string messageType;
}
public class PlayerNotification
{
    public string messageType;
}

public static class NetworkManager
{
    public static string serverURL = Main.IsDevelopmentBuild ? "ws://127.0.0.1:2052" : "ws://vranks.uk:2052";
    
    static WebSocket ws;
    static Timer heartbeatTimer;
    
    public static bool isConnectionUp()
    {
        return ws.IsAlive;
    }
    
    
    public static void initialise()
    {
        ws = new WebSocket (serverURL);
        ws.EnableRedirection = true;
        ws.WaitTime = TimeSpan.FromSeconds(60);
        //ws.Log.Level = LogLevel.Debug;
        
        ws.OnMessage += (sender,e) =>
        {
            NetworkManager.onMessageRecieved(e);
        };
        
        ws.OnError += (sender,e) =>
        {
            NetworkManager.handleError(e);
        };
        
        
        ws.OnClose += (sender,e) =>
        {
            Logging.Message("Server closed connection.");
            if(e.WasClean)
            {
                MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage("Disconnected from server.");
            }
            else
            {
                MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage("Server error, disconnected.");
            }
            
        };
    }
    
    public static void handleError(ErrorEventArgs e)
    {
        Logging.Warn("Network error happened");
        Logging.Error(e.Message);
        Logging.Error(e.Exception.ToString());
        
        //MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage("<color=orange>Network error</color>");
        
        
        /*if(ws.IsAlive)
        {
            Logging.Warn("Network error happened but our connection is still alive, so likely wasn't from us");
        }
        else
        {
            Logging.Error("Network error occurred on our end");

            
            if(GameManager.isInBingoLevel)
            {
                /*MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage("Connection to the server was lost.\nExitting in 5 seconds...");
                await Task.Delay(5000);
                Logging.Message("Trying to return to main menu");
                SceneManager.LoadScene("Main Menu");
            }
        }*/
    }
    
    public static string DecodeMessage(string encodedMessage)
    {
        var base64EncodedBytes = System.Convert.FromBase64String(encodedMessage);
        return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
    }
    
    public static void sendEncodedMessage(string jsonToEncode)
    {
        byte[] encodedBytes = System.Text.Encoding.UTF8.GetBytes(jsonToEncode);
        string encodedJson = System.Convert.ToBase64String(encodedBytes);
        
        ws.Send(encodedJson);
    }
    
    public static void ConnectWebSocket()
    {
        MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage("Connecting to server...");
        ws.Connect();
        setupHeartbeat();
        if(ws.IsAlive)
        {
            MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage("Connected.");
        }
    }
    public static void setupHeartbeat()
    {
        heartbeatTimer = new Timer(20000); //Ping once every 20 seconds
        heartbeatTimer.Elapsed += pingAttempt;
        heartbeatTimer.AutoReset = true;
        heartbeatTimer.Enabled = true;
    }
    
    public static void DisconnectWebSocket(ushort code=1000,string reason="Disconnect reason not specified")
    {
        ws.Close(code,reason);
    }
    
    public static void pingAttempt(Object source, ElapsedEventArgs e)
    {
        ws.Ping();
    }
    
    public static void CreateRoom()
    {
        CreateRoomRequest crr = new CreateRoomRequest();
        
        crr.roomName = "TestRoom";
        crr.roomPassword = "password";
        crr.maxPlayers = 8;
        crr.gameType = 1;
        crr.pRankRequired = false;
        
        crr.hostSteamName = Steamworks.SteamClient.Name;
        crr.hostSteamId = Steamworks.SteamClient.SteamId.ToString();
        
        sendEncodedMessage(JsonConvert.SerializeObject(crr));
    }
    
    public static void JoinGame(int roomId)
    {
        Logging.Message("Requesting to join game ID " + roomId);
        
        JoinRoomRequest jrr = new JoinRoomRequest();
        jrr.roomId = roomId;
        jrr.username = Steamworks.SteamClient.Name;
        jrr.steamId = Steamworks.SteamClient.SteamId.ToString();
        sendEncodedMessage(JsonConvert.SerializeObject(jrr));
        
    }
    
    public static void SendStartGameSignal(int roomId)
    {
        StartGameRequest gameRequest = new StartGameRequest();
        
        gameRequest.roomId = roomId;
        
        sendEncodedMessage(JsonConvert.SerializeObject(gameRequest));
    }
    
    public static void SubmitRun(SubmitRunRequest srr)
    {
        sendEncodedMessage(JsonConvert.SerializeObject(srr));
    }
    
    public static void SendLeaveGameRequest(int roomId)
    {
        LeaveGameRequest leaveRequest = new LeaveGameRequest();
        leaveRequest.username = Steamworks.SteamClient.Name;
        leaveRequest.steamId = Steamworks.SteamClient.SteamId.ToString();
        leaveRequest.roomId = roomId;
        
        sendEncodedMessage(JsonConvert.SerializeObject(leaveRequest));
    }
    
    public static void onMessageRecieved(MessageEventArgs e)
    {
        EncapsulatedMessage em = JsonConvert.DeserializeObject<EncapsulatedMessage>(DecodeMessage(e.Data));
        switch(em.header)
        {
            case "CreateRoomResponse":
            {
                Logging.Message("Got response for room create request");
                CreateRoomResponse response = JsonConvert.DeserializeObject<CreateRoomResponse>(em.contents);
                if(response == null)
                {
                    Logging.Error("Failed to deserialize");
                    break;
                }
                CreateRoomResponseHandler.handle(response);
                break;
            }
            case "JoinRoomResponse":
            {
                Logging.Message("Got response for join room request");
                JoinRoomResponse response = JsonConvert.DeserializeObject<JoinRoomResponse>(em.contents);
                JoinRoomResponseHandler.handle(response);
                break;
            }
            case "JoinRoomNotification":
            {
                Logging.Message("Player has joined our room");
                PlayerJoiningMessage response = JsonConvert.DeserializeObject<PlayerJoiningMessage>(em.contents);
                PlayerJoiningResponseHandler.handle(response);
                break;
            }
            case "RoomUpdate":
            {
                Logging.Message("Settings for our currently connected room has updated");
                UpdateRoomSettingsNotification response = JsonConvert.DeserializeObject<UpdateRoomSettingsNotification>(em.contents);
                UpdateRoomSettingsHandler.handle(response);
                break;
            }
            case "StartGame":
            {
                Logging.Message("Got start signal from server!");
                StartGameResponse sgr = JsonConvert.DeserializeObject<StartGameResponse>(em.contents);
                StartGameResponseHandler.handle(sgr);

                break;
            }
            case "LevelClaimed":
            {
                Logging.Message("Someone claimed a level");
                LevelClaimNotification response = JsonConvert.DeserializeObject<LevelClaimNotification>(em.contents);
                LevelClaimHandler.handle(response);
                break;
            }
            case "HostLeftGame":
            {
                Logging.Message("Host left our game, ending");
                HostLeftGameHandler.handle();
                break;
            }
            case "Disconnect":
            {
                Logging.Message("Received disconnect signal from server");
                DisconnectSignal response = JsonConvert.DeserializeObject<DisconnectSignal>(em.contents);
                DisconnectSignalHandler.handle(response);
                break;
            }
            case "DisconnectNotification":
            {
                Logging.Message("Someone left our game");
                DisconnectNotification response = JsonConvert.DeserializeObject<DisconnectNotification>(em.contents);
                DisconnectNotificationHandler.handle(response);
                break;
            }
            case "TimeoutNotification":
            {
                Logging.Message("Someone in our game lost connection");
                TimeoutSignal response = JsonConvert.DeserializeObject<TimeoutSignal>(em.contents);
                TimeoutSignalHandler.handle(response);
                break;
            }
            case "GameEnd":
            {
                Logging.Message("Game over!");
                EndGameSignal response = JsonConvert.DeserializeObject<EndGameSignal>(em.contents);
                EndGameSignalHandler.handle(response);
                break;
            }
            case "Pong":
            {
                //No need to do anything here, ping/pong just keeps the connection alive
                break;
            }
            default: {Logging.Warn("Unknown or unimplemented packet received from server ("+em.header+"), discarding");break;}
        }
    }
    
}