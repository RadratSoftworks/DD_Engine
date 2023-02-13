using System;
using System.Collections.Generic;
using UnityEngine;

public class GUIControlBackgroundLabelDescription : GUIControlDescription
{
    public Vector2 TopPosition { get; set; }
    public string TextId { get; set; }
    public int unk1 { get; set; }
    public int unk2 { get; set; }
    public int unk3 { get; set; }
    public int unk4 { get; set; }

    public Color color1 { get; set; }

    public Color color2 { get; set; }

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

        unk1 = reader.ReadInt16BE();
        unk2 = reader.ReadInt16BE();

        float alpha = (byte)reader.ReadUInt16BE() / 255.0f;

        color1 = new Color((byte)reader.ReadUInt16BE() / 255.0f,
            (byte)reader.ReadUInt16BE() / 255.0f,
            (byte)reader.ReadUInt16BE() / 255.0f,
            alpha);

        color2 = new Color((byte)reader.ReadUInt16BE() / 255.0f,
            (byte)reader.ReadUInt16BE() / 255.0f,
            (byte)reader.ReadUInt16BE() / 255.0f,
            alpha);

        unk3 = reader.ReadInt16BE();
        unk4 = reader.ReadInt16BE();
    }
}
