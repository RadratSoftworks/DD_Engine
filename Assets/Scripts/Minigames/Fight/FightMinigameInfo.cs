namespace DDEngine.Minigame.Fight
{
    public class FightMinigameInfo
    {
        public string BackgroundSoundPath;
        public string BackgroundPicture;

        public FightPlayerInfo PlayerInfo = new FightPlayerInfo();
        public FightOpponentInfo OpponentInfo = new FightOpponentInfo();

        public string FileWonScript;
        public string FileLoseScript;
    };
}