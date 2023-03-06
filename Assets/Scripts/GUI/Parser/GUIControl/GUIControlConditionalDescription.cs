using System;
using System.Collections.Generic;
using DDEngine.Utils;

namespace DDEngine.GUI.Parser
{
    public class GUIControlConditionalDescription : GUIControlDescription
    {
        private String conditionValueVariable;
        private bool unk0;
        private List<Tuple<string, List<GUIControlDescription>>> controlOnCases = new List<Tuple<string, List<GUIControlDescription>>>();

        public String ConditionValueVariable => conditionValueVariable;
        public List<Tuple<string, List<GUIControlDescription>>> ControlShowOnCases => controlOnCases;

        public GUIControlConditionalDescription(GUIControlDescription parent, List<Injection.Injector> injectors, BinaryReader2 reader)
        {
            Internalize(parent, injectors, reader);
        }

        private void Internalize(GUIControlDescription parent, List<Injection.Injector> injectors, BinaryReader2 reader)
        {
            conditionValueVariable = reader.ReadWordLengthString();
            Depth = int.MaxValue;

            unk0 = (reader.ReadByte() != 0);

            List<GUIControlDescription> condValuesWithCases = GUIControlListReader.InternalizeControls(parent, reader, injectors);

            foreach (GUIControlDescription control in condValuesWithCases)
            {
                if (!(control is GUIControlValueDescription))
                {
                    throw new InvalidOperationException(string.Format("Expect condition followed controls to be of value-type control, got type {} instead", control.GetType()));
                }
                else
                {
                    GUIControlValueDescription value = (GUIControlValueDescription)control;
                    controlOnCases.Add(new Tuple<string, List<GUIControlDescription>>(value.ExactValue, value.ControlsOnExactValueMatch));
                }
            }
        }
    }
}
