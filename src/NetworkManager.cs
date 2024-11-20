using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Timers;
using System.Xml.Linq;
using AngryLevelLoader.Fields;
using AngryLevelLoader.Managers;
using Newtonsoft.Json;
using TMPro;
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
    
    private static readonly HttpClient Client = new HttpClient();
    
    public static string serverCatalogURL = Main.IsDevelopmentBuild ? "http://127.0.0.1/bingoCatalog.toml" : "http://vranks.uk/bingoCatalog.toml";
    public static string serverMapPoolCatalogURL = Main.IsDevelopmentBuild ? "http://127.0.0.1/bingoMapPool.toml" : "http://vranks.uk/bingoMapPool.toml";
    
    static WebSocket ws;
    static Timer heartbeatTimer;
    
    public static void prepareHttpRequest()
    {
        Client.DefaultRequestHeaders.Accept.Add( new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("text/plain"));
        Client.DefaultRequestHeaders.UserAgent.TryParseAdd("request");
        Client.Timeout = TimeSpan.FromSeconds(10);
    } 
    
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
            //GetGameObjectChild(BingoMainMenu.MapCheck,"Text").GetComponent<TextMeshProUGUI>().text = "Unable to retrieve level catalog. Please check your connection.";
            //GetGameObjectChild(BingoMainMenu.MapCheck,"Button").GetComponent<Button>().interactable = false;
            
        }
        return null;
    }
    
    public static async void analyseCatalog()
    {
        Logging.Message("--Verifying level catalog...--");
        
        List<String> missingMaps = new List<string>();

        string catalogString = await FetchCatalog(NetworkManager.serverURL);
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
            populateMissingMaps();
        }
        else
        {
            Logging.Message("All maps downloaded, good to go");
            BingoMainMenu.MapCheck.SetActive(false);
        }
        
    }
    
    public static void populateMissingMaps()
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
    
    public static bool isConnectionUp()
    {
        return ws.IsAlive;
    }
    
    public static void initialise()
    {
        ws = new WebSocket (serverURL);
        ws.EnableRedirection = true;
        ws.WaitTime = TimeSpan.FromSeconds(60);
        ws.Log.Level = LogLevel.Trace;
        
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
            if(e.WasClean)
            {
                Logging.Warn("Disconnected cleanly from server");
            }
            else
            {
                Logging.Error("Network connection error.");
                Logging.Error(e.Reason);
            }
            
        };
    }

    
    public static async void handleError(ErrorEventArgs e)
    {
        Logging.Warn("Network error happened");
        Logging.Error(e.Message);
        Logging.Error(e.Exception.ToString());
        
        if(GameManager.isInBingoLevel)
        {
            MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage("Connection to the game was lost.\nExitting in 5 seconds...");
            GameManager.clearGameVariables();
            await Task.Delay(5000);
            Logging.Message("Trying to return to main menu");
            SceneHelper.LoadScene("Main Menu");
        }
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
        
        crr.hostSteamName = sanitiseUsername(Steamworks.SteamClient.Name);
        crr.hostSteamId = Steamworks.SteamClient.SteamId.ToString();
        
        sendEncodedMessage(JsonConvert.SerializeObject(crr));
    }
    
    public static void JoinGame(int roomId)
    {
        Logging.Message("Requesting to join game ID " + roomId);
        
        JoinRoomRequest jrr = new JoinRoomRequest();
        jrr.roomId = roomId;
        jrr.username = sanitiseUsername(Steamworks.SteamClient.Name);
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
        leaveRequest.username = sanitiseUsername(Steamworks.SteamClient.Name);
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
            case "UpdateTeamsNotif":
            {
                Logging.Message("Teams in our room were updated");
                UpdateTeamsNotification response = JsonConvert.DeserializeObject<UpdateTeamsNotification>(em.contents);
                UpdateTeamsNotificationHandler.handle(response);
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
            case "ServerDisconnection":
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
            case "CheatNotification":
            {
                Logging.Message("Someone in our game tried to activate cheats lol");
                CheatNotification response = JsonConvert.DeserializeObject<CheatNotification>(em.contents);
                CheatNotificationHandler.handle(response);
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