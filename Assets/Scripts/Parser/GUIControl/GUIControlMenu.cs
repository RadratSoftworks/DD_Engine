using System;
using System.Collections.Generic;
using UnityEngine;

public class GUIControlMenu : GUIControl
{
    private List<GUIControl> menuItemControls = new List<GUIControl>();

    public Vector2 TopPosition { get; set; }

    public int unk8 { get; set; }

    // The image that will be put beside the menu. In Dirk Dagger it's the detective
    // Usually the image will be placed with the bottom-left point be the bottom-left of the screen
    public string SideImagePath { get; set;}

    public string ActionHandlerFilePath { get; set; }

    public List<GUIControl> MenuItemControls => menuItemControls;

    public GUIControlMenu(BinaryReader2 reader)
    {
        Internalize(reader);
    }

    private void Internalize(BinaryReader2 reader)
    {
        float x = reader.ReadInt16BE();
        float y = reader.ReadInt16BE();
        unk8 = reader.ReadInt16BE();

        TopPosition = new Vector2(x, y);
        SideImagePath = reader.ReadWordLengthString();
        ActionHandlerFilePath = reader.ReadWordLengthString();

        menuItemControls = GUIControlListReader.InternalizeControls(reader);
    }
}
