using System.Collections.Generic;
using System.IO;

public class GUIControlDescriptionFile
{
    private List<GUIControlDescription> controls = new List<GUIControlDescription>();
    private bool unk0;
    private bool unk1;
    private bool unk2;

    public List<GUIControlDescription> Controls => controls;
    public string Filename { get; set; }

    public GUIControlDescriptionFile(string filename)
    {
        Filename = filename;

        using (FileStream fileStream = File.OpenRead(filename))
        {
            Internalize(fileStream);
        }
    }

    public GUIControlDescriptionFile(Stream baseStream)
    {
        Internalize(baseStream);
    }

    private void Internalize(Stream fileStream)
    {
        using (var fileBinaryReader = new BinaryReader2(fileStream))
        {
            byte[] header = new byte[8];
            if (fileStream.Read(header) != 8)
            {
                throw new InvalidDataException("Can't read GUI control file header!");
            }

            if (System.Text.Encoding.UTF8.GetString(header) != "fwd2DIST")
            {
                throw new InvalidDataException("Invalid GUI control file header!");
            }

            fileStream.Seek(13, SeekOrigin.Begin);

            unk0 = (fileBinaryReader.ReadByte() != 0);
            unk1 = (fileBinaryReader.ReadByte() != 0);
            unk2 = (fileBinaryReader.ReadByte() != 0);

            controls = GUIControlListReader.InternalizeControls(fileBinaryReader);
        }
    }
}