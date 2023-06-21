using System;
using System.Collections.Generic;
using System.IO;

using UnityEngine;

namespace DDEngine
{
    public class ResourceFileSystem : FileSystem
    {
        protected Dictionary<string, byte[]> assetDatas = new(StringComparer.OrdinalIgnoreCase);

        protected string GetFullResourcePath(string path)
        {
            return Path.Join("Full/", path);
        }

        public bool Exists(string path)
        {
            return (Resources.Load<TextAsset>(GetFullResourcePath(path)) != null);
        }

        public virtual Stream OpenFile(string path)
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
