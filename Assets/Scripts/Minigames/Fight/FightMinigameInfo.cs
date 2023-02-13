using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class FightMinigameInfo
{
    public string BackgroundSoundPath;
    public string BackgroundPicture;

    public FightPlayerInfo PlayerInfo = new FightPlayerInfo();
    public FightOpponentInfo OpponentInfo = new FightOpponentInfo();

    public string FileWonScript;
    public string FileLoseScript;
};