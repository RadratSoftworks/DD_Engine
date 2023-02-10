using System;
using UnityEngine;

public class GUIControlScrollingPictureDescription : GUIControlDescription
{
    public string ImagePath { get; set; }
    public Vector2 TopPosition { get; set; }
    public Vector2 Scroll { get; set; }
    public Vector2 ScrollSize { get; set; }

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

        short scrollX = reader.ReadInt16BE();
        short scrollY = reader.ReadInt16BE();
        short scrollWidth = reader.ReadInt16BE();
        short scrollHeight = reader.ReadInt16BE();

        Scroll = new Vector2(scrollX, scrollY);
        ScrollSize = new Vector2(scrollWidth, scrollHeight);

        ImagePath = reader.ReadWordLengthString();
    }
}
