using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XR;

public class FightPlayerAttackingState : IState
{
    private SpriteAnimatorController jabAnim;
    private SpriteAnimatorController punchAnim;
    private SpriteAnimatorController strongPunchAnim;

    private int jabDamageCount;
    private int punchDamageCount;
    private int strongPunchDamageCount;

    private SpriteAnimatorController currentAnim;
    private int currentDamageCount;

    private FightPunchType punchType;
    private FightAttackResult attackResult;

    private FightPlayerController stateMachine;

    private int framesCountTriggerDamage = -1;
    private int frameTriggerDamage;
    private int frameTriggerIntent;

    private int frameTriggerJabDamage;
    private int frameTriggerPunchDamage;
    private int frameTriggerStrongPunchDamage;
    private bool animationDone = false;

    private FightSoundMakerController soundMakerController;

    public FightPlayerAttackingState(FightPlayerController playerController, FightPlayerInfo playerInfo)
    {
        this.stateMachine = playerController;

        soundMakerController = playerController.GetComponent<FightSoundMakerController>();

        jabAnim = MinigameConstructUtils.InstantiateAndGet(stateMachine.animationPrefabObject,
            stateMachine.transform, playerInfo.Jab.AnimationPath, Vector2.zero,
            FightPlayerConstants.PlayerSpriteDepth, allowLoop: false);

        punchAnim = MinigameConstructUtils.InstantiateAndGet(stateMachine.animationPrefabObject,
            stateMachine.transform, playerInfo.Punch.AnimationPath, Vector2.zero,
            FightPlayerConstants.PlayerSpriteDepth, allowLoop: false);

        strongPunchAnim = MinigameConstructUtils.InstantiateAndGet(stateMachine.animationPrefabObject,
            stateMachine.transform, playerInfo.StrongPunch.AnimationPath, Vector2.zero,
            FightPlayerConstants.PlayerSpriteDepth, allowLoop: false);

        jabAnim.Done += OnAttackAnimationDone;
        punchAnim.Done += OnAttackAnimationDone;
        strongPunchAnim.Done += OnAttackAnimationDone;

        jabDamageCount = playerInfo.Jab.HitPower;
        punchDamageCount = playerInfo.Punch.HitPower;
        strongPunchDamageCount = playerInfo.StrongPunch.HitPower;

        frameTriggerJabDamage = playerInfo.Jab.HitTimes;
        frameTriggerPunchDamage = playerInfo.Punch.HitTimes;
        frameTriggerStrongPunchDamage = playerInfo.StrongPunch.HitTimes;
    }

    private FightDirection AttackDirection => (currentAnim == jabAnim) ? FightDirection.Right : FightDirection.Left;

    private void UpdateActivePunchType()
    {
        switch (punchType)
        {
            case FightPunchType.Jab:
                currentAnim = jabAnim;
                currentDamageCount = jabDamageCount;
                frameTriggerDamage = frameTriggerJabDamage;

                break;

            case FightPunchType.Punch:
                currentAnim = punchAnim;
                currentDamageCount = punchDamageCount;
                frameTriggerDamage = frameTriggerPunchDamage;

                break;

            case FightPunchType.StrongPunch:
                currentAnim = strongPunchAnim;
                currentDamageCount = strongPunchDamageCount;
                frameTriggerDamage = frameTriggerStrongPunchDamage;

                break;

            default:
                break;
        }

        frameTriggerDamage = GameManager.Instance.GetRealFrames(frameTriggerDamage);
        frameTriggerIntent = GameManager.Instance.GetRealFrames(stateMachine.frameTriggerIntent);

        currentAnim.Enable();
    }

    private void OnAttackAnimationDone(SpriteAnimatorController controller)
    {
        animationDone = true;
    }

    public void Enter()
    {
        punchType = FightPunchType.None;
        attackResult = FightAttackResult.None;
        framesCountTriggerDamage = 0;
        animationDone = false;
    }

    public void Leave()
    {
        if (currentAnim != null)
        {
            currentAnim.Disable();
        }
    }

    public void ReceiveData(IStateMachine sender, object data)
    {
        if (data is FightPunchType)
        {
            if (punchType == FightPunchType.None)
            {
                punchType = (FightPunchType)data;
                UpdateActivePunchType();
            }
        }
        else if (data is FightDamage)
        {
            stateMachine.Transition(FighterState.TakingDamage, (data as FightDamage).DamagePoint, sender);
        }
        else if (data is FightAttackResult)
        {
            attackResult = (FightAttackResult)data;
            soundMakerController.PlayBasedOnAttackResult(attackResult, punchType);
        }
    }

    public void Update()
    {
        if (framesCountTriggerDamage < 0)
        {
            if (animationDone && (attackResult != FightAttackResult.None))
            {
                if (attackResult == FightAttackResult.KnockedOut)
                {
                    stateMachine.Transition(FighterState.Idle, new FighterStopAttackingIntent(FighterStopAttackingReason.OpponentKnockedOut));
                } else
                {
                    stateMachine.Transition(FighterState.Idle);
                }
            }

            return;
        }

        framesCountTriggerDamage++;

        if (framesCountTriggerDamage == frameTriggerIntent)
        {
            stateMachine.directOpponent.GiveDataFrom(stateMachine, new FightAttackIntent()
            {
                Direction = AttackDirection
            });
        }

        if (framesCountTriggerDamage == frameTriggerDamage)
        {
            stateMachine.directOpponent.GiveDataFrom(stateMachine, new FightDamage()
            {
                Direction = AttackDirection,
                DamagePoint = currentDamageCount
            });

            framesCountTriggerDamage = -1;
        }
    }
}
