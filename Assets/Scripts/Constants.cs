using System.Collections.Generic;
using UnityEngine;

namespace DDEngine
{
    public class Constants
    {
        public const int PixelsPerUnit = 100;
        public const string SortingLayerUIName = "UI";
        public const int CanvasX = 240;
        public const int CanvasY = 320;
        public static readonly Vector2 CanvasSize = new Vector2(CanvasX, CanvasY);

        public const string OnClickScriptEventName = "onClick";
        public const string OnFocusScriptEventName = "onFocus";
        public const int SoundFrequency = 11025;
        public const int SoundChannelCount = 1;
        public const int BaseGameFps = 31;

        public const string TextBalloonObjectName = "TextBalloon";
        public const string IconLayer = "Icon";
        public const string Anonymous = "Anonymous";
        public const string SaveExistsVarName = "save_exists";
        public const string CompletedGameVarName = "completed_game";

        public const string SwitchNgiUri = "https://anni.12l1.com";
        public const int TotalGameLayers = 27;  // Include background

        public static readonly Dictionary<GameLanguage, string> GameLanguageToResourceLanguageCodeDict =
            new Dictionary<GameLanguage, string>()
            {
            { GameLanguage.English, "en" },
            { GameLanguage.French, "fr" },
            { GameLanguage.Deutsch, "de" },
            { GameLanguage.Italian, "it" },
            { GameLanguage.Spanish, "es" },
            { GameLanguage.SimplifiedChinese, "cn" },
            { GameLanguage.TraditionalChinese, "tw" }
            };

        public static readonly Dictionary<string, string> IconNameToAnimationPath =
            new Dictionary<string, string>()
            {
            { "plusinv", "animations/inv.anim" },
            { "plusmap", "animations/Plus_map.anim" },
            { "newtask", "animations/New_quest.anim" },
            { "taskdone", "animations/Check.anim" }
        };
    };
}