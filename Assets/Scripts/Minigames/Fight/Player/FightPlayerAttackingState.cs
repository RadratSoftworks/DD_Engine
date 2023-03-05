using UnityEngine;

using DDEngine.Utils;
using DDEngine.Utils.FSM;

namespace DDEngine.Minigame.Fight
{
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

        private float timeCountTriggerDamage = -1.0f;
        private float timeTriggerDamage;
        private float timeTriggerIntent;

        private float timeTriggerJabDamage;
        private float timeTriggerPunchDamage;
        private float timeTriggerStrongPunchDamage;
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

            timeTriggerJabDamage = GameUtils.GetDurationFromFramesInSeconds(playerInfo.Jab.HitTimes);
            timeTriggerPunchDamage = GameUtils.GetDurationFromFramesInSeconds(playerInfo.Punch.HitTimes);
            timeTriggerStrongPunchDamage = GameUtils.GetDurationFromFramesInSeconds(playerInfo.StrongPunch.HitTimes);
        }

        private FightDirection AttackDirection => (currentAnim == jabAnim) ? FightDirection.Right : FightDirection.Left;

        private void UpdateActivePunchType()
        {
            switch (punchType)
            {
                case FightPunchType.Jab:
                    currentAnim = jabAnim;
                    currentDamageCount = jabDamageCount;
                    timeTriggerDamage = timeTriggerJabDamage;

                    break;

                case FightPunchType.Punch:
                    currentAnim = punchAnim;
                    currentDamageCount = punchDamageCount;
                    timeTriggerDamage = timeTriggerPunchDamage;

                    break;

                case FightPunchType.StrongPunch:
                    currentAnim = strongPunchAnim;
                    currentDamageCount = strongPunchDamageCount;
                    timeTriggerDamage = timeTriggerStrongPunchDamage;

                    break;

                default:
                    break;
            }

            timeTriggerIntent = stateMachine.timeTriggerIntent;
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
            timeCountTriggerDamage = 0.0f;
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
            if (timeCountTriggerDamage < 0.0f)
            {
                if (animationDone && (attackResult != FightAttackResult.None))
                {
                    if (attackResult == FightAttackResult.KnockedOut)
                    {
                        stateMachine.Transition(FighterState.Idle, new FighterStopAttackingIntent(FighterStopAttackingReason.OpponentKnockedOut));
                    }
                    else
                    {
                        stateMachine.Transition(FighterState.Idle);
                    }
                }

                return;
            }
            
            if ((timeTriggerIntent > 0.0f) && (timeCountTriggerDamage >= timeTriggerIntent))
            {
                stateMachine.directOpponent.GiveDataFrom(stateMachine, new FightAttackIntent()
                {
                    Direction = AttackDirection
                });

                timeTriggerIntent = -1.0f;
            }

            if ((timeTriggerDamage > 0.0f) && (timeCountTriggerDamage >= timeTriggerDamage))
            {
                stateMachine.directOpponent.GiveDataFrom(stateMachine, new FightDamage()
                {
                    Direction = AttackDirection,
                    DamagePoint = currentDamageCount
                });

                timeCountTriggerDamage = -1.0f;
            }

            if (timeCountTriggerDamage >= 0.0f)
            {
                timeCountTriggerDamage += Time.deltaTime;
            }
        }
    }
}