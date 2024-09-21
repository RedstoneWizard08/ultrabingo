using Newtonsoft.Json;
using UltraBINGO;
using UltraBINGO.NetworkMessages;
using UltraBINGO.UI_Elements;
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
    
    
    public static void initialise()
    {
        ws = new WebSocket (serverURL);
        ws.EnableRedirection = true;
        //ws.Log.Level = LogLevel.Debug;
        
        ws.OnMessage += (sender,e) =>
        {
            NetworkManager.onMessageRecieved(e);
        };
        
        ws.OnClose += (sender,e) =>
        {
            Logging.Message("Connection closed");
        };
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
        ws.Connect();
    }
    
    public static void DisconnectWebSocket(ushort code=1000,string reason="Disconnect reason not specified")
    {
        ws.Close(code,reason);
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
        sendEncodedMessage(JsonConvert.SerializeObject(jrr));
        
        Logging.Message("Request sent");
        
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
        Logging.Message("Incoming message from server");
        EncapsulatedMessage em = JsonConvert.DeserializeObject<EncapsulatedMessage>(DecodeMessage(e.Data));
        Logging.Message(em.header);
        
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
                Logging.Message("Got signal from server to start game we're in!");
                StartGameResponse sgr = JsonConvert.DeserializeObject<StartGameResponse>(em.contents);
                GameManager.currentTeam = sgr.teamColor;
                GameManager.teammates = sgr.teammates;
                Logging.Message("We are on the "+GameManager.currentTeam + " team");
                
                BingoMenuController.StartGame();
                break;
            }
            case "LevelClaimed":
            {
                Logging.Message("Someone claimed a level");
                LevelClaimNotification response = JsonConvert.DeserializeObject<LevelClaimNotification>(em.contents);
                LevelClaimHandler.handle(response);
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
            case "GameEnd":
            {
                Logging.Message("Game over!");
                EndGameSignal response = JsonConvert.DeserializeObject<EndGameSignal>(em.contents);
                EndGameSignalHandler.handle(response);
                break;
            }
            default: {Logging.Warn("Unknown or unimplemented packet received from server ("+em.header+"), discarding");break;}
        }
    }
    
}