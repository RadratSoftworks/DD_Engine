﻿using UnityEngine;

public class ConstructionSiteMinigameLoader : MonoBehaviour
{
    public static ConstructionSiteMinigameLoader Instance;

    [SerializeField]
    private GameObject constructionSiteScenePrefabObject;

    private void Start()
    {
        Instance = this;
    }

    public GUIControlSet Load(ConstructionSiteMinigameInfo info, string filename, Vector2 viewResolution)
    {
        GUIControlSet constructionSiteControlSet = new GUIControlSet(GUIControlSetFactory.Instance.container,
            constructionSiteScenePrefabObject, filename, viewResolution,
            new GUIControlSetInstantiateOptions(destroyWhenDisabled: true));

        ConstructionSiteSceneController constructionSiteSceneController = constructionSiteControlSet.GameObject.GetComponent<ConstructionSiteSceneController>();
        if (constructionSiteSceneController != null)
        {
            constructionSiteSceneController.Setup(info);
        }

        return constructionSiteControlSet;
    }
}
