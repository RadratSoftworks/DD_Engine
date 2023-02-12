using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

// Unity JSON
using Newtonsoft.Json;

public class ProtectedFilePatcher
{
    private List<ProtectedFilePatches> filePatches = new List<ProtectedFilePatches>();

    private class ProtectedFilePatchesList
    {
        public List<ProtectedFilePatches> files = new List<ProtectedFilePatches>();
    };

    private Dictionary<string, byte[]> patchedFileData = new Dictionary<string, byte[]>();

    // Use Scriptable Object here does not feel right to me, especially since we want to retain
    // some of the hex form as integer for better debugging! Probably will consider more in the future.
    private static readonly string PatchFileName = "ProtectedDataPatch";
    private static readonly long MaxProtectedFileSizeAllowed = 2 * 1024 * 1024;

    public ProtectedFilePatcher()
    {
        TextAsset targetJson = Resources.Load<TextAsset>(PatchFileName);
        if (targetJson == null)
        {
            throw new FileNotFoundException("Unable to find data patch file!");
        }
        
        var filePatchesClass = JsonConvert.DeserializeObject<ProtectedFilePatchesList>(targetJson.text);
        filePatches = filePatchesClass.files;
    }

    public Stream OpenFile(string path)
    {
        // This is fine I think. Not too many resource pack file to be using Dictionary
        var filenameOnly = Path.GetFileName(path);
        var filePatch = filePatches.Find(file => file.FileName.Equals(filenameOnly, StringComparison.OrdinalIgnoreCase));
        if (filePatch == null)
        {
            return File.OpenRead(path);
        }

        if (patchedFileData.TryGetValue(filePatch.FileName, out byte[] rawData))
        {
            return new MemoryStream(rawData);
        }

        using (FileStream stream = File.OpenRead(path))
        {
            if (stream.Length > MaxProtectedFileSizeAllowed)
            {
                throw new FileLoadException(string.Format("File too big to be patched! (filename = {0})", path));
            }

            using (MemoryStream memoryStream = new MemoryStream())
            {
                stream.CopyTo(memoryStream);
                byte[] rawBytes = memoryStream.ToArray();

                foreach (ProtectedFilePatch patchInfo in filePatch.Patches)
                {
                    byte[] patchBytes = patchInfo.BytesInHex.Split(' ')
                        .Select(hexString => byte.Parse(hexString, NumberStyles.HexNumber))
                        .ToArray();
                
                    if (patchBytes.Length > 0)
                    {
                        Buffer.BlockCopy(patchBytes, 0, rawBytes, int.Parse(patchInfo.Offset, NumberStyles.HexNumber), patchBytes.Length);
                    }
                }

                patchedFileData.Add(filePatch.FileName, rawBytes);
                return new MemoryStream(rawBytes);
            }
        }
    }
}
