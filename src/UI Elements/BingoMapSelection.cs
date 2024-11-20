using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TMPro;
using Tommy;
using UltraBINGO;
using UltraBINGO.Components;
using UltraBINGO.NetworkMessages;
using UltraBINGO.UI_Elements;
using UltrakillBingoClient;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

using static UltraBINGO.CommonFunctions;

public class BingoMapSelection
{
    public static GameObject FetchText;
    public static GameObject SelectedMapsTotal;
    
    public static GameObject MapContainer;
    public static GameObject MapContainerDescription;
    
    public static GameObject MapContainerDescriptionTitle;
    public static GameObject MapContainerDescriptionDesc;
    public static GameObject MapContainerDescriptionNumMaps;
    public static GameObject MapContainerDescriptionMapList;
    
    public static GameObject MapList;
    public static GameObject MapListButtonTemplate;
    
    public static GameObject Back;
    
    public static int NumOfMapsTotal = 0;
    
    public static HashSet<string> SelectedIds = new HashSet<string>();
    public static List<GameObject> MapPoolButtons = new List<GameObject>();
    public static List<MapPoolContainer> AvailableMapPools = new List<MapPoolContainer>();
    public static bool HasAlreadyFetched = false;
    
    public static void ReturnToLobby()
    {
        BingoEncapsulator.BingoMapSelectionMenu.SetActive(false);
        BingoEncapsulator.BingoLobbyScreen.SetActive(true);
    }
    
    public static void UpdateNumber()
    {
        SelectedMapsTotal.GetComponent<TextMeshProUGUI>().text = "Total maps in pool: <color=orange>"+NumOfMapsTotal+"</color>";
    }
    
    public static void ToggleMapPool(ref GameObject mapPool)
    {
        mapPool.GetComponent<MapPoolData>().Toggle();
        bool isEnabled = mapPool.GetComponent<MapPoolData>().mapPoolEnabled;
        
        GetGameObjectChild(mapPool,"Image").GetComponent<Image>().color = (isEnabled ? new Color(1,1,1,1) :new Color(1,1,1,0));
        
        NumOfMapsTotal += mapPool.GetComponent<MapPoolData>().mapPoolNumOfMaps*(isEnabled ? 1 : -1);
        UpdateNumber();
        
        string mapPoolId = mapPool.GetComponent<MapPoolData>().mapPoolId;
        if(isEnabled && !SelectedIds.Contains(mapPoolId))
        {
            SelectedIds.Add(mapPoolId);
        }
        else if (!isEnabled && SelectedIds.Contains(mapPoolId))
        {
            SelectedIds.Remove(mapPoolId);
        }
        
        UpdateMapPool ump = new UpdateMapPool();
        ump.gameId = GameManager.CurrentGame.gameId;
        ump.mapPoolIds = BingoMapSelection.SelectedIds.ToList();
        NetworkManager.sendEncodedMessage(JsonConvert.SerializeObject(ump));
    }
    
    public static void ShowMapPoolData(PointerEventData data)
    {
        //Small sanity check to prevent exceptions
        if(data.pointerEnter.gameObject.name != "Image" && data.pointerEnter.gameObject.name != "Text")
        {
            MapContainerDescription.transform.parent.gameObject.SetActive(false);
            MapPoolData poolData = data.pointerEnter.transform.gameObject.GetComponent<MapPoolData>();
            
            MapContainerDescriptionTitle.GetComponent<TextMeshProUGUI>().text = "-- <color=orange>" + poolData.mapPoolName + "</color> --";
            MapContainerDescriptionDesc.GetComponent<TextMeshProUGUI>().text = poolData.mapPoolDescription;
            MapContainerDescriptionNumMaps.GetComponent<TextMeshProUGUI>().text = "Number of maps: <color=orange>" + poolData.mapPoolNumOfMaps + "</color>";
            
            string mapString = "";
            foreach(string map in poolData.mapPoolMapList)
            {
                mapString += map + "\n";
            }

            MapContainerDescriptionMapList.GetComponent<TextMeshProUGUI>().text = mapString;
            MapContainerDescriptionMapList.SetActive(true);
            MapContainerDescription.transform.parent.gameObject.SetActive(true);
        }
    }
    
    public static void HideMapPoolData()
    {
        MapContainerDescription.SetActive(false);
    }
    
    public static async Task<int> ObtainMapPools()
    {
        string catalogString = await NetworkManager.FetchCatalog(NetworkManager.serverMapPoolCatalogURL);
        
        StringReader read = new StringReader(catalogString);
        TomlTable catalog = TOML.Parse(read);  
        
        Logging.Message("Scanning received map pool catalog");
        if (catalog["mapPools"] is TomlTable mapPools)
        {
            // Loop through each map pool defined in the file.
            foreach (var mapPoolKey in mapPools.Keys)
            {
                if(mapPools[mapPoolKey] is TomlTable mapPoolTable)
                {
                    string name = mapPoolTable["name"];
                    string description = mapPoolTable["description"];
                    int numOfMaps = mapPoolTable["numOfMaps"];
                    List<string> mapList = new List<string>();

                    if(mapPoolTable["maps"] is TomlArray mapArray)
                    {
                        foreach (var item in mapArray)
                        {
                            if (item is TomlString mapItem)
                            {
                                mapList.Add(mapItem.Value);
                            }
                        }
                    }
                    MapPoolContainer currentMapPool = new MapPoolContainer(mapPoolKey,name,description,numOfMaps,mapList);
                    AvailableMapPools.Add(currentMapPool);
                    Logging.Message("Found " + mapPoolKey + " with " + numOfMaps + " maps");
                }
            }
            Logging.Message(AvailableMapPools.Count + " mappools loaded");
            return 0;
        }
        else
        {
            Logging.Error("Failed to load map pools from file! Is the file malformed or corrupt?");
            return -1;
        }
    }
    
    public static async void Setup()
    {
        if(!HasAlreadyFetched)
        {
            Logging.Message("Fetching map pool catalog...");
            int obtainResult = await ObtainMapPools();
            if(obtainResult == 0)
            {
                foreach(MapPoolContainer currentMapPool in AvailableMapPools)
                {
                    GameObject newMapPool = GameObject.Instantiate(MapListButtonTemplate,MapListButtonTemplate.transform.parent);
                    
                    MapPoolData poolData = newMapPool.AddComponent<MapPoolData>();
                    poolData.mapPoolId = currentMapPool.mapPoolId;
                    poolData.mapPoolName = currentMapPool.mapPoolName;
                    poolData.mapPoolDescription = currentMapPool.description;
                    poolData.mapPoolNumOfMaps = currentMapPool.numOfMaps;
                    poolData.mapPoolMapList = currentMapPool.mapList;
                    
                    GetGameObjectChild(newMapPool,"Text").GetComponent<Text>().text = currentMapPool.mapPoolName;
                    
                    newMapPool.AddComponent<EventTrigger>();
                    EventTrigger.Entry mouseEnter = new EventTrigger.Entry();
                    mouseEnter.eventID = EventTriggerType.PointerEnter;
                    mouseEnter.callback.AddListener((data) =>
                    {
                        ShowMapPoolData((PointerEventData)data);
                    });
                    newMapPool.GetComponent<EventTrigger>().triggers.Add(mouseEnter);
                
                    EventTrigger.Entry mouseExit = new EventTrigger.Entry();
                    mouseExit.eventID = EventTriggerType.PointerExit;
                    mouseExit.callback.AddListener((data) =>
                    {
                        HideMapPoolData();
                    });
                    
                    newMapPool.GetComponent<Button>().onClick.AddListener(delegate
                    {
                        ToggleMapPool(ref newMapPool);
                    });
                    
                    MapPoolButtons.Add(newMapPool);
                    newMapPool.SetActive(true);
                }
            }
            HasAlreadyFetched = true;
            FetchText.SetActive(false);
            MapContainer.SetActive(true);
        }
    }
    
    public static void Init(ref GameObject MapSelection)
    {
        FetchText = GetGameObjectChild(MapSelection,"FetchText");
        
        MapContainer = GetGameObjectChild(MapSelection,"MapContainer");
        SelectedMapsTotal = GetGameObjectChild(GetGameObjectChild(MapContainer,"MapPoolList"),"SelectedMapsTotal");
        
        MapContainerDescription = GetGameObjectChild(GetGameObjectChild(MapContainer,"MapPoolDescription"),"Contents");

        MapContainerDescriptionTitle = GetGameObjectChild(MapContainerDescription,"Title");
        MapContainerDescriptionDesc = GetGameObjectChild(MapContainerDescription,"Description");
        MapContainerDescriptionNumMaps = GetGameObjectChild(MapContainerDescription,"NumMaps");
        MapContainerDescriptionMapList = GetGameObjectChild(GetGameObjectChild(MapContainerDescription,"MapsList"),"MapName");
        
        MapList = GetGameObjectChild(GetGameObjectChild(MapContainer,"MapPoolList"),"List");
        MapListButtonTemplate = GetGameObjectChild(MapList,"MapListButton");
        Back = GetGameObjectChild(MapSelection,"Back");
        Back.GetComponent<Button>().onClick.AddListener(delegate
        {
            ReturnToLobby();
        });
    }
}