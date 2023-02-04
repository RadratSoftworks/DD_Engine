using System;
using UnityEngine;

public static class GameSettings
{
    private static readonly string LanguageSettingKey = "GameLanguage";
    private static GameLanguage CachedLanguage = GameLanguage.Undefined;

    public static GameLanguage GetGameLanguage()
    {
        if (CachedLanguage != GameLanguage.Undefined)
        {
            return CachedLanguage;
        }

        string result = PlayerPrefs.GetString(LanguageSettingKey);
        if ((result == null) || !Enum.TryParse(result, out CachedLanguage))
        {
            CachedLanguage = GameLanguage.English;
            PlayerPrefs.SetString(LanguageSettingKey, CachedLanguage.ToString());
        }

        return CachedLanguage;
    }

    public static void SetGameLanguage(GameLanguage gameLang)
    {
        PlayerPrefs.SetString(LanguageSettingKey, gameLang.ToString());
        CachedLanguage = gameLang;
    }
} 