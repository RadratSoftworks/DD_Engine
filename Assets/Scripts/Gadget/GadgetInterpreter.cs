using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DDEngine.Action;
using DDEngine.BaseScript;
using DDEngine.Dialogue;
using DDEngine.Game;
using DDEngine.GUI;
using DDEngine.Utils;

namespace DDEngine.Gadget
{
    public class GadgetInterpreter
    {
        private GameSceneController parent;
        private GameDialogue dialogue;

        private Dictionary<char, GameObject> gameObjectByLayer;
        private ScriptBlock<GadgetOpcode> scriptBlock;

        private ActionInterpreter actionInterpreter;

        public GadgetInterpreter(ActionInterpreter guiActionInterpreter, GameSceneController parent, ScriptBlock<GadgetOpcode> scriptBlock, GameDialogue dialogue = null)
        {
            this.parent = parent;
            this.scriptBlock = scriptBlock;
            this.dialogue = dialogue;

            gameObjectByLayer = new Dictionary<char, GameObject>();
            actionInterpreter = guiActionInterpreter;
        }

        private void HandleAnimation(ScriptCommand<GadgetOpcode> command)
        {
            if (command.Arguments.Count < 4)
            {
                Debug.LogError("Not enough argument for animation creation!");
                return;
            }

            string animFileName = command.Arguments[1] as string;
            char layer = (command.Arguments[0] as string)[0];
            Vector2 position = new Vector2(int.Parse(command.Arguments[2] as string), int.Parse(command.Arguments[3] as string));

            if (gameObjectByLayer.TryGetValue(layer, out GameObject existingObject))
            {
                if ((existingObject != null) && existingObject.name.Equals(animFileName, StringComparison.OrdinalIgnoreCase))
                {
                    SpriteAnimatorController existingController = existingObject.GetComponentInChildren<SpriteAnimatorController>();
                    if (existingController != null)
                    {
                        existingController.SetSortOrderUnity(parent.GetSortingOrder(layer));
                        existingController.Restart(position);
                    }

                    return;
                }
            }

            GameObject animObject = GameObject.Instantiate(GameManager.Instance.gameAnimationPrefabObject, parent.gameObject.transform, false);
            animObject.name = animFileName;
            animObject.transform.localPosition = Vector2.zero;

            SpriteAnimatorController controller = animObject.GetComponentInChildren<SpriteAnimatorController>();
            if (controller != null)
            {
                controller.Setup(position, parent.GetSortingOrder(layer), animFileName, isSortOrderUnity: true);
            }

            if (existingObject)
            {
                gameObjectByLayer[layer] = animObject;
                GameObject.Destroy(existingObject);
            }
            else
            {
                gameObjectByLayer.Add(layer, animObject);
            }
        }

        private void HandleImage(ScriptCommand<GadgetOpcode> command)
        {
            if (command.Arguments.Count < 2)
            {
                Debug.LogError("Not enough argument for image creation!");
                return;
            }

            char layer = (command.Arguments[0] as string)[0];
            string path = command.Arguments[1] as string;

            if (gameObjectByLayer.TryGetValue(layer, out GameObject existingObject))
            {
                if ((existingObject != null) && existingObject.name.Equals(path, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }
            }

            GameObject imageObject = GameObject.Instantiate(GameManager.Instance.gameImagePrefabObject, parent.gameObject.transform, false);
            imageObject.transform.localPosition = Vector3.zero;
            imageObject.name = path;

            SpriteRenderer spriteRenderer = imageObject.GetComponent<SpriteRenderer>();

            if (spriteRenderer != null)
            {
                spriteRenderer.sprite = SpriteManager.Instance.Load(ResourceManager.Instance.PickBestResourcePackForFile(path), path);
                spriteRenderer.sortingOrder = parent.GetSortingOrder(layer);
            }

            if (existingObject)
            {
                gameObjectByLayer[layer] = imageObject;
                GameObject.Destroy(existingObject);
            }
            else
            {
                gameObjectByLayer.Add(layer, imageObject);
            }
        }

        private void HandlePlay(ScriptCommand<GadgetOpcode> command)
        {
            if (command.Arguments.Count < 2)
            {
                Debug.LogError("Not enough argument for sound play request!");
                return;
            }

            GameManager.Instance.PlayAudioInDialogue(command.Arguments[1] as string, command.Arguments[0] as string);
        }

        private void HandleText(ScriptCommand<GadgetOpcode> command)
        {
            string textId = command.Arguments[0] as string;

            if (!dialogue.Strings.TryGetValue(textId, out string text))
            {
                text = textId;
            }

            GameManager.Instance.ShowText(text, command.Arguments[1] as string);
        }

        private void HandleHideText(ScriptCommand<GadgetOpcode> command)
        {
            GameManager.Instance.HideText(command.Arguments[0] as string);
        }

        private void HandleBalloon(ScriptCommand<GadgetOpcode> command)
        {
            GameManager.Instance.SetBalloon(command.Arguments[0] as string, command.Arguments[1] as string);
        }

        private void HandleStinger(ScriptCommand<GadgetOpcode> command)
        {
            Vector2 position = new Vector2(int.Parse(command.Arguments[0] as string), int.Parse(command.Arguments[1] as string));
            string filename = command.Arguments[2] as string;
            string where = command.Arguments[3] as string;

            GameManager.Instance.SetStinger(position, filename, where);
        }

        private void HandleIcon(ScriptCommand<GadgetOpcode> command)
        {
            Vector2 position = new Vector2(int.Parse(command.Arguments[1] as string), int.Parse(command.Arguments[2] as string));
            string iconName = command.Arguments[0] as string;

            GameManager.Instance.DisplayIcon(iconName, position);
        }

        private void HandleClear(ScriptCommand<GadgetOpcode> command)
        {
            char layer = (command.Arguments[0] as string)[0];
            if (gameObjectByLayer.TryGetValue(layer, out GameObject obj))
            {
                gameObjectByLayer.Remove(layer);
                GameObject.Destroy(obj);
            }
        }

        private IEnumerator StartAction(ScriptCommand<GadgetOpcode> command)
        {
            if (command.Arguments.Count == 0)
            {
                Debug.LogWarning("Empty action to execute!");
                yield break;
            }

            yield return actionInterpreter.Execute(command.Arguments[0] as ScriptBlock<ActionOpcode>);
        }

        private void HandlePan(ScriptCommand<GadgetOpcode> command)
        {
            if (command.Arguments.Count < 4)
            {
                Debug.LogError("Not enough argument for panning!");
                return;
            }

            char layer = (command.Arguments[0] as string)[0];

            if (gameObjectByLayer.TryGetValue(layer, out GameObject gameObj))
            {
                Vector2 target = new Vector2(int.Parse(command.Arguments[1] as string), int.Parse(command.Arguments[2] as string));
                target = GameUtils.ToUnityCoordinates(target);

                int duration = int.Parse(command.Arguments[3] as string);
                duration = GameManager.Instance.GetRealFrames(duration);

                GamePanController panController = gameObj.GetComponent<GamePanController>();
                if (panController != null)
                {
                    panController.Pan(target, duration);
                }
            }
        }

        private void HandleFadeInOut(ScriptCommand<GadgetOpcode> command, bool isFadeIn)
        {
            if (command.Arguments.Count < 2)
            {
                Debug.LogError("Not enough argument for fade in/out!");
                return;
            }

            char layer = (command.Arguments[0] as string)[0];

            if (gameObjectByLayer.TryGetValue(layer, out GameObject gameObj))
            {
                int frames = int.Parse(command.Arguments[1] as string);
                frames = GameManager.Instance.GetRealFrames(frames);

                GameImageFadeController fadeController = gameObj.GetComponentInChildren<GameImageFadeController>();

                if (fadeController != null)
                {
                    fadeController.Fade(isFadeIn ? GameImageFadeController.FadeType.In : GameImageFadeController.FadeType.Out, frames);
                }
            }
        }

        private void HandleFade(ScriptCommand<GadgetOpcode> command)
        {
            if (command.Arguments.Count < 3)
            {
                Debug.LogError("Not enough argument for fade!");
                return;
            }

            char layer = (command.Arguments[0] as string)[0];

            if (gameObjectByLayer.TryGetValue(layer, out GameObject gameObj))
            {
                int targetAlpha = int.Parse(command.Arguments[1] as string);
                int frames = int.Parse(command.Arguments[2] as string);
                frames = GameManager.Instance.GetRealFrames(frames);

                GameImageFadeController fadeController = gameObj.GetComponentInChildren<GameImageFadeController>();

                if (fadeController != null)
                {
                    fadeController.Fade(GameImageFadeController.FadeType.ToValue, frames, targetAlpha);
                }
            }
        }

        private void HandleChoice(ScriptCommand<GadgetOpcode> command)
        {
            if (command.Arguments.Count == 0)
            {
                Debug.LogError("Invalid number of argument for choices!");
            }

            List<GadgetChoiceInfo> choices = command.Arguments[0] as List<GadgetChoiceInfo>;
            List<Tuple<string, int>> finalDisplayChoices = new List<Tuple<string, int>>();

            foreach (var choice in choices)
            {
                if (!actionInterpreter.GUIConditionResult(choice.ConditionalVariables, choice.ConditionalVariableValues))
                {
                    continue;
                }

                if (!dialogue.Strings.TryGetValue(choice.TextId, out string text))
                {
                    text = choice.TextId;
                }

                finalDisplayChoices.Add(new Tuple<string, int>(text, choice.DialogueId));
            }

            GameManager.Instance.AddChoices(dialogue, finalDisplayChoices);
        }

        private void HandleContinue(ScriptCommand<GadgetOpcode> command)
        {
            if (command.Arguments.Count == 0)
            {
                Debug.LogError("Not enough argument for continue dialoging!");
                return;
            }

            int id = int.Parse(command.Arguments[0] as string);
            DialogueSlide slide = dialogue.GetDialogueSlideWithId(id);

            if (slide == null)
            {
                Debug.LogError("Invalid dialogue slide id " + id + " in file: " + dialogue.FileName);
            }

            GameManager.Instance.ReturnGadget();
            GameManager.Instance.LoadDialogueSlide(dialogue, slide);
        }

        private void HandleBackground(ScriptCommand<GadgetOpcode> command)
        {
            if (command.Arguments.Count < 4)
            {
                Debug.LogError("Not enough arguments for background command!");
                return;
            }

            float r = int.Parse(command.Arguments[0] as string) / 255.0f;
            float g = int.Parse(command.Arguments[1] as string) / 255.0f;
            float b = int.Parse(command.Arguments[2] as string) / 255.0f;
            float a = int.Parse(command.Arguments[3] as string) / 255.0f;

            parent.BackgroundColor = new Color(r, g, b, a);
        }

        private void HandleVibrate(ScriptCommand<GadgetOpcode> command)
        {
            if (command.Arguments.Count < 2)
            {
                Debug.LogError("Not enough arguments for vibrate command!");
                return;
            }

            float frameCount = int.Parse(command.Arguments[0] as string);
            int amplitude = int.Parse(command.Arguments[0] as string);

            if (GameSettings.Vibration)
            {
                Vibrator.Vibrate(frameCount / Constants.BaseGameFps, amplitude);
            }
        }

        private void HandleStopAmbient()
        {
            GameManager.Instance.StopAudioAmbient();
        }

        public IEnumerator Execute(System.Action onGadgetStart = null)
        {
            if (GameManager.Instance.GUIBusy)
            {
                yield return new WaitUntil(() => !GameManager.Instance.GUIBusy);
            }

            if (scriptBlock.Skippable)
            {
                GameManager.Instance.StartHearingConfirmation();
            }

            onGadgetStart?.Invoke();

            foreach (var command in scriptBlock.Commands)
            {
                if (scriptBlock.Skippable)
                {
                    if (command.Opcode != GadgetOpcode.StartAction)
                    {
                        if (GameManager.Instance.ContinueConfirmed)
                        {
                            continue;
                        }
                    }
                }

                switch (command.Opcode)
                {
                    case GadgetOpcode.Pause:
                        {
                            int frameToPause = int.Parse(command.Arguments[0] as string);
                            frameToPause = GameManager.Instance.GetRealFrames(frameToPause);

                            if (frameToPause < 0)
                            {
                                // Wait for confirmation basically!
                                yield return new WaitUntil(() => GameManager.Instance.LastTextFinished);
                                GameManager.Instance.StartHearingConfirmation();
                                yield return new WaitUntil(() => GameManager.Instance.ContinueConfirmed);
                            }
                            else
                            {
                                for (int i = 0; i < frameToPause; i++)
                                {
                                    yield return null;

                                    if (scriptBlock.Skippable)
                                    {
                                        if (GameManager.Instance.ContinueConfirmed)
                                        {
                                            break;
                                        }
                                    }
                                }
                            }

                            break;
                        }

                    case GadgetOpcode.Pan:
                        HandlePan(command);
                        break;

                    case GadgetOpcode.FadeIn:
                    case GadgetOpcode.FadeOut:
                        HandleFadeInOut(command, (command.Opcode == GadgetOpcode.FadeIn));
                        break;

                    case GadgetOpcode.Fade:
                        HandleFade(command);
                        break;

                    case GadgetOpcode.Load:
                        HandleImage(command);
                        break;

                    case GadgetOpcode.Animation:
                        HandleAnimation(command);
                        break;

                    case GadgetOpcode.Clear:
                        HandleClear(command);
                        break;

                    case GadgetOpcode.StartAction:
                        yield return StartAction(command);
                        break;

                    case GadgetOpcode.Continue:
                        yield return new WaitUntil(() => GameManager.Instance.LastTextFinished);
                        GameManager.Instance.StartHearingConfirmation();
                        yield return new WaitUntil(() => GameManager.Instance.ContinueConfirmed);

                        HandleContinue(command);
                        break;

                    case GadgetOpcode.Play:
                        HandlePlay(command);
                        break;

                    case GadgetOpcode.Text:
                        HandleText(command);
                        break;

                    case GadgetOpcode.HideText:
                        HandleHideText(command);
                        break;

                    case GadgetOpcode.Balloon:
                        HandleBalloon(command);
                        break;

                    case GadgetOpcode.Stinger:
                        HandleStinger(command);
                        break;

                    case GadgetOpcode.Icon:
                        HandleIcon(command);
                        break;

                    case GadgetOpcode.Choice:
                        HandleChoice(command);
                        break;

                    case GadgetOpcode.Background:
                        HandleBackground(command);
                        break;

                    case GadgetOpcode.Vibrate:
                        HandleVibrate(command);
                        break;

                    case GadgetOpcode.StopAmbient:
                        HandleStopAmbient();
                        break;

                    default:
                        Debug.LogError("Unhandled gadget opcode " + command.Opcode);
                        break;
                }
            }

            yield break;
        }
    };
}