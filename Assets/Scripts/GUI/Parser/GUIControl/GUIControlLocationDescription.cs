using System;
using System.Collections.Generic;
using UnityEngine;

public class GUIControlLocationDescription : GUIControlDescription
{
    private List<GUIControlDescription> layers = new List<GUIControlDescription>();

    public Vector2 TopPosition { get; set; }

    public string Name { get; set; }

    public List<GUIControlDescription> Layers => layers;

    public GUIControlLocationDescription(GUIControlDescription parent, BinaryReader2 reader)
    {
        Internalize(parent, reader);
    }

    private void Internalize(GUIControlDescription parent, BinaryReader2 reader)
    {
        float x = reader.ReadInt16BE();
        float y = reader.ReadInt16BE();

        TopPosition = new Vector2(x, y);

        Depth = reader.ReadInt16BE();
        Name = reader.ReadWordLengthString();

        layers = GUIControlListReader.InternalizeControls(this, reader);
    }
}
