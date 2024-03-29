﻿using DDEngine.BaseScript;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace DDEngine.Action
{
    public class ActionParser
    {
        private static Dictionary<string, ActionOpcode> stringToActionOps = new Dictionary<string, ActionOpcode>(StringComparer.OrdinalIgnoreCase)
        {
            { "loadlocation", ActionOpcode.LoadLocation },
            { "return", ActionOpcode.Return },
            { "clearglobals", ActionOpcode.ClearGlobals },
            { "setglobal", ActionOpcode.SetGlobal },
            { "loaddialogue", ActionOpcode.LoadDialogue },
            { "loadgadget", ActionOpcode.LoadGadget },
            { "play", ActionOpcode.Play },
            { "locationoffset", ActionOpcode.SetLocationOffset },
            { "panlocation", ActionOpcode.PanLocation },
            { "scrollspeeds", ActionOpcode.SetScrollSpeeds },
            { "achieve", ActionOpcode.Achieve },
            { "loadminigame", ActionOpcode.LoadMiniGame },
            { "resumesave", ActionOpcode.ResumeSave },
            { "switchngi", ActionOpcode.SwitchNgi },
            { "timer", ActionOpcode.Timer },
            { "deletesaves", ActionOpcode.DeleteSaves },
            { "deletesettings", ActionOpcode.DeleteSettings },
            { "savesettings", ActionOpcode.SaveSettings },
            { "quit", ActionOpcode.Quit },
            { "setsetting", ActionOpcode.SetSetting }
        };

        public static ScriptBlock<ActionOpcode> ParseEmbedded(Stream stream)
        {
            ScriptBlock<ActionOpcode> blocks = new ScriptBlock<ActionOpcode>();

            using (StreamReader reader = new StreamReader(stream))
            {
                do
                {
                    string commandLine = reader.ReadLine();
                    if (commandLine == null)
                    {
                        break;
                    }
                    commandLine = commandLine.Trim();
                    if (commandLine.StartsWith('#') || (commandLine == ""))
                    {
                        continue;
                    }
                    var commands = Regex.Split(commandLine, @"\s{1,}"); ;

                    if (!stringToActionOps.ContainsKey(commands[0]))
                    {
                        Debug.LogError("Unrecognised action opcode: " + commands[0]);
                    }
                    else
                    {
                        ScriptCommand<ActionOpcode> command = new ScriptCommand<ActionOpcode>();
                        command.Opcode = stringToActionOps[commands[0]];

                        if (commands.Length > 1)
                        {
                            command.Arguments.AddRange(commands.Skip(1));
                        }

                        blocks.Commands.Add(command);
                    }
                } while (!reader.EndOfStream);
            }

            return blocks;
        }

        public static ActionLibrary Parse(Stream input)
        {
            Dictionary<string, Dictionary<string, ScriptBlock<ActionOpcode>>> actionHandlers = new
                Dictionary<string, Dictionary<string, ScriptBlock<ActionOpcode>>>();

            using (StreamReader reader = new StreamReader(input))
            {
                string handlerSpecs = null;
                string actionScriptCurrent = null;

                while (true)
                {
                    string line = reader.ReadLine();
                    bool isHandlerLine = false;

                    if (line != null)
                    {
                        line = line.Trim();
                        if ((line.StartsWith('#')) || (line == ""))
                        {
                            continue;
                        }

                        isHandlerLine = (line.StartsWith("@"));

                        if ((handlerSpecs == null) && !isHandlerLine)
                        {
                            continue;
                        }
                    }

                    if (isHandlerLine || (line == null))
                    {
                        if ((actionScriptCurrent != null) && (actionScriptCurrent.Length > 0))
                        {
                            var comps = handlerSpecs.Substring(1).Split('.');
                            if (comps.Length < 2)
                            {
                                Debug.LogError("Invalid event register line: " + handlerSpecs + ". Skipping this block!");
                            }
                            else
                            {
                                ScriptBlock<ActionOpcode> block = ParseEmbedded(new MemoryStream(Encoding.UTF8.GetBytes(actionScriptCurrent)));
                                if (!actionHandlers.ContainsKey(comps[0]))
                                {
                                    actionHandlers.Add(comps[0], new Dictionary<string, ScriptBlock<ActionOpcode>>());
                                }
                                if (actionHandlers[comps[0]].ContainsKey(comps[1]))
                                {
                                    actionHandlers[comps[0]][comps[1]] = block;
                                }
                                else
                                {
                                    actionHandlers[comps[0]].Add(comps[1], block);
                                }
                            }
                        }

                        handlerSpecs = line;
                        actionScriptCurrent = null;
                    }
                    else
                    {
                        actionScriptCurrent += line + '\n';
                    }

                    if (line == null)
                    {
                        break;
                    }
                }
            }

            return new ActionLibrary(actionHandlers);
        }
    }
}