use crate::pools;

pools! [
    {
        id = CAMPAIGN,
        key = "campaign",
        name = "Campaign",
        desc = "All official campaign levels.\nPrime Sanctums and Encores are available as seperate map pools.",

        maps = [
            "INTO THE FIRE" => "Level 0-1",
            "THE MEATGRINDER" => "Level 0-2",
            "DOUBLE DOWN" => "Level 0-3",
            "A ONE-MACHINE ARMY" => "Level 0-4",
            "CERBERUS" => "Level 0-5",
            "HEART OF THE SUNRISE" => "Level 1-1",
            "THE BURNING WORLD" => "Level 1-2",
            "HALL OF SACRED REMAINS" => "Level 1-3",
            "CLAIR DE LUNE" => "Level 1-4",
            "BRIDGEBURNER" => "Level 2-1",
            "DEATH AT 20,000 VOLTS" => "Level 2-2",
            "SHEER HEART ATTACK" => "Level 2-3",
            "COURT OF THE CORPSE KING" => "Level 2-4",
            "BELLY OF THE BEAST" => "Level 3-1",
            "IN THE FLESH" => "Level 3-2",
            "SLAVES TO POWER" => "Level 4-1",
            "GOD DAMN THE SUN" => "Level 4-2",
            "A SHOT IN THE DARK" => "Level 4-3",
            "CLAIR DE SOLEIL" => "Level 4-4",
            "IN THE WAKE OF POSEIDON" => "Level 5-1",
            "WAVES OF THE STARLESS SEA" => "Level 5-2",
            "SHIP OF FOOLS" => "Level 5-3",
            "LEVIATHAN" => "Level 5-4",
            "CRY FOR THE WEEPER" => "Level 6-1",
            "AESTHETICS OF HATE" => "Level 6-2",
            "GARDEN OF FORKING PARKS" => "Level 7-1",
            "LIGHT UP THE NIGHT" => "Level 7-2",
            "NO SOUND, NO MEMORY" => "Level 7-3",
            "...LIKE ANTENNAS TO HEAVEN" => "Level 7-4",
        ],
    },

    {
        id = PRIME_SANCTUMS,
        key = "primeSanctums",
        name = "Prime Sanctums",
        desc = "Prime Sanctums from the official campaign.",

        maps = [
            "SOUL SURVIVOR" => "Level P-1",
            "WAIT OF THE WORLD" => "Level P-2",
        ],
    },

    {
        id = ENCORES,
        key = "encore",
        name = "Encores",
        desc = "Encore levels from the official campaign.",

        maps = [
            "THIS HEAT, AN EVIL HEAT" => "Level 0-E",
            "...THEN FELL THE ASHES" => "Level 1-E",
        ],
    },

    angry {
        id = ANGRY_STANDARD,
        key = "angryStandard",
        name = "Angry (Standard)",
        desc = "A collection of Angry levels suitable for newer players.\nMaps are shorter in length and easier in combat difficulty.",

        maps = [
            "FUN SIZED HERESY" => "fnchannel.fsheresy.fireswithinfires" => "ae6eeb9c8e3741441986985171f75b56",
            "WALLS OF WICKED DREAMS" => "robi.heresy.wowd" => "309b60fc131a95d49921d31c0ec7560f",
            "RE-LUDE" => "elequacity.relude.heatseeker" => "97220d9c4569778488734e80e0daa734",
            "A PRIMUM MOBILE" => "YoullTim.APrimumMobile1" => "fd4229c3005a73744ab5a4597cfaf75f",
            "EPITAPH" => "willem1321-epitaph" => "a9137dc898362c44593878839cd899a6",
            "V3'S SHOWDOWN - PHASE 1" => "t.trinity.v3" => "91a952cfd5574ef47bf624e09c311260",
        ],
    },

    angry {
        id = ANGRY_HARDCORE,
        key = "angryHardcore",
        name = "Angry (Hardcore)",
        desc = "A collection of Angry levels which are greater in combat difficulty and/or length.\nRecommended for players looking for longer games.",

        maps = [
            "FREEDOM DIVE" => "aaaa.aaaaaa.aaaaaeeeeeaaa" => "f907c5991d40a2a48941d1c1dde860c7",
            "FRAUDULENCE - FOOLS GOLD" => "Spelunky.FRAUDULENCE_FIRST" => "033ce9db13ba74d4aa07bdae343d49c2",
            "FRAUDULENCE - HIGH SOCIETY" => "Spelunky.FRAUDULENCE_SECOND" => "033ce9db13ba74d4aa07bdae343d49c2",
            "HEART OF THE MACHINE" => "bobot.hellfacility.hotm" => "255ce156d5ae53c449106c1a31ed384a",
            "V3'S SHOWDOWN - PHASE 2" => "trinity.v3mech" => "91a952cfd5574ef47bf624e09c311260",
        ],
    },

    angry {
        id = TESTING,
        key = "testing",
        name = "Angry (Testing)",
        desc = "A collection of maps available for testing.\nMaps in this pool may vary greatly in length & difficulty, and aren't guaranteed to be added in future.\n\n<color=orange>Use this map pool with the intention of testing & giving feedback.</color>",

        maps = [
            "MACHINATION - THE BLASTPIPE" => "MachinationM-1V2" => "a1f9bb7c418870d499158f9c2b55731e",
            "OPERETTAS - WHAT COULD HAVE BEEN" => "tuli.snowlimbo" => "7b9762c4a51906e4ca36acfcbcdbde3e",
            "TOWER OF STEEL - TOTAL WAR" => "tos_1" => "adb0ef3e5cc07c84889dd27f7898af96",
        ],
    },
];
