using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSwitchMinigameInfo 
{
    public string WonScript;
    public string LoseScript;

    public int ForcePercentage;
    public int MaxSpeedPercentage;
    public int WinPercentage;

    public ItemSwitchBackgroundInfo BackgroundInfo = new ItemSwitchBackgroundInfo();
    public ItemSwitchDirkHandInfo LeftHandInfo = new ItemSwitchDirkHandInfo();
    public ItemSwitchDirkHandInfo RightHandInfo = new ItemSwitchDirkHandInfo();

    public ItemSwitchStressMachineInfo StressMachineInfo = new ItemSwitchStressMachineInfo();
    public ItemSwitchTimerInfo TimerInfo = new ItemSwitchTimerInfo();
}
