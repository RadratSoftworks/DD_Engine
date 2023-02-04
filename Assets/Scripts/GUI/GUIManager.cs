using System;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.IO;
using UnityEngine.UIElements;

public class GUIManager : MonoBehaviour
{
    public static GUIManager Instance;
    private Dictionary<string, GUIControlSet> controlSets;

    public GameObject guiPicturePrefabObject;
    public GameObject guiMenuOptionPrefabObject;
    public GameObject guiAnimationPrefabObject;
    public GameObject guiMenuPrefabObject;
    public GameObject container;

    private void Start()
    {
        Instance = this;
        controlSets = new Dictionary<string, GUIControlSet>(StringComparer.OrdinalIgnoreCase);
    }

    private void LoadGuiPicture(GameObject parent, GUIControlPictureDescription description,bool relativeToBottomLeft = false)
    {
        GameObject anotherInstance = Instantiate(guiPicturePrefabObject, parent.transform);
        anotherInstance.transform.localPosition = GameUtils.ToUnityCoordinates(description.TopPosition);

        if (description.Id != null)
        {
            anotherInstance.name = description.Id;
        }
        SpriteRenderer renderer = anotherInstance.GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            renderer.sortingOrder = description.SortingPosition;
            renderer.sprite = SpriteManager.Instance.Load(ResourceManager.Instance.GeneralResources, description.ImagePath);

            if (relativeToBottomLeft)
            {
                anotherInstance.transform.localPosition = GameUtils.ToUnityCoordinates(new Vector2(0.0f, Constants.CanvasY - renderer.sprite.rect.size.y) - description.TopPosition);
            }
        }
    }

    private void LoadGuiConditional(GUIControlSet ownSet, GameObject parent, GUIControlConditionalDescription description)
    {
        // TODO: Un-hardcode
        string variableValue = "null";
        if (description.ControlShowOnCases.ContainsKey(variableValue))
        {
            InstantiateControls(ownSet, parent, description.ControlShowOnCases[variableValue]);
        } else
        {
            Debug.Log("No value \"" + variableValue + "\" is handled in conditional!");
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
            SortingPosition = description.SortingPosition
        }, true);

        GUIMenuController controller = gameObject.GetComponent<GUIMenuController>();
        controller.LoadActionScript(description.ActionHandlerFilePath);

        GameObject gameObjectOptions = gameObject.transform.Find("MenuOptions")?.gameObject;
        Vector2 currentPosition = new Vector2(0.0f, 0.0f);

        LoadGuiAnimation(gameObject, new GUIControlAnimationDescription()
        {
            SortingPosition = description.SortingPosition,
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
            renderer.sortingOrder = description.SortingPosition;
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
            meshRenderer.sortingOrder = description.SortingPosition;
        }
    }

    private void LoadGuiAnimation(GameObject parent, GUIControlAnimationDescription description)
    {
        GameObject anotherInstance = Instantiate(guiAnimationPrefabObject, parent.transform, false);
        anotherInstance.transform.localPosition = GameUtils.ToUnityCoordinates(description.TopPosition);

        SpriteRenderer renderer = anotherInstance.GetComponent<SpriteRenderer>();
        SpriteAnimatorController animController = anotherInstance.GetComponent<SpriteAnimatorController>();

        if (renderer != null)
        {
            renderer.sortingOrder = description.SortingPosition;
        }

        if (animController != null)
        {
            animController.AnimationFilename = description.AnimationPath;
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
