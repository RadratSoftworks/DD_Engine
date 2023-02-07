using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class GUIControlSetDescription : GUIControlDescription
{
    public List<GUIControlDescription> Controls { get; set; }
    public int Unk1 { get; set; }
    public int Unk2 { get; set; }

    public GUIControlSetDescription(GUIControlDescription parent, BinaryReader2 reader)
    {
        Internalize(parent, reader);
    }

    public void Internalize(GUIControlDescription parent, BinaryReader2 reader)
    {
        Unk1 = reader.ReadByte();
        Unk2 = reader.ReadByte();

        Controls = GUIControlListReader.InternalizeControls(this, reader);
    }
}
