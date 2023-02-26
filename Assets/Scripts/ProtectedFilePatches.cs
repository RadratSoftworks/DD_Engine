using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace DDEngine
{
    [Serializable]
    public class ProtectedFilePatches
    {

        [JsonProperty(PropertyName = "name")]
        public string FileName;

        [JsonProperty(PropertyName = "patch")]
        public List<ProtectedFilePatch> Patches = new List<ProtectedFilePatch>();
    }
}
