using System.Collections.Generic;
using DDEngine.Utils;

namespace DDEngine.GUI.Parser
{
    public class GUIControlValueDescription : GUIControlDescription
    {
        public string ExactValue { get; set; }

        private List<GUIControlDescription> controlsOnExactValueMatch = new List<GUIControlDescription>();

        public List<GUIControlDescription> ControlsOnExactValueMatch => controlsOnExactValueMatch;

        public GUIControlValueDescription(GUIControlDescription parent, BinaryReader2 reader)
        {
            Internalize(parent, reader);
        }

        private void Internalize(GUIControlDescription parent, BinaryReader2 reader)
        {
            ExactValue = reader.ReadWordLengthString();
            controlsOnExactValueMatch = GUIControlListReader.InternalizeControls(parent, reader);
        }
    }
}