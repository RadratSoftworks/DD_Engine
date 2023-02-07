using System;
using UnityEngine;
public class GUIControlAnimationDescription : GUIControlDescription
{
    public string AnimationPath { get; set; }
    public Vector2 TopPosition { get; set; }

    public GUIControlAnimationDescription()
    {

    }

    public GUIControlAnimationDescription(GUIControlDescription parent, BinaryReader2 reader)
    {
        Internalize(parent, reader);
    }

    private void Internalize(GUIControlDescription parent, BinaryReader2 reader)
    {
        short x = reader.ReadInt16BE();
        short y = reader.ReadInt16BE();

        Depth = reader.ReadInt16BE();

        TopPosition = new Vector2(x, y);
        AnimationPath = reader.ReadWordLengthString();
    }
}
