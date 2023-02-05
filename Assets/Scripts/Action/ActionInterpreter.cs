using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

public class ActionInterpreter
{
    private static Dictionary<string, string> globalScriptValues = new Dictionary<string, string>();
    private ScriptBlock<ActionOpcode> block;

    public ActionInterpreter(ScriptBlock<ActionOpcode> block)
    {
        this.block = block;
    }

    public void Execute()
    {
        foreach (var command in block.Commands)
        {
            switch (command.Opcode)
            {
                case ActionOpcode.Return:
                    RunReturn(command);
                    break;

                case ActionOpcode.LoadLocation:
                    RunLoadLocation(command);
                    break;

                case ActionOpcode.ClearGlobals:
                    ClearGlobals(command);
                    break;

                case ActionOpcode.SetGlobal:
                    SetGlobal(command);
                    break;

                case ActionOpcode.LoadDialogue:
                    LoadDialogue(command);
                    break;

                case ActionOpcode.LoadGadget:
                    LoadGadget(command);
                    break;

                case ActionOpcode.Play:
                    Play(command);
                    break;

                default:
                    Debug.LogWarning("Unhandled gadget opcode: " + command.Opcode);
                    break;
            }
        }
    }

    private void RunReturn(ScriptCommand<ActionOpcode> command)
    {
        GameManager.Instance.ReturnCurrent();
    }

    private void RunLoadLocation(ScriptCommand<ActionOpcode> command)
    {
        if (command.Arguments.Count == 0)
        {
            Debug.LogError("Not enough argument for load location!");
            return;
        }

        GUIControlSet set = GUIManager.Instance.LoadControlSet((string)command.Arguments[0]);
        if (set != null)
        {
            GameManager.Instance.SetCurrent(set.GameObject);
        }
    }

    private void ClearGlobals(ScriptCommand<ActionOpcode> command)
    {
        globalScriptValues.Clear();
    }

    private void SetGlobal(ScriptCommand<ActionOpcode> command)
    {
        if (command.Arguments.Count < 2)
        {
            Debug.LogError("Not enough argument for set global variable!");
            return;
        }

        string varName = command.Arguments[0] as string;
        string value = command.Arguments[1] as string;

        if (!globalScriptValues.ContainsKey(varName))
        {
            globalScriptValues.Add(varName, value);
        } else
        {
            globalScriptValues[varName] = value;
        }
    }

    private void LoadDialogue(ScriptCommand<ActionOpcode> command)
    {
        if (command.Arguments.Count == 0)
        {
            Debug.LogError("Not enough argument for load dialogue!");
            return;
        }

        string filename = command.Arguments[0] as string;
        GameManager.Instance.LoadDialogue(filename);
    }

    private void LoadGadget(ScriptCommand<ActionOpcode> command)
    {
        if (command.Arguments.Count == 0)
        {
            Debug.LogError("Not enough argument for load gadget!");
            return;
        }

        string filename = command.Arguments[0] as string;
        GameManager.Instance.LoadGadget(filename);
    }

    private void Play(ScriptCommand<ActionOpcode> command)
    {
        if (command.Arguments.Count < 2)
        {
            Debug.LogError("Not enough argument for play audio!");
            return;
        }

        GameManager.Instance.PlayAudioPersistent(command.Arguments[1] as string, command.Arguments[0] as string);
    }
}
