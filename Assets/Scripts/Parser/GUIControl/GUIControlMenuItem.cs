using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class GUIControlMenuItem: GUIControl
{
    public string ImagePath { get; set; }
    public string TextName { get; set; }
    public string Id { get; set; }

    public GUIControlMenuItem(BinaryReader2 reader)
    {
        Internalize(reader);
    }

    private void Internalize(BinaryReader2 reader)
    {
        reader.ReadInt16BE();
        reader.ReadInt16BE();
        reader.ReadInt16BE();

        ImagePath = reader.ReadWordLengthString();
        TextName = reader.ReadWordLengthString();
        Id = reader.ReadWordLengthString();
    }
}
