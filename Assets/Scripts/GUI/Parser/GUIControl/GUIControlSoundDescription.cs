using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class GUIControlSoundDescription : GUIControlDescription
{
    public int Type { get; set; }
    public string Path { get; set; }

    public GUIControlSoundDescription(BinaryReader2 reader)
    {
        Internalize(reader);
    }

    private void Internalize(BinaryReader2 reader)
    {
        Type = reader.ReadByte();
        Path = reader.ReadWordLengthString();
    }
}
