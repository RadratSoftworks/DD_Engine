using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

using DDEngine.Utils;
using DDEngine.GUI.Parser;

namespace DDEngine.GUI
{
    public class GUIControlSetFactory : GameBaseAssetManager<GUIControlSet>
    {
        public static GUIControlSetFactory Instance;

        [SerializeField]
        private GameObject guiPicturePrefabObject;

        [SerializeField]
        private GameObject guiMenuOptionPrefabObject;

        [SerializeField]
        private GameObject guiSetingMultiValuesOptionPrefabObject;

        [SerializeField]
        private GameObject guiAnimationPrefabObject;

        [SerializeField]
        private GameObject guiMenuPrefabObject;

        [SerializeField]
        private GameObject guiLayerPrefabObject;

        [SerializeField]
        private GameObject guiLocationPrefabObject;

        [SerializeField]
        private GameObject guiActivePrefabObject;

        [SerializeField]
        private GameObject guiSoundPrefabObject;

        [SerializeField]
        private GameObject guiConditionalPrefabObject;

        [SerializeField]
        private GameObject guiScrollingPicturePrefabObject;

        [SerializeField]
        private GameObject guiLabelPrefabObject;

        [SerializeField]
        private GameObject guiBackgroundLabelPrefabObject;

        [SerializeField]
        public GameObject container;

        private void Start()
        {
            Instance = this;

            GUICanvasSetup setup = container.GetComponent<GUICanvasSetup>();
            if (setup != null)
            {
                setup.SetCanvasSize(Constants.CanvasX, Constants.CanvasY);
            }
        }

        private void LoadGuiPicture(GameObject parent, GUIControlPictureDescription description, bool relativeToBottomLeft = false)
        {
            GameObject anotherInstance = Instantiate(guiPicturePrefabObject, parent.transform, false);
            anotherInstance.transform.localPosition = GameUtils.ToUnityCoordinates(description.TopPosition);

            if (description.Id != null)
            {
                anotherInstance.name = description.Id;
            }
            SpriteRenderer renderer = anotherInstance.GetComponent<SpriteRenderer>();
            if (renderer != null)
            {
                renderer.sortingOrder = GameUtils.ToUnitySortingPosition(description.AbsoluteDepth);
                renderer.sprite = SpriteManager.Instance.Load(ResourceManager.Instance.GeneralResources, description.ImagePath);

                if (relativeToBottomLeft)
                {
                    anotherInstance.transform.localPosition = GameUtils.ToUnityCoordinates(new Vector2(0.0f, Constants.CanvasY - renderer.sprite.rect.size.y) - description.TopPosition);
                }
            }
        }

        private void LoadGuiConditional(GUIControlSet ownSet, GameObject parent, GUIControlConditionalDescription description, GUIControlSetInstantiateOptions options)
        {
            foreach (var pairSet in description.ControlShowOnCases)
            {
                GameObject anotherInstance = Instantiate(guiConditionalPrefabObject, parent.transform, false);
                anotherInstance.name = string.Format("Conditional_{0}_{1}", description.ConditionValueVariable, pairSet.Item1);

                GUIConditionalController conditionalController = anotherInstance.GetComponent<GUIConditionalController>();
                conditionalController.Setup(ownSet, description.ConditionValueVariable, pairSet.Item1);

                InstantiateControls(ownSet, anotherInstance, pairSet.Item2, options);
            }
        }

        private void LoadGuiMenu(GUIControlSet ownSet, GameObject parent, GUIControlMenuDescription description)
        {
            GameObject gameObject = Instantiate(guiMenuPrefabObject, parent.transform, false);
            gameObject.transform.localPosition = GameUtils.ToUnityCoordinates(description.TopPosition);

            // Assign the side-picture on the left side
            LoadGuiPicture(gameObject, new GUIControlPictureDescription()
            {
                ImagePath = description.SideImagePath,
                TopPosition = description.TopPosition,
                Id = "MenuSideImage",
                Depth = description.Depth,
                AbsoluteDepth = description.AbsoluteDepth,
            }, true);

            GUIMenuController controller = gameObject.GetComponent<GUIMenuController>();
            controller.Setup(ownSet, description.ActionHandlerFilePath);

            GameObject gameObjectOptions = controller.Options;
            Vector2 currentPosition = new Vector2(0.0f, 0.0f);

            LoadGuiAnimation(gameObject, new GUIControlAnimationDescription()
            {
                Depth = description.Depth,
                AbsoluteDepth = description.AbsoluteDepth,
                TopPosition = currentPosition,
                AnimationPath = FilePaths.MenuLensFlareAnimaionFilename
            });

            if (gameObjectOptions != null)
            {
                foreach (GUIControlDescription optionDesc in description.MenuItemControls)
                {
                    if (optionDesc is GUIControlMenuItemDescription)
                    {
                        LoadGuiMenuItem(ownSet, gameObjectOptions, optionDesc as GUIControlMenuItemDescription, ref currentPosition);
                    }
                    else if (optionDesc is GUIControlSettingMultiValuesOptionDescription)
                    {
                        LoadGuiSettingMultiValuesOption(ownSet, gameObjectOptions, optionDesc as GUIControlSettingMultiValuesOptionDescription, ref currentPosition);
                    }
                    else
                    {
                        Debug.LogError("Control type " + description.GetType() + " is not supported in menu option control list!");
                    }
                }
            }

            ownSet.PreferredDpad = true;
            ownSet.HasMenuTrigger = false;
        }

        private void LoadGuiMenuItem(GUIControlSet ownSet, GameObject parent, GUIControlMenuItemDescription description, ref Vector2 position)
        {
            GameObject anotherInstance = Instantiate(guiMenuOptionPrefabObject, parent.transform, false);
            GUIMenuItemController controller = anotherInstance.GetComponent<GUIMenuItemController>();

            if (controller != null)
            {
                controller.Setup(ownSet, description, ref position);
            }
        }

        private void LoadGuiSettingMultiValuesOption(GUIControlSet ownSet, GameObject parent, GUIControlSettingMultiValuesOptionDescription description, ref Vector2 position)
        {
            GameObject anotherInstance = Instantiate(guiSetingMultiValuesOptionPrefabObject, parent.transform, false);
            GUISettingMultiValuesOptionController controller = anotherInstance.GetComponent<GUISettingMultiValuesOptionController>();

            if (controller != null)
            {
                controller.Setup(ownSet, description, ref position);
            }
        }

        private void LoadGuiAnimation(GameObject parent, GUIControlAnimationDescription description)
        {
            GameObject anotherInstance = Instantiate(guiAnimationPrefabObject, parent.transform, false);
            anotherInstance.transform.localPosition = GameUtils.ToUnityCoordinates(description.TopPosition);

            SpriteAnimatorController animController = anotherInstance.GetComponent<SpriteAnimatorController>();
            if (animController != null)
            {
                animController.Setup(description.TopPosition, description.AbsoluteDepth, description.AnimationPath);
            }
        }

        private GameObject LoadGuiLayer(GUIControlSet ownSet, GameObject parent, GUIControlLayerDescription description, GUIControlSetInstantiateOptions options)
        {
            GameObject anotherInstance = Instantiate(guiLayerPrefabObject, parent.transform, false);
            anotherInstance.transform.localPosition = GameUtils.ToUnityCoordinates(description.TopPosition);

            GUILayerController controller = anotherInstance.GetComponent<GUILayerController>();
            if (controller != null)
            {
                controller.SetProperties(ownSet, description.TopPosition, description.Scroll, description.Size);
            }

            InstantiateControls(ownSet, anotherInstance, description.Controls, options);
            return anotherInstance;
        }

        private void LoadGuiLocation(GUIControlSet ownSet, GameObject parent, GUIControlLocationDescription description, GUIControlSetInstantiateOptions options)
        {
            if (ownSet.Location != null)
            {
                throw new InvalidOperationException("Only one location can exist in one control set!");
            }

            GameObject anotherInstance = Instantiate(guiLocationPrefabObject, parent.transform, false);
            anotherInstance.transform.localPosition = GameUtils.ToUnityCoordinates(description.TopPosition);
            anotherInstance.name = description.Name;

            GameObject panLayer = null;

            foreach (GUIControlDescription layerUncasted in description.Layers)
            {
                if (!(layerUncasted is GUIControlLayerDescription))
                {
                    Debug.LogError("Expected layer description in location description, got type: " + layerUncasted.GetType());
                }
                else
                {
                    GUIControlLayerDescription layerDesc = layerUncasted as GUIControlLayerDescription;

                    GameObject result = LoadGuiLayer(ownSet, anotherInstance, layerDesc, options);
                    if ((panLayer == null) || (layerDesc.DefinesPan))
                    {
                        panLayer = result;
                    }
                }
            }

            GUILocationController controller = anotherInstance.GetComponent<GUILocationController>();
            if (controller != null)
            {
                controller.Setup(ownSet, panLayer);
            }

            ownSet.Location = controller;
        }

        private void LoadGuiActive(GUIControlSet ownSet, GameObject parent, GUIControlActiveDescription description, GUIControlSetInstantiateOptions options)
        {
            GameObject anotherInstance = Instantiate(guiActivePrefabObject, parent.transform, false);
            anotherInstance.name = description.Id;

            GUIActiveController controller = anotherInstance.GetComponent<GUIActiveController>();

            if (controller != null)
            {
                controller.Setup(ownSet, description.TopPosition, description.Size, new Rect(description.BoundPosition, description.BoundSize), options.PanToActiveWhenSelected);
            }
        }

        private void LoadGuiSound(GameObject parent, GUIControlSoundDescription description)
        {
            GameObject anotherInstance = Instantiate(guiSoundPrefabObject, parent.transform, false);
            anotherInstance.name = GameUtils.ToUnityName(description.Path);

            GUISoundController controller = anotherInstance.GetComponent<GUISoundController>();

            if (controller != null)
            {
                controller.Setup(description.Path, description.Type);
            }
        }

        private void LoadGuiScrollingPicture(GameObject parent, GUIControlScrollingPictureDescription description)
        {
            GameObject anotherInstance = Instantiate(guiScrollingPicturePrefabObject, parent.transform, false);
            anotherInstance.name = GameUtils.ToUnityName(description.ImagePath);

            GUIScrollingPictureController controller = anotherInstance.GetComponent<GUIScrollingPictureController>();

            if (controller != null)
            {
                Sprite pictureSprite = SpriteManager.Instance.Load(ResourceManager.Instance.GeneralResources,
                    description.ImagePath, forScrolling: true);

                if (pictureSprite == null)
                {
                    Debug.LogError("Unable to load picture for scrolling!");
                }

                controller.Setup(pictureSprite, description.TopPosition, description.Scroll, description.AbsoluteDepth);
            }
        }

        private void LoadGuiLabel(GUIControlSet ownSet, GameObject parent, GUIControlLabelDescription description)
        {
            GameObject anotherInstance = Instantiate(guiLabelPrefabObject, parent.transform, false);
            GUILabelController controller = anotherInstance.GetComponent<GUILabelController>();

            if (controller != null)
            {
                controller.Setup(ownSet, description.TopPosition, description.TextName, description.AbsoluteDepth);
            }
        }

        private void LoadGuiBgLabel(GUIControlSet ownSet, GameObject parent, GUIControlBackgroundLabelDescription description)
        {
            GameObject anotherInstance = Instantiate(guiBackgroundLabelPrefabObject, parent.transform, false);
            GUIBackgroundLabelController controller = anotherInstance.GetComponent<GUIBackgroundLabelController>();

            if (controller != null)
            {
                controller.Setup(ownSet, description);
            }
        }

        public void InstantiateControls(GUIControlSet ownSet, GameObject parent, List<GUIControlDescription> descriptions, GUIControlSetInstantiateOptions options)
        {
            foreach (GUIControlDescription description in descriptions)
            {
                if (description is GUIControlPictureDescription)
                {
                    LoadGuiPicture(parent, description as GUIControlPictureDescription);
                }
                else if (description is GUIControlConditionalDescription)
                {
                    LoadGuiConditional(ownSet, parent, description as GUIControlConditionalDescription, options);
                }
                else if (description is GUIControlMenuDescription)
                {
                    LoadGuiMenu(ownSet, parent, description as GUIControlMenuDescription);
                }
                else if (description is GUIControlAnimationDescription)
                {
                    LoadGuiAnimation(parent, description as GUIControlAnimationDescription);
                }
                else if (description is GUIControlLocationDescription)
                {
                    LoadGuiLocation(ownSet, parent, description as GUIControlLocationDescription, options);
                }
                else if (description is GUIControlSoundDescription)
                {
                    LoadGuiSound(parent, description as GUIControlSoundDescription);
                }
                else if (description is GUIControlActiveDescription)
                {
                    LoadGuiActive(ownSet, parent, description as GUIControlActiveDescription, options);
                }
                else if (description is GUIControlScrollingPictureDescription)
                {
                    LoadGuiScrollingPicture(parent, description as GUIControlScrollingPictureDescription);
                }
                else if (description is GUIControlLabelDescription)
                {
                    LoadGuiLabel(ownSet, parent, description as GUIControlLabelDescription);
                }
                else if (description is GUIControlBackgroundLabelDescription)
                {
                    LoadGuiBgLabel(ownSet, parent, description as GUIControlBackgroundLabelDescription);
                }
                else
                {
                    Debug.Log("Unhandled control description type " + description.GetType() + " to instantiate control!");
                }
            }
        }
        protected override bool IsAssetSuitableForPrunge(GUIControlSet asset)
        {
            return !asset.GameObject.activeSelf;
        }

        protected override void OnRemovalFromCache(GUIControlSet asset)
        {
            // The control set will delete itself probably
            GameObject.Destroy(asset.GameObject);
        }

        private GUIControlSet LoadControlSet(Stream stream, Vector2 viewResolution, GUIControlSetInstantiateOptions options, string path = null)
        {
            var controlDesc = new GUIControlDescriptionFile(stream);
            controlDesc.Filename = path;

            var controlSet = new GUIControlSet(container, controlDesc, viewResolution, options);

            if (!options.DestroyWhenDisabled)
            {
                AddToCache(path, controlSet);
            }

            return controlSet;
        }

        public GUIControlSet LoadControlSet(string path, Vector2 viewResolution, GUIControlSetInstantiateOptions options)
        {
            GUIControlSet cache = GetFromCache(path);
            if (cache != null)
            {
                return cache;
            }

            ResourceFile resourcesToPick = ResourceManager.Instance.PickBestResourcePackForFile(path);

            if (!resourcesToPick.Exists(path))
            {
                resourcesToPick = ResourceManager.Instance.ProtectedGeneralResources;
                if (!resourcesToPick.Exists(path))
                {
                    return null;
                }
            }

            ResourceInfo controlSetRescInfo = resourcesToPick.Resources[path];
            byte[] controlSetInfoData = resourcesToPick.ReadResourceData(controlSetRescInfo);

            using (MemoryStream memStream = new MemoryStream(controlSetInfoData))
            {
                return LoadControlSet(memStream, viewResolution, options, path);
            }
        }
    }
}