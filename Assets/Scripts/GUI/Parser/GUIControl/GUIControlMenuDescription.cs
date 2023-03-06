using System.Collections.Generic;
using UnityEngine;
using DDEngine.Utils;

namespace DDEngine.GUI.Parser
{
    public class GUIControlMenuDescription : GUIControlDescription
    {
        private List<GUIControlDescription> menuItemControls = new List<GUIControlDescription>();

        public Vector2 TopPosition { get; set; }

        // The image that will be put beside the menu. In Dirk Dagger it's the detective
        // Usually the image will be placed with the bottom-left point be the bottom-left of the screen
        public string SideImagePath { get; set; }

        public string ActionHandlerFilePath { get; set; }

        public List<GUIControlDescription> MenuItemControls => menuItemControls;

        public GUIControlMenuDescription(GUIControlDescription parent, List<Injection.Injector> injectors, BinaryReader2 reader)
        {
            Internalize(parent, injectors, reader);
        }

        private void Internalize(GUIControlDescription parent, List<Injection.Injector> injectors, BinaryReader2 reader)
        {
            float x = reader.ReadInt16BE();
            float y = reader.ReadInt16BE();

            Depth = reader.ReadInt16BE();

            TopPosition = new Vector2(x, y);
            SideImagePath = reader.ReadWordLengthString();
            ActionHandlerFilePath = reader.ReadWordLengthString();

            menuItemControls = GUIControlListReader.InternalizeControls(this, reader, injectors, new List<GUIControlID> { GUIControlID.MenuItem, GUIControlID.SettingMultiValuesOption });

            foreach (var injector in injectors)
            {
                injector.CallInjectorFunction(GUIControlID.Menu, this);
            }
        }
    }
}