using System;
using UnityEngine;

public class GUIControlPictureDescription: GUIControlDescription
{
    public string ImagePath { get; set; }
    public string Id { get; set; }
    public Vector2 TopPosition { get; set; }
    public int SortingPosition { get; set; }

    public GUIControlPictureDescription(BinaryReader2 reader)
    {
        Internalize(reader);
    }

    public GUIControlPictureDescription()
    {

    }

    private void Internalize(BinaryReader2 reader)
    {
        Id = reader.ReadWordLengthString();

        short x = reader.ReadInt16BE();
        short y = reader.ReadInt16BE();
        SortingPosition = GameUtils.ToUnitySortingPosition(reader.ReadInt16BE());   // not really sure if it's flags or not!

        TopPosition = new Vector2(x, y);
        ImagePath = reader.ReadWordLengthString();
    }
}
