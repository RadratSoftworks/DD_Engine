using System.Collections.Generic;
using UnityEngine;

public class PogoJumpMinigameInfo
{
    public string WonScript;
    public string BackgroundImagePath;
    public Vector2 PlayerPosition;
    public List<string> JumpTierAnimations = new List<string>();
    public int Difficulty;
    public string JumpSoundPath;

    public List<PogoJumpImageInfo> Images = new List<PogoJumpImageInfo>();
}
