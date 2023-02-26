using DDEngine.Utils;

namespace DDEngine.GUI.Parser
{
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
}