using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GUIControlSet
{
    private GameObject gameObject;
    private ActionInterpreter actionInterpreter;

    // Standard action library uses by control
    // Some explicitly state their action file, like Menu control
    private ActionLibrary standardActionLibrary;

    private Dictionary<string, string> langStrings = new Dictionary<string, string>();

    public Dictionary<string, string> Strings => langStrings;
    public GameObject GameObject => gameObject;
    public ActionInterpreter ActionInterpreter => actionInterpreter;
    public string Name { get; set; }

    public delegate void OnStateChanged(bool enabled);
    public event OnStateChanged StateChanged;

    public delegate void OnOffsetChanged(Vector2 newOffset);
    public event OnOffsetChanged OffsetChanged;

    public int performingBusyAnimationCount = 0;

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
        standardActionLibrary = ActionLibraryLoader.Load(Path.ChangeExtension(description.Filename, ActionLibraryLoader.FileExtension));

        Name = description.Filename;

        actionInterpreter = new ActionInterpreter();

        gameObject = new GameObject(Path.ChangeExtension(description.Filename, null));
        gameObject.transform.parent = parentContainer.transform;
        gameObject.transform.localScale = Vector3.one;
        gameObject.transform.position = Vector3.zero;
        gameObject.SetActive(false);

        GUIControlSetFactory.Instance.InstantiateControls(this, gameObject, description.Controls);
    }

    public IEnumerator HandleAction(string id, string actionName)
    {
        if (standardActionLibrary != null)
        {
            yield return standardActionLibrary.HandleAction(actionInterpreter, id, actionName);
        }

        yield break;
    }

    public void Disable()
    {
        gameObject.SetActive(false);
        StateChanged?.Invoke(false);
    }

    public void Enable()
    {
        performingBusyAnimationCount = 0;

        gameObject.SetActive(true);
        StateChanged?.Invoke(true);
    }

    public void RegisterPerformingBusyAnimation()
    {
        performingBusyAnimationCount++;
    }

    public void UnregisterPerformingBusyAnimation()
    {
        performingBusyAnimationCount--;
    }

    public void SetOffset(Vector2 offset)
    {
        OffsetChanged?.Invoke(offset);
    }

    public bool Busy => performingBusyAnimationCount != 0;
}
