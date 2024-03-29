﻿using System.Collections.Generic;
using DDEngine.Utils;

namespace DDEngine.GUI.Parser
{
    public class GUIControlSetDescription : GUIControlDescription
    {
        public List<GUIControlDescription> Controls { get; set; }
        public int Unk1 { get; set; }
        public int Unk2 { get; set; }

        public GUIControlSetDescription(GUIControlDescription parent, List<Injection.Injector> injectors, BinaryReader2 reader)
        {
            Internalize(parent, injectors, reader);
        }

        public void Internalize(GUIControlDescription parent, List<Injection.Injector> injectors, BinaryReader2 reader)
        {
            Unk1 = reader.ReadByte();
            Unk2 = reader.ReadByte();

            Controls = GUIControlListReader.InternalizeControls(this, reader, injectors);
        }
    }
}