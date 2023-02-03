using System;
using System.Collections.Generic;
using UnityEngine;

public class GUIControlMenuDescription : GUIControlDescription
{
    private List<GUIControlDescription> menuItemControls = new List<GUIControlDescription>();

    public Vector2 TopPosition { get; set; }

    public int SortingPosition { get; set; }

    // The image that will be put beside the menu. In Dirk Dagger it's the detective
    // Usually the image will be placed with the bottom-left point be the bottom-left of the screen
    public string SideImagePath { get; set;}

    public string ActionHandlerFilePath { get; set; }

    public List<GUIControlDescription> MenuItemControls => menuItemControls;

    public GUIControlMenuDescription(BinaryReader2 reader)
    {
        Internalize(reader);
    }

    private void Internalize(BinaryReader2 reader)
    {
        float x = reader.ReadInt16BE();
        float y = reader.ReadInt16BE();
        SortingPosition = GameUtils.ToUnitySortingPosition(reader.ReadInt16BE());

        TopPosition = new Vector2(x, y);
        SideImagePath = reader.ReadWordLengthString();
        ActionHandlerFilePath = reader.ReadWordLengthString();

        menuItemControls = GUIControlListReader.InternalizeControls(reader);
    }
}
