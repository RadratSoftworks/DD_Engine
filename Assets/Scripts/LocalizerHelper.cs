using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using UnityEngine.TextCore;
using UnityEngine;

public static class LocalizerHelper
{
    public static Dictionary<string, string> GetStrings(ResourceFile resources, string filename)
    {
        string langFilename = Path.ChangeExtension(filename, ".lang");
        if (!resources.Exists(langFilename))
        {
            return null;
        }

        byte[] animationFileData = resources.ReadResourceData(resources.Resources[langFilename]);
        using (StreamReader reader = new StreamReader(new MemoryStream(animationFileData)))
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            do
            {
                string lineDesc = reader.ReadLine();
                if ((lineDesc == "") || (lineDesc == null))
                {
                    break;
                }
                lineDesc = lineDesc.Trim();
                if (lineDesc.StartsWith('#'))
                {
                    continue;
                }
                var comps = lineDesc.Split('=');
                if (comps.Length != 2)
                {
                    Debug.Log("Invalid localization line: " + lineDesc);
                    continue;
                }

                string decoded = WebUtility.HtmlDecode(comps[1])
                    .Replace("{BR}", "<br>", StringComparison.OrdinalIgnoreCase)
                    .Replace("{B}", "<b>", StringComparison.OrdinalIgnoreCase)
                    .Replace("{/B}", "</b>", StringComparison.OrdinalIgnoreCase)
                    .Replace("{I}", "<i>", StringComparison.OrdinalIgnoreCase)
                    .Replace("{/I}", "</i>", StringComparison.OrdinalIgnoreCase);

                dict.Add(comps[0], decoded);
            } while (true);

            return dict;
        }
    }
}
