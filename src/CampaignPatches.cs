using UltrakillBingoClient;
using UnityEngine;

using static UltraBINGO.CommonFunctions;

namespace UltraBINGO;

public static class CampaignPatches
{
    public static void Apply(string levelName)
    {
        switch(levelName)
        {
            case "Level 0-2":
            {
                GameObject pedestal = GetGameObjectChild(GetGameObjectChild(GetInactiveRootObject("5B - Secret Arena"),"5B Nonstuff"),"Altar");
                pedestal.SetActive(false);
                break;
            }
            case "Level 1-1":
            {
                GameObject fountain = GetGameObjectChild(GetGameObjectChild(GetInactiveRootObject("1 - First Field"),"1 Stuff"),"Fountain");
                fountain.GetComponent<Door>().enabled = false;
                break;
            }
            case "Level 2-3":
            {
                GameObject box = GetGameObjectChild(GetGameObjectChild(GetInactiveRootObject("4 - End Hallway"),"4 Nonstuff"),"Electricitybox");
                box.SetActive(false);
                break;
            }
            case "Level 3-1":
            {
                GameObject door = GetGameObjectChild(GetGameObjectChild(GetInactiveRootObject("6S - P Door"),"6S Nonstuff"),"HellgateLimboSwitch Variant");
                door.GetComponent<LimboSwitchLock>().enabled = false;
                break;
            }
            case "Level 4-2":
            {
                GameObject button = GetInactiveRootObject("GreedSwitch Variant");
                button.SetActive(false);
                break;   
            }
            case "Level 5-1":
            {
                GameObject door = GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(GetInactiveRootObject("2 - Elevator"),"2B Secret"),"FinalRoom 1"),"FinalDoor");
                door.GetComponent<FinalDoor>().enabled = false;
                break;   
            }
            case "Level 6-2":
            {
                GameObject door = GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(GetInactiveRootObject("1S - P Door"),"6S Nonstuff"),"ToActivate"),"HellgateLimboSwitch Variant");
                door.GetComponent<LimboSwitchLock>().enabled = false;
                break;   
            }
            case "Level 7-3":
            {
                GameObject plane = GetGameObjectChild(GetGameObjectChild(GetInactiveRootObject("Doors"),"1 -> S"),"Plane (1)");
                plane.GetComponent<Flammable>().enabled = false;
                break;   
            }
            default:
            {
                Logging.Warn("Not in campaign level with alt exit");
                break;
            }
        }
    }
}