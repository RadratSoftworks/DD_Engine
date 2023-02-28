using System;
using System.Linq;
using UnityEngine;

namespace DDEngine
{
    public static class GameSettings
    {
        private const string LanguageSettingKey = "GameLanguage";
        private const string CacheThresholdKey = "CacheThreshold";
        private const string CachedChangeDeltaKey = "CachedChangeDelta";
        private const string GameVolumeKey = "GameVolume";
        private const string TextSpeedKey = "TextSpeed";
        private const string GameStartLocationKey = "GameStartLocation";
        private const string GameCompletedKey = "GameCompleted";
        private const string GameCompletedCompatKey = "completed_game";
        private const string VibraKey = "vibra";
        private const int defaultCachedChangeDelta = 10;
        private const int defaultCacheThreshold = 5;

        private static GameLanguage cachedLanguage = GameLanguage.Undefined;
        private static GameTextSpeed textSpeed = GameTextSpeed.Undefined;
        private static GameLanguage stagingLanguage = GameLanguage.Undefined;
        private static GameStartLocation startLocation = GameStartLocation.Undefined;
        private static int cachedChangeDelta = -1;
        private static int cachedThreshold = -1;

        private static GameLanguage GetDefaultSystemLanguage()
        {
            SystemLanguage lang = Application.systemLanguage;
            switch (lang)
            {
                case SystemLanguage.English:
                    return GameLanguage.English;

                case SystemLanguage.French:
                    return GameLanguage.French;

                case SystemLanguage.Spanish:
                    return GameLanguage.Spanish;

                case SystemLanguage.ChineseSimplified:
                    return GameLanguage.SimplifiedChinese;

                case SystemLanguage.ChineseTraditional:
                    return GameLanguage.TraditionalChinese;

                case SystemLanguage.German:
                    return GameLanguage.Deutsch;

                case SystemLanguage.Italian:
                    return GameLanguage.Italian;

                default:
                    return GameLanguage.English;
            }
        }

        public static GameLanguage GameLanguage
        {
            get
            {
                if (cachedLanguage != GameLanguage.Undefined)
                {
                    return cachedLanguage;
                }

                string result = PlayerPrefs.GetString(LanguageSettingKey);
                if ((result == null) || !Enum.TryParse(result, out cachedLanguage))
                {
                    cachedLanguage = GetDefaultSystemLanguage();
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

        public static GameLanguage StagingGameLanguage
        {
            get { return stagingLanguage; }
            set { stagingLanguage = value; }
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

        public static GameStartLocation StartLocation
        {
            get
            {
                if (startLocation == GameStartLocation.Undefined)
                {
                    string result = PlayerPrefs.GetString(GameStartLocationKey);
                    if ((result == null) || !Enum.TryParse(result, true, out startLocation))
                    {
                        startLocation = GameStartLocation.Menu;
                        PlayerPrefs.SetString(GameStartLocationKey, startLocation.ToString());
                    }

                    return startLocation;
                }

                return startLocation;
            }
            set
            {
                PlayerPrefs.SetString(GameStartLocationKey, value.ToString());
                startLocation = value;
            }
        }

        public static bool Vibration
        {
            get
            {
                return PlayerPrefs.GetString(VibraKey, "on") == "on";
            }
            set
            {
                PlayerPrefs.SetString(VibraKey, (value == false) ? "off" : "on");
            }
        }

        public static void RestoreSettings()
        {
            AudioListener.volume = PlayerPrefs.GetFloat(GameVolumeKey, 1.0f);
        }

        public static void Reset()
        {
            cachedLanguage = GameLanguage.Undefined;
            stagingLanguage = GameLanguage.Undefined;
            textSpeed = GameTextSpeed.Normal;
            AudioListener.volume = 1.0f;

            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
        }

        public static void Save()
        {
            if (stagingLanguage != GameLanguage.Undefined)
            {
                GameLanguage = stagingLanguage;
                stagingLanguage = GameLanguage.Undefined;
            }

            PlayerPrefs.Save();
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
                        }
                        else
                        {
                            return val.ToString();
                        }
                    }

                case "language":
                    return Constants.GameLanguageToResourceLanguageCodeDict[
                        (stagingLanguage != GameLanguage.Undefined) ? stagingLanguage : GameLanguage];

                case "textspeed":
                    // Same value but just all lowercased
                    return TextSpeed.ToString().ToLower();

                case "gamestart":
                    // Same value but just all lowercased
                    return StartLocation.ToString().ToLower();

                case "completed_game":
                    {
                        int compatValue = PlayerPrefs.GetInt(GameCompletedCompatKey, -1);
                        if (compatValue < 0)
                        {
                            compatValue = PlayerPrefs.GetInt(GameCompletedKey);
                        }

                        return (compatValue <= 0) ? "null" : "true";
                    }

                case "vibra":
                    {
                        return Vibration ? "on" : "off";
                    }

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
                        }
                        else
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
                            stagingLanguage = langCode;
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

                case "gamestart":
                    {
                        if (Enum.TryParse(value, true, out GameStartLocation location))
                        {
                            StartLocation = location;
                        }

                        break;
                    }

                case "completed_game":
                    {
                        PlayerPrefs.SetInt(GameCompletedKey, (value.Equals("true", StringComparison.OrdinalIgnoreCase) ? 1 : 0));
                        break;
                    }

                case "vibra":
                    {
                        Vibration = (value.Equals("on", StringComparison.OrdinalIgnoreCase) ? true : false);
                        break;
                    }

                default:
                    PlayerPrefs.SetString(key, value);
                    break;
            }
        }
    }
}