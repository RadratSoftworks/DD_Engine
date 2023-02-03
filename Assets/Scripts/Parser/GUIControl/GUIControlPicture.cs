using System;
using UnityEngine;

public class GUIControlPicture: GUIControl
{
    public string ImagePath { get; set; }
    public string Id { get; set; }
    public Vector2 TopPosition { get; set; }
    public short Flags { get; set; }

    public GUIControlPicture(BinaryReader2 reader)
    {
        Internalize(reader);
    }

    private void Internalize(BinaryReader2 reader)
    {
        Id = reader.ReadWordLengthString();

        short x = reader.ReadInt16BE();
        short y = reader.ReadInt16BE();
        Flags = reader.ReadInt16BE();   // not really sure if it's flags or not!

        TopPosition = new Vector2(x, y);
        ImagePath = reader.ReadWordLengthString();
    }
}
