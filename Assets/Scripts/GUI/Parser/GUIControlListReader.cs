using System;
using System.Collections.Generic;
using UnityEngine;

public class GUIControlListReader
{
    public static List<GUIControlDescription> InternalizeControls(GUIControlDescription parent, BinaryReader2 reader, List<GUIControlID> acceptableID = null)
    {
        List<GUIControlDescription> controls = new List<GUIControlDescription>();

        short length = reader.ReadInt16BE();
        for (short i = 0; i < length; i++)
        {
            if (reader.BaseStream.Position == reader.BaseStream.Length)
            {
                Debug.LogWarning("Premature end-offile reached while reading GUI control file description! Finishing!");
                break;
            }

            GUIControlID controlID = (GUIControlID)reader.ReadByte();
            if ((acceptableID != null) && !acceptableID.Contains(controlID))
            {
                Debug.LogWarningFormat("Unmatched requested control ID: target={0}, result={1}", acceptableID, controlID);
                reader.RewindByte();

                break;
            }

            if (controlID == (GUIControlID)0)
            {
                Debug.LogWarning("Encountering 0 control id, breaking!");
                break;
            }

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

                case GUIControlID.SettingMultiValuesOption:
                    controls.Add(new GUIControlSettingMultiValuesOptionDescription(reader));
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
