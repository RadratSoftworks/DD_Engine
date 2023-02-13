using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class MinigameFileParser
{
    private static void LogWarning(string filename, int lineIndex, string error)
    {
        Debug.LogWarning(string.Format("PARSER ERROR - {0} Line {1}: {2}", filename, lineIndex, error));
    }

    public static MinigameVariable Parse(Stream fileStream, string fileStreamPath)
    {
        fileStreamPath = fileStreamPath ?? Constants.Anonymous;
        MinigameVariable root = new MinigameVariable("root");

        using (StreamReader reader = new StreamReader(fileStream))
        {
            int lineIndex = 0;
            do
            {
                string line = reader.ReadLine();
                if (line == null)
                {
                    break;
                }

                lineIndex++;
                line = line.Trim();
                
                // Trim off comments
                int commentIndex = line.IndexOf('#');

                if (commentIndex >= 0)
                {
                    line = line.Substring(0, commentIndex);
                }

                if (line.Length == 0)
                {
                    continue;
                }

                string[] comps = line.Split('=');

                if (comps.Length < 2)
                {
                    LogWarning(fileStreamPath, lineIndex, "No data assignment is performed on this line!");
                    continue;
                }

                string lhs = comps[0].Trim();
                string rhs = comps[1].Trim();

                string[] varLayers = lhs.Split('.');

                MinigameVariable accessVariable = root;
                for (int i = 0; i < varLayers.Length - 1; i++)
                {
                    accessVariable = accessVariable.AddOrGetChildMember(varLayers[i]);
                }

                if (int.TryParse(rhs, out int parsedInt))
                {
                    accessVariable.AddProperty(varLayers[varLayers.Length - 1], parsedInt);
                } else
                {
                    accessVariable.AddProperty(varLayers[varLayers.Length - 1], rhs);
                }
            } while (true);
        }

        return root;
    }
}
