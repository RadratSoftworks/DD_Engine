using System;
using System.Collections.Generic;
using UnityEngine;

namespace DDEngine
{
    [Serializable]
    public class GameSave
    {
        public const int CurrentVersion = 2;

        // Correspond location name -  Value dictionary
        public Dictionary<string, Dictionary<string, string>> ActionValues;

        // Can be minigame, or control set. If mini-game, then location offset is irrelevant
        public string CurrentControlSetPath;
        public Vector2 CurrentLocationOffset;

        public GadgetObjectInfo[] Gadgets;

        public int Version = -1;

        // Compatibility
        public string CurrentGadgetPath;
        public int CurrentGadgetId;

        public GameSave()
        {
            ActionValues = new Dictionary<string, Dictionary<string, string>>();
        }
    }
}