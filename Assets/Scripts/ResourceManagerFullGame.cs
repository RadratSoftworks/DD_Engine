using System;
using System.Collections;
using System.Collections.Generic;

using System.IO;
using UnityEngine;

using TMPro;

namespace DDEngine
{
    public class ResourceManagerFullGame : ResourceManager
    {
        private ResourceFile generalResources;
        private ResourceFile localizationResources;
        private ResourceFile introResources;
        private ResourceFile introLocalizationResources;
        private ResourceFile protectedGeneralResources;
        private ResourceFile protectedLocalizationResources;
        private FileSystem fileSystem;

        public override ResourceFile GeneralResources => generalResources;
        public override ResourceFile LocalizationResources => localizationResources;
        public override ResourceFile ProtectedGeneralResources => protectedGeneralResources;
        public override ResourceFile ProtectedLocalizationResources => protectedLocalizationResources;

        public override ResourceFile PickBestResourcePackForFile(string filepath)
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

        public override void UpdateLanguagePack()
        {
            localizationResources = new ResourceFile(fileSystem, string.Format(FilePaths.LocalizationResourceFileName, GetLanguageCodeForLocalization()));
            protectedLocalizationResources = new ResourceFile(fileSystem, string.Format(FilePaths.ProtectedLocalizationResourceFileName, GetLanguageCodeForLocalization()));
        }

        private IEnumerator LoadDataCoroutine()
        {
            generalResources = new ResourceFile(fileSystem, FilePaths.GeneralResourceFileName);
            yield return null;
            introResources = new ResourceFile(fileSystem, FilePaths.IntroResourceFileName);
            yield return null;
            introLocalizationResources = new ResourceFile(fileSystem, string.Format(FilePaths.IntroLocalizedResourceFileName, GetLanguageCodeForLocalization()));
            yield return null;
            protectedGeneralResources = new ResourceFile(fileSystem, FilePaths.ProtectedGeneralResourceFileName);
            yield return null;
            localizationResources = new ResourceFile(fileSystem, string.Format(FilePaths.LocalizationResourceFileName, GetLanguageCodeForLocalization()));
            yield return null;
            protectedLocalizationResources = new ResourceFile(fileSystem, string.Format(FilePaths.ProtectedLocalizationResourceFileName, GetLanguageCodeForLocalization()));
            yield return null;
            FireResourceReady();
            yield break;
        }

        void Start()
        {
            fileSystem = new ProtectedFilePatcher();

            QueryAllSupportedLocalizationPack(FilePaths.LocalizationResourceFileName, FilePaths.ProtectedLocalizationResourceFileName);
            StartCoroutine(LoadDataCoroutine());
        }
    }
}