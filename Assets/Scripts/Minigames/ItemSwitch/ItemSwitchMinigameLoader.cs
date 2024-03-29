﻿using UnityEngine;
using DDEngine.GUI;

namespace DDEngine.Minigame.ItemSwitch
{
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
                new GUIControlSetInstantiateOptions(destroyWhenDisabled: true, preferredDpad: true));

            itemSwitchControlSet.StateChanged += enabled =>
            {
                if (enabled)
                {
                    GameInputManager.Instance.ItemSwitchMinigameActionMap.Enable();
                }
                else
                {
                    GameInputManager.Instance.ItemSwitchMinigameActionMap.Disable();
                }
            };

            ItemSwitchSceneController itemSwitchSceneController = itemSwitchControlSet.GameObject.GetComponent<ItemSwitchSceneController>();
            if (itemSwitchSceneController != null)
            {
                itemSwitchSceneController.Setup(info);
            }

            return itemSwitchControlSet;

        }
    }
}