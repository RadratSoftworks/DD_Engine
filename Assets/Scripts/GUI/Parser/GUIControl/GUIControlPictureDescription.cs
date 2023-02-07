using System;
using UnityEngine;

public class GUIControlPictureDescription: GUIControlDescription
{
    public string ImagePath { get; set; }
    public string Id { get; set; }
    public Vector2 TopPosition { get; set; }

    public GUIControlPictureDescription(GUIControlDescription parent, BinaryReader2 reader)
    {
        Internalize(parent, reader);
    }

    public GUIControlPictureDescription()
    {
    }

    private void Internalize(GUIControlDescription parent, BinaryReader2 reader)
    {
        Id = reader.ReadWordLengthString();

        short x = reader.ReadInt16BE();
        short y = reader.ReadInt16BE();

        Depth = reader.ReadInt16BE();

        TopPosition = new Vector2(x, y);
        ImagePath = reader.ReadWordLengthString();
    }
}
