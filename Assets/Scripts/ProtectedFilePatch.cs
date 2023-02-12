using System;
using Newtonsoft.Json;

[Serializable]
public class ProtectedFilePatch
{
    [JsonProperty(PropertyName = "offset")]
    public string Offset;

    [JsonProperty(PropertyName = "newBytes")]
    public string BytesInHex;

    [JsonProperty(PropertyName = "context")]
    public string Context;
}