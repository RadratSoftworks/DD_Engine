using System.Collections.Generic;
using UnityEngine;
using DDEngine.Utils;

namespace DDEngine.GUI.Parser
{
    public class GUIControlLayerDescription : GUIControlDescription
    {
        private List<GUIControlDescription> controls = new List<GUIControlDescription>();

        public Vector2 TopPosition { get; set; }
        public Vector2 Scroll { get; set; }
        public Vector2 Size { get; set; }
        public bool DefinesPan { get; set; }

        public List<GUIControlDescription> Controls => controls;

        public GUIControlLayerDescription(GUIControlDescription parent, List<Injection.Injector> injectors, BinaryReader2 reader)
        {
            Internalize(parent, injectors, reader);
        }

        private void Internalize(GUIControlDescription parent, List<Injection.Injector> injectors, BinaryReader2 reader)
        {
            float x = reader.ReadInt16BE();
            float y = reader.ReadInt16BE();

            TopPosition = new Vector2(x, y);

            float width = reader.ReadInt16BE();
            float height = reader.ReadInt16BE();

            Size = new Vector2(width, height);
            Depth = reader.ReadInt16BE();

            float scrollX = reader.ReadInt16BE();
            float scrollY = reader.ReadInt16BE();

            Scroll = new Vector2(scrollX, scrollY);
            DefinesPan = (reader.ReadByte() == 1);

            controls = GUIControlListReader.InternalizeControls(this, reader, injectors);

            foreach (var injector in injectors)
            {
                injector.CallInjectorFunction(GUIControlID.Layer, this);
            }
        }
    }
}