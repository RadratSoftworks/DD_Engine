using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public GameObject gameAnimationPrefabObject;
    public GameObject gameImagePrefabObject;
    public GameObject gameLayerPrefabObject;
    public GameObject dialogueContainer;

    private GameObject activeScene;
    private Dictionary<string, Dialogue> dialogueCache;
    private Dictionary<string, ScriptBlock<GadgetOpcode>> gadgetCache;

    // Start is called before the first frame update
    void Start()
    {
        Instance = this;

        dialogueCache = new Dictionary<string, Dialogue>(StringComparer.OrdinalIgnoreCase);
        gadgetCache = new Dictionary<string, ScriptBlock<GadgetOpcode>>(StringComparer.OrdinalIgnoreCase);

        Application.targetFrameRate = 30;
        QualitySettings.vSyncCount = 1;

        ResourceManager.Instance.OnResourcesReady += OnResourcesReady;
    }

    public void ReturnCurrent()
    {
        if (activeScene == null)
        {
            return;
        }

        activeScene.SetActive(false);

        // Delete, need to reload
        if (activeScene.transform.parent == dialogueContainer.transform)
        {
            GameObject.Destroy(activeScene);
            activeScene = null;
        }
    }

    public void SetCurrent(GameObject newScene)
    {
        activeScene = newScene;
        newScene.SetActive(true);
    }

    public GameObject GetCurrentScene()
    {
        return activeScene;
    }

    public void OnResourcesReady()
    {
        GUIControlSet set = GUIManager.Instance.LoadControlSet(FilePaths.MainChapterGUIControlFileName);
        if (set != null)
        {
            SetCurrent(set.GameObject);
        }
    }

    public void LoadDialogueSlide(Dialogue parent, DialogueSlide slide)
    {
        GameObject containerObject = new GameObject(string.Format("Slide_{0}_{1}", slide.Id, parent.FileName));
        containerObject.transform.SetParent(dialogueContainer.transform, true);
        containerObject.transform.localPosition = Vector3.zero;
        containerObject.transform.localScale = Vector3.one;

        SetCurrent(containerObject);

        GadgetInterpreter interpreter = new GadgetInterpreter(containerObject, slide.DialogScript, parent);
        StartCoroutine(interpreter.Execute());
    }

    public void LoadDialogue(string filename)
    {
        Dialogue dialogue = null;
        if (dialogueCache.ContainsKey(filename))
        {
            dialogue = dialogueCache[filename];
        } else
        {
            ResourceFile generalResources = ResourceManager.Instance.GeneralResources;
            if (!generalResources.Exists(filename))
            {
                throw new FileNotFoundException("Can't find dialogue script file " + filename);
            }
            byte[] data = generalResources.ReadResourceData(generalResources.Resources[filename]);

            dialogue = DialogueParser.Parse(new MemoryStream(data));
            dialogue.FileName = filename;

            if (dialogue != null)
            {
                dialogueCache.Add(filename, dialogue);
            }
        }

        if (dialogue != null)
        {
            LoadDialogueSlide(dialogue, dialogue.GetStartingDialogueSlide());
        }
    }

    public void LoadGadget(string filename)
    {
        ScriptBlock<GadgetOpcode> block = null;
        if (gadgetCache.ContainsKey(filename))
        {
            block = gadgetCache[filename];
        }
        else
        {
            ResourceFile generalResources = ResourceManager.Instance.GeneralResources;
            if (!generalResources.Exists(filename))
            {
                throw new FileNotFoundException("Can't find gadget script file " + filename);
            }
            byte[] data = generalResources.ReadResourceData(generalResources.Resources[filename]);

            block = GadgetParser.Parse(new MemoryStream(data));
            if (block != null)
            {
                gadgetCache.Add(filename, block);
            }
        }

        if (block != null)
        {
            GadgetInterpreter interpreter = new GadgetInterpreter(null, block);
            StartCoroutine(interpreter.Execute());
        }

    }
}
