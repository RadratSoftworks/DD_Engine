using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class GUIControlValueDescription : GUIControlDescription
{
    public string ExactValue { get; set; }

    private List<GUIControlDescription> controlsOnExactValueMatch = new List<GUIControlDescription>();

    public List<GUIControlDescription> ControlsOnExactValueMatch => controlsOnExactValueMatch;

    public GUIControlValueDescription(BinaryReader2 reader)
    {
        Internalize(reader);
    }

    private void Internalize(BinaryReader2 reader)
    {
        ExactValue = reader.ReadWordLengthString();
        controlsOnExactValueMatch = GUIControlListReader.InternalizeControls(reader);
    }
}
