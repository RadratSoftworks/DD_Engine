using System;
using System.Collections;
using UnityEngine;

public class ItemSwitchTimerController : MonoBehaviour
{
    private SpriteAnimatorController animController;
    private ItemSwitchTimerInfo timerInfo;

    public event Action CountdownFinished;
    public event Action ReadyFinished;

    private void Awake()
    {
        animController = GetComponent<SpriteAnimatorController>();
    }

    private void OnCountdownAnimationDone(SpriteAnimatorController controller)
    {
        animController.Done -= OnCountdownAnimationDone;
        CountdownFinished?.Invoke();
    }

    private IEnumerator KickOffIndicatorCoroutine()
    {
        yield return new WaitForSeconds(3);
        ReadyFinished?.Invoke();
    }

    public void Setup(ItemSwitchStressMachineController stressMachine, ItemSwitchTimerInfo timerInfo)
    {
        this.timerInfo = timerInfo;

        animController.Setup(timerInfo.Position, SpriteAnimatorController.SortOrderNotSet, timerInfo.ReadyAnimationPath, allowLoop: false);
        StartCoroutine(KickOffIndicatorCoroutine());
    }

    public void StartCountdown()
    {
        animController.Done += OnCountdownAnimationDone;
        animController.Reload(timerInfo.Position, timerInfo.CountdownAnimationPath, allowLoop: false);
    }

    public void SetGameStatus(bool won)
    {
        animController.StopAnimating();
        animController.Reload(timerInfo.Position, won ? timerInfo.WonAnimationPath : timerInfo.LostAnimationPath, allowLoop: false);
    }
}
