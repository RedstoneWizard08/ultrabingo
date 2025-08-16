using System.Collections.Generic;
using System.Runtime.Serialization;

namespace UltraBINGO.Types;

public class BingoMapPool {
    [DataMember(Name = "name")] public string Name = "";
    [DataMember(Name = "description")] public string Description = "";
    [DataMember(Name = "numOfMaps")] public int NumOfMaps;
    [DataMember(Name = "maps")] public List<string> Maps = [];
}
