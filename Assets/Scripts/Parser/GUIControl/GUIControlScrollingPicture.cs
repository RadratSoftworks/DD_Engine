using System;
using UnityEngine;

public class GUIControlScrollingPicture : GUIControl
{
    public string ImagePath { get; set; }
    public Vector2 TopPosition { get; set; }
    public short Flags { get; set; }

    public GUIControlScrollingPicture(BinaryReader2 reader)
    {
        Internalize(reader);
    }

    private void Internalize(BinaryReader2 reader)
    {
        short x = reader.ReadInt16BE();
        short y = reader.ReadInt16BE();
        Flags = reader.ReadInt16BE();   // not really sure if it's flags or not!

        TopPosition = new Vector2(x, y);

        short c1 = reader.ReadInt16BE();
        short c2 = reader.ReadInt16BE();
        short c3 = reader.ReadInt16BE();
        short c4 = reader.ReadInt16BE();

        ImagePath = reader.ReadWordLengthString();
    }
}
