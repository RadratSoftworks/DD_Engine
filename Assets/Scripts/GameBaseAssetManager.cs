using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace DDEngine
{
    public class GameBaseAssetManager<T> : MonoBehaviour where T : class
    {
        private struct CachedAsset
        {
            public T Value;
            public float Timestamp;

            public CachedAsset(T value, float timestamp)
            {
                this.Value = value;
                this.Timestamp = timestamp;
            }
        }

        private Dictionary<string, CachedAsset> cachedAssets = new Dictionary<string, CachedAsset>(StringComparer.OrdinalIgnoreCase);

        protected virtual bool IsAssetSuitableForPrunge(T asset)
        {
            return true;
        }

        protected virtual void OnRemovalFromCache(T asset)
        {

        }

        protected T GetFromCache(string key)
        {
            if (!cachedAssets.ContainsKey(key))
            {
                return null;
            }

            CachedAsset asset = cachedAssets[key];
            float currentTime = Time.realtimeSinceStartup;

            if (currentTime - asset.Timestamp > GameSettings.CachedChangeDelta)
            {
                asset.Timestamp = currentTime;
                cachedAssets[key] = asset;
            }

            return asset.Value;
        }

        protected void AddToCache(string key, T value)
        {
            if (cachedAssets.ContainsKey(key))
            {
                throw new Exception(string.Format("The key: {0} already existed in cache dictionary!", key));
            }

            cachedAssets.Add(key, new CachedAsset(value, Time.realtimeSinceStartup));
        }

        public void PruneCache()
        {
            if (GameSettings.CacheThreshold == 0)
            {
                return;
            }

            float currentTime = Time.realtimeSinceStartup;
            int thresholdInSeconds = GameSettings.CacheThresholdInSeconds;

            foreach (var pairVal in cachedAssets.Where(pair => IsAssetSuitableForPrunge(pair.Value.Value) && (currentTime - pair.Value.Timestamp > thresholdInSeconds)).ToList())
            {
                cachedAssets.Remove(pairVal.Key);
                OnRemovalFromCache(pairVal.Value.Value);
            }
        }
    }
}