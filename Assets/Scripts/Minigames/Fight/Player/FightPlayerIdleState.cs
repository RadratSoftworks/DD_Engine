using UnityEngine;

public class FightPlayerIdleState : IState
{
    private FightPlayerController stateMachine;
    private SpriteAnimatorController idleAnimation;
    private FightPunchType requestedPunchType;
    private bool stopAttacking = false;

    public FightPlayerIdleState(FightPlayerController stateMachine, FightPlayerInfo playerInfo)
    {
        this.stateMachine = stateMachine;

        idleAnimation = MinigameConstructUtils.InstantiateAndGet(stateMachine.animationPrefabObject,
            stateMachine.transform, playerInfo.IdleAnimPath, Vector2.zero,
            FightPlayerConstants.PlayerSpriteDepth);
    }

    public void Enter()
    {
        requestedPunchType = FightPunchType.None;
        idleAnimation.Enable();
    }

    public void Leave()
    {
        idleAnimation.Disable();
    }

    public void ReceiveData(IStateMachine sender, object data)
    {
        if (data is FightPunchType)
        {
            if (!stopAttacking)
            {
                requestedPunchType = (FightPunchType)data;
            }
        }
        else if (data is FightDamage)
        {
            stateMachine.Transition(FighterState.TakingDamage, (data as FightDamage).DamagePoint, sender);
        }
        else if (data is FighterStopAttackingIntent)
        {
            stopAttacking = true;
        }
    }

    public void Update()
    {
        if (requestedPunchType != FightPunchType.None)
        {
            if (requestedPunchType == FightPunchType.Dodging)
            {
                stateMachine.Transition(FighterState.Dodging);
            } else
            {
                stateMachine.Transition(FighterState.Attacking, requestedPunchType);

            }
        }
    }
}
