using System;
using System.Collections.Generic;
using UnityEngine;

public class FightOpponentController : StateMachine<FighterState>
{
    public GameObject animationPrefabObject;
    public GameObject movingHands;

    public FightPlayerController directOpponent;

    [Range(0, 100)]
    public int percentageOfSuccessDodging = 80;

    public float confusedStateDuration = 0.3f;

    public float blockAttackStateDuration = 0.2f;

    public float attackingInterval = 4.0f;

    public int damageDealt = 10;

    [HideInInspector]
    public SpriteAnimatorController headAnim;

    [HideInInspector]
    public SpriteAnimatorController normalLeftHand;

    [HideInInspector]
    public SpriteAnimatorController normalRightHand;

    public void Setup(FightOpponentInfo opponentInfo, string scriptToRun)
    {
        transform.localPosition = GameUtils.ToUnityCoordinates(opponentInfo.Position);

        headAnim = MinigameConstructUtils.InstantiateAndGet(animationPrefabObject, transform,
            opponentInfo.HeadAnimPath, Vector2.zero, FightOpponentConstants.OpponentHeadDepth,
            deactiveByDefault: false, allowLoop: true);

        normalLeftHand = MinigameConstructUtils.InstantiateAndGet(animationPrefabObject, movingHands.transform,
            opponentInfo.LeftHandAnimPath, Vector2.zero, FightOpponentConstants.OpponentHandDepth,
            deactiveByDefault: false, allowLoop: true);

        normalRightHand = MinigameConstructUtils.InstantiateAndGet(animationPrefabObject, movingHands.transform,
            opponentInfo.RightHandAnimPath, Vector2.zero, FightOpponentConstants.OpponentHandDepth,
            deactiveByDefault: false, allowLoop: true);

        base.AddState(FighterState.Idle, new FightOpponentIdleState(this, opponentInfo));
        base.AddState(FighterState.TakingDamage, new FightOpponentTakingDamageState(this, opponentInfo));
        base.AddState(FighterState.TotallyUndefended, new FightOpponentTotallyUndefendedState(this, opponentInfo));
        base.AddState(FighterState.Dodging, new FightOpponentDodgingState(this, opponentInfo));
        base.AddState(FighterState.PrepareAttacking, new FightOpponentPrepareAttackState(this, opponentInfo));
        base.AddState(FighterState.Attacking, new FightOpponentAttackingState(this, opponentInfo));
        base.AddState(FighterState.KnockedOut, new FightOpponentKnockedOutState(this, opponentInfo, scriptToRun));
    }

    protected override FighterState GetInitialState()
    {
        return FighterState.Idle;
    }
}
