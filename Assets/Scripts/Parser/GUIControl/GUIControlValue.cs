using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class GUIControlValue : GUIControl
{
    public string ExactValue { get; set; }

    private List<GUIControl> controlsOnExactValueMatch = new List<GUIControl>();

    public List<GUIControl> ControlsOnExactValueMatch => controlsOnExactValueMatch;

    public GUIControlValue(BinaryReader2 reader)
    {
        Internalize(reader);
    }

    private void Internalize(BinaryReader2 reader)
    {
        ExactValue = reader.ReadWordLengthString();
        controlsOnExactValueMatch = GUIControlListReader.InternalizeControls(reader);
    }
}
