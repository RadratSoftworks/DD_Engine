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

    public static List<GUIControlDescription> InternalizeControls(BinaryReader2 reader)
    {
        List<GUIControlDescription> controls = new List<GUIControlDescription>();

        short length = reader.ReadInt16BE();
        for (short i = 0; i < length; i++)
        {
            GUIControlID controlID = (GUIControlID)reader.ReadByte();
            switch (controlID)
            {
                case GUIControlID.Condition:
                    controls.Add(new GUIControlConditionalDescription(reader));
                    break;

                case GUIControlID.Value:
                    controls.Add(new GUIControlValueDescription(reader));
                    break;

                case GUIControlID.Menu:
                    controls.Add(new GUIControlMenuDescription(reader));
                    break;

                case GUIControlID.MenuItem:
                    controls.Add(new GUIControlMenuItemDescription(reader));
                    break;

                case GUIControlID.Picture:
                    controls.Add(new GUIControlPictureDescription(reader));
                    break;

                case GUIControlID.Animation:
                    controls.Add(new GUIControlAnimationDescription(reader));
                    break;

                case GUIControlID.Label:
                    controls.Add(new GUIControlLabelDescription(reader));
                    break;

                case GUIControlID.ScrollingPicture:
                    controls.Add(new GUIControlScrollingPictureDescription(reader));
                    break;

                default:
                    throw new InvalidOperationException("Unknown control ID " + (int)controlID + " to internalize!");
            }
        }

        return controls;
    }
}
