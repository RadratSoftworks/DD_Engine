using System;
using System.Collections;
using System.Collections.Generic;

using System.IO;
using UnityEngine;

using TMPro;

namespace DDEngine
{
    public abstract class ResourceManager : MonoBehaviour
    {
        public static ResourceManager Instance;

        private List<GameLanguage> supportedGameLanguages = new List<GameLanguage>();

        public delegate void ResourcesReady();
        public event ResourcesReady OnResourcesReady;

        [SerializeField]
        protected TMP_FontAsset EnglishFontAsset;

        [SerializeField]
        protected TMP_FontAsset SimplifiedChineseFontAsset;

        public abstract ResourceFile GeneralResources { get; }
        public abstract ResourceFile LocalizationResources { get; }
        public abstract ResourceFile ProtectedGeneralResources { get; }
        public abstract ResourceFile ProtectedLocalizationResources { get; }

        public abstract ResourceFile PickBestResourcePackForFile(string filepath);
        public abstract void UpdateLanguagePack();

        protected void FireResourceReady()
        {
            OnResourcesReady?.Invoke();
        }

        protected static string GetLanguageCodeForLocalization()
        {
            return Constants.GameLanguageToResourceLanguageCodeDict[GameSettings.GameLanguage];
        }

        protected void QueryAllSupportedLocalizationPack(string baseLocalizationPathFormat, string protectedBaseLocalizationPathFormat = null)
        {
            foreach (GameLanguage language in Enum.GetValues(typeof(GameLanguage)))
            {
                if (language == GameLanguage.Undefined)
                {
                    continue;
                }
                string langCode = Constants.GameLanguageToResourceLanguageCodeDict[language];
                if (File.Exists(string.Format(baseLocalizationPathFormat, langCode)))
                {
                    if ((protectedBaseLocalizationPathFormat == null) || ((protectedBaseLocalizationPathFormat != null) && File.Exists(string.Format(baseLocalizationPathFormat, langCode))))
                    {
                        supportedGameLanguages.Add(language);
                    }
                }
            }

            if (!supportedGameLanguages.Contains(GameSettings.GameLanguage))
            {
                GameSettings.GameLanguage = GameLanguage.English;
            }
        }

        private TMP_FontAsset GetFontAssetForLanguage(GameLanguage language)
        {
            switch (language)
            {
                case GameLanguage.SimplifiedChinese:
                    return SimplifiedChineseFontAsset;

                default:
                    return EnglishFontAsset;
            }
        }

        public TMP_FontAsset GetFontAssetForStagingLanguageText()
        {
            return GetFontAssetForLanguage((GameSettings.StagingGameLanguage == GameLanguage.Undefined) ?
                GameSettings.GameLanguage : GameSettings.StagingGameLanguage);
        }

        public TMP_FontAsset GetFontAssetForLocalization()
        {
            return GetFontAssetForLanguage(GameSettings.GameLanguage);
        }

        private void Awake()
        {
            Instance = this;
        }
    }
}