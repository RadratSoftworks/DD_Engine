using System;
using UnityEngine;

public static class GameSettings
{
    private const string LanguageSettingKey = "GameLanguage";
    private const string CacheThresholdKey = "CacheThreshold";
    private const string CachedChangeDeltaKey = "CachedChangeDelta";
    private const int defaultCachedChangeDelta = 10;
    private const int defaultCacheThreshold = 5;

    private static GameLanguage cachedLanguage = GameLanguage.Undefined;
    private static int cachedChangeDelta = -1;
    private static int cachedThreshold = -1;

    public static GameLanguage GameLanguage
    {
        get {
            if (cachedLanguage != GameLanguage.Undefined)
            {
                return cachedLanguage;
            }

            string result = PlayerPrefs.GetString(LanguageSettingKey);
            if ((result == null) || !Enum.TryParse(result, out cachedLanguage))
            {
                cachedLanguage = GameLanguage.English;
                PlayerPrefs.SetString(LanguageSettingKey, cachedLanguage.ToString());
            }

            return cachedLanguage;
        }
        set
        {
            PlayerPrefs.SetString(LanguageSettingKey, value.ToString());
            cachedLanguage = value;
        }
    }

    public static int CacheThreshold
    {
        get
        {
            if (cachedThreshold <= 0)
            {
                cachedThreshold = PlayerPrefs.GetInt(CacheThresholdKey, defaultCacheThreshold);
            }
            return cachedThreshold;
        }
        set
        {
            PlayerPrefs.SetInt(CacheThresholdKey, value);
            cachedThreshold = value;
        }
    }

    public static int CacheThresholdInSeconds => CacheThreshold * 60;

    public static int CachedChangeDelta
    {
        get
        {
            if (cachedChangeDelta <= 0)
            {
                cachedChangeDelta = PlayerPrefs.GetInt(CachedChangeDeltaKey, defaultCachedChangeDelta);
            }

            return cachedChangeDelta;
        }
        set
        {
            PlayerPrefs.SetInt(CachedChangeDeltaKey, value);
            cachedChangeDelta = value;
        }
    }
} 