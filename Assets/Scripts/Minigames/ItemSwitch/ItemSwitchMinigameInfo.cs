using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSwitchMinigameInfo 
{
    public string WonScript;
    public string LoseScript;

    public int ChangeStartDurationPercentage;
    public int MaxSpeedPercentage;
    public int WinPercentage;

    public ItemSwitchBackgroundInfo BackgroundInfo = new ItemSwitchBackgroundInfo();
    public ItemSwitchDirkHandInfo LeftHandInfo = new ItemSwitchDirkHandInfo();
    public ItemSwitchDirkHandInfo RightHandInfo = new ItemSwitchDirkHandInfo();

    public ItemSwitchStressIndicatorInfo StressIndicatorInfo = new ItemSwitchStressIndicatorInfo();
    public ItemSwitchTimerInfo TimerInfo = new ItemSwitchTimerInfo();
}
