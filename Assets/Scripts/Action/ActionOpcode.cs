﻿namespace DDEngine.Action
{
    public enum ActionOpcode
    {
        Return,
        LoadLocation,
        ClearGlobals,
        SetGlobal,
        LoadDialogue,
        LoadGadget,
        Play,
        SetLocationOffset,
        PanLocation,
        SetScrollSpeeds,
        Achieve,
        LoadMiniGame,
        ResumeSave,
        SwitchNgi,
        Timer,
        DeleteSaves,
        SaveSettings,
        DeleteSettings,
        SetSetting,
        Quit
    };
}