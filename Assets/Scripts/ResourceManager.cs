using System;
using System.Collections;
using System.Collections.Generic;

using System.IO;
using UnityEngine;

using TMPro;

namespace DDEngine
{
    public class ResourceManager : MonoBehaviour
    {
        public string GameDataPath => Application.persistentDataPath;

        private ResourceFile generalResources;
        private ResourceFile localizationResources;
        private ResourceFile introResources;
        private ResourceFile introLocalizationResources;
        private ResourceFile protectedGeneralResources;
        private ResourceFile protectedLocalizationResources;
        private ProtectedFilePatcher filePatcher;

        private List<GameLanguage> supportedGameLanguages = new List<GameLanguage>();

        public delegate void ResourcesReady();
        public event ResourcesReady OnResourcesReady;

        public ResourceFile GeneralResources => generalResources;
        public ResourceFile LocalizationResources => localizationResources;
        public ResourceFile IntroResources => introResources;
        public ResourceFile ProtectedGeneralResources => protectedGeneralResources;
        public ResourceFile ProtectedLocalizationResources => protectedLocalizationResources;

        public static ResourceManager Instance;

        public TMP_FontAsset EnglishFontAsset;
        public TMP_FontAsset SimplifiedChineseFontAsset;

        private void QueryAllSupportedLocalizationPack()
        {
            foreach (GameLanguage language in Enum.GetValues(typeof(GameLanguage)))
            {
                if (language == GameLanguage.Undefined)
                {
                    continue;
                }
                string langCode = Constants.GameLanguageToResourceLanguageCodeDict[language];
                if (File.Exists(Path.Join(GameDataPath, string.Format(FilePaths.LocalizationResourceFileName, langCode))))
                {
                    if (File.Exists(Path.Join(GameDataPath, string.Format(FilePaths.ProtectedLocalizationResourceFileName, langCode))))
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

        private string GetLanguageCodeForLocalization()
        {
            return Constants.GameLanguageToResourceLanguageCodeDict[GameSettings.GameLanguage];
        }

        public ResourceFile PickBestResourcePackForFile(string filepath)
        {
            bool langFile = (Path.GetExtension(filepath) == ".lang");

            // DRM-protected, so in another file
            if (filepath.StartsWith("chapters/chapter2"))
            {
                if (langFile)
                {
                    return protectedLocalizationResources;
                }

                return ProtectedGeneralResources;
            }

            if (filepath.StartsWith("intro", StringComparison.OrdinalIgnoreCase) ||
                filepath.Contains("game_intro", StringComparison.OrdinalIgnoreCase))
            {
                if (langFile)
                {
                    return introLocalizationResources;
                }

                return introResources;
            }

            if (langFile)
            {
                return localizationResources;
            }

            return generalResources;
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

        public void UpdateLanguagePack()
        {
            localizationResources = new ResourceFile(filePatcher, Path.Join(GameDataPath, string.Format(FilePaths.LocalizationResourceFileName, GetLanguageCodeForLocalization())));
            protectedLocalizationResources = new ResourceFile(filePatcher, Path.Join(GameDataPath, string.Format(FilePaths.ProtectedLocalizationResourceFileName, GetLanguageCodeForLocalization())));
        }

        private IEnumerator LoadDataCoroutine()
        {
            generalResources = new ResourceFile(filePatcher, Path.Join(GameDataPath, FilePaths.GeneralResourceFileName));
            yield return null;
            introResources = new ResourceFile(filePatcher, Path.Join(GameDataPath, FilePaths.IntroResourceFileName));
            yield return null;
            introLocalizationResources = new ResourceFile(filePatcher, Path.Join(GameDataPath, string.Format(FilePaths.IntroLocalizedResourceFileName, GetLanguageCodeForLocalization())));
            yield return null;
            protectedGeneralResources = new ResourceFile(filePatcher, Path.Join(GameDataPath, FilePaths.ProtectedGeneralResourceFileName));
            yield return null;
            localizationResources = new ResourceFile(filePatcher, Path.Join(GameDataPath, string.Format(FilePaths.LocalizationResourceFileName, GetLanguageCodeForLocalization())));
            yield return null;
            protectedLocalizationResources = new ResourceFile(filePatcher, Path.Join(GameDataPath, string.Format(FilePaths.ProtectedLocalizationResourceFileName, GetLanguageCodeForLocalization())));
            yield return null;
            OnResourcesReady();
            yield break;
        }

        private void Awake()
        {
            Instance = this;
        }

        void Start()
        {
            filePatcher = new ProtectedFilePatcher();

            QueryAllSupportedLocalizationPack();
            StartCoroutine(LoadDataCoroutine());
        }
    }
}