using System;
using System.Collections.Generic;
using UnityEngine;

public class FightOpponentDodgingState : FightOpponentBlockStateBase
{
    private SpriteAnimatorController bodyBlockAnim;

    public FightOpponentDodgingState(FightOpponentController stateMachine, FightOpponentInfo opponentInfo) 
        : base(stateMachine, opponentInfo, FighterState.Idle)
    {

        bodyBlockAnim = MinigameConstructUtils.InstantiateAndGet(stateMachine.animationPrefabObject, stateMachine.transform,
            opponentInfo.BodyBlockAnimPath, Vector2.zero, FightOpponentConstants.OpponentBodyDepth);
    }

    public override void Enter()
    {
        bodyBlockAnim.Enable();
        base.Enter();
    }

    public override void Leave()
    {
        bodyBlockAnim.Disable();
        base.Leave();
    }
}
