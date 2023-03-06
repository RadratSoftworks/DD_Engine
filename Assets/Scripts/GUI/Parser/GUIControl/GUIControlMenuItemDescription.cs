using UnityEngine;
using DDEngine.Utils;

namespace DDEngine.GUI.Parser
{
    public class GUIControlMenuItemDescription : GUIControlDescription
    {
        public string ImagePath { get; set; }
        public string TextName { get; set; }
        public string Id { get; set; }

        public Vector2 Position { get; set; }

        public GUIControlMenuItemDescription(GUIControlDescription parent, BinaryReader2 reader)
        {
            Internalize(parent, reader);
        }

        public GUIControlMenuItemDescription()
        {
        }

        private void Internalize(GUIControlDescription parent, BinaryReader2 reader)
        {
            int x = reader.ReadInt16BE();
            int y = reader.ReadInt16BE();

            Depth = reader.ReadInt16BE();

            Position = new Vector2(x, y);
            ImagePath = reader.ReadWordLengthString();
            TextName = reader.ReadWordLengthString();
            Id = reader.ReadWordLengthString();
        }
    }
}