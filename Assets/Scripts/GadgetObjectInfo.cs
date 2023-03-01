using UnityEngine;

namespace DDEngine
{
    [System.Serializable]
    public class GadgetObjectInfo
    {
        public string Path;
        public int Id;
        public bool Saveable;

        [System.NonSerialized]
        public GameObject Object;
    }
}
