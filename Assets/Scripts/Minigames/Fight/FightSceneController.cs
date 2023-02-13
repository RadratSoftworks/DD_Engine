using System;
using System.Collections.Generic;
using UnityEngine;

public class FightSceneController : MonoBehaviour
{
    [SerializeField]
    private FightPlayerController playerController;

    [SerializeField]
    private FightOpponentController opponentController;

    [SerializeField]
    private FightSoundMakerController playerSoundMakerController;

    [SerializeField]
    private FightSoundMakerController opponentSoundMakerController;

    [SerializeField]
    private SpriteRenderer backgroundSpriteRenderer;

    [SerializeField]
    private GUISoundController backgroundSoundController;

    public void Setup(FightMinigameInfo fightInfo)
    {
        playerController.Setup(fightInfo.PlayerInfo, fightInfo.FileLoseScript);
        playerSoundMakerController.Setup(fightInfo.PlayerInfo.SoundInfo);

        opponentController.Setup(fightInfo.OpponentInfo, fightInfo.FileWonScript);
        opponentSoundMakerController.Setup(fightInfo.OpponentInfo.SoundInfo);

        backgroundSpriteRenderer.sprite = SpriteManager.Instance.Load(ResourceManager.Instance.GeneralResources,
            fightInfo.BackgroundPicture);

        backgroundSoundController.Setup(fightInfo.BackgroundSoundPath, 0);
    }
}
