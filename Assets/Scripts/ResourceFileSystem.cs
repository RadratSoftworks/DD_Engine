using System;
using System.Collections.Generic;
using System.IO;

using UnityEngine;

namespace DDEngine
{
    public class ResourceFileSystem : FileSystem
    {
        private Dictionary<string, byte[]> assetDatas = new(StringComparer.OrdinalIgnoreCase);

        private string GetFullResourcePath(string path)
        {
            return Path.Join("Resources/", path);
        }

        public bool Exists(string path)
        {
            return (Resources.Load<TextAsset>(GetFullResourcePath(path)) != null);
        }

        public Stream OpenFile(string path)
        {
            if (assetDatas.TryGetValue(path, out byte[] assetData))
            {
                return new MemoryStream(assetData);
            }

            TextAsset result = Resources.Load<TextAsset>(GetFullResourcePath(path));
            if (result == null)
            {
                return null;
            }

            byte[] dataBuffer = result.bytes;
            assetDatas.Add(path, dataBuffer);

            return new MemoryStream(dataBuffer);
        }
    }
}
