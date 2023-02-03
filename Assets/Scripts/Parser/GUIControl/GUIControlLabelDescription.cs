using System;
using UnityEngine;
public class GUIControlLabelDescription : GUIControlDescription
{
    public string TextName { get; set; }
    public Vector2 TopPosition { get; set; }
    public int SortingPosition { get; set; }
    public string Id { get; set; }

    public GUIControlLabelDescription(BinaryReader2 reader)
    {
        Internalize(reader);
    }

    private void Internalize(BinaryReader2 reader)
    {
        short x = reader.ReadInt16BE();
        short y = reader.ReadInt16BE();
        SortingPosition = GameUtils.ToUnitySortingPosition(reader.ReadInt16BE());   // not really sure if it's flags or not!

        TopPosition = new Vector2(x, y);
        TextName = reader.ReadWordLengthString();
        Id = reader.ReadWordLengthString();
    }
}
