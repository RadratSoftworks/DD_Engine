using System;
using System.Collections;
using System.IO;

namespace DDEngine
{
    public class ResourceManagerDemo : ResourceManager
    {
        private ResourceFile generalResources;
        private ResourceFile localizationResources;

        public override ResourceFile GeneralResources => generalResources;
        public override ResourceFile LocalizationResources => localizationResources;
        public override ResourceFile ProtectedGeneralResources => generalResources;
        public override ResourceFile ProtectedLocalizationResources => localizationResources;

        public ResourceManagerDemo()
            : base(() => new ResourceFileSystem())
        {
        }

        public override ResourceFile PickBestResourcePackForFile(string filepath)
        {
            bool langFile = (Path.GetExtension(filepath) == ".lang");
            return langFile ? localizationResources : generalResources;
        }

        public override void UpdateLanguagePack()
        {
            localizationResources = new ResourceFile(fileSystem, string.Format(FilePaths.LocalizationDemoResourceFilename, GetLanguageCodeForLocalization()));
        }

        private IEnumerator LoadDataCoroutine()
        {
            generalResources = new ResourceFile(fileSystem, FilePaths.GeneralDemoResourceFilename);
            yield return null;
            localizationResources = new ResourceFile(fileSystem, string.Format(FilePaths.LocalizationDemoResourceFilename, GetLanguageCodeForLocalization()));
            yield return null;
            FireResourceReady();
            yield break;
        }

        void Start()
        {
            QueryAllSupportedLocalizationPack(FilePaths.LocalizationDemoResourceFilename, null);
            StartCoroutine(LoadDataCoroutine());
        }
    }
}
