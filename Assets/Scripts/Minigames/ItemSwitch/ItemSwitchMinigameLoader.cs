using System;
using System.Collections.Generic;
using UnityEngine;

public class ItemSwitchMinigameLoader : MonoBehaviour
{
    public static ItemSwitchMinigameLoader Instance;

    [SerializeField]
    private GameObject itemSwitchScenePrefabObject;

    private void Start()
    {
        Instance = this;
    }

    public GUIControlSet Load(ItemSwitchMinigameInfo info, string filename, Vector2 viewResolution)
    {
        GUIControlSet itemSwitchControlSet = new GUIControlSet(GUIControlSetFactory.Instance.container,
            itemSwitchScenePrefabObject, filename, viewResolution,
            new GUIControlSetInstantiateOptions(destroyWhenDisabled: true));

        ItemSwitchSceneController itemSwitchSceneController = itemSwitchControlSet.GameObject.GetComponent<ItemSwitchSceneController>();
        if (itemSwitchSceneController != null)
        {
            itemSwitchSceneController.Setup(info);
        }

        return itemSwitchControlSet;

    }
}
