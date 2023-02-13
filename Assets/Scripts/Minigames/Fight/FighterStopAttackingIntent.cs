using System;

public enum FighterStopAttackingReason
{
    Pause = 0,
    OpponentKnockedOut = 1
};

public class FighterStopAttackingIntent
{
    public FighterStopAttackingReason Reason { get; set; }

    public FighterStopAttackingIntent(FighterStopAttackingReason reason)
    {
        Reason = reason;
    }
}
