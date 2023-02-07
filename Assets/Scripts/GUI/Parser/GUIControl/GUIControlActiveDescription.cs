using System;
using System.Collections.Generic;
using UnityEngine;

public class GUIControlActiveDescription : GUIControlDescription
{
    public Vector2 TopPosition { get; set; }
    public Vector2 Size { get; set; }
    public Vector2 BoundPosition { get; set; }
    public Vector2 BoundSize { get; set; }

    public string Id { get; set; }


    public GUIControlActiveDescription(BinaryReader2 reader)
    {
        Internalize(reader);
    }

    private void Internalize(BinaryReader2 reader)
    {
        float x = reader.ReadInt16BE();
        float y = reader.ReadInt16BE();

        TopPosition = new Vector2(x, y);

        float width = reader.ReadInt16BE();
        float height = reader.ReadInt16BE();

        Size = new Vector2(width, height);
        Id = reader.ReadWordLengthString();

        float boundX = reader.ReadInt16BE();
        float boundY = reader.ReadInt16BE();

        BoundPosition = new Vector2(boundX, boundY);

        float boundWidth = reader.ReadInt16BE();
        float boundHeight = reader.ReadInt16BE();

        BoundSize = new Vector2(boundWidth, boundHeight);
    }
}
