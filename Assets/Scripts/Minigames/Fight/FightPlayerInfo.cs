using UnityEngine;

namespace DDEngine.Minigame.Fight
{
    public class FightPlayerInfo
    {
        public Vector2 Position;

        public string IdleAnimPath;
        public string DodgeAnimPath;
        public string DodgeRevertAnimPath;

        public FightAttackTypeInfo Jab = new FightAttackTypeInfo();
        public FightAttackTypeInfo Punch = new FightAttackTypeInfo();
        public FightAttackTypeInfo StrongPunch = new FightAttackTypeInfo();

        public string GetHitAnimPath;
        public string GameOverAnimPath;
        public string KnockedOutAnimPath;

        public FightSoundInfo SoundInfo = new FightSoundInfo();
    }
}