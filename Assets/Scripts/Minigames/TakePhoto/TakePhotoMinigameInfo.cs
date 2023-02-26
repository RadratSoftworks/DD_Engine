using UnityEngine;

namespace DDEngine.Minigame.TakePhoto
{
    public class TakePhotoMinigameInfo
    {
        public Vector2 StartPoint { get; set; }
        public Vector2 FocusPoint { get; set; }

        public string ControlSetPath { get; set; }

        public TakePhotoCameraSideInfo BackSide { get; set; } = new TakePhotoCameraSideInfo();

        public TakePhotoCameraSideInfo FrontSide { get; set; } = new TakePhotoCameraSideInfo();
    };
}