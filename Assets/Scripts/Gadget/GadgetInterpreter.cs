using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GadgetInterpreter
{
    private GameObject parent;
    private Dialogue dialogue;

    private Dictionary<char, GameObject> layerGameObject;
    private ScriptBlock<GadgetOpcode> scriptBlock;

    private GameSceneAudioController audioController;

    public GadgetInterpreter(GameObject parent, ScriptBlock<GadgetOpcode> scriptBlock, Dialogue dialogue = null)
    {
        this.parent = parent;
        this.scriptBlock = scriptBlock;
        this.dialogue = dialogue;

        layerGameObject = new Dictionary<char, GameObject>();
    }

    private GameObject ParentByLayer(char layer)
    {
        if (!layerGameObject.ContainsKey(layer))
        {
            GameObject emptyLayerObject = GameObject.Instantiate(GameManager.Instance.gameLayerPrefabObject, parent.transform, false); ;
            emptyLayerObject.name = string.Format("Group {0}", layer.ToString());

            layerGameObject.Add(layer, emptyLayerObject);
            return emptyLayerObject;
        }

        return layerGameObject[layer];
    }

    private string GetSortingLayer(char layer)
    {
        return string.Format("GameLayer{0}", Char.ToUpper(layer));
    }

    private int GetSortingOrderForNewbie(char layer)
    {
        if (!layerGameObject.ContainsKey(layer))
        {
            return 0;
        }
        return layerGameObject[layer].transform.childCount;
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

        GameObject layerObject = ParentByLayer(layer);
        GameObject existingAnim = layerObject.transform.Find(animFileName)?.gameObject;
        Vector2 position = GameUtils.ToUnityCoordinates(new Vector2(int.Parse(command.Arguments[2] as string), int.Parse(command.Arguments[3] as string)));

        if (existingAnim != null)
        {
            SpriteAnimatorController existingController = existingAnim.GetComponent<SpriteAnimatorController>();
            if (existingController != null)
            {
                existingController.Restart(position);
            }

            return;
        }

        GameObject animObject = GameObject.Instantiate(GameManager.Instance.gameAnimationPrefabObject, layerObject.transform, false);
        animObject.transform.localPosition = position;
        animObject.name = animFileName;

        SpriteAnimatorController controller = animObject.GetComponent<SpriteAnimatorController>();
        if (controller != null)
        {
            controller.AnimationFilename = animFileName;
        }

        SpriteRenderer renderer = animObject.GetComponent<SpriteRenderer>();

        if (renderer != null)
        {
            renderer.sortingOrder = GetSortingOrderForNewbie(layer);
            renderer.sortingLayerName = GetSortingLayer(layer);
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

        GameObject imageObject = GameObject.Instantiate(GameManager.Instance.gameImagePrefabObject, ParentByLayer(layer).transform, false);
        imageObject.transform.localPosition = Vector3.zero;
        
        SpriteRenderer spriteRenderer = imageObject.GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
        {
            string path = command.Arguments[1] as string;
            spriteRenderer.sprite = SpriteManager.Instance.Load(ResourceManager.Instance.PickBestResourcePackForFile(path), path);
            spriteRenderer.sortingOrder = GetSortingOrderForNewbie(layer);
            spriteRenderer.sortingLayerName = GetSortingLayer(layer);
        }
    }

    private void HandlePlay(ScriptCommand<GadgetOpcode> command)
    {
        if (command.Arguments.Count < 2)
        {
            Debug.LogError("Not enough argument for sound play request!");
            return;
        }

        if (audioController == null)
        {
            audioController = parent.GetComponent<GameSceneAudioController>();
        }

        if (audioController == null)
        {
            return;
        }

        audioController.Play(command.Arguments[1] as string, command.Arguments[0] as string);
    }

    private void HandleClear(ScriptCommand<GadgetOpcode> command)
    {
        char layer = (command.Arguments[0] as string)[0];
        if (layerGameObject.ContainsKey(layer))
        {
            GameObject layerObj = layerGameObject[layer];

            // We are running in coroutine! So destroy like this is fine...
            for (int i = 0; i < layerObj.transform.childCount; i++)
            {
                GameObject.Destroy(layerObj.transform.GetChild(i).gameObject);
            }

            layerObj.transform.localPosition = Vector3.zero;
        }
    }

    private void StartAction(ScriptCommand<GadgetOpcode> command)
    {
        if (command.Arguments.Count == 0)
        {
            Debug.LogWarning("Empty action to execute!");
            return;
        }

        ActionInterpreter interpreter = new ActionInterpreter(command.Arguments[0] as ScriptBlock<ActionOpcode>);
        interpreter.Execute();
    }

    public void HandlePan(ScriptCommand<GadgetOpcode> command)
    {
        if (command.Arguments.Count < 4)
        {
            Debug.LogError("Not enough argument for panning!");
            return;
        }

        char layer = (command.Arguments[0] as string)[0];
        GameObject layerObj = ParentByLayer(layer);

        if (layerObj != null) {
            Vector2 target = new Vector2(int.Parse(command.Arguments[1] as string), int.Parse(command.Arguments[2] as string));
            target = GameUtils.ToUnityCoordinates(target);

            int duration = int.Parse(command.Arguments[3] as string);

            GamePanController panController = layerObj.GetComponent<GamePanController>();
            if (panController != null)
            {
                panController.Pan(target, duration);
            }
        }
    }

    public void HandleFade(ScriptCommand<GadgetOpcode> command, bool isFadeIn)
    {
        if (command.Arguments.Count < 2)
        {
            Debug.LogError("Not enough argument for fade!");
            return;
        }

        char layer = (command.Arguments[0] as string)[0];
        GameObject layerObj = ParentByLayer(layer);

        if (layerObj != null)
        {
            int frames = int.Parse(command.Arguments[1] as string);

            for (int i = 0; i < layerObj.transform.childCount; i++)
            {
                GameObject individualObj = layerObj.transform.GetChild(i).gameObject;
                GameImageFadeController fadeController = individualObj.GetComponent<GameImageFadeController>();
                
                if (fadeController != null)
                {
                    fadeController.Fade(isFadeIn, frames);
                }
            }
        }
    }

    public void HandleContinue(ScriptCommand<GadgetOpcode> command)
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

        GameManager.Instance.ReturnCurrent();
        GameManager.Instance.LoadDialogueSlide(dialogue, slide);
    }

    public IEnumerator Execute()
    {
        foreach (var command in scriptBlock.Commands)
        {
            switch (command.Opcode)
            {
                case GadgetOpcode.Pause:
                    {
                        int frameToPause = int.Parse(command.Arguments[0] as string);
                        for (int i = 0; i < frameToPause; i++)
                        {
                            yield return null;
                        }

                        break;
                    }

                case GadgetOpcode.Pan:
                    HandlePan(command);
                    break;

                case GadgetOpcode.FadeIn:
                case GadgetOpcode.FadeOut:
                    HandleFade(command, (command.Opcode == GadgetOpcode.FadeIn));
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
                    StartAction(command);
                    break;

                case GadgetOpcode.Continue:
                    yield return new WaitForSeconds(5);
                    HandleContinue(command);
                    break;

                case GadgetOpcode.Play:
                    HandlePlay(command);
                    break;

                default:
                    Debug.LogError("Unhandled gadget opcode " + command.Opcode);
                    break;
            }
        }

        yield break;
    }
};