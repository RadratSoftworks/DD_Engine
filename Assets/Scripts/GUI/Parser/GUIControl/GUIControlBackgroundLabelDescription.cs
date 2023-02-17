using System;
using System.Collections.Generic;
using UnityEngine;

public class GUIControlBackgroundLabelDescription : GUIControlDescription
{
    public Vector2 TopPosition { get; set; }
    public string TextId { get; set; }
    public Vector2 MarginSize { get; set; }
    public string UnkStr { get; set; }

    public Color FillColor { get; set; }

    public Color TextColor { get; set; }

    public GUIControlBackgroundLabelDescription(GUIControlDescription parent, BinaryReader2 reader)
    {
        Internalize(parent, reader);
    }

    private void Internalize(GUIControlDescription parent, BinaryReader2 reader)
    {
        short x = reader.ReadInt16BE();
        short y = reader.ReadInt16BE();

        Depth = reader.ReadInt16BE();

        TopPosition = new Vector2(x, y);
        TextId = reader.ReadWordLengthString();

        short marginX = reader.ReadInt16BE();
        short marginY = reader.ReadInt16BE();

        MarginSize = new Vector2(marginX, marginY);
        float alpha = (byte)reader.ReadUInt16BE() / 255.0f;

        FillColor = new Color((byte)reader.ReadUInt16BE() / 255.0f,
            (byte)reader.ReadUInt16BE() / 255.0f,
            (byte)reader.ReadUInt16BE() / 255.0f,
            alpha);

        TextColor = new Color((byte)reader.ReadUInt16BE() / 255.0f,
            (byte)reader.ReadUInt16BE() / 255.0f,
            (byte)reader.ReadUInt16BE() / 255.0f,
            alpha);

        UnkStr = reader.ReadWordLengthString();
    }
}
