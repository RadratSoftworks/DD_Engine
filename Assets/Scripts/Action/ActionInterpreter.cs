using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionInterpreter
{
    private static Dictionary<string, string> globalScriptValues = new Dictionary<string, string>();
    private ScriptBlock<ActionOpcode> block;

    public delegate void OnVariableChanged(List<string> nameChanged);
    public event OnVariableChanged VariableChanged;

    private List<string> variableChangedReportList = new List<string>();

    public ActionInterpreter()
    {
    }

    public IEnumerator Execute(ScriptBlock<ActionOpcode> block)
    {
        this.block = block;

        foreach (var command in block.Commands)
        {
            switch (command.Opcode)
            {
                case ActionOpcode.Return:
                    RunReturn(command);
                    break;

                case ActionOpcode.LoadLocation:
                    // Wait for the current GUI to done any of the stuffs it wants to do
                    if (GameManager.Instance.GUIBusy)
                    {
                        yield return new WaitUntil(() => !GameManager.Instance.GUIBusy);
                    }

                    RunLoadLocation(command);
                    break;

                case ActionOpcode.SetLocationOffset:
                    RunSetLocationOffset(command);
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

        // Report variable changed event. If we prematurely report it in the group the execute action
        // coroutine may be canceled
        VariableChanged?.Invoke(variableChangedReportList);
        variableChangedReportList.Clear();

        yield break;
    }

    private void RunReturn(ScriptCommand<ActionOpcode> command)
    {
        GameManager.Instance.ExitDialogueOrLocation();
    }

    private void RunLoadLocation(ScriptCommand<ActionOpcode> command)
    {
        if (command.Arguments.Count == 0)
        {
            Debug.LogError("Not enough argument for load location!");
            return;
        }

        GameManager.Instance.LoadControlSet(command.Arguments[0] as string);
    }

    private void RunSetLocationOffset(ScriptCommand<ActionOpcode> command)
    {
        if (command.Arguments.Count < 2)
        {
            Debug.LogError("Not enough argument for set location offset!");
            return;
        }

        Vector2 offset = new Vector2(int.Parse(command.Arguments[0] as string),
            int.Parse(command.Arguments[1] as string));

        GameManager.Instance.SetControlSetOffset(offset);
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

        Debug.Log(varName + " " + value);

        variableChangedReportList.Add(varName);
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

    public string GetValue(string variableName, out bool isGlobal)
    {
        isGlobal = false;

        if (globalScriptValues.TryGetValue(variableName, out string val))
        {
            isGlobal = true;
            return val;
        }

        return "null";
    }
}
