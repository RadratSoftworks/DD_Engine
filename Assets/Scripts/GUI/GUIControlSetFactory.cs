using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class GUIControlSetFactory : MonoBehaviour
{
    public static GUIControlSetFactory Instance;
    private Dictionary<string, GUIControlSet> controlSets;

    public GameObject guiPicturePrefabObject;
    public GameObject guiMenuOptionPrefabObject;
    public GameObject guiAnimationPrefabObject;
    public GameObject guiMenuPrefabObject;
    public GameObject guiLayerPrefabObject;
    public GameObject guiLocationPrefabObject;
    public GameObject guiActivePrefabObject;
    public GameObject guiSoundPrefabObject;
    public GameObject guiConditionalPrefabObject;
    public GameObject container;

    private void Start()
    {
        Instance = this;
        controlSets = new Dictionary<string, GUIControlSet>(StringComparer.OrdinalIgnoreCase);

        GUICanvasSetup setup = container.GetComponent<GUICanvasSetup>();
        if (setup != null)
        {
            setup.SetCanvasSize(Constants.CanvasX, Constants.CanvasY);
        }
    }

    private void LoadGuiPicture(GameObject parent, GUIControlPictureDescription description,bool relativeToBottomLeft = false)
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

    private void LoadGuiConditional(GUIControlSet ownSet, GameObject parent, GUIControlConditionalDescription description)
    {
        foreach (var pairSet in description.ControlShowOnCases)
        {
            GameObject anotherInstance = Instantiate(guiConditionalPrefabObject, parent.transform, false);
            anotherInstance.name = string.Format("Conditional_{0}_{1}", description.ConditionValueVariable, pairSet.Key);

            GUIConditionalController conditionalController = anotherInstance.GetComponent<GUIConditionalController>();
            conditionalController.Setup(ownSet, description.ConditionValueVariable, pairSet.Key);

            InstantiateControls(ownSet, anotherInstance, pairSet.Value);
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

        GameObject gameObjectOptions = gameObject.transform.Find("MenuOptions")?.gameObject;
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
                else
                {
                    Debug.LogError("Control type " + description.GetType() + " is not supported in menu option control list!");
                }
            }
        }
    }

    private void LoadGuiMenuItem(GUIControlSet ownSet, GameObject parent, GUIControlMenuItemDescription description, ref Vector2 position)
    {
        GameObject anotherInstance = Instantiate(guiMenuOptionPrefabObject, parent.transform, false);
        anotherInstance.transform.localPosition = GameUtils.ToUnityCoordinates(position);

        if (description.Id != null)
        {
            anotherInstance.name = description.Id;
        }
        SpriteRenderer renderer = anotherInstance.GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            renderer.sortingOrder = GameUtils.ToUnitySortingPosition(description.AbsoluteDepth);
            renderer.sprite = SpriteManager.Instance.Load(ResourceManager.Instance.GeneralResources, description.ImagePath);

            position += renderer.sprite.textureRect.size * Vector2.up;
            Vector2 sizeReal = renderer.sprite.textureRect.size / Constants.PixelsPerUnit;

            // Adjust the rect so that text can properly render
            RectTransform transform = anotherInstance.GetComponent<RectTransform>();
            if (transform)
            {
                transform.sizeDelta = sizeReal;
            }

            var text = anotherInstance.GetComponentInChildren<TMPro.TMP_Text>();
            text.rectTransform.sizeDelta = sizeReal;
            text.text = ownSet.GetLanguageString(description.TextName);
            text.font = ResourceManager.Instance.GetFontAssetForLocalization();

            var meshRenderer = anotherInstance.GetComponentInChildren<MeshRenderer>();
            meshRenderer.sortingOrder = GameUtils.ToUnitySortingPosition(description.AbsoluteDepth);
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

    private void LoadGuiLayer(GUIControlSet ownSet, GameObject parent, GUIControlLayerDescription description)
    {
        GameObject anotherInstance = Instantiate(guiLayerPrefabObject, parent.transform, false);
        anotherInstance.transform.localPosition = GameUtils.ToUnityCoordinates(description.TopPosition);

        GUILayerController controller = anotherInstance.GetComponent<GUILayerController>();
        if (controller != null)
        {
            controller.SetProperties(description.TopPosition, description.Scroll, description.Size);
        }

        InstantiateControls(ownSet, anotherInstance, description.Controls);
    }

    private void LoadGuiLocation(GUIControlSet ownSet, GameObject parent, GUIControlLocationDescription description)
    {
        GameObject anotherInstance = Instantiate(guiLocationPrefabObject, parent.transform, false);
        anotherInstance.transform.localPosition = GameUtils.ToUnityCoordinates(description.TopPosition);
        anotherInstance.name = description.Name;

        Debug.Log(anotherInstance.transform.localPosition);

        GUILocationController controller = anotherInstance.GetComponent<GUILocationController>();
        if (controller != null)
        {
            controller.Setup(ownSet);
        }

        foreach (GUIControlDescription layerUncasted in description.Layers)
        {
            if (!(layerUncasted is GUIControlLayerDescription))
            {
                Debug.LogError("Expected layer description in location description, got type: " + layerUncasted.GetType());
            } else
            {
                LoadGuiLayer(ownSet, anotherInstance, layerUncasted as GUIControlLayerDescription);
            }
        }
    }

    private void LoadGuiActive(GUIControlSet ownSet, GameObject parent, GUIControlActiveDescription description)
    {
        GameObject anotherInstance = Instantiate(guiActivePrefabObject, parent.transform, false);
        anotherInstance.name = description.Id;
        
        GUIActiveController controller = anotherInstance.GetComponent<GUIActiveController>();

        if (controller != null)
        {
            controller.Setup(ownSet, description.TopPosition, description.Size, new Rect(description.BoundPosition, description.BoundSize));
        }
    }

    private void LoadGuiSound(GameObject parent, GUIControlSoundDescription description)
    {
        GameObject anotherInstance = Instantiate(guiSoundPrefabObject, parent.transform, false);
        anotherInstance.name = description.Path;

        GUISoundController controller = anotherInstance.GetComponent<GUISoundController>();

        if (controller != null)
        {
            controller.Setup(description.Path, description.Type);
        }
    }

    public void InstantiateControls(GUIControlSet ownSet, GameObject parent, List<GUIControlDescription> descriptions)
    {
        foreach (GUIControlDescription description in descriptions)
        {
            if (description is GUIControlPictureDescription)
            {
                LoadGuiPicture(parent, description as GUIControlPictureDescription);
            }
            else if (description is GUIControlConditionalDescription)
            {
                LoadGuiConditional(ownSet, parent, description as GUIControlConditionalDescription);
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
                LoadGuiLocation(ownSet, parent, description as GUIControlLocationDescription);
            }
            else if (description is GUIControlSoundDescription)
            {
                LoadGuiSound(parent, description as GUIControlSoundDescription);
            }
            else if (description is GUIControlActiveDescription)
            {
                LoadGuiActive(ownSet, parent, description as GUIControlActiveDescription);
            }
            else
            {
                Debug.Log("Unhandled control description type " + description.GetType() + " to instantiate control!");
            }
        }
    }

    public GUIControlSet LoadControlSet(string path)
    {
        if (controlSets.ContainsKey(path))
        {
            return controlSets[path];
        }

        ResourceFile generalResources = ResourceManager.Instance.GeneralResources;

        if (generalResources.Exists(path))
        {
            ResourceInfo controlSetRescInfo = generalResources.Resources[path];
            byte[] controlSetInfoData = generalResources.ReadResourceData(controlSetRescInfo);

            using (MemoryStream memStream = new MemoryStream(controlSetInfoData))
            {
                var controlDesc = new GUIControlDescriptionFile(memStream);
                controlDesc.Filename = path;

                var controlSet = new GUIControlSet(container, controlDesc);

                controlSets.Add(path, controlSet);
                return controlSet;
            }
        }

        return null;
    }
}
