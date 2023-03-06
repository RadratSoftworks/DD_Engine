using System;
using System.Collections.Generic;
using UnityEngine;
using DDEngine.Utils;

namespace DDEngine.GUI.Parser
{
    public class GUIControlSettingMultiValuesOptionDescription : GUIControlDescription
    {
        public Vector2 Position { get; set; }
        public string BackgroundImagePath { get; set; }
        public string Id { get; set; }
        public string SettingName { get; set; }
        public List<Tuple<string, string>> ValuesAndValueTextIds = new List<Tuple<string, string>>();
        public string FocusIdleAnimationPath { get; set; }

        public GUIControlSettingMultiValuesOptionDescription()
        {

        }

        public GUIControlSettingMultiValuesOptionDescription(BinaryReader2 reader)
        {
            Internalize(reader);
        }

        private void Internalize(BinaryReader2 reader)
        {
            short posX = reader.ReadInt16();
            short posY = reader.ReadInt16();

            Position = new Vector2(posX, posY);
            Depth = reader.ReadInt16();

            BackgroundImagePath = reader.ReadWordLengthString();
            Id = reader.ReadWordLengthString();
            SettingName = reader.ReadWordLengthString();

            string valuesConned = reader.ReadWordLengthString();
            string textIdForValuesConned = reader.ReadWordLengthString();

            string[] values = valuesConned.Split(',');
            string[] textIdForValues = textIdForValuesConned.Split(',');

            for (int i = 0; i < Mathf.Min(values.Length, textIdForValues.Length); i++)
            {
                ValuesAndValueTextIds.Add(new Tuple<string, string>(values[i], textIdForValues[i]));
            }

            FocusIdleAnimationPath = reader.ReadWordLengthString();
        }
    }
}