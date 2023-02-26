using UnityEngine;

namespace DDEngine.Minigame.Fight
{
    public class FightOpponentInfo
    {
        public Vector2 Position;

        public string BodyIdleAnimPath;
        public string BodyBlockAnimPath;
        public string BodyPunchPrepAnimPath;
        public string BodyConfusedAnimPath;
        public string BodyGroggyAnimPath;

        public string HeadAnimPath;
        public string HeadConfusedAnimPath;
        public string HeadConfusedAnimEffectPath;
        public string LeftHandAnimPath;
        public string LeftHandBlockAnimPath;
        public string RightHandAnimPath;
        public string RightHandAnimBlockPath;

        public string PunchAnimPath;
        public string PunchEffectAnimPath;
        public string GettingHitAnimPath;
        public string KnockedOutAnimPath;
        public string GameOverAnimPath;
        public string GameOver2AnimPath;

        public int PunchHitTime = 0;

        public FightSoundInfo SoundInfo = new FightSoundInfo();
    }
}