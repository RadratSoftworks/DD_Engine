﻿using System.Collections.Generic;
using System.IO;

public class GUIControlDescriptionFile
{
    private GUIControlSetDescription controlSetRoot;

    public List<GUIControlDescription> Controls => controlSetRoot.Controls;
    public string Filename { get; set; }

    public GUIControlDescriptionFile(string filename)
    {
        Filename = filename;

        using (FileStream fileStream = File.OpenRead(filename))
        {
            Internalize(fileStream);
        }
    }

    public GUIControlDescriptionFile(Stream baseStream)
    {
        Internalize(baseStream);
    }

    private void Internalize(Stream fileStream)
    {
        using (var fileBinaryReader = new BinaryReader2(fileStream))
        {
            byte[] header = new byte[8];
            if (fileStream.Read(header) != 8)
            {
                throw new InvalidDataException("Can't read GUI control file header!");
            }

            if (System.Text.Encoding.UTF8.GetString(header) != "fwd2DIST")
            {
                throw new InvalidDataException("Invalid GUI control file header!");
            }

            fileStream.Seek(14, SeekOrigin.Begin);
        
            controlSetRoot = new GUIControlSetDescription(null, fileBinaryReader);
            GUIDescriptionDepthNormalizer.Normalize(controlSetRoot);
        }
    }
}