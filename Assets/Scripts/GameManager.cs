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
    public GameObject gameScenePrefabObject;
    public GameObject dialogueContainer;
    public GameObject textBalloonTop;
    public GameObject textBalloonBottom;
    public GameObject continueConfirmator;
    public GameObject dialogueChoices;
    public GameObject backgroundObject;

    private GameObject activeGadget;
    private GUIControlSet activeGUI;

    private Dictionary<string, Dialogue> dialogueCache;
    private Dictionary<string, SpriteAnimatorController> iconCache;
    private Dictionary<string, ScriptBlock<GadgetOpcode>> gadgetCache;

    private SceneAudioController persistentAudioController;
    private SceneAudioController dialogueAudioController;
    private GameTextBalloonController textBalloonTopController;
    private GameTextBalloonController textBalloonBottomController;
    private ActionInterpreter defaultActionInterpreter;
    private GameContinueConfirmatorController confirmedController;
    private GameChoicesController dialogueChoicesController;
    private SpriteRenderer backgroundRenderer;

    private Stack<GameObject> gadgets;

    public ActionInterpreter CurrentActionInterpreter => activeGUI?.ActionInterpreter ?? defaultActionInterpreter;

    // When there's no more dialogue display, the dialogue state will be disabled
    public delegate void OnDialogueStateChanged(bool enable);
    public event OnDialogueStateChanged DialogueStateChanged;

    // Start is called before the first frame update
    void Start()
    {
        Instance = this;

        dialogueCache = new Dictionary<string, Dialogue>(StringComparer.OrdinalIgnoreCase);
        gadgetCache = new Dictionary<string, ScriptBlock<GadgetOpcode>>(StringComparer.OrdinalIgnoreCase);
        iconCache = new Dictionary<string, SpriteAnimatorController>(StringComparer.OrdinalIgnoreCase);
        gadgets = new Stack<GameObject>();

        persistentAudioController = GetComponent<SceneAudioController>();
        dialogueAudioController = dialogueContainer.GetComponent<SceneAudioController>();

        textBalloonTopController = textBalloonTop.GetComponent<GameTextBalloonController>();
        textBalloonBottomController = textBalloonBottom.GetComponent<GameTextBalloonController>();
        confirmedController = continueConfirmator.GetComponent<GameContinueConfirmatorController>();
        dialogueChoicesController = dialogueChoices.GetComponent<GameChoicesController>();
        backgroundRenderer = backgroundObject.GetComponent<SpriteRenderer>();

        textBalloonTopController.Setup(Constants.CanvasSize, false);
        textBalloonBottomController.Setup(Constants.CanvasSize, true);
        dialogueChoicesController.Setup(Constants.CanvasSize);

        backgroundRenderer.size = GameUtils.ToUnitySize(Constants.CanvasSize);
        dialogueChoicesController.ChoiceConfirmed += OnChoiceConfirmed;

        defaultActionInterpreter = new ActionInterpreter();

        Application.targetFrameRate = 30;
        QualitySettings.vSyncCount = 1;

        ResourceManager.Instance.OnResourcesReady += OnResourcesReady;
    }

    private void HideGadgetRelatedObjects()
    {
        textBalloonBottomController.HideText();
        textBalloonTopController.HideText();
        dialogueChoicesController.Close();
        backgroundRenderer.color = Color.clear;
    }

    public void ReturnGadget()
    {
        if (gadgets.Count == 0)
        {
            Debug.LogError("No active dialogues available! De-activating the GUI");

            if (activeGUI != null)
            {
                activeGUI.Disable();
            }

            dialogueAudioController.StopAll();
            activeGUI = null;

            return;
        }

        gadgets.Pop();

        GameObject.Destroy(activeGadget);

        HideGadgetRelatedObjects();
        activeGadget = (gadgets.Count == 0) ? null : gadgets.Peek();

        if (activeGadget == null)
        {
            GameInputManager.Instance.SetGUIInputActionMapState(true);
            DialogueStateChanged?.Invoke(false);
        }
    }

    public void PushGadget(GameObject newActive)
    {
        activeGadget = newActive;
        activeGadget.SetActive(true);

        gadgets.Push(activeGadget);
    }

    private void CleanAllPendingGadgets()
    {
        while (gadgets.Count != 0)
        {
            GameObject.Destroy(gadgets.Pop());
        }

        HideGadgetRelatedObjects();
        activeGadget = null;

        GameInputManager.Instance.SetGUIInputActionMapState(true);
        DialogueStateChanged?.Invoke(false);
    }

    public void SetCurrentGUI(GUIControlSet newGUI)
    {
        if (activeGUI == newGUI)
        {
            return;
        }

        // Deactive an activating GUI
        if (activeGUI != null)
        {
            activeGUI.Disable();
        }

        dialogueAudioController.StopAll();
        CleanAllPendingGadgets();

        activeGUI = newGUI;
        activeGUI.Enable();
    }

    public void OnResourcesReady()
    {
        //LoadMinigame("ch4/minigames/constructionSite.mini");
        LoadControlSet(FilePaths.MainChapterGUIControlFileName);
    }

    private void LoadGadgetScriptBlock(Dialogue parent, string name, ScriptBlock<GadgetOpcode> scriptBlock)
    {
        GameObject containerObject = Instantiate(gameScenePrefabObject, dialogueContainer.transform, false);
        containerObject.name = name;
        containerObject.transform.localPosition = Vector3.zero;
        containerObject.transform.localScale = Vector3.one;

        PushGadget(containerObject);

        GadgetInterpreter interpreter = new GadgetInterpreter(CurrentActionInterpreter, containerObject, scriptBlock, parent);
        StartCoroutine(interpreter.Execute(() => {
            GameInputManager.Instance.SetGUIInputActionMapState(false);
            DialogueStateChanged?.Invoke(true);
        }));
    }

    public void LoadDialogueSlide(Dialogue parent, DialogueSlide slide)
    {
        LoadGadgetScriptBlock(parent, string.Format("Slide_{0}_{1}", slide.Id, parent.FileName), slide.DialogScript);
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
            dialogue.Strings = LocalizerHelper.GetStrings(filename);

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
            Dialogue tempDialogue = new Dialogue(null, 0);
            tempDialogue.FileName = filename;
            tempDialogue.Strings = LocalizerHelper.GetStrings(filename);

            LoadGadgetScriptBlock(tempDialogue, string.Format("standaloneGadget_{0}", filename), block);
        }
    }

    public void LoadControlSet(string filename)
    {
        GUIControlSet set = GUIControlSetFactory.Instance.LoadControlSet(filename, Constants.CanvasSize, new GUIControlSetInstantiateOptions());
        if (set != null)
        {
            SetCurrentGUI(set);
        }
    }

    public void LoadMinigame(string filename)
    {
        ResourceFile resourcesToLoadFrom = ResourceManager.Instance.GeneralResources;
        if (!resourcesToLoadFrom.Exists(filename))
        {
            throw new FileNotFoundException("Can't find minigame script file " + filename);
        }
        byte[] data = resourcesToLoadFrom.ReadResourceData(resourcesToLoadFrom.Resources[filename]);

        using (MemoryStream dataStream = new MemoryStream(data))
        {
            GUIControlSet set = MinigameLoader.Load(dataStream, filename, Constants.CanvasSize);
            if (set != null)
            {
                SetCurrentGUI(set);
            }
        }
    }

    public void SetControlSetOffset(Vector2 offset)
    {
        if (activeGUI != null)
        {
            activeGUI.SetOffset(offset);
        }
    }

    public void PanControlSet(Vector2 panVector)
    {
        if (activeGUI != null)
        {
            activeGUI.Pan(panVector);
        }
    }

    public void PlayAudioPersistent(string filename, string type)
    {
        persistentAudioController.Play(filename, type);
    }

    public void PlayAudioInDialogue(string filename, string type)
    {
        if (type.Equals("normal", StringComparison.OrdinalIgnoreCase))
        {
            // Use dialogue audio source
            dialogueAudioController.Play(filename, type);
        } else
        {
            // Use persistent audio source for it to remain throughout the location
            persistentAudioController.Play(filename, type);
        }
    }

    private GameTextBalloonController SelectTextController(string position)
    {
        bool isTop = position.Equals("top", StringComparison.OrdinalIgnoreCase);
        return isTop ? textBalloonTopController : textBalloonBottomController;
    }

    public void ShowText(string text, string where)
    {
        SelectTextController(where).ChangeText(text);
    }

    public void HideText(string where)
    {
        SelectTextController(where).HideText();
    }

    public void SetBalloon(string filename, string where)
    {
        SelectTextController(where).SetBalloon(filename);
    }

    public void SetStinger(Vector2 position, string filename, string where)
    {
        SelectTextController(where).SetStinger(position, filename);
    }

    private void LoadIconAnimationToCache(string name, string path, Vector2 position)
    {
        GameObject animationObj = Instantiate(gameAnimationPrefabObject, dialogueContainer.transform, false);
        animationObj.transform.localScale = Vector3.one;
        animationObj.transform.localPosition = Vector3.zero;
        animationObj.name = string.Format("icon_{0}", name);

        SpriteAnimatorController controller = animationObj.GetComponent<SpriteAnimatorController>();
        if (controller != null)
        {
            controller.Setup(position, 0.0f, path, Constants.IconLayer, null, false, true);
        }

        iconCache.Add(name, controller);
    }

    public void DisplayIcon(string iconName, Vector2 position)
    {
        if (iconCache.TryGetValue(iconName, out SpriteAnimatorController controller))
        {
            controller.Enable();
        } else
        {
            if (Constants.IconNameToAnimationPath.TryGetValue(iconName, out string pathValue))
            {
                LoadIconAnimationToCache(iconName, pathValue, position);
            }
        }
    }

    public void StartHearingConfirmation()
    {
        confirmedController.StartHearing();
    }

    public void AddChoices(Dialogue dialogue, List<Tuple<string, int>> dialogueIdFollows)
    {
        foreach (var stringAndDialogueId in dialogueIdFollows)
        {
            dialogueChoicesController.AddChoice(stringAndDialogueId.Item1, dialogue, stringAndDialogueId.Item2);
        }

        dialogueChoicesController.UpdateTextAndRenderLayout();
    }

    private void OnChoiceConfirmed(Dialogue dialogue, DialogueSlide dialogueSlide)
    {
        ReturnGadget();
        LoadDialogueSlide(dialogue, dialogueSlide);
    }

    public void SetBackgroundColor(Color color)
    {
        backgroundRenderer.color = color;
    }

    // Which layer that has all zero scroll x and y will not be assigned
    public void SetControlSetScrollSpeeds(Vector2[] speed)
    {
        activeGUI.SetLocationScrollSpeeds(speed);
    }

    public void RunPersistentCoroutine(IEnumerator coroutine)
    {
        StartCoroutine(coroutine);
    }

    public bool ContinueConfirmed => confirmedController.Confirmed;
    public bool LastTextFinished => textBalloonTopController.LastTextFinished && textBalloonBottomController.LastTextFinished;

    public bool GUIBusy => (activeGUI != null) && (activeGUI.Busy);
}
