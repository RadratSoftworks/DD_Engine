﻿using System;
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
};
