using System;
using System.Collections;
using System.Collections.Generic;

using System.IO;
using UnityEngine;

using TMPro;

public class ResourceManager : MonoBehaviour
{
    public string GameDataPath = "G:\\bent\\DD_RV\\opes\\";

    private ResourceFile generalResources;
    private ResourceFile localizationResources;
    private ResourceFile introResources;
    private ResourceFile protectedGeneralResources;

    private List<GameLanguage> supportedGameLanguages = new List<GameLanguage>();

    public delegate void ResourcesReady();
    public event ResourcesReady OnResourcesReady;

    public ResourceFile GeneralResources => generalResources;
    public ResourceFile LocalizationResources => localizationResources;
    public ResourceFile IntroResources => introResources;
    public ResourceFile ProtectedGeneralResources => protectedGeneralResources;


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

        if (!supportedGameLanguages.Contains(GameSettings.GetGameLanguage()))
        {
            GameSettings.SetGameLanguage(GameLanguage.English);
        }
    }

    private string GetLanguageCodeForLocalization()
    {
        return Constants.GameLanguageToResourceLanguageCodeDict[GameSettings.GetGameLanguage()];
    }

    public ResourceFile PickBestResourcePackForFile(string filepath)
    {
        if (filepath.StartsWith("intro", StringComparison.OrdinalIgnoreCase) ||
            filepath.Contains("game_intro", StringComparison.OrdinalIgnoreCase))
        {
            return introResources;
        }

        return generalResources;
    }

    public TMP_FontAsset GetFontAssetForLocalization()
    {
        switch (GameSettings.GetGameLanguage())
        {
            case GameLanguage.SimplifiedChinese:
                return SimplifiedChineseFontAsset;

            default:
                return EnglishFontAsset;
        }
    }

    private IEnumerator LoadDataCoroutine()
    {
        generalResources = new ResourceFile(Path.Join(GameDataPath, FilePaths.GeneralResourceFileName));
        yield return null;
        localizationResources = new ResourceFile(Path.Join(GameDataPath, string.Format(FilePaths.LocalizationResourceFileName, GetLanguageCodeForLocalization())));
        yield return null;
        introResources = new ResourceFile(Path.Join(GameDataPath, FilePaths.IntroResourceFileName));
        yield return null;
        protectedGeneralResources = new ResourceFile(Path.Join(GameDataPath, FilePaths.ProtectedGeneralResourceFileName));
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
        QueryAllSupportedLocalizationPack();
        StartCoroutine(LoadDataCoroutine());
    }
}
