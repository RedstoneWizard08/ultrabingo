using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Timers;
using AngryLevelLoader.Fields;
using AngryLevelLoader.Managers;
using BepInEx.Configuration;
using Newtonsoft.Json;
using Tommy;
using UltraBINGO;
using UltraBINGO.NetworkMessages;
using UltraBINGO.UI_Elements;
using UnityEngine;
using UnityEngine.UI;
using WebSocketSharp;

using static UltraBINGO.CommonFunctions;
using ErrorEventArgs = WebSocketSharp.ErrorEventArgs;
using Object = System.Object;

namespace UltrakillBingoClient;

public enum AsyncAction
{
    None,
    Host,
    Join,
    ModCheck
}

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
    public static AsyncAction pendingAction = AsyncAction.None;
    public static string pendingPassword = "";
    public static VerifyModRequest pendingVmr = null;
    
    public static ConfigEntry<string> serverURLConfig;
    public static ConfigEntry<string> serverPortConfig;
    
    public static string serverURL;
    
    private static readonly HttpClient Client = new HttpClient();
    
    public static string serverCatalogURL;
    public static string serverMapPoolCatalogURL;
    
    public static bool modlistCheck = false;
    private static string steamTicket;
    
    static WebSocket ws;
    static Timer heartbeatTimer;
    
    public static void HandleAsyncConnect()
    {
        SetupHeartbeat();
        switch (pendingAction)
        {
            case AsyncAction.Host:
                MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage("Connecting to server...");
                CreateRoom();
                break;
            case AsyncAction.Join:
                MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage("Joining game...");
                JoinGame(pendingPassword);
                break;
            case AsyncAction.ModCheck:
                SendEncodedMessage(JsonConvert.SerializeObject(pendingVmr));
                break;
            default:
                break;
        }
    }
    
    public static void SendModCheck(VerifyModRequest vmr)
    {
        pendingAction = AsyncAction.ModCheck;
        pendingVmr = vmr;
        ConnectWebSocket();
    }
    
    public static string GetSteamTicket()
    {
        return steamTicket;
    }
    public static void SetSteamTicket(string ticket)
    {
        steamTicket = ticket;
    }
    
    //Fetch the bingo map catalog from the server.
    public static async Task<string> FetchCatalog(string urlToRequest)
    {
        string url = urlToRequest;
        try
        {
            string responseTomlRaw = await Client.GetStringAsync(url);
            return responseTomlRaw;
        }
        catch (Exception e)
        {
            Logging.Error("Something went wrong while fetching from the URL");
            Logging.Error(e.Message);
            return null;
            //GetGameObjectChild(BingoMainMenu.MapCheck,"Text").GetComponent<TextMeshProUGUI>().text = "Unable to retrieve level catalog. Please check your connection.";
            //GetGameObjectChild(BingoMainMenu.MapCheck,"Button").GetComponent<Button>().interactable = false;
        }
    }
    
    //Analyse and display any maps in the bingo catalog from the server that are not already downloaded.
    public static async void analyseCatalog()
    {
        Logging.Message("--Verifying level catalog...--");
        
        List<String> missingMaps = new List<string>();
        string catalogString = await FetchCatalog(serverURL);
        if(catalogString == null)
        {
            return;
        }
        StringReader read = new StringReader(catalogString);
        
        TomlTable catalog = TOML.Parse(read);
        foreach(TomlNode node in catalog["catalog"]["levelCatalog"])
        { 
            TomlNode subNode = node.AsArray;
            if(OnlineLevelsManager.onlineLevels[subNode[1]].status != OnlineLevelField.OnlineLevelStatus.installed)
            { 
                missingMaps.Add(subNode[0]);
            }
        }
        
        Main.missingMaps = missingMaps;
        if(missingMaps.Count > 0)
        {
            Logging.Message(missingMaps.Count + " maps missing from the map pool");
            PopulateMissingMaps();
        }
        else
        {
            Logging.Message("All maps downloaded, good to go");
            BingoMainMenu.MapCheck.SetActive(false);
        }
    }
    
    //Display missing maps in the UI dialog box.
    public static void PopulateMissingMaps()
    {
        GameObject template = GetGameObjectChild(BingoMainMenu.MissingMapsList,"MapName");
        template.SetActive(false);
        
        //Clear out the previous list before displaying the new one.
        foreach(Transform child in BingoMainMenu.MissingMapsList.transform)
        {
            if(child.gameObject.name != "MapName")
            {
                GameObject.Destroy(child.gameObject);
            }
        }
        
        foreach(string map in Main.missingMaps)
        {
            GameObject mapToAdd = GameObject.Instantiate(template,template.transform.parent);
            mapToAdd.GetComponent<Text>().text = map;
            mapToAdd.SetActive(true);
        }
    }
    
    //Check if the WebSocket connection to the server is active and alive.
    public static bool IsConnectionUp()
    {
        return ws.IsAlive;
    }
    
    //Init and setup the WebSocket connection.
    public static void Initialise(string url, string port,bool isDev=false)
    {
        serverURL = isDev ? "ws://127.0.0.1:2052" : "ws://" + url + ":" + port;
        
        serverMapPoolCatalogURL = isDev ? "http://127.0.0.1/bingoMapPool.toml" : "http://"+ url + "/bingoMapPool.toml";
        
        serverCatalogURL = isDev ? "http://127.0.0.1/bingoCatalog.toml" : "http://"+ url + "/bingoCatalog.toml"; 

        ws = new WebSocket (serverURL);
        ws.EnableRedirection = true;
        ws.WaitTime = TimeSpan.FromSeconds(90);
        
        ws.OnOpen += (sender,e) => { HandleAsyncConnect(); };
        ws.OnMessage += (sender,e) => { onMessageRecieved(e); };
        ws.OnError += (sender,e) => { HandleError(e); };
        ws.OnClose += (sender,e) =>
        {
            if(e.WasClean) { Logging.Warn("Disconnected cleanly from server"); }
            else
            {
                Logging.Error("Network connection error.");
                Logging.Error(e.Reason);
            }
        };
    }
    
    //Handle any errors that happen with the WebSocket connection.
    public static async void HandleError(ErrorEventArgs e)
    {
        Logging.Warn("Network error happened");
        Logging.Error(e.Message);
        Logging.Error(e.Exception.ToString());
        
        if(GameManager.IsInBingoLevel)
        {
            MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage("Connection to the game was lost.\nExiting in 5 seconds...");
            GameManager.ClearGameVariables();
            await Task.Delay(5000);
            
            Logging.Message("Trying to return to main menu");
            SceneHelper.LoadScene("Main Menu");
        }
    }
    
    //Decode base64 messages recieved from the server.
    public static string DecodeMessage(string encodedMessage)
    {
        var base64EncodedBytes = System.Convert.FromBase64String(encodedMessage);
        return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
    }
    
    //Encode and send base64 messages to the server.
    public static void SendEncodedMessage(string jsonToEncode)
    {
        if(!ws.IsAlive)
        {
            ws.Connect();
        }
        
        byte[] encodedBytes = System.Text.Encoding.UTF8.GetBytes(jsonToEncode);
        string encodedJson = System.Convert.ToBase64String(encodedBytes);
        
        ws.Send(encodedJson);
    }
    
    public static RegisterTicket CreateRegisterTicket()
    {
        RegisterTicket rt = new RegisterTicket();
        rt.steamId = Steamworks.SteamClient.SteamId.ToString();
        rt.steamTicket = GetSteamTicket();
        rt.steamUsername = Steamworks.SteamClient.Name;
        rt.gameId = GameManager.CurrentGame.gameId;
        
        return rt;
    }
    
    public static void RegisterConnection()
    {
        Logging.Warn("Registering connection with server");
        RegisterTicket rt = CreateRegisterTicket();
        SendEncodedMessage(JsonConvert.SerializeObject(rt));
    }
    
    //Connect the WebSocket to the server.
    public static void ConnectWebSocket()
    {
        ws.ConnectAsync();
    }
    
    //Disconnect WebSocket.
    public static void DisconnectWebSocket(ushort code=1000,string reason="Disconnect reason not specified")
    {
        ws.Close(code,reason);
    }
    
    //Setup WebSocket heartbeat.
    public static void SetupHeartbeat()
    {
        Logging.Warn("Setting up heartbeat");
        heartbeatTimer = new Timer(10000); //Ping once every 10 seconds
        heartbeatTimer.Elapsed += SendPing;
        heartbeatTimer.AutoReset = true;
        heartbeatTimer.Enabled = true;
    }
    
    //Ping the WebSocket server to keep the connection alive.
    public static void SendPing(Object source, ElapsedEventArgs e)
    {
        ws.Ping();
    }
    
    //Create a new bingo game room.
    public static void CreateRoom()
    {
        CreateRoomRequest crr = new CreateRoomRequest();
        
        crr.roomName = "TestRoom";
        crr.roomPassword = "password";
        crr.maxPlayers = 8;
        crr.gameType = 1;
        crr.pRankRequired = false;
        
        crr.hostSteamName = sanitiseUsername(Steamworks.SteamClient.Name);
        crr.hostSteamId = Steamworks.SteamClient.SteamId.ToString();
        
        SendEncodedMessage(JsonConvert.SerializeObject(crr));
    }
    
    public static void JoinGame(string password)
    {
        JoinRoomRequest jrr = new JoinRoomRequest();
        jrr.password = password;
        jrr.username = sanitiseUsername(Steamworks.SteamClient.Name);
        jrr.steamId = Steamworks.SteamClient.SteamId.ToString();
        SendEncodedMessage(JsonConvert.SerializeObject(jrr));
    }
    
    public static void SendStartGameSignal(int roomId)
    {
        StartGameRequest gameRequest = new StartGameRequest();
        gameRequest.roomId = roomId;
        gameRequest.ticket = CreateRegisterTicket();
        
        SendEncodedMessage(JsonConvert.SerializeObject(gameRequest));
    }
    
    public static void SubmitRun(SubmitRunRequest srr)
    {
        SendEncodedMessage(JsonConvert.SerializeObject(srr));
    }
    
    public static void SendLeaveGameRequest(int roomId)
    {
        LeaveGameRequest leaveRequest = new LeaveGameRequest();
        leaveRequest.username = sanitiseUsername(Steamworks.SteamClient.Name);
        leaveRequest.steamId = Steamworks.SteamClient.SteamId.ToString();
        leaveRequest.roomId = roomId;
        
        SendEncodedMessage(JsonConvert.SerializeObject(leaveRequest));
    }
    
    public static void KickPlayer(string steamId)
    {
        Logging.Warn("Sending kick request");
        
        KickPlayer kp = new KickPlayer();
        kp.gameId = GameManager.CurrentGame.gameId;
        kp.playerToKick = steamId;
        kp.ticket = CreateRegisterTicket();
        
        SendEncodedMessage(JsonConvert.SerializeObject(kp));
        
    }
    
    //Handle all incoming messages received from the server.
    public static void onMessageRecieved(MessageEventArgs e)
    {
        EncapsulatedMessage em = JsonConvert.DeserializeObject<EncapsulatedMessage>(DecodeMessage(e.Data));
        switch(em.header)
        {
            case "CreateRoomResponse":
            {
                CreateRoomResponse response = JsonConvert.DeserializeObject<CreateRoomResponse>(em.contents);
                CreateRoomResponseHandler.handle(response);
                break;
            }
            case "JoinRoomResponse":
            {
                JoinRoomResponse response = JsonConvert.DeserializeObject<JoinRoomResponse>(em.contents);
                JoinRoomResponseHandler.handle(response);
                break;
            }
            case "JoinRoomNotification":
            {
                Logging.Message("Player joined");
                PlayerJoiningMessage response = JsonConvert.DeserializeObject<PlayerJoiningMessage>(em.contents);
                PlayerJoiningResponseHandler.handle(response);
                break;
            }
            case "UpdateTeamsNotif":
            {
                Logging.Message("Teams in game updated");
                UpdateTeamsNotification response = JsonConvert.DeserializeObject<UpdateTeamsNotification>(em.contents);
                UpdateTeamsNotificationHandler.handle(response);
                break;
            }
            case "RoomUpdate":
            {
                Logging.Message("Room settings have updated");
                UpdateRoomSettingsNotification response = JsonConvert.DeserializeObject<UpdateRoomSettingsNotification>(em.contents);
                UpdateRoomSettingsHandler.handle(response);
                break;
            }
            case "StartGame":
            {
                Logging.Message("Starting game");
                StartGameResponse sgr = JsonConvert.DeserializeObject<StartGameResponse>(em.contents);
                StartGameResponseHandler.handle(sgr);
                break;
            }
            case "LevelClaimed":
            {
                Logging.Message("Player claimed a level");
                LevelClaimNotification response = JsonConvert.DeserializeObject<LevelClaimNotification>(em.contents);
                LevelClaimHandler.handle(response);
                break;
            }
            case "ServerDisconnection":
            {
                Logging.Message("Server disconnected us");
                DisconnectSignal response = JsonConvert.DeserializeObject<DisconnectSignal>(em.contents);
                DisconnectSignalHandler.handle(response);
                break;
            }
            case "DisconnectNotification":
            {
                Logging.Message("Player left our game");
                DisconnectNotification response = JsonConvert.DeserializeObject<DisconnectNotification>(em.contents);
                DisconnectNotificationHandler.handle(response);
                break;
            }
            case "TimeoutNotification":
            {
                Logging.Message("Player in our game timed out");
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
            case "CheatNotification":
            {
                CheatNotification response = JsonConvert.DeserializeObject<CheatNotification>(em.contents);
                CheatNotificationHandler.handle(response);
                break;
            }
            case "ModVerificationResponse":
            {
                ModVerificationResponse response = JsonConvert.DeserializeObject<ModVerificationResponse>(em.contents);
                ModVerificationHandler.handle(response);
                break;
            }
            case "KickNotification":
            {
                KickNotification response =JsonConvert.DeserializeObject<KickNotification>(em.contents);
                KickNotificationHandler.handle(response);
                break;
            }
            case "Kicked":
            {
                MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage("You were kicked from the game.");
                NetworkManager.DisconnectWebSocket();
                KickHandler.handle();
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