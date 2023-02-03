using System;
using UnityEngine;
public class GUIControlAnimation : GUIControl
{
    public string AnimationPath { get; set; }
    public Vector2 TopPosition { get; set; }
    public short Flags { get; set; }

    public GUIControlAnimation(BinaryReader2 reader)
    {
        Internalize(reader);
    }

    private void Internalize(BinaryReader2 reader)
    {
        short x = reader.ReadInt16BE();
        short y = reader.ReadInt16BE();
        Flags = reader.ReadInt16BE();   // not really sure if it's flags or not!

        TopPosition = new Vector2(x, y);
        AnimationPath = reader.ReadWordLengthString();
    }
}
