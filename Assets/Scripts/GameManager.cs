using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using UnityEngine;
using UnityEngine.InputSystem;

using Newtonsoft.Json;

using DDEngine.Action;
using DDEngine.BaseScript;
using DDEngine.Dialogue;
using DDEngine.Gadget;
using DDEngine.Game;
using DDEngine.Minigame;
using DDEngine.GUI;
using DDEngine.Utils;

namespace DDEngine
{
    public class GameManager : MonoBehaviour
    {
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

        private GadgetObjectInfo activeGadget;
        private GUIControlSet activeGUI;

        private Dictionary<string, GameDialogue> dialogueCache;
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

        private Stack<GadgetObjectInfo> gadgets;

        private GameSave gameSave;
        private int saveLock = 0;
        private int skipAllowance = 0;
        private bool inLoad = false;

        public ActionInterpreter CurrentActionInterpreter => activeGUI?.ActionInterpreter ?? defaultActionInterpreter;
        public bool AllowSave => (saveLock <= 0);
        public bool Skippable => (skipAllowance > 0);

        public bool ContinueConfirmed => confirmedController.Confirmed;
        public bool LastTextFinished => textBalloonTopController.LastTextFinished && textBalloonBottomController.LastTextFinished;
        public bool GUIBusy => (activeGUI != null) && (activeGUI.Busy);
        public bool GadgetActive => (activeGadget != null);
        private string SavePath => Path.Join(Application.persistentDataPath, "save.json");
        private bool SaveAvailable => File.Exists(SavePath);

        private const string globalVarDictKey = "global";

        // When there's no more dialogue display, the dialogue state will be disabled
        public delegate void OnDialogueStateChanged(bool enable);
        public event OnDialogueStateChanged DialogueStateChanged;
        private event System.Action ControlSetChanged;

        // Start is called before the first frame update
        void Start()
        {
            Instance = this;

            dialogueCache = new Dictionary<string, GameDialogue>(StringComparer.OrdinalIgnoreCase);
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

            textBalloonTopController.Setup(Constants.CanvasSize, GameTextBalloonController.Placement.Top);
            textBalloonMiddleController.Setup(Constants.CanvasSize, GameTextBalloonController.Placement.Middle);
            textBalloonBottomController.Setup(Constants.CanvasSize, GameTextBalloonController.Placement.Bottom);
            dialogueChoicesController.Setup(Constants.CanvasSize);

            dialogueChoicesController.ChoiceConfirmed += OnChoiceConfirmed;

            defaultActionInterpreter = new ActionInterpreter();

            QualitySettings.vSyncCount = 1;
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
        }

        private void ConsiderGadgetLock(GadgetObjectInfo info, bool unlock)
        {
            if (!info.Saveable)
            {
                if (unlock)
                {
                    saveLock--;
                } else
                {
                    saveLock++;
                }
            }

            if (info.Skippable)
            {
                if (unlock)
                {
                    skipAllowance--;
                } else
                {
                    skipAllowance++;
                }
            }
        }

        public void ReturnGadget(bool saveGame = true)
        {
            if (gadgets.Count == 0)
            {
                Debug.LogError("No active dialogues available! De-activating the GUI");

                if (activeGUI != null)
                {
                    if (!activeGUI.Saveable)
                    {
                        saveLock--;
                    }

                    activeGUI.Disable();
                }

                dialogueAudioController.StopAll();
                defaultActionInterpreter.ClearState();
                activeGUI = null;

                // NOTE: Do not save here
                gameSave.CurrentControlSetPath = null;

                // Enable menu trigger. An actual menu widget will disable it if there is one
                GameInputManager.Instance.SetGUIMenuTriggerActionMapState(true);
                return;
            }

            ConsiderGadgetLock(activeGadget, true);
            gadgets.Pop();

            GameObject.Destroy(activeGadget.Object);
            if (activeGadget.InDialogue)
            {
                HideGadgetRelatedObjects();
            }

            activeGadget = (gadgets.Count == 0) ? null : gadgets.Peek();

            if (activeGadget == null)
            {
                GameInputManager.Instance.SetGUIInputActionMapState(true);

                if (activeGUI != null)
                {
                    activeGUI.EnableRecommendedControls();
                }

                DialogueStateChanged?.Invoke(false);
            }

            if (saveGame)
            {
                SaveGame();
            }
        }

        private void PushGadget(GadgetObjectInfo newActive)
        {
            activeGadget = newActive;
            activeGadget.Object.SetActive(true);

            gadgets.Push(activeGadget);
            ConsiderGadgetLock(newActive, false);
        
            // If this is the first gadget in the stack
            if (gadgets.Count == 1)
            {
                SaveGame();
            }
        }

        private void CleanAllPendingGadgets()
        {
            if (gadgets.Count == 0)
            {
                return;
            }

            while (gadgets.Count != 0)
            {
                var info = gadgets.Pop();
                ConsiderGadgetLock(info, true);
                GameObject.Destroy(info.Object);
            }

            HideGadgetRelatedObjects();
            activeGadget = null;

            DialogueStateChanged?.Invoke(false);
            StopAllCoroutines();
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
                if (!activeGUI.Saveable)
                {
                    saveLock--;
                }

                activeGUI.Disable();
                defaultActionInterpreter.ClearState();
            }

            dialogueAudioController.StopAll();
            CleanAllPendingGadgets();
            PruneCache();

            activeGUI = newGUI;
            ControlSetChanged?.Invoke();

            // Enable menu trigger. An actual menu widget will disable it if there is one
            GameInputManager.Instance.SetGUIMenuTriggerActionMapState(true);

            if (activeGUI != null)
            {
                // Disable input actions, this is to prevent the new input event subscribe from being fired
                // immediately. It looks like re-enable will clear the pending events :)
                GameInputManager.Instance.SetGUIInputActionMapState(false);

                activeGUI.Enable();
                activeGUI.EnableRecommendedControls();

                DialogueStateChanged?.Invoke(false);

                GameInputManager.Instance.SetGUIInputActionMapState(true);

                if (!activeGUI.Saveable)
                {
                    saveLock++;
                }

                gameSave.CurrentControlSetPath = activeGUI.Name;
            }
            else
            {
                gameSave.CurrentControlSetPath = null;
            }

            if (!notSave)
            {
                SaveGame();
            }
        }

        public void OnResourcesReady()
        {
            if ((GameSettings.StartLocation == GameStartLocation.Menu) || !SaveAvailable)
            {
                LoadGadget(FilePaths.IntroMenuGadgetScript);
            }
            else
            {
                LoadGadget(FilePaths.IntroGameGadgetScript);
            }
        }

        private void LoadGadgetScriptBlock(GameDialogue parent, string name, ScriptBlock<GadgetOpcode> scriptBlock, int scriptId = -1)
        {
            GameObject containerObject = Instantiate(gameScenePrefabObject, dialogueContainer.transform, false);
            containerObject.name = name;
            containerObject.transform.localPosition = Vector3.zero;
            containerObject.transform.localScale = Vector3.one;

            PushGadget(new GadgetObjectInfo()
            {
                Path = parent.FileName,
                Id = scriptId,
                Saveable = scriptBlock.Saveable,
                Skippable = scriptBlock.Skippable,
                Object = containerObject
            });

            GameSceneController controller = containerObject.GetComponent<GameSceneController>();
            controller.Setup(Constants.CanvasSize, gadgets.Count - 1);

            GadgetInterpreter interpreter = new GadgetInterpreter(controller, scriptBlock, parent);
            StartCoroutine(interpreter.Execute(() => {
                // Use a dpad for dialogue navigation
                GameInputManager.Instance.SetNavigationTouchControl(false);
                GameInputManager.Instance.SetGUIInputActionMapState(false);

                DialogueStateChanged?.Invoke(true);
            }));
        }

        private GameDialogue RetrieveDialogue(string filename)
        {
            GameDialogue dialogue = null;
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

        public void LoadDialogueSlide(GameDialogue parent, DialogueSlide slide)
        {
            GameInputManager.Instance.SetGUIMenuTriggerActionMapState(true);
            LoadGadgetScriptBlock(parent, string.Format("Slide_{0}_{1}", slide.Id, parent.FileName), slide.DialogScript, slide.Id);
        }

        public void LoadDialogue(string filename)
        {
            GameDialogue dialogue = RetrieveDialogue(filename);

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
                ResourceFile generalResources = ResourceManager.Instance.PickBestResourcePackForFile(filename);
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
                GameDialogue tempDialogue = new GameDialogue(null, 0);
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

        public void PlayNormalAudioPersistent(AudioClip clip)
        {
            persistentAudioController.PlayNormal(clip);
        }

        public void PlayAudioPersistent(string filename, string type)
        {
            persistentAudioController.Play(filename, type);
        }

        public void StopAudioAmbient()
        {
            persistentAudioController.StopAmbient();
        }

        public void PlayAudioInDialogue(string filename, string type)
        {
            if (type.Equals("normal", StringComparison.OrdinalIgnoreCase))
            {
                // Use dialogue audio source
                dialogueAudioController.Play(filename, type);
            }
            else
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

            ControlSetChanged += () =>
            {
                controller.Disable();
            };
        }

        public void DisplayIcon(string iconName, Vector2 position)
        {
            if (iconCache.TryGetValue(iconName, out SpriteAnimatorController controller))
            {
                controller.Enable();
            }
            else
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

        public void AddChoices(GameDialogue dialogue, List<Tuple<string, int>> dialogueIdFollows)
        {
            foreach (var stringAndDialogueId in dialogueIdFollows)
            {
                dialogueChoicesController.AddChoice(stringAndDialogueId.Item1, dialogue, stringAndDialogueId.Item2);
            }

            dialogueChoicesController.UpdateTextAndRenderLayout();
        }

        private void OnChoiceConfirmed(GameDialogue dialogue, DialogueSlide dialogueSlide)
        {
            ReturnGadget();
            LoadDialogueSlide(dialogue, dialogueSlide);
        }

        public void RunPersistentCoroutine(IEnumerator coroutine)
        {
            StartCoroutine(coroutine);
        }

        public void StopPersistentCoroutine(IEnumerator coroutine)
        {
            StopCoroutine(coroutine);
        }

        private void RestoreGameSaveTracking()
        {
            gameSave.ActionValues[globalVarDictKey] = ActionInterpreter.GlobalScriptValues;
        }

        private void LoadSingleGadget(string gadgetPath, int gadgetId)
        {
            if (gadgetPath != null)
            {
                if (Path.GetExtension(gadgetPath).Equals(FilePaths.GadgetScriptFileExtension, StringComparison.OrdinalIgnoreCase))
                {
                    LoadGadget(gadgetPath);
                }
                else
                {
                    if (gadgetId >= 0)
                    {
                        GameDialogue dialogue = RetrieveDialogue(gadgetPath);
                        DialogueSlide dialogueSlide = dialogue.GetDialogueSlideWithId(gadgetId);

                        LoadDialogueSlide(dialogue, dialogueSlide);
                    }
                }
            }
        }

        private void InitializeFromGameSave()
        {
            inLoad = true;

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
            }
            else
            {
                // Still clear active control set anyway
                SetCurrentGUI(null);
                CleanAllPendingGadgets();
            }

            if (gameSave.CurrentGadgetPath != null)
            {
                LoadSingleGadget(gameSave.CurrentGadgetPath, gameSave.CurrentGadgetId);
            }

            inLoad = false;

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
            if (!AllowSave || inLoad)
            {
                return;
            }

            bool saveWasAvailable = SaveAvailable;

            using (StreamWriter file = new StreamWriter(SavePath))
            {
                gameSave.Version = GameSave.CurrentVersion;

                if (gadgets.Count != 0)
                {
                    GadgetObjectInfo initiator = gadgets.Last();

                    gameSave.CurrentGadgetPath = initiator.Path;
                    gameSave.CurrentGadgetId = initiator.Id;
                } else
                {
                    gameSave.CurrentGadgetPath = null;
                }

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

            if (!saveWasAvailable)
            {
                defaultActionInterpreter.SetGlobalVariable(Constants.SaveExistsVarName, "true");
            }
        }

        public void ClearSave()
        {
            File.Delete(SavePath);
            defaultActionInterpreter.SetGlobalVariable(Constants.SaveExistsVarName, "null");
        }
    }
}