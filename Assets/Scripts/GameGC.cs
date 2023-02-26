using UnityEngine;

namespace DDEngine
{
    public class GameGC
    {
        private const float NextUnloadDelta = 180.0f;
        private static float nextUnloadedAssetsTime = Time.realtimeSinceStartup;

        public static void TryUnloadUnusedAssets()
        {
            float currentTime = Time.realtimeSinceStartup;
            if (currentTime >= nextUnloadedAssetsTime)
            {
                Resources.UnloadUnusedAssets();
                nextUnloadedAssetsTime = currentTime + NextUnloadDelta;
            }
        }
    }
}