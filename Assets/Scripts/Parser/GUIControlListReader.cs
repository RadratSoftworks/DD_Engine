using System;
using System.Collections.Generic;

public class GUIControlListReader
{
    enum GUIControlID
    {
        Picture = 3,
        Label = 4,
        Animation = 5,
        ScrollingPicture = 6,
        Menu = 10,
        MenuItem = 11,
        Condition = 50,
        Value = 51
    };

    public static List<GUIControl> InternalizeControls(BinaryReader2 reader)
    {
        List<GUIControl> controls = new List<GUIControl>();

        short length = reader.ReadInt16BE();
        for (short i = 0; i < length; i++)
        {
            GUIControlID controlID = (GUIControlID)reader.ReadByte();
            switch (controlID)
            {
                case GUIControlID.Condition:
                    controls.Add(new GUIControlConditional(reader));
                    break;

                case GUIControlID.Value:
                    controls.Add(new GUIControlValue(reader));
                    break;

                case GUIControlID.Menu:
                    controls.Add(new GUIControlMenu(reader));
                    break;

                case GUIControlID.MenuItem:
                    controls.Add(new GUIControlMenuItem(reader));
                    break;

                case GUIControlID.Picture:
                    controls.Add(new GUIControlPicture(reader));
                    break;

                case GUIControlID.Animation:
                    controls.Add(new GUIControlAnimation(reader));
                    break;

                case GUIControlID.Label:
                    controls.Add(new GUIControlLabel(reader));
                    break;

                case GUIControlID.ScrollingPicture:
                    controls.Add(new GUIControlScrollingPicture(reader));
                    break;

                default:
                    throw new InvalidOperationException("Unknown control ID " + (int)controlID + " to internalize!");
            }
        }

        return controls;
    }
}
