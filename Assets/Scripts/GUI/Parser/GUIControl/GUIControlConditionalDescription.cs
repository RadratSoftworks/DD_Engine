using System;
using System.Collections.Generic;
using UnityEngine;

public class GUIControlConditionalDescription : GUIControlDescription
{
    private String conditionValueVariable;
    private bool unk0;
    private Dictionary<string, List<GUIControlDescription>> controlOnCases = new Dictionary<string, List<GUIControlDescription>>();

    public String ConditionValueVariable => conditionValueVariable;
    public Dictionary<string, List<GUIControlDescription>> ControlShowOnCases => controlOnCases;

    public GUIControlConditionalDescription(GUIControlDescription parent, BinaryReader2 reader)
    {
        Internalize(parent,reader);
    }

    private void Internalize(GUIControlDescription parent, BinaryReader2 reader)
    {
        conditionValueVariable = reader.ReadWordLengthString();
        unk0 = (reader.ReadByte() != 0);
        List<GUIControlDescription> condValuesWithCases = GUIControlListReader.InternalizeControls(parent, reader);

        foreach (GUIControlDescription control in condValuesWithCases)
        {
            if (!(control is GUIControlValueDescription))
            {
                throw new InvalidOperationException(string.Format("Expect condition followed controls to be of value-type control, got type {} instead", control.GetType()));
            }
            else
            {
                GUIControlValueDescription value = (GUIControlValueDescription)control;
                controlOnCases.Add(value.ExactValue, value.ControlsOnExactValueMatch);
            }
        }
    }
}
