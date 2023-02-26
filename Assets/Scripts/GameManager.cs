using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

using UnityEngine;
using UnityEngine.InputSystem;

using Newtonsoft.Json;
using System.Threading.Tasks;

public class GameManager : MonoBehaviour
{
    private class GadgetObjectInfo
    {
        public string Path;
        public int Id;
        public bool Saveable;
        public GameObject Object;
    }

    public static GameManager Instance;

    public GameObject gameAnimationPrefabObject;
    public GameObject gameImagePrefabObject;
    public GameObject gameScenePrefabObject;
    public GameObject dialogueContainer;
    public GameObject textBalloonTop;
    public GameObject textBalloonMiddle;
    public GameObject textBalloonBottom;
    public GameObject continueConfirmator;
    public GameObject dialogueChoices;
    public GameObject backgroundObject;

    private GadgetObjectInfo activeGadget;
    private GUIControlSet activeGUI;

    private Dictionary<string, Dialogue> dialogueCache;
    private Dictionary<string, SpriteAnimatorController> iconCache;
    private Dictionary<string, ScriptBlock<GadgetOpcode>> gadgetCache;

    private SceneAudioController persistentAudioController;
    private SceneAudioController dialogueAudioController;
    private GameTextBalloonController textBalloonTopController;
    private GameTextBalloonController textBalloonMiddleController;
    private GameTextBalloonController textBalloonBottomController;
    private ActionInterpreter defaultActionInterpreter;
    private GameContinueConfirmatorController confirmedController;
    private GameChoicesController dialogueChoicesController;
    private SpriteRenderer backgroundRenderer;

    private Stack<GadgetObjectInfo> gadgets;
    private float targetFrameFactor = 3;

    private GameSave gameSave;
    private bool allowSave = false;

    public ActionInterpreter CurrentActionInterpreter => activeGUI?.ActionInterpreter ?? defaultActionInterpreter;

    public bool ContinueConfirmed => confirmedController.Confirmed;
    public bool LastTextFinished => textBalloonTopController.LastTextFinished && textBalloonBottomController.LastTextFinished;
    public bool GUIBusy => (activeGUI != null) && (activeGUI.Busy);
    public bool GadgetActive => (activeGadget != null);
    public float FrameScale => targetFrameFactor;

    private string SavePath => Path.Join(Application.persistentDataPath, "save.json");
    private bool SaveAvailable => File.Exists(SavePath);

    private const string globalVarDictKey = "global";

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
        gadgets = new Stack<GadgetObjectInfo>();
        gameSave = new GameSave();

        persistentAudioController = GetComponent<SceneAudioController>();
        dialogueAudioController = dialogueContainer.GetComponent<SceneAudioController>();

        textBalloonTopController = textBalloonTop.GetComponent<GameTextBalloonController>();
        textBalloonMiddleController = textBalloonMiddle.GetComponent<GameTextBalloonController>();
        textBalloonBottomController = textBalloonBottom.GetComponent<GameTextBalloonController>();
        confirmedController = continueConfirmator.GetComponent<GameContinueConfirmatorController>();
        dialogueChoicesController = dialogueChoices.GetComponent<GameChoicesController>();
        backgroundRenderer = backgroundObject.GetComponent<SpriteRenderer>();

        textBalloonTopController.Setup(Constants.CanvasSize, GameTextBalloonController.Placement.Top);
        textBalloonMiddleController.Setup(Constants.CanvasSize, GameTextBalloonController.Placement.Middle);
        textBalloonBottomController.Setup(Constants.CanvasSize, GameTextBalloonController.Placement.Bottom);
        dialogueChoicesController.Setup(Constants.CanvasSize);

        backgroundRenderer.size = GameUtils.ToUnitySize(Constants.CanvasSize);
        dialogueChoicesController.ChoiceConfirmed += OnChoiceConfirmed;

        defaultActionInterpreter = new ActionInterpreter();

        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 0;
        targetFrameFactor = 60.0f / Constants.BaseGameFps;

        ResourceManager.Instance.OnResourcesReady += OnResourcesReady;

        // Add save exists variable to global values
        if (SaveAvailable)
        {
            ActionInterpreter.GlobalScriptValues.Add(Constants.SaveExistsVarName, "true");
        }

        gameSave.ActionValues[globalVarDictKey] = ActionInterpreter.GlobalScriptValues;
        GameSettings.RestoreSettings();

        SetupMenuTrigger();
    }

    private void SetupMenuTrigger()
    {
        // Hook with menu trigger
        InputAction action = GameInputManager.Instance.MenuTriggerAtionMap.FindAction("Menu Triggered");
        if (action != null)
        {
            action.performed += OnMenuTriggered; 
        }
    }

    private void OnMenuTriggered(InputAction.CallbackContext context)
    {
        SaveGame();

        allowSave = false;
        persistentAudioController.StopAll();

        LoadControlSet(FilePaths.MainChapterGUIControlFileName);
    }

    private void HideGadgetRelatedObjects()
    {
        textBalloonBottomController.HideText();
        textBalloonTopController.HideText();
        textBalloonMiddleController.HideText();
        dialogueChoicesController.Close();
        continueConfirmator.SetActive(false);
        backgroundRenderer.color = Color.clear;
    }

    private void TrySaveOnDialogue()
    {
        gameSave.CurrentGadgetPath = activeGadget.Path;
        gameSave.CurrentGadgetId = activeGadget.Id;

        allowSave = activeGadget.Saveable;

        SaveGame();
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

            gameSave.CurrentControlSetPath = null;
            SaveGame();

            // Enable menu trigger. An actual menu widget will disable it if there is one
            GameInputManager.Instance.SetGUIMenuTriggerActionMapState(true);
            return;
        }

        gadgets.Pop();

        GameObject.Destroy(activeGadget.Object);

        HideGadgetRelatedObjects();
        activeGadget = (gadgets.Count == 0) ? null : gadgets.Peek();

        if (activeGadget == null)
        {
            GameInputManager.Instance.SetGUIInputActionMapState(true);

            if (activeGUI != null) {
                activeGUI.EnableRecommendedTouchControl();
            }

            DialogueStateChanged?.Invoke(false);
        } else
        {
            TrySaveOnDialogue();
        }
    }

    private void PushGadget(GadgetObjectInfo newActive)
    {
        activeGadget = newActive;
        activeGadget.Object.SetActive(true);

        TrySaveOnDialogue();
        gadgets.Push(activeGadget);
    }

    private void CleanAllPendingGadgets()
    {
        if (gadgets.Count == 0)
        {
            return;
        }

        while (gadgets.Count != 0)
        {
            GameObject.Destroy(gadgets.Pop().Object);
        }

        HideGadgetRelatedObjects();
        activeGadget = null;

        DialogueStateChanged?.Invoke(false);
        StopAllCoroutines();

        // Clear gadget path
        gameSave.CurrentGadgetPath = null;
        gameSave.CurrentGadgetId = -1;

        SaveGame();
    }

    private void PruneCache()
    {
        // Most likely only used in a control set only, so we can clear
        gadgetCache.Clear();
        dialogueCache.Clear();

        SpriteManager.Instance.PruneCache();
        SoundManager.Instance.PruneCache();
        GUIControlSetFactory.Instance.PruneCache();

        GameGC.TryUnloadUnusedAssets();
    }

    public void SetCurrentGUI(GUIControlSet newGUI, bool notSave = false)
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
        PruneCache();

        activeGUI = newGUI;

        // Enable menu trigger. An actual menu widget will disable it if there is one
        GameInputManager.Instance.SetGUIMenuTriggerActionMapState(true);

        if (activeGUI != null)
        {
            activeGUI.Enable();
            activeGUI.EnableRecommendedTouchControl();

            DialogueStateChanged?.Invoke(false);

            GameInputManager.Instance.SetGUIInputActionMapState(true);

            if (activeGUI.Saveable)
            {
                gameSave.CurrentControlSetPath = activeGUI.Name;

                if (!notSave)
                {
                    SaveGame();
                }
            }
        } else
        {
            gameSave.CurrentControlSetPath = null;
        }
    }

    public void OnResourcesReady()
    {
        if ((GameSettings.StartLocation == GameStartLocation.Menu) || !SaveAvailable)
        {
            LoadControlSet(FilePaths.MainChapterGUIControlFileName);
        } else
        {
		    // Enable menu trigger. An actual menu widget will disable it if there is one
			GameInputManager.Instance.SetGUIMenuTriggerActionMapState(true);
            LoadGame();
        }
    }

    private void LoadGadgetScriptBlock(Dialogue parent, string name, ScriptBlock<GadgetOpcode> scriptBlock, int scriptId = -1, bool savable = true)
    {
        GameObject containerObject = Instantiate(gameScenePrefabObject, dialogueContainer.transform, false);
        containerObject.name = name;
        containerObject.transform.localPosition = Vector3.zero;
        containerObject.transform.localScale = Vector3.one;

        PushGadget(new GadgetObjectInfo()
        {
            Path = parent.FileName,
            Id = scriptId,
            Saveable = savable,
            Object = containerObject
        });

        GadgetInterpreter interpreter = new GadgetInterpreter(CurrentActionInterpreter, containerObject, scriptBlock, parent);
        StartCoroutine(interpreter.Execute(() => {
            // Use a dpad for dialogue navigation
            GameInputManager.Instance.SetNavigationTouchControl(false);
            DialogueStateChanged?.Invoke(true);
        }));
    }

    private Dialogue RetrieveDialogue(string filename)
    {
        Dialogue dialogue = null;
        if (dialogueCache.ContainsKey(filename))
        {
            dialogue = dialogueCache[filename];
        }
        else
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

        return dialogue;
    }

    public void LoadDialogueSlide(Dialogue parent, DialogueSlide slide)
    {
        // A dialogue can be saved or not depends on the Gadget script that load the location or dialogue. We can also depends on the previous slide too, no worries,
        // as they all assign savable from the same script root.
        LoadGadgetScriptBlock(parent, string.Format("Slide_{0}_{1}", slide.Id, parent.FileName), slide.DialogScript, slide.Id, allowSave);
    }

    public void LoadDialogue(string filename)
    {
        Dialogue dialogue = RetrieveDialogue(filename);

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

            LoadGadgetScriptBlock(tempDialogue, string.Format("standaloneGadget_{0}", filename), block, savable: block.Saveable);
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
                SetCurrentGUI(set, true);
            }

            // Save when we entered a minigame
            gameSave.CurrentControlSetPath = filename;
            SaveGame();
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
        bool isMiddle = position.Equals("mid", StringComparison.OrdinalIgnoreCase);
        return isTop ? textBalloonTopController : (isMiddle ? textBalloonMiddleController : textBalloonBottomController);
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
        animationObj.transform.localPosition = GameUtils.ToUnityCoordinates(position);
        animationObj.name = string.Format("icon_{0}", name);

        SpriteAnimatorController controller = animationObj.GetComponentInChildren<SpriteAnimatorController>();
        if (controller != null)
        {
            controller.Setup(Vector2.zero, 0.0f, path, Constants.IconLayer, null, false, true);
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

    public void StopPersistentCoroutine(IEnumerator coroutine)
    {
        StopCoroutine(coroutine);
    }

    public int GetRealFrames(int gameFrames)
    {
        return (int)(gameFrames * targetFrameFactor);
    }

    private void RestoreGameSaveTracking()
    {
        gameSave.ActionValues[globalVarDictKey] = ActionInterpreter.GlobalScriptValues;
    }

    private void InitializeFromGameSave()
    {
        allowSave = false;

        // Update global action values
        foreach (var item in gameSave.ActionValues[globalVarDictKey])
        {
            if (item.Key != Constants.SaveExistsVarName)
            {
                ActionInterpreter.GlobalScriptValues[item.Key] = item.Value;
            }
        }

        if (gameSave.CurrentControlSetPath != null)
        {
            if (Path.GetExtension(gameSave.CurrentControlSetPath).Equals(FilePaths.MinigameFileExtension, StringComparison.OrdinalIgnoreCase))
            {
                LoadMinigame(gameSave.CurrentControlSetPath);
            }
            else
            {
                LoadControlSet(gameSave.CurrentControlSetPath);

                if ((activeGUI != null) && (activeGUI.Location != null))
                {
                    activeGUI.Location.ScrollFromOrigin(gameSave.CurrentLocationOffset);
                }
            }
        } else
        {
            // Still clear active control set anyway
            SetCurrentGUI(null);
        }

        if (gameSave.CurrentGadgetPath != null)
        {
            if (Path.GetExtension(gameSave.CurrentGadgetPath).Equals(FilePaths.GadgetScriptFileExtension, StringComparison.OrdinalIgnoreCase))
            {
                LoadGadget(gameSave.CurrentGadgetPath);
            } else
            {
                if (gameSave.CurrentGadgetId >= 0)
                {
                    Dialogue dialogue = RetrieveDialogue(gameSave.CurrentGadgetPath);
                    DialogueSlide dialogueSlide = dialogue.GetDialogueSlideWithId(gameSave.CurrentGadgetId);

                    LoadDialogueSlide(dialogue, dialogueSlide);
                }
            }
        }

        allowSave = true;

        // After we done loading, restore the game save data to keep tracking current game progress
        RestoreGameSaveTracking();
    }

    public void LoadGame()
    {
        using (StreamReader file = File.OpenText(SavePath))
        {
            JsonSerializer serializer = new JsonSerializer()
            {
                NullValueHandling = NullValueHandling.Ignore,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };

            gameSave = serializer.Deserialize(file, typeof(GameSave)) as GameSave;
        }

        if (gameSave != null)
        {
            InitializeFromGameSave();
        }
    }

    private void SaveGame()
    {
        if (!allowSave)
        {
            return;
        }

        using (StreamWriter file = new StreamWriter(SavePath))
        {
            if ((activeGUI != null) && (activeGUI.Location != null))
            {
                gameSave.CurrentLocationOffset = activeGUI.Location.GetCurrentScrollOffset();
            }

            JsonSerializer serializer = new JsonSerializer()
            {
                NullValueHandling = NullValueHandling.Ignore,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };

            serializer.Serialize(file, gameSave);
        }
    }

    public void ClearSave()
    {
        File.Delete(SavePath);
        defaultActionInterpreter.SetGlobalVariable(Constants.SaveExistsVarName, "null");
    }
}
