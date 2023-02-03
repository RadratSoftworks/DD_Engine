using System;
using System.Collections.Generic;
using UnityEngine;

public class GUIControlConditional : GUIControl
{
    private String conditionValueVariable;
    private bool unk0;
    private Dictionary<string, List<GUIControl>> controlOnCases = new Dictionary<string, List<GUIControl>>();

    public String ConditionValueVariable => conditionValueVariable;
    public Dictionary<string, List<GUIControl>> ControlShowOnCases => controlOnCases;

    public GUIControlConditional(BinaryReader2 reader)
    {
        Internalize(reader);
    }

    private void Internalize(BinaryReader2 reader)
    {
        conditionValueVariable = reader.ReadWordLengthString();
        unk0 = (reader.ReadByte() != 0);
        List<GUIControl> condValuesWithCases = GUIControlListReader.InternalizeControls(reader);

        foreach (GUIControl control in condValuesWithCases)
        {
            if (!(control is GUIControlValue))
            {
                throw new InvalidOperationException(string.Format("Expect condition followed controls to be of value-type control, got type {} instead", control.GetType()));
            }
            else
            {
                GUIControlValue value = (GUIControlValue)control;
                controlOnCases.Add(value.ExactValue, value.ControlsOnExactValueMatch);
            }
        }
    }
}
