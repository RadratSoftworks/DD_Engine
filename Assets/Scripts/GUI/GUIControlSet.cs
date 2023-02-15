using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.VisualScripting;

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

    public delegate void OnPanRequested(Vector2 amount);
    public event OnPanRequested PanRequested;

    public delegate void OnLocationScrollSpeedSetRequested(Vector2[] speeds);
    public event OnLocationScrollSpeedSetRequested LocationScrollSpeedSetRequested;

    private int performingBusyAnimationCount = 0;
    private bool destroyOnDisable;

    public Vector2 ViewSize => viewSize;
    private Vector2 viewSize;

    public string GetLanguageString(string key)
    {
        if (!langStrings.ContainsKey(key))
        {
            Debug.LogWarning("Can't find localized text for the key: " + key);
            return key;
        }

        return langStrings[key];
    }

    public GUIControlSet(GameObject parentContainer, GUIControlDescriptionFile description, Vector2 viewSize, GUIControlSetInstantiateOptions options)
    {
        langStrings = LocalizerHelper.GetStrings(description.Filename);
        standardActionLibrary = ActionLibraryLoader.Load(Path.ChangeExtension(description.Filename, ActionLibraryLoader.FileExtension));

        Name = description.Filename;

        actionInterpreter = new ActionInterpreter();
        destroyOnDisable = options.DestroyWhenDisabled;

        gameObject = new GameObject(Path.ChangeExtension(GameUtils.ToUnityName(description.Filename), null));
        gameObject.transform.parent = parentContainer.transform;
        gameObject.transform.localScale = Vector3.one;
        gameObject.transform.position = Vector3.zero;

        this.viewSize = GameUtils.ToUnitySize(viewSize);
        GUIControlSetFactory.Instance.InstantiateControls(this, gameObject, description.Controls, options);
    }

    public GUIControlSet(GameObject parentContainer, GameObject prefab, string filename, Vector2 viewSize, GUIControlSetInstantiateOptions options)
    {
        gameObject = GameObject.Instantiate(prefab, parentContainer.transform, false);
        Name = GameUtils.ToUnityName(filename);

        gameObject.name = Path.ChangeExtension(GameUtils.ToUnityName(filename), null);
        gameObject.transform.position = Vector3.zero;
        gameObject.transform.localScale = Vector3.one;

        actionInterpreter = new ActionInterpreter();
        destroyOnDisable = options.DestroyWhenDisabled;
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
        StateChanged?.Invoke(false);

        if (destroyOnDisable)
        {
            GameObject.Destroy(gameObject);
        } else
        {
            gameObject.SetActive(false);
        }
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

    public void Pan(Vector2 amount)
    {
        PanRequested?.Invoke(amount);
    }

    public void SetLocationScrollSpeeds(Vector2[] speeds)
    {
        LocationScrollSpeedSetRequested?.Invoke(speeds);
    }

    public bool Busy => performingBusyAnimationCount != 0;
}
