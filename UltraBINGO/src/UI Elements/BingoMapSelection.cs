using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TMPro;
using Tommy;
using UltraBINGO.Components;
using UltraBINGO.NetworkMessages;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static UltraBINGO.CommonFunctions;

namespace UltraBINGO.UI_Elements;

public static class BingoMapSelection {
    private static GameObject? _fetchText;
    private static GameObject? _selectedMapsTotal;

    private static GameObject? _mapContainer;
    private static GameObject? _mapContainerDescription;

    private static GameObject? _mapContainerDescriptionTitle;
    private static GameObject? _mapContainerDescriptionDesc;
    private static GameObject? _mapContainerDescriptionNumMaps;
    private static GameObject? _mapContainerDescriptionMapList;

    private static GameObject? _mapList;
    private static GameObject? _mapListButtonTemplate;

    private static GameObject? _back;

    public static int NumOfMapsTotal;

    public static HashSet<string> SelectedIds = [];
    public static List<GameObject> MapPoolButtons = [];
    private static readonly List<MapPoolContainer> AvailableMapPools = [];
    private static bool _hasAlreadyFetched;

    private static void ReturnToLobby() {
        BingoEncapsulator.BingoMapSelectionMenu?.SetActive(false);
        BingoEncapsulator.BingoLobbyScreen?.SetActive(true);
    }

    public static void ClearList(bool force = false) {
        SelectedIds.Clear();

        if (GetSceneName() == "Main Menu" && !force) return;

        MapPoolButtons.Clear();
        AvailableMapPools.Clear();
        _hasAlreadyFetched = false;
    }

    public static void UpdateNumber() {
        var gridSize = GameManager.CurrentGame.gameSettings.gridSize;
        var requiredMaps = gridSize * gridSize;

        if (_selectedMapsTotal != null)
            _selectedMapsTotal.GetComponent<TextMeshProUGUI>().text =
                $"Total maps in pool: {(NumOfMapsTotal > requiredMaps
                    ? "<color=green>"
                    : "<color=orange>")}{NumOfMapsTotal}</color>/{requiredMaps}";
    }

    private static void ToggleMapPool(ref GameObject mapPool) {
        mapPool.GetComponent<MapPoolData>().Toggle();
        var isEnabled = mapPool.GetComponent<MapPoolData>().mapPoolEnabled;
        var img = GetGameObjectChild(mapPool, "Image")?.GetComponent<Image>();

        if (img is not null)
            img.color =
                isEnabled ? new Color(1, 1, 1, 1) : new Color(1, 1, 1, 0);

        NumOfMapsTotal += mapPool.GetComponent<MapPoolData>().mapPoolNumOfMaps * (isEnabled ? 1 : -1);
        UpdateNumber();

        var mapPoolId = mapPool.GetComponent<MapPoolData>().mapPoolId;

        switch (isEnabled) {
            case true when !SelectedIds.Contains(mapPoolId):
                SelectedIds.Add(mapPoolId);
                break;
            case false when SelectedIds.Contains(mapPoolId):
                SelectedIds.Remove(mapPoolId);
                break;
        }

        var ump = new UpdateMapPool {
            gameId = GameManager.CurrentGame.gameId,
            mapPoolIds = SelectedIds.ToList(),
            ticket = NetworkManager.CreateRegisterTicket()
        };

        NetworkManager.SendEncodedMessage(JsonConvert.SerializeObject(ump)).Wait();
    }

    private static void ShowMapPoolData(PointerEventData data) {
        //Small sanity check to prevent exceptions
        if (data.pointerEnter.gameObject.name is "Image" or "Text") return;

        _mapContainerDescription?.transform.parent.gameObject.SetActive(false);
        var poolData = data.pointerEnter.transform.gameObject.GetComponent<MapPoolData>();

        if (_mapContainerDescriptionTitle != null)
            _mapContainerDescriptionTitle.GetComponent<TextMeshProUGUI>().text =
                "-- <color=orange>" + poolData.mapPoolName + "</color> --";
        if (_mapContainerDescriptionDesc != null)
            _mapContainerDescriptionDesc.GetComponent<TextMeshProUGUI>().text = poolData.mapPoolDescription;
        if (_mapContainerDescriptionNumMaps != null)
            _mapContainerDescriptionNumMaps.GetComponent<TextMeshProUGUI>().text =
                "Number of maps: <color=orange>" + poolData.mapPoolNumOfMaps + "</color>";

        var mapString = poolData.mapPoolMapList.Aggregate("", (current, map) => current + (map + "\n"));

        if (_mapContainerDescriptionMapList != null) {
            _mapContainerDescriptionMapList.GetComponent<TextMeshProUGUI>().text = mapString;
            _mapContainerDescriptionMapList.SetActive(true);
        }

        _mapContainerDescription?.transform.parent.gameObject.SetActive(true);
    }

    private static void HideMapPoolData() {
        _mapContainerDescription?.SetActive(false);
    }

    private static async Task<int> ObtainMapPools() {
        var catalogString = await NetworkManager.FetchCatalog(NetworkManager.serverMapPoolCatalogURL ?? "");

        var read = new StringReader(catalogString ?? "");
        var catalog = TOML.Parse(read);

        if (catalog["mapPools"] is TomlTable mapPools) {
            // Loop through each map pool defined in the file.
            foreach (var mapPoolKey in mapPools.Keys)
                if (mapPools[mapPoolKey] is TomlTable mapPoolTable) {
                    string name = mapPoolTable["name"];
                    string description = mapPoolTable["description"];
                    int numOfMaps = mapPoolTable["numOfMaps"];
                    var mapList = new List<string>();

                    if (mapPoolTable["maps"] is TomlArray mapArray)
                        foreach (var item in mapArray)
                            if (item is TomlString mapItem)
                                mapList.Add(mapItem.Value);

                    var currentMapPool = new MapPoolContainer(mapPoolKey, name, description, numOfMaps, mapList);
                    AvailableMapPools.Add(currentMapPool);
                    Logging.Message("Found " + mapPoolKey + " with " + numOfMaps + " maps");
                }

            Logging.Message(AvailableMapPools.Count + " mappools loaded");
            return 0;
        } else {
            Logging.Error("Failed to load map pools from file! Is the file malformed or corrupt?");
            return -1;
        }
    }

    public static async Task Setup() {
        if (_hasAlreadyFetched) return;

        Logging.Message("Fetching map pool catalog...");

        var obtainResult = await ObtainMapPools();

        if (obtainResult == 0)
            foreach (var currentMapPool in AvailableMapPools) {
                var newMapPool = Object.Instantiate(_mapListButtonTemplate, _mapListButtonTemplate?.transform.parent);
                var poolData = newMapPool?.AddComponent<MapPoolData>();

                if (poolData != null) {
                    poolData.mapPoolId = currentMapPool.mapPoolId;
                    poolData.mapPoolName = currentMapPool.mapPoolName;
                    poolData.mapPoolDescription = currentMapPool.description;
                    poolData.mapPoolNumOfMaps = currentMapPool.numOfMaps;
                    poolData.mapPoolMapList = currentMapPool.mapList;
                }

                var text = GetGameObjectChild(newMapPool, "Text");

                if (text != null) text.GetComponent<Text>().text = currentMapPool.mapPoolName;

                newMapPool?.AddComponent<EventTrigger>();

                var mouseEnter = new EventTrigger.Entry {
                    eventID = EventTriggerType.PointerEnter
                };

                mouseEnter.callback.AddListener(data => { ShowMapPoolData((PointerEventData)data); });
                newMapPool?.GetComponent<EventTrigger>().triggers.Add(mouseEnter);

                var mouseExit = new EventTrigger.Entry {
                    eventID = EventTriggerType.PointerExit
                };
                mouseExit.callback.AddListener(_ => { HideMapPoolData(); });

                newMapPool?.GetComponent<Button>().onClick.AddListener(delegate { ToggleMapPool(ref newMapPool); });

                if (newMapPool == null) continue;

                MapPoolButtons.Add(newMapPool);
                newMapPool.SetActive(true);
            }

        _hasAlreadyFetched = true;
        _fetchText?.SetActive(false);
        _mapContainer?.SetActive(true);
    }

    public static void Init(ref GameObject mapSelection) {
        _fetchText = GetGameObjectChild(mapSelection, "FetchText");

        _mapContainer = GetGameObjectChild(mapSelection, "MapContainer");
        _selectedMapsTotal = GetGameObjectChild(GetGameObjectChild(_mapContainer, "MapPoolList"), "SelectedMapsTotal");

        _mapContainerDescription =
            GetGameObjectChild(GetGameObjectChild(_mapContainer, "MapPoolDescription"), "Contents");

        _mapContainerDescriptionTitle = GetGameObjectChild(_mapContainerDescription, "Title");
        _mapContainerDescriptionDesc = GetGameObjectChild(_mapContainerDescription, "Description");
        _mapContainerDescriptionNumMaps = GetGameObjectChild(_mapContainerDescription, "NumMaps");
        _mapContainerDescriptionMapList =
            GetGameObjectChild(GetGameObjectChild(_mapContainerDescription, "MapsList"), "MapName");

        _mapList = GetGameObjectChild(
            GetGameObjectChild(
                GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(_mapContainer, "MapPoolList"), "Scroll View"),
                    "Scroll Rect"), "Content"), "List");

        _mapListButtonTemplate = GetGameObjectChild(_mapList, "MapListButton");

        _back = GetGameObjectChild(mapSelection, "Back");
        _back?.GetComponent<Button>().onClick.AddListener(ReturnToLobby);
    }
}