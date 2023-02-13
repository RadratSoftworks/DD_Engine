using System;

public enum FightDirection
{
    Left,
    Right
};

public class FightDamage
{
    public FightDirection Direction { get; set; }

    public int DamagePoint { get; set; }
}