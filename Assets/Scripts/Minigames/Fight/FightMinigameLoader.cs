using System;
using System.Collections.Generic;
using UnityEngine;

public class FightMinigameLoader : MonoBehaviour
{
    public static FightMinigameLoader Instance;

    [SerializeField]
    private GameObject fightScenePrefabObject;

    private void Awake()
    {
        Instance = this;
    }

    public GUIControlSet Load(FightMinigameInfo fightInfo, string filename, Vector2 viewSize)
    {
        GUIControlSet fightSceneControlSet = new GUIControlSet(GUIControlSetFactory.Instance.container,
            fightScenePrefabObject, filename, viewSize, new GUIControlSetInstantiateOptions(destroyWhenDisabled: true, preferredDpad: true));

        fightSceneControlSet.StateChanged += enabled =>
        {
            if (enabled)
            {
                GameInputManager.Instance.FightMinigameActionMap.Enable();
            }
            else
            {
                GameInputManager.Instance.FightMinigameActionMap.Disable();
            }
        };

        FightSceneController fightSceneController = fightSceneControlSet.GameObject.GetComponent<FightSceneController>();
        if (fightSceneController != null)
        {
            fightSceneController.Setup(fightInfo);
        }

        return fightSceneControlSet;
    }
}