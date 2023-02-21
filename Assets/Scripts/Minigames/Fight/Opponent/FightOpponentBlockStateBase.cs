using System;
using System.Collections;
using UnityEngine;

public class FightOpponentBlockStateBase : IState
{
    protected FightOpponentController stateMachine;

    private SpriteAnimatorController leftHandBlockAnim;
    private SpriteAnimatorController rightHandBlockAnim;

    private FightDirection direction;
    private FighterState targetState;

    private FightAttackIntent lastestFightIntent;
    private IEnumerator blockDoneCoroutine;

    public FightOpponentBlockStateBase(FightOpponentController stateMachine, FightOpponentInfo opponentInfo, FighterState targetState)
    {
        this.stateMachine = stateMachine;
        this.targetState = targetState;

        leftHandBlockAnim = MinigameConstructUtils.InstantiateAndGet(stateMachine.animationPrefabObject, stateMachine.movingHands.transform,
            opponentInfo.LeftHandBlockAnimPath, Vector2.zero, FightOpponentConstants.OpponentHandDepth, true);

        rightHandBlockAnim = MinigameConstructUtils.InstantiateAndGet(stateMachine.animationPrefabObject, stateMachine.movingHands.transform,
            opponentInfo.RightHandAnimBlockPath, Vector2.zero, FightOpponentConstants.OpponentHandDepth, true);
    }

    private IEnumerator BlockDoneCoroutine()
    {
        var waitForState = new WaitForSeconds(stateMachine.blockAttackStateDuration);
        yield return waitForState;

        stateMachine.Transition(targetState, lastestFightIntent);
    }

    public virtual void Enter()
    {
        stateMachine.headAnim.Enable();
    }

    public virtual void Leave()
    {
        stateMachine.headAnim.Disable();

        stateMachine.StopCoroutine(blockDoneCoroutine);
        UpdateFightBlockAnimation(false);
    }

    protected virtual void UpdateFightBlockAnimation(bool blocking)
    {
        if (direction == FightDirection.Left)
        {
            stateMachine.normalRightHand.SetEnableState(blocking);
            stateMachine.normalLeftHand.SetEnableState(!blocking);
            leftHandBlockAnim.SetEnableState(blocking);
        }
        else
        {
            stateMachine.normalLeftHand.SetEnableState(blocking);
            stateMachine.normalRightHand.SetEnableState(!blocking);
            rightHandBlockAnim.SetEnableState(blocking);
        }
    }

    public void ReceiveData(IStateMachine sender, object data)
    {
        if (data is FightDirection)
        {
            direction = (FightDirection)data;

            UpdateFightBlockAnimation(true);

            blockDoneCoroutine = BlockDoneCoroutine();
            stateMachine.StartCoroutine(blockDoneCoroutine);
        }
        else if (data is FightAttackIntent)
        {
            UpdateFightBlockAnimation(false);

            lastestFightIntent = data as FightAttackIntent;
            direction = lastestFightIntent.Direction;

            sender.GiveData(FightAttackResult.Miss);

            UpdateFightBlockAnimation(true);
        }
        else if (data is FightDamage)
        {
            lastestFightIntent = null;
        }
    }

    public void Update()
    {
    }
}
