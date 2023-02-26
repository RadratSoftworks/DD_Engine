using System;
using UnityEngine;

public class FightOpponentAttackingState : IState
{
    private SpriteAnimatorController punchAnim;
    private SpriteAnimatorController punchFxAnim;

    private FightOpponentController stateMachine;
    private int punchFrame;
    private int currentFrame;

    private int pendingAnimation = 0;

    private FightSoundMakerController soundMakerController;

    public FightOpponentAttackingState(FightOpponentController stateMachine, FightOpponentInfo opponentInfo)
    {
        this.stateMachine = stateMachine;

        soundMakerController = stateMachine.GetComponent<FightSoundMakerController>();

        punchAnim = MinigameConstructUtils.InstantiateAndGet(stateMachine.animationPrefabObject, stateMachine.transform,
            opponentInfo.PunchAnimPath, Vector2.zero, FightOpponentConstants.OpponentFullSpriteDepth, allowLoop: false);

        punchFxAnim = MinigameConstructUtils.InstantiateAndGet(stateMachine.animationPrefabObject, stateMachine.transform,
            opponentInfo.PunchEffectAnimPath, Vector2.zero, FightOpponentConstants.OpponentHandFxDepth, allowLoop: false);

        punchFrame = GameManager.Instance.GetRealFrames(opponentInfo.PunchHitTime);

        punchAnim.Done += OnAnimationDone;
        punchFxAnim.Done += OnAnimationDone;
    }

    private void OnAnimationDone(SpriteAnimatorController controller)
    {
        pendingAnimation--;
    }

    public void Enter()
    {
        currentFrame = 0;
        pendingAnimation = 2;

        stateMachine.movingHands.SetActive(false);

        punchAnim.Enable();
    }

    public void Leave()
    {
        punchFxAnim.Disable();
        punchAnim.Disable();

        stateMachine.movingHands.SetActive(true);
    }

    public void ReceiveData(IStateMachine sender, object data)
    {
        // Invicible in this state!
        if (data is FightAttackResult)
        {
            FightAttackResult attackResult = (FightAttackResult)data;
            soundMakerController.PlayBasedOnAttackResult(attackResult, FightPunchType.Punch);

            if (attackResult == FightAttackResult.Miss)
            {
                punchFxAnim.Enable();
            }
            else
            {
                if (attackResult == FightAttackResult.KnockedOut)
                {
                    stateMachine.Transition(FighterState.Idle, new FighterStopAttackingIntent(FighterStopAttackingReason.OpponentKnockedOut));
                } else
                {
                    stateMachine.Transition(FighterState.Idle);
                }
            }
        } else if (data is FightDamage)
        {
            // Other players are attacking while an attack animation is performed
            // Make this a miss
            sender.GiveData(FightAttackResult.Miss);
        }
    }

    public void Update()
    {
        if (currentFrame >= 0)
        {
            currentFrame++;
        }

        if (currentFrame >= punchFrame)
        {
            // Deal damage to the opponent
            stateMachine.directOpponent.GiveDataFrom(stateMachine, new FightDamage()
            {
                DamagePoint = stateMachine.damageDealt,
                Direction = FightDirection.Left
            });

            currentFrame = -1;
        }

        if ((currentFrame < 0) && (pendingAnimation == 0)) {
            stateMachine.Transition(FighterState.TotallyUndefended);
        }
    }
}
