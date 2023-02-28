using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using UnityEngine;

using DDEngine.Action;
using DDEngine.BaseScript;
using DDEngine.GUI;

namespace DDEngine.Gadget
{
    public class GadgetParser
    {
        private static Dictionary<string, GadgetOpcode> stringToGadgetOps = new Dictionary<string, GadgetOpcode>(StringComparer.OrdinalIgnoreCase)
        {
            { "play", GadgetOpcode.Play },
            { "animation", GadgetOpcode.Animation },
            { "startaction", GadgetOpcode.StartAction },
            { "background", GadgetOpcode.Background },
            { "load", GadgetOpcode.Load },
            { "pause", GadgetOpcode.Pause },
            { "wait", GadgetOpcode.Pause },     // Intentional...
            { "pan", GadgetOpcode.Pan },
            { "fadein", GadgetOpcode.FadeIn },
            { "fadeout", GadgetOpcode.FadeOut },
            { "clear", GadgetOpcode.Clear },
            { "continue", GadgetOpcode.Continue },
            { "text", GadgetOpcode.Text },
            { "balloon", GadgetOpcode.Balloon },
            { "stinger", GadgetOpcode.Stinger },
            { "icon", GadgetOpcode.Icon },
            { "hidetext", GadgetOpcode.HideText },
            { "choice", GadgetOpcode.Choice },
            { "conditionalchoice", GadgetOpcode.ChoiceConditional },
            { "vibra", GadgetOpcode.Vibrate },
            { "stopambient", GadgetOpcode.StopAmbient }
        };

        public static ScriptBlock<GadgetOpcode> Parse(Stream stream)
        {
            ScriptBlock<GadgetOpcode> blocks = new ScriptBlock<GadgetOpcode>();

            using (StreamReader reader = new StreamReader(stream))
            {
                string actionStrings = null;
                List<GadgetChoiceInfo> choiceInfos = new List<GadgetChoiceInfo>();

                do
                {
                    string commandLine = reader.ReadLine();
                    if (commandLine == null)
                    {
                        if (actionStrings != null)
                        {
                            ScriptCommand<GadgetOpcode> command = new ScriptCommand<GadgetOpcode>();
                            command.Opcode = GadgetOpcode.StartAction;
                            command.Arguments.Add(ActionParser.ParseEmbedded(new MemoryStream(Encoding.UTF8.GetBytes(actionStrings))));

                            blocks.Commands.Add(command);
                        }

                        if (choiceInfos.Count != 0)
                        {
                            ScriptCommand<GadgetOpcode> command = new ScriptCommand<GadgetOpcode>();
                            command.Opcode = GadgetOpcode.Choice;
                            command.Arguments.Add(choiceInfos);

                            blocks.Commands.Add(command);
                        }

                        break;
                    }
                    commandLine = commandLine.Trim();
                    if (commandLine.StartsWith('#') || (commandLine == ""))
                    {
                        continue;
                    }
                    if (commandLine == "unsavable")
                    {
                        blocks.Saveable = false;
                        continue;
                    }
                    var commands = commandLine.Split(' ');

                    if (!stringToGadgetOps.ContainsKey(commands[0]))
                    {
                        Debug.LogError("Unrecognised gadget opcode: " + commands[0]);
                    }
                    else
                    {
                        GadgetOpcode opcode = stringToGadgetOps[commands[0]];

                        if (opcode == GadgetOpcode.StartAction)
                        {
                            string actionString = commandLine.Substring(commands[0].Length).Trim().Trim('\"');
                            if (actionStrings == null)
                            {
                                actionStrings = actionString + '\n';
                            }
                            else
                            {
                                actionStrings += actionString + '\n';
                            }
                        }
                        else
                        {
                            if (actionStrings != null)
                            {
                                ScriptCommand<GadgetOpcode> command = new ScriptCommand<GadgetOpcode>();
                                command.Opcode = GadgetOpcode.StartAction;
                                command.Arguments.Add(ActionParser.ParseEmbedded(new MemoryStream(Encoding.UTF8.GetBytes(actionStrings))));

                                blocks.Commands.Add(command);
                                actionStrings = null;
                            }

                            if (opcode == GadgetOpcode.Choice)
                            {
                                if (commands.Length < 4)
                                {
                                    Debug.LogError("Unsufficent number of arguments for opcode choice!");
                                }

                                GadgetChoiceInfo info = new GadgetChoiceInfo()
                                {
                                    DialogueId = int.Parse(commands[1]),
                                    TextId = commands[2],
                                    ChoiceKind = commands[3]
                                };

                                choiceInfos.Add(info);
                            }
                            else if (opcode == GadgetOpcode.ChoiceConditional)
                            {
                                if (commands.Length < 6)
                                {
                                    Debug.LogError("Unsufficent number of arguments for opcode conditional choice!");
                                }

                                GadgetChoiceInfo info = new GadgetChoiceInfo()
                                {
                                    ConditionalVariables = GUIConditionHelper.GetParticipateVariablesInCondition(commands[1]),
                                    ConditionalVariableValues = GUIConditionHelper.GetRequiredValues(commands[2]),
                                    DialogueId = int.Parse(commands[3]),
                                    TextId = commands[4],
                                    ChoiceKind = commands[5]
                                };

                                choiceInfos.Add(info);
                            }
                            else
                            {
                                ScriptCommand<GadgetOpcode> individualCommand = new ScriptCommand<GadgetOpcode>();
                                individualCommand.Opcode = stringToGadgetOps[commands[0]];

                                if (commands.Length > 1)
                                {
                                    individualCommand.Arguments.AddRange(commands.Skip(1));
                                }

                                blocks.Commands.Add(individualCommand);
                            }
                        }
                    }
                } while (true);
            }

            return blocks;
        }
    }
}