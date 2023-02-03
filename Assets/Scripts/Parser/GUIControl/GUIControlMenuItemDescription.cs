using System;
using UnityEngine;

public class GUIControlMenuItemDescription: GUIControlDescription
{
    public string ImagePath { get; set; }
    public string TextName { get; set; }
    public string Id { get; set; }

    public Vector2 Position { get; set; }
    public int SortingPosition { get; set; }

    public GUIControlMenuItemDescription(BinaryReader2 reader)
    {
        Internalize(reader);
    }

    private void Internalize(BinaryReader2 reader)
    {
        int x = reader.ReadInt16BE();
        int y = reader.ReadInt16BE();
        SortingPosition = GameUtils.ToUnitySortingPosition(reader.ReadInt16BE());

        Position = new Vector2(x, y);
        ImagePath = reader.ReadWordLengthString();
        TextName = reader.ReadWordLengthString();
        Id = reader.ReadWordLengthString();
    }
}
