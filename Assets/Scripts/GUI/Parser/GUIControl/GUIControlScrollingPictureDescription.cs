using System;
using UnityEngine;

public class GUIControlScrollingPictureDescription : GUIControlDescription
{
    public string ImagePath { get; set; }
    public Vector2 TopPosition { get; set; }

    public GUIControlScrollingPictureDescription(GUIControlDescription parent, BinaryReader2 reader)
    {
        Internalize(parent, reader);
    }

    private void Internalize(GUIControlDescription parent, BinaryReader2 reader)
    {
        short x = reader.ReadInt16BE();
        short y = reader.ReadInt16BE();
        Depth = reader.ReadInt16BE();

        TopPosition = new Vector2(x, y);

        short c1 = reader.ReadInt16BE();
        short c2 = reader.ReadInt16BE();
        short c3 = reader.ReadInt16BE();
        short c4 = reader.ReadInt16BE();

        ImagePath = reader.ReadWordLengthString();
    }
}
