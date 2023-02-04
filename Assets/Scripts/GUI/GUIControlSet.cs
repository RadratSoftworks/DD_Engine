using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

public class GUIControlSet
{
    private GameObject gameObject;
    private Dictionary<string, string> langStrings = new Dictionary<string, string>();

    public Dictionary<string, string> Strings => langStrings;
    public GameObject GameObject => gameObject;

    public string GetLanguageString(string key)
    {
        if (!langStrings.ContainsKey(key))
        {
            Debug.LogWarning("Can't find localized text for the key: " + key);
            return key;
        }

        return langStrings[key];
    }

    public GUIControlSet(GameObject parentContainer, GUIControlDescriptionFile description)
    {
        langStrings = LocalizerHelper.GetStrings(ResourceManager.Instance.LocalizationResources, description.Filename);

        gameObject = new GameObject(Path.ChangeExtension(description.Filename, null));
        gameObject.transform.parent = parentContainer.transform;
        gameObject.transform.localScale = Vector3.one;

        GUIManager.Instance.InstantiateControls(this, gameObject, description.Controls);
    }
}
