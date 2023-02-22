using System;
using System.Linq;
using UnityEngine;

public static class GameSettings
{
    private const string LanguageSettingKey = "GameLanguage";
    private const string CacheThresholdKey = "CacheThreshold";
    private const string CachedChangeDeltaKey = "CachedChangeDelta";
    private const string GameVolumeKey = "GameVolume";
    private const string TextSpeedKey = "TextSpeed";
    private const int defaultCachedChangeDelta = 10;
    private const int defaultCacheThreshold = 5;

    private static GameLanguage cachedLanguage = GameLanguage.Undefined;
    private static GameTextSpeed textSpeed = GameTextSpeed.Undefined;
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
    public static GameTextSpeed TextSpeed
    {
        get
        {
            if (textSpeed == GameTextSpeed.Undefined)
            {
                string result = PlayerPrefs.GetString(TextSpeedKey);
                if ((result == null) || !Enum.TryParse(result, true, out textSpeed))
                {
                    textSpeed = GameTextSpeed.Normal;
                    PlayerPrefs.SetString(TextSpeedKey, textSpeed.ToString());
                }

                return textSpeed;
            }

            return textSpeed;
        }
        set
        {
            PlayerPrefs.SetString(TextSpeedKey, value.ToString());
            textSpeed = value;
        }
    }


    public static void RestoreSettings()
    {
        AudioListener.volume = PlayerPrefs.GetFloat(GameVolumeKey, 1.0f);
    }

    public static string GetIngameSettingValue(string key)
    {
        switch (key)
        {
            case "audio":
                {
                    int val = Mathf.Clamp((int)(AudioListener.volume * 10), 0, 10);
                    if (val == 0)
                    {
                        return "off";
                    } else
                    {
                        return val.ToString();
                    }
                }

            case "language":
                return Constants.GameLanguageToResourceLanguageCodeDict[GameLanguage];

            case "textspeed":
                // Same value but just all lowercased
                return TextSpeed.ToString().ToLower();

            default:
                return PlayerPrefs.GetString(key);
        }
    }

    public static void SetIngameSettingValue(string key, string value)
    {
        switch (key)
        {
            case "audio":
                {
                    if (value == "off")
                    {
                        AudioListener.volume = 0;
                    } else
                    {
                        AudioListener.volume = Mathf.Clamp(int.Parse(value) / 10.0f, 0.0f, 1.0f);
                    }

                    PlayerPrefs.SetFloat(GameVolumeKey, AudioListener.volume);
                    break;
                }

            case "language":
                {
                    GameLanguage langCode = Constants.GameLanguageToResourceLanguageCodeDict.FirstOrDefault(x => x.Value == value).Key;
                    if (langCode != GameLanguage.Undefined)
                    {
                        GameLanguage = langCode;
                    }
                    break;
                }

            case "textspeed":
                {
                    if (Enum.TryParse(value, true, out GameTextSpeed speed))
                    {
                        TextSpeed = speed;
                    }

                    break;
                }

            default:
                PlayerPrefs.SetString(key, value);
                break;
        }
    }
} 