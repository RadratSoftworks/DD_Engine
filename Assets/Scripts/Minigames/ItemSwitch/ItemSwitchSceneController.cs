using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ItemSwitchSceneController : MonoBehaviour
{
    [SerializeField]
    private SpriteRenderer backgroundSpriteRenderer;

    [SerializeField]
    private SpriteAnimatorController backgroundEffectAnimController;

    [SerializeField]
    private ItemSwitchHandController leftHandController;

    [SerializeField]
    private ItemSwitchHandController rightHandController;

    [SerializeField]
    private ItemSwitchStressMachineController stressMachineController;

    [SerializeField]
    private ItemSwitchTimerController timerController;

    private PlayerInput controlInput;

    private bool gameFinished;
    private bool gameWon;

    private string wonScript;
    private string lostScript;

    private void Awake()
    {
        controlInput = GetComponent<PlayerInput>();
    }

    private void SetupTimerLogic()
    {
        timerController.ReadyFinished += () =>
        {
            stressMachineController.KickOff();
            timerController.StartCountdown();
        };

        timerController.CountdownFinished += () =>
        {
            leftHandController.Shutdown();
            rightHandController.Shutdown();

            timerController.SetGameStatus(stressMachineController.Passed);
            stressMachineController.Freeze();

            gameFinished = true;
            gameWon = stressMachineController.Passed;
        };
    }

    private void SetupStressLogic()
    {
        stressMachineController.StressStatusEntered += stressStatus =>
        {
            if (stressStatus == ItemSwitchStressStatus.Shutdown)
            {
                timerController.SetGameStatus(false);
                gameFinished = true;
                gameWon = false;
            }
        };
    }

    public void Setup(ItemSwitchMinigameInfo minigameInfo)
    {
        gameFinished = false;
        wonScript = minigameInfo.WonScript;
        lostScript = minigameInfo.LoseScript;

        backgroundSpriteRenderer.sprite = SpriteManager.Instance.Load(ResourceManager.Instance.GeneralResources,
            minigameInfo.BackgroundInfo.ImagePath);

        backgroundEffectAnimController.Setup(minigameInfo.BackgroundInfo.EffectPosition, SpriteAnimatorController.SortOrderNotSet,
            minigameInfo.BackgroundInfo.EffectAnimationPath);

        // TODO: Actually use win percentage?
        stressMachineController.Setup(minigameInfo.StressMachineInfo,
            Math.Clamp(minigameInfo.ForcePercentage / 100.0f, 0.0f, 1.0f),
            Math.Clamp(minigameInfo.MaxSpeedPercentage / 100.0f, 0.0f, 1.0f));

        leftHandController.Setup(stressMachineController, minigameInfo.LeftHandInfo);
        rightHandController.Setup(stressMachineController, minigameInfo.RightHandInfo);

        timerController.Setup(stressMachineController, minigameInfo.TimerInfo);

        SetupStressLogic();
        SetupTimerLogic();
    }

    private void OnConfirmPressed()
    {
        if (gameFinished)
        {
            controlInput.enabled = false;
            GameManager.Instance.LoadGadget(gameWon ? wonScript : lostScript);
        }
    }
}
