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
        BackgroundLabel = 7,
        Menu = 10,
        MenuItem = 11,
        Location = 30,
        Layer = 31,
        Active = 40,
        Condition = 50,
        Value = 51,
        Sound = 80
    };

    public static List<GUIControlDescription> InternalizeControls(GUIControlDescription parent, BinaryReader2 reader)
    {
        List<GUIControlDescription> controls = new List<GUIControlDescription>();

        short length = reader.ReadInt16BE();
        for (short i = 0; i < length; i++)
        {
            GUIControlID controlID = (GUIControlID)reader.ReadByte();
            switch (controlID)
            {
                case GUIControlID.Condition:
                    controls.Add(new GUIControlConditionalDescription(parent, reader));
                    break;

                case GUIControlID.Value:
                    controls.Add(new GUIControlValueDescription(parent, reader));
                    break;

                case GUIControlID.Menu:
                    controls.Add(new GUIControlMenuDescription(parent, reader));
                    break;

                case GUIControlID.MenuItem:
                    controls.Add(new GUIControlMenuItemDescription(parent, reader));
                    break;

                case GUIControlID.Picture:
                    controls.Add(new GUIControlPictureDescription(parent, reader));
                    break;

                case GUIControlID.Animation:
                    controls.Add(new GUIControlAnimationDescription(parent, reader));
                    break;

                case GUIControlID.Label:
                    controls.Add(new GUIControlLabelDescription(parent, reader));
                    break;

                case GUIControlID.ScrollingPicture:
                    controls.Add(new GUIControlScrollingPictureDescription(parent, reader));
                    break;

                case GUIControlID.Layer:
                    controls.Add(new GUIControlLayerDescription(parent, reader));
                    break;

                case GUIControlID.Location:
                    controls.Add(new GUIControlLocationDescription(parent, reader));
                    break;

                case GUIControlID.Active:
                    controls.Add(new GUIControlActiveDescription(reader));
                    break;

                case GUIControlID.Sound:
                    controls.Add(new GUIControlSoundDescription(reader));
                    break;

                case GUIControlID.BackgroundLabel:
                    controls.Add(new GUIControlBackgroundLabelDescription(parent, reader));
                    break;

                default:
                    throw new InvalidOperationException("Unknown control ID " + (int)controlID + " to internalize! Stream position: " + reader.BaseStream.Position);
            }
        }

        return controls;
    }
}
