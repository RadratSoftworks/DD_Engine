using System;
using System.Collections.Generic;
using UnityEngine;

public class FightPlayerTakingDamageState : IState
{
    private FightPlayerController stateMachine;
    private SpriteAnimatorController damageAnimation;
    private SpriteAnimatorController deadAnimation;
    private FighterHealthController playerHealthController;

    public FightPlayerTakingDamageState(FightPlayerController stateMachine, FightPlayerInfo playerInfo)
    {
        this.stateMachine = stateMachine;

        playerHealthController = stateMachine.GetComponent<FighterHealthController>();
        damageAnimation = MinigameConstructUtils.InstantiateAndGet(stateMachine.animationPrefabObject,
            stateMachine.transform, playerInfo.GetHitAnimPath, Vector2.zero,
            FightPlayerConstants.PlayerSpriteDepth, allowLoop: false);

        deadAnimation = MinigameConstructUtils.InstantiateAndGet(stateMachine.animationPrefabObject,
            stateMachine.transform, playerInfo.GameOverAnimPath, Vector2.zero,
            FightPlayerConstants.PlayerSpriteDepth, allowLoop: false);

        damageAnimation.Done += OnAnimationDone;
        deadAnimation.Done += OnAnimationDone;
    }

    private void OnAnimationDone(SpriteAnimatorController target)
    {
        if (playerHealthController.IsDead)
        {
            stateMachine.Transition(FighterState.KnockedOut);
        }
        else
        {
            stateMachine.Transition(FighterState.Idle);
        }
    }

    public void Enter()
    {
    }

    public void Leave()
    {
        damageAnimation.Disable();
        deadAnimation.Disable();
    }

    public void ReceiveData(IStateMachine sender, object data)
    {
        if (data is int)
        {
            playerHealthController.TakeDamage((int)data);

            if (playerHealthController.IsDead)
            {
                deadAnimation.Enable();
                sender.GiveData(FightAttackResult.KnockedOut);
            } else
            {
                damageAnimation.Enable();
                sender.GiveData(FightAttackResult.DealtDamage);
            }
        }
    }

    public void Update()
    {
    }
}