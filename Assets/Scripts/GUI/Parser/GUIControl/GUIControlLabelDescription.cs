using System;
using UnityEngine;
public class GUIControlLabelDescription : GUIControlDescription
{
    public string TextName { get; set; }
    public Vector2 TopPosition { get; set; }
    public string Id { get; set; }

    public GUIControlLabelDescription(GUIControlDescription parent, BinaryReader2 reader)
    {
        Internalize(parent, reader);
    }

    private void Internalize(GUIControlDescription parent, BinaryReader2 reader)
    {
        short x = reader.ReadInt16BE();
        short y = reader.ReadInt16BE();

        Depth = reader.ReadInt16BE();

        TopPosition = new Vector2(x, y);
        TextName = reader.ReadWordLengthString();
        Id = reader.ReadWordLengthString();
    }
}
