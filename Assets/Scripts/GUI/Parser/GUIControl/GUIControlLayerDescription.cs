using System;
using System.Collections.Generic;
using UnityEngine;

public class GUIControlLayerDescription : GUIControlDescription
{
    private List<GUIControlDescription> controls = new List<GUIControlDescription>();

    public Vector2 TopPosition { get; set; }
    public Vector2 Scroll { get; set; }
    public Vector2 Size { get; set; }
    public bool DefineSpan { get; set; }

    public List<GUIControlDescription> Controls => controls;

    public GUIControlLayerDescription(GUIControlDescription parent, BinaryReader2 reader)
    {
        Internalize(parent, reader);
    }

    private void Internalize(GUIControlDescription parent, BinaryReader2 reader)
    {
        float x = reader.ReadInt16BE();
        float y = reader.ReadInt16BE();

        TopPosition = new Vector2(x, y);

        float width = reader.ReadInt16BE();
        float height = reader.ReadInt16BE();

        Size = new Vector2(width, height);
        Depth = reader.ReadInt16BE();

        float scrollX = reader.ReadInt16BE();
        float scrollY = reader.ReadInt16BE();

        Scroll = new Vector2(scrollX, scrollY);
        DefineSpan = (reader.ReadByte() == 1);

        controls = GUIControlListReader.InternalizeControls(this, reader);
    }
}
