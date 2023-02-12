using System;
using System.Collections.Generic;
using Newtonsoft.Json;

[Serializable]
public class ProtectedFilePatches
{

    [JsonProperty(PropertyName = "name")]
    public string FileName;

    [JsonProperty(PropertyName = "patch")]
    public List<ProtectedFilePatch> Patches = new List<ProtectedFilePatch>();
}
