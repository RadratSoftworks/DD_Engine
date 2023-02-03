using System;
using System.Collections;
using System.IO;
using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    public string GameDataPath = "G:\\bent\\DD_RV\\opes\\";
    private ResourceFile generalResources;
    private ResourceFile localizationResources;

    public delegate void ResourcesReady();
    public event ResourcesReady OnResourcesReady;

    public ResourceFile GeneralResources => generalResources;
    public ResourceFile LocalizationResources => localizationResources;

    public static ResourceManager Instance;

    private string GetLanguageCodeForLocalization()
    {
        return "en";
    }

    private IEnumerator LoadDataCoroutine()
    {
        generalResources = new ResourceFile(Path.Join(GameDataPath, FilePaths.GeneralResourceFileName));
        yield return null;
        localizationResources = new ResourceFile(Path.Join(GameDataPath, string.Format(FilePaths.LocalizationResourceFileName, GetLanguageCodeForLocalization())));
        yield return null;
        OnResourcesReady();
        yield break;
    }

    void Start()
    {
        Instance = this;
        StartCoroutine(LoadDataCoroutine());
    }
}
