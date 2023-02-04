using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using UnityEngine;

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
        { "pan", GadgetOpcode.Pan },
        { "fadein", GadgetOpcode.FadeIn },
        { "fadeout", GadgetOpcode.FadeOut },
        { "clear", GadgetOpcode.Clear },
        { "continue", GadgetOpcode.Continue }
    };

    public static ScriptBlock<GadgetOpcode> Parse(Stream stream)
    {
        ScriptBlock<GadgetOpcode> blocks = new ScriptBlock<GadgetOpcode>();

        using (StreamReader reader = new StreamReader(stream))
        {
            string actionStrings = null;
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

                    break;
                }
                commandLine = commandLine.Trim();
                if (commandLine.StartsWith('#') || (commandLine == ""))
                {
                    continue;
                }
                var commands = commandLine.Split(' ');

                if (!stringToGadgetOps.ContainsKey(commands[0]))
                {
                    Debug.LogError("Unrecognised gadget opcode: " + commands[0]);
                }
                else
                {
                    if (string.Equals(commands[0], "startaction", StringComparison.OrdinalIgnoreCase))
                    {
                        string actionString = commandLine.Substring(commands[0].Length).Trim().Trim('\"');
                        if (actionStrings == null)
                        {
                            actionStrings = actionString + '\n';
                        } else
                        {
                            actionStrings += actionString + '\n';
                        }
                    } else
                    {
                        if (actionStrings != null)
                        {
                            ScriptCommand<GadgetOpcode> command = new ScriptCommand<GadgetOpcode>();
                            command.Opcode = GadgetOpcode.StartAction;
                            command.Arguments.Add(ActionParser.ParseEmbedded(new MemoryStream(Encoding.UTF8.GetBytes(actionStrings))));

                            blocks.Commands.Add(command);
                            actionStrings = null;
                        }

                        ScriptCommand<GadgetOpcode> individualCommand = new ScriptCommand<GadgetOpcode>();
                        individualCommand.Opcode = stringToGadgetOps[commands[0]];

                        if (commands.Length > 1)
                        {
                            individualCommand.Arguments.AddRange(commands.Skip(1));
                        }

                        blocks.Commands.Add(individualCommand);
                    }
                }
            } while (true);
        }

        return blocks;
    }
}
