using UnityEngine;

namespace DDEngine
{
    public class GadgetObjectInfo
    {
        public string Path;
        public int Id;
        public bool Saveable;

        [System.NonSerialized]
        public GameObject Object;
    }
}
