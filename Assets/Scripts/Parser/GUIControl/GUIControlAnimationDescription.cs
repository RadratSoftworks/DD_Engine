using System;
using UnityEngine;
public class GUIControlAnimationDescription : GUIControlDescription
{
    public string AnimationPath { get; set; }
    public Vector2 TopPosition { get; set; }
    public int SortingPosition { get; set; }

    public GUIControlAnimationDescription(BinaryReader2 reader)
    {
        Internalize(reader);
    }

    private void Internalize(BinaryReader2 reader)
    {
        short x = reader.ReadInt16BE();
        short y = reader.ReadInt16BE();
        SortingPosition = GameUtils.ToUnitySortingPosition(reader.ReadInt16BE());   // not really sure if it's flags or not!

        TopPosition = new Vector2(x, y);
        AnimationPath = reader.ReadWordLengthString();
    }
}
