using UnityEngine;
using DDEngine.Utils;

namespace DDEngine.GUI.Parser
{
    public class GUIControlScrollingTextDescription : GUIControlDescription
    {
        public string Text { get; set; }
        public Vector2 TopPosition { get; set; }
        public int Width { get; set; }
        public Vector2 Scroll { get; set; }

        public GUIControlScrollingTextDescription(GUIControlDescription parent, BinaryReader2 reader)
        {
            Internalize(parent, reader);
        }

        private void Internalize(GUIControlDescription parent, BinaryReader2 reader)
        {
            short x = reader.ReadInt16BE();
            short y = reader.ReadInt16BE();
            Depth = reader.ReadInt16BE();

            TopPosition = new Vector2(x, y);
            Text = reader.ReadWordLengthString();

            Width = reader.ReadInt16BE();
            short scrollSpeedX = reader.ReadInt16BE();
            short scrollSpeedY = reader.ReadInt16BE();

            Scroll = new Vector2(scrollSpeedX, scrollSpeedY);
        }
    }
}
