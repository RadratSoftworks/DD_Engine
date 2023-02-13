using System;
using System.Collections.Generic;
using UnityEngine;

public class FightOpponentIdleState : IState
{
    private SpriteAnimatorController bodyIdleAnim;
    private FightOpponentController stateMachine;

    private float timePassed = 0.0f;
    private float previousTimePoint = -1.0f;

    private bool stopAttacking = false;

    public FightOpponentIdleState(FightOpponentController stateMachine, FightOpponentInfo opponentInfo)
    {
        this.stateMachine = stateMachine;

        bodyIdleAnim = MinigameConstructUtils.InstantiateAndGet(stateMachine.animationPrefabObject, stateMachine.transform,
            opponentInfo.BodyIdleAnimPath, Vector2.zero, FightOpponentConstants.OpponentBodyDepth, true);
    }

    public void Enter()
    {
        bodyIdleAnim.Enable();

        stateMachine.normalLeftHand.Enable();
        stateMachine.normalRightHand.Enable();

        stateMachine.headAnim.Enable();
    }

    public void Leave()
    {
        bodyIdleAnim.Disable();

        stateMachine.normalLeftHand.Disable();
        stateMachine.normalRightHand.Disable();

        stateMachine.headAnim.Disable();
    }

    private void HandleFightDamage(IStateMachine sender, FightDamage directedToDamage)
    {
        stateMachine.Transition(FighterState.TakingDamage, directedToDamage.DamagePoint, sender);
    }

    private void HandlePossibleRealizeAttackIntent(IStateMachine sender, FightAttackIntent intent)
    {
        bool canDodge = GameUtils.MakeChoice(stateMachine.percentageOfSuccessDodging);
        if (canDodge)
        {
            sender.GiveDataFrom(sender, FightAttackResult.Miss);
            stateMachine.Transition(FighterState.Dodging, intent.Direction);
        }
    }

    public void ReceiveData(IStateMachine sender, object data)
    {
        if (data is FightDamage)
        {
            HandleFightDamage(sender, data as FightDamage);
        }
        else if (data is FightAttackIntent)
        {
            HandlePossibleRealizeAttackIntent(sender, data as FightAttackIntent);
        }
        else if (data is FighterStopAttackingIntent)
        {
            stopAttacking = true;
        }
    }

    public void Update()
    {
        if (stopAttacking)
        {
            return;
        }

        // Need to calculate and trigger the attack in duration of real time
        // Avoiding continious dodge not triggering this
        if (previousTimePoint < 0)
        {
            previousTimePoint = Time.time;
        }

        timePassed += Time.time - previousTimePoint;
        previousTimePoint = Time.time;

        if (timePassed > stateMachine.attackingInterval)
        {
            stateMachine.Transition(FighterState.PrepareAttacking, FightDirection.Right);
            timePassed = 0.0f;
        }
    }
}
