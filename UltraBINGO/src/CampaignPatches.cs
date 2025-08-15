using static UltraBINGO.Util.CommonFunctions;

namespace UltraBINGO;

public static class CampaignPatches {
    public static void Apply(string levelName) {
        switch (levelName) {
            case "Level 0-2": {
                var pedestal = FindObjectWithInactiveRoot(
                    "5B - Secret Arena",
                    "5B Nonstuff",
                    "Altar (Blue Skull) Variant"
                );

                pedestal?.SetActive(false);

                break;
            }

            case "Level 1-1": {
                var fountain =
                    GetGameObjectChild(
                        GetGameObjectChild(GetInactiveRootObject("1 - First Field"), "1 Stuff"),
                        "Fountain"
                    );

                if (fountain != null) {
                    fountain.GetComponent<Door>().enabled = false;

                    var cylinder = fountain.transform.GetChild(0).gameObject;
                    var cylinder2 = fountain.transform.GetChild(1).gameObject;

                    cylinder.SetActive(false);
                    cylinder2.SetActive(false);
                }

                var finalRoom =
                    GetGameObjectChild(
                        GetGameObjectChild(GetInactiveRootObject("1 - First Field"), "1 Nonstuff"),
                        "FinalRoom 1"
                    );

                if (finalRoom != null) {
                    finalRoom.SetActive(false);
                    finalRoom.GetComponent<FinalRoom>().enabled = false;
                }

                var pit = GetGameObjectChild(
                    GetGameObjectChild(GetInactiveRootObject("1 - First Field"), "1 Nonstuff"),
                    "FinalRoom SecretEntrance"
                );

                pit?.SetActive(false);

                break;
            }

            case "Level 2-3": {
                var box = GetGameObjectChild(
                    GetGameObjectChild(GetInactiveRootObject("4 - End Hallway"), "4 Nonstuff"),
                    "ElectricityBox"
                );

                box?.SetActive(false);

                var secretEntrance =
                    GetGameObjectChild(
                        GetGameObjectChild(GetInactiveRootObject("2 - Sewer Arena"), "2 Nonstuff"),
                        "Secret Level Entrance"
                    );

                secretEntrance?.SetActive(false);

                break;
            }

            case "Level 3-1": {
                var door = GetGameObjectChild(
                    GetGameObjectChild(GetInactiveRootObject("6S - P Door"), "6S Nonstuff"),
                    "HellgateLimboSwitch Variant"
                );

                if (door != null) door.GetComponent<LimboSwitchLock>().enabled = false;

                break;
            }

            case "Level 4-2": {
                GetInactiveRootObject("GreedSwitch Variant")?.SetActive(false);
                break;
            }

            case "Level 5-1": {
                var door = GetGameObjectChild(
                    GetGameObjectChild(
                        GetGameObjectChild(GetInactiveRootObject("2 - Elevator"), "2B Secret"),
                        "FinalRoom 1"
                    ),
                    "FinalDoor"
                );

                if (door != null) door.GetComponent<FinalDoor>().enabled = false;

                var finalroom =
                    GetGameObjectChild(
                        GetGameObjectChild(GetInactiveRootObject("2 - Elevator"), "2B Secret"),
                        "FinalRoom 1"
                    );

                finalroom?.SetActive(false);

                var pit = GetGameObjectChild(
                    GetGameObjectChild(GetInactiveRootObject("2 - Elevator"), "2B Secret"),
                    "FinalRoom SecretEntrance"
                );

                pit?.SetActive(false);

                break;
            }

            case "Level 6-2": {
                var door = GetGameObjectChild(
                    GetGameObjectChild(
                        GetGameObjectChild(GetInactiveRootObject("1S - P Door"), "6S Nonstuff"),
                        "ToActivate"
                    ),
                    "HellgateLimboSwitch Variant"
                );

                if (door != null) door.GetComponent<LimboSwitchLock>().enabled = false;

                break;
            }

            case "Level 7-3": {
                var plane = GetGameObjectChild(
                    GetGameObjectChild(GetInactiveRootObject("Doors"), "1 -> S"),
                    "Plane (1)"
                );

                if (plane != null) plane.GetComponent<Flammable>().enabled = false;

                var pit = GetGameObjectChild(
                    GetGameObjectChild(
                        GetGameObjectChild(GetInactiveRootObject("2 - Garden Maze"), "Secret"),
                        "FinalRoom 1"
                    ),
                    "Pit"
                );

                pit?.SetActive(false);

                var pit2 = GetGameObjectChild(
                    GetGameObjectChild(GetInactiveRootObject("2 - Garden Maze"), "Secret"),
                    "FinalRoom SecretEntrance"
                );

                pit2?.SetActive(false);

                break;
            }
        }
    }
}