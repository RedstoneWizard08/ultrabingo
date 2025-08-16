using System.Collections.Generic;
using System.Runtime.Serialization;

namespace UltraBINGO.Types;

public class BingoMapPools {
    [DataMember(Name = "version")] public int Version;
    [DataMember(Name = "mapPools")] public Dictionary<string, BingoMapPool> MapPools = [];
}