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
    public GameObject gameScenePrefabObject;
    public GameObject dialogueContainer;
    public GameObject textBalloonTop;
    public GameObject textBalloonBottom;
    public GameObject continueConfirmator;
    public GameObject dialogueChoices;

    private GameObject activeDialogueSlide;
    private GUIControlSet activeGUI;

    private Dictionary<string, Dialogue> dialogueCache;
    private Dictionary<string, GameObject> iconCache;
    private Dictionary<string, ScriptBlock<GadgetOpcode>> gadgetCache;

    private Dictionary<string, string> iconToPathMap;

    private SceneAudioController persistentAudioController;
    private SceneAudioController dialogueAudioController;
    private GameTextBalloonController textBalloonTopController;
    private GameTextBalloonController textBalloonBottomController;
    private ActionInterpreter defaultActionInterpreter;
    private GameContinueConfirmatorController confirmedController;
    private GameChoicesController dialogueChoicesController;

    public GameObject CurrentactiveDialogueSlide => activeDialogueSlide;

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
        iconCache = new Dictionary<string, GameObject>(StringComparer.OrdinalIgnoreCase);

        persistentAudioController = GetComponent<SceneAudioController>();
        dialogueAudioController = dialogueContainer.GetComponent<SceneAudioController>();

        textBalloonTopController = textBalloonTop.GetComponent<GameTextBalloonController>();
        textBalloonBottomController = textBalloonBottom.GetComponent<GameTextBalloonController>();
        confirmedController = continueConfirmator.GetComponent<GameContinueConfirmatorController>();
        dialogueChoicesController = dialogueChoices.GetComponent<GameChoicesController>();

        Vector2 canvas = new Vector2(Constants.CanvasX, Constants.CanvasY);
        textBalloonTopController.Setup(canvas, false);
        textBalloonBottomController.Setup(canvas, true);
        dialogueChoicesController.Setup(canvas);

        dialogueChoicesController.ChoiceConfirmed += OnChoiceConfirmed;

        defaultActionInterpreter = new ActionInterpreter();
        iconToPathMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        iconToPathMap.Add("plusinv", "animations/inv.anim");
        iconToPathMap.Add("plusmap", "animations/Plus_map.anim");
        iconToPathMap.Add("newtask", "animations/New_quest.anim");

        Application.targetFrameRate = 30;
        QualitySettings.vSyncCount = 1;

        ResourceManager.Instance.OnResourcesReady += OnResourcesReady;
    }

    public void ExitDialogueOrLocation()
    {
        // Return order: dialogue then location
        if (activeDialogueSlide != null)
        {
            GameObject.Destroy(activeDialogueSlide);

            dialogueAudioController.StopAll();

            textBalloonBottomController.HideText();
            textBalloonTopController.HideText();
            dialogueChoicesController.Close();

            activeDialogueSlide = null;
            DialogueStateChanged?.Invoke(false);

            return;
        }

        if (activeGUI != null)
        {
            activeGUI.Disable();

            dialogueAudioController.StopAll();
            activeGUI = null;
        }
    }

    public void SetactiveDialogueSlideSlide(GameObject newActive)
    {
        if (activeDialogueSlide != null)
        {
            GameObject.Destroy(activeDialogueSlide);
            activeDialogueSlide = null;

            textBalloonBottomController.HideText();
            textBalloonTopController.HideText();
            dialogueChoicesController.Close();

            dialogueAudioController.StopAll();
        }

        activeDialogueSlide = newActive;
        activeDialogueSlide.SetActive(true);

        DialogueStateChanged?.Invoke(true);
    }

    public void SetCurrentGUI(GUIControlSet newGUI)
    {
        // Deactive an activating GUI
        if (activeGUI != null)
        {
            activeGUI.Disable();
        }

        dialogueAudioController.StopAll();

        activeGUI = newGUI;
        activeGUI.Enable();
    }

    public void OnResourcesReady()
    {
        LoadControlSet(FilePaths.MainChapterGUIControlFileName);
    }

    public void LoadDialogueSlide(Dialogue parent, DialogueSlide slide)
    {
        GameObject containerObject = Instantiate(gameScenePrefabObject, dialogueContainer.transform, false);
        containerObject.name = string.Format("Slide_{0}_{1}", slide.Id, parent.FileName);
        containerObject.transform.localPosition = Vector3.zero;
        containerObject.transform.localScale = Vector3.one;

        SetactiveDialogueSlideSlide(containerObject);

        GadgetInterpreter interpreter = new GadgetInterpreter(CurrentActionInterpreter, containerObject, slide.DialogScript, parent);
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
            dialogue.Strings = LocalizerHelper.GetStrings(ResourceManager.Instance.LocalizationResources, filename);

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
            GadgetInterpreter interpreter = new GadgetInterpreter(CurrentActionInterpreter, null, block);
            StartCoroutine(interpreter.Execute());
        }
    }

    public void LoadControlSet(string filename)
    {
        GUIControlSet set = GUIControlSetFactory.Instance.LoadControlSet(filename);
        if (set != null)
        {
            SetCurrentGUI(set);
        }
    }

    public void SetControlSetOffset(Vector2 offset)
    {
        if (activeGUI != null)
        {
            activeGUI.GameObject.transform.localPosition = GameUtils.ToUnityCoordinates(offset);
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

        iconCache.Add(name, animationObj);
    }

    public void DisplayIcon(string iconName, Vector2 position)
    {
        if (iconCache.TryGetValue(iconName, out GameObject animObj))
        {
            animObj.SetActive(true);
            
            SpriteAnimatorController animController = animObj.GetComponent<SpriteAnimatorController>();
            if (animController != null)
            {
                animController.Restart(position);
            }
        } else
        {
            if (iconToPathMap.TryGetValue(iconName, out string pathValue))
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
        LoadDialogueSlide(dialogue, dialogueSlide);
    }

    public bool ContinueConfirmed => confirmedController.Confirmed;
    public bool LastTextFinished => textBalloonTopController.LastTextFinished && textBalloonBottomController.LastTextFinished;
    public bool DialogueSlideActivated => (activeDialogueSlide != null);

    public bool GUIBusy => (activeGUI != null) && (activeGUI.Busy);
}
