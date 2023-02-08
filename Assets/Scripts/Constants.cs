using System;
using System.Collections.Generic;
using UnityEngine;

public class Constants
{
    public static readonly int PixelsPerUnit = 100;
    public static readonly string SortingLayerUIName = "UI";
    public static readonly int CanvasX = 240;
    public static readonly int CanvasY = 320;
    public static readonly Vector2 CanvasSize = new Vector2(CanvasX, CanvasY);

    public static readonly string OnClickScriptEventName = "onClick";
    public static readonly int SoundFrequency = 11025;
    public static readonly int SoundChannelCount = 1;

    public static readonly string TextBalloonObjectName = "TextBalloon";
    public static readonly string IconLayer = "Icon";

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
            { "newtask", "animations/New_quest.anim" }
        };
};
