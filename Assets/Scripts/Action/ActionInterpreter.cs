using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DDEngine.BaseScript;
using DDEngine.Utils;
using Cysharp.Threading.Tasks;

namespace DDEngine.Action
{
    public class ActionInterpreter
    {
        private class TimerInfo
        {
            public string ScriptToRun { get; set; }
            public IEnumerator Coroutine { get; set; }
        };

        private static readonly List<string> GlobalVarsBridgeToGameSettingsList = new() {
            "completed_game"
        };

        private static Dictionary<string, string> globalScriptValues = new Dictionary<string, string>();
        private static Dictionary<string, TimerInfo> timers = new Dictionary<string, TimerInfo>();

        public delegate void OnVariableChanged(List<string> nameChanged);
        public event OnVariableChanged VariableChanged;

        private List<string> variableChangedReportList = new List<string>();

        public static Dictionary<string, string> GlobalScriptValues => globalScriptValues;

        public ActionInterpreter()
        {
        }

        public static void ClearState()
        {
            foreach (var pendingTimer in timers)
            {
                GameManager.Instance.StopPersistentCoroutine(pendingTimer.Value.Coroutine);
            }

            timers.Clear();
            globalScriptValues.Clear();
        }

        private IEnumerator TimerCoroutine(string name, int frames, string script)
        {
            float duration = GameUtils.GetDurationFromFramesInSeconds(frames);
            yield return new WaitForSeconds(duration);
            yield return new WaitUntil(() => !GameManager.Instance.GadgetActive);
            // Remove from management
            timers.Remove(name);
            GameManager.Instance.LoadGadget(script);
            yield break;
        }

        public IEnumerator Execute(ScriptBlock<ActionOpcode> block) => UniTask.ToCoroutine(async () =>
        {
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
                            await UniTask.WaitUntil(() => !GameManager.Instance.GUIBusy);
                        }

                        await RunLoadLocation(command);
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

                    case ActionOpcode.PanLocation:
                        RunPanLocation(command);
                        break;

                    case ActionOpcode.SetScrollSpeeds:
                        RunSetScrollSpeeds(command);
                        break;

                    case ActionOpcode.LoadMiniGame:
                        RunLoadMiniGame(command);
                        break;

                    case ActionOpcode.ResumeSave:
                        RunResumeSave(command);
                        break;

                    case ActionOpcode.SwitchNgi:
                        RunSwitchNgi(command);
                        break;

                    case ActionOpcode.Timer:
                        RunTimer(command);
                        break;

                    case ActionOpcode.SaveSettings:
                        RunSaveSettings();
                        break;

                    case ActionOpcode.DeleteSaves:
                        RunDeleteSaves();
                        break;

                    case ActionOpcode.Quit:
                        RunQuit();
                        break;

                    case ActionOpcode.DeleteSettings:
                        RunDeleteSettings();
                        break;

                    case ActionOpcode.SetSetting:
                        RunSetSetting(command);
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
        });

        private void RunReturn(ScriptCommand<ActionOpcode> command)
        {
            GameManager.Instance.ReturnGadget();
        }

        private IEnumerator RunLoadLocation(ScriptCommand<ActionOpcode> command) => UniTask.ToCoroutine(async () =>
        {
            if (command.Arguments.Count == 0)
            {
                Debug.LogError("Not enough argument for load location!");
                return;
            }

            await GameManager.Instance.LoadControlSet(command.Arguments[0] as string);
        });

        private void RunLoadMiniGame(ScriptCommand<ActionOpcode> command)
        {
            if (command.Arguments.Count == 0)
            {
                Debug.LogError("Not enough argument for load minigame!");
                return;
            }

            GameManager.Instance.LoadMinigame(command.Arguments[0] as string);
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

        private void RunPanLocation(ScriptCommand<ActionOpcode> command)
        {
            if (command.Arguments.Count < 2)
            {
                Debug.LogError("Not enough argument for set location offset!");
                return;
            }

            Vector2 amount = new Vector2(int.Parse(command.Arguments[0] as string),
                int.Parse(command.Arguments[1] as string));

            GameManager.Instance.PanControlSet(amount);
        }

        private void RunResumeSave(ScriptCommand<ActionOpcode> command)
        {
            GameManager.Instance.LoadGame();
        }

        private void RunSwitchNgi(ScriptCommand<ActionOpcode> command)
        {
            Application.OpenURL(Constants.SwitchNgiUri);
        }

        private void ClearGlobals(ScriptCommand<ActionOpcode> command)
        {
            bool exists = globalScriptValues.TryGetValue(Constants.SaveExistsVarName, out var saveExistsVar);
            globalScriptValues.Clear();

            if (exists)
            {
                globalScriptValues[Constants.SaveExistsVarName] = saveExistsVar;
            }
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

            SetGlobalVariable(varName, value);
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

        private void RunSetScrollSpeeds(ScriptCommand<ActionOpcode> command)
        {
            Debug.LogWarning("Unknown how scroll speeds actually works!");
        }

        private void RunTimer(ScriptCommand<ActionOpcode> command)
        {
            if (command.Arguments.Count < 2)
            {
                Debug.LogError("Not enough arguments for timer!");
                return;
            }

            string name = command.Arguments[0] as string;
            string action = command.Arguments[1] as string;

            if (action.Equals("stop", StringComparison.OrdinalIgnoreCase))
            {
                if (timers.ContainsKey(name))
                {
                    GameManager.Instance.StopPersistentCoroutine(timers[name].Coroutine);
                    timers.Remove(name);
                }
            }
            else
            {
                if (command.Arguments.Count < 4)
                {
                    Debug.LogError("Not enough arguments to start timer!");
                    return;
                }

                int frames = int.Parse(command.Arguments[2] as string);
                string scriptPath = command.Arguments[3] as string;

                TimerInfo timer = new TimerInfo()
                {
                    Coroutine = TimerCoroutine(name, frames, scriptPath),
                    ScriptToRun = scriptPath
                };

                if (timers.ContainsKey(name))
                {
                    Debug.LogErrorFormat("An timer with the same name ({0}) is still pending!", name);
                }
                else
                {
                    timers[name] = timer;
                }

                GameManager.Instance.RunPersistentCoroutine(timer.Coroutine);
            }
        }

        private void RunSetSetting(ScriptCommand<ActionOpcode> command)
        {
            if (command.Arguments.Count < 2)
            {
                Debug.LogError("Not enough arguments for set setting!");
                return;
            }

            string varname = command.Arguments[0] as string;
            string value = command.Arguments[1] as string;

            GameSettings.SetIngameSettingValue(varname, value);
        }

        private void RunSaveSettings()
        {
            GameSettings.Save();
            ResourceManager.Instance.UpdateLanguagePack();
        }

        private void RunDeleteSettings()
        {
            GameSettings.Reset();
            ResourceManager.Instance.UpdateLanguagePack();
        }

        private void RunDeleteSaves()
        {
            GameManager.Instance.ClearSave();
        }

        private void RunQuit()
        {
            Application.Quit();
        }

        public string GetValue(string variableName, out bool isGlobal)
        {
            isGlobal = false;

            if (GlobalVarsBridgeToGameSettingsList.Contains(variableName))
            {
                return GameSettings.GetIngameSettingValue(variableName);
            }
            else
            {
                if (globalScriptValues.TryGetValue(variableName, out string val))
                {
                    isGlobal = true;
                    return val;
                }

                return "null";
            }
        }

        public void SetGlobalVariable(string varName, string value)
        {
            if (GlobalVarsBridgeToGameSettingsList.Contains(varName))
            {
                GameSettings.SetIngameSettingValue(varName, value);
            }
            else
            {
                if (!globalScriptValues.ContainsKey(varName))
                {
                    globalScriptValues.Add(varName, value);
                }
                else
                {
                    globalScriptValues[varName] = value;
                }
            }

#if UNITY_EDITOR
            Debug.Log(varName + " " + value);
#endif

            variableChangedReportList.Add(varName);
        }
    }
}