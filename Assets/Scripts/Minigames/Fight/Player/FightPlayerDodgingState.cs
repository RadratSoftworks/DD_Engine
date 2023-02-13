using System;
using System.Collections.Generic;
using UnityEngine;

public class FightPlayerDodgingState : IState
{
    private FightPlayerController stateMachine;
    private SpriteAnimatorController dodgingAnimation;
    private SpriteAnimatorController dodgingRevertAnimation;

    public FightPlayerDodgingState(FightPlayerController stateMachine, FightPlayerInfo playerInfo)
    {
        this.stateMachine = stateMachine;

        dodgingAnimation = MinigameConstructUtils.InstantiateAndGet(stateMachine.animationPrefabObject,
            stateMachine.transform, playerInfo.DodgeAnimPath, Vector2.zero,
            FightPlayerConstants.PlayerSpriteDepth, allowLoop: false);

        dodgingRevertAnimation = MinigameConstructUtils.InstantiateAndGet(stateMachine.animationPrefabObject,
            stateMachine.transform, playerInfo.DodgeRevertAnimPath, Vector2.zero, allowLoop: false);

        if (dodgingAnimation != null)
        {
            dodgingAnimation.Done += OnAnimationComplete;
        }

        if (dodgingRevertAnimation != null)
        {
            dodgingRevertAnimation.Done += OnAnimationComplete;
        }
    }

    private void OnAnimationComplete(SpriteAnimatorController controller)
    {
        if (controller == dodgingRevertAnimation) {
            stateMachine.Transition(FighterState.Idle);
        }
        else if (controller == dodgingAnimation)
        {
            dodgingAnimation.Disable();
            dodgingRevertAnimation.Enable();
        }
    }

    public void Enter()
    {
        dodgingAnimation.Enable();
    }

    public void Leave()
    {
        dodgingRevertAnimation.Disable();
        dodgingAnimation.Disable();
    }

    public void ReceiveData(IStateMachine sender, object data)
    {
        if (data is FightDamage)
        {
            sender.GiveData(FightAttackResult.Miss);
        }
    }

    public void Update()
    {
    }
}
