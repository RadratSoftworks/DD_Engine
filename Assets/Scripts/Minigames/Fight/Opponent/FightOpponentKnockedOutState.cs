using System;
using UnityEngine;

public class FightOpponentKnockedOutState : IState
{
    private SpriteAnimatorController knockedOutAnim;
    private string scriptToRun;

    public FightOpponentKnockedOutState(FightOpponentController stateMachine, FightOpponentInfo opponentInfo, string scriptToRun)
    {
        knockedOutAnim = MinigameConstructUtils.InstantiateAndGet(stateMachine.animationPrefabObject, stateMachine.transform,
            opponentInfo.KnockedOutAnimPath, Vector2.zero, FightOpponentConstants.OpponentFullSpriteDepth);

        this.scriptToRun = scriptToRun;
    }

    public void Enter()
    {
        knockedOutAnim.Enable();
    }

    public void Leave()
    {
    }

    public void ReceiveData(IStateMachine sender, object data)
    {
        if (!(data is FightEndIntent))
        {
            return;
        }

        GameManager.Instance.SetCurrentGUI(null);
        GameManager.Instance.LoadGadget(scriptToRun);
    }

    public void Update()
    {
    }
}
