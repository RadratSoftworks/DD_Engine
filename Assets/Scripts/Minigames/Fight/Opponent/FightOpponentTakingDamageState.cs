using UnityEngine;
using DDEngine.Utils.FSM;

namespace DDEngine.Minigame.Fight
{
    public class FightOpponentTakingDamageState : IState
    {
        private SpriteAnimatorController gettingHitAnim;
        private SpriteAnimatorController deadAnim;

        private FightOpponentController stateMachine;
        private FighterHealthController healthController;

        public FightOpponentTakingDamageState(FightOpponentController stateMachine, FightOpponentInfo opponentInfo)
        {
            this.stateMachine = stateMachine;
            this.healthController = stateMachine.GetComponent<FighterHealthController>();

            gettingHitAnim = MinigameConstructUtils.InstantiateAndGet(stateMachine.animationPrefabObject, stateMachine.transform,
                opponentInfo.GettingHitAnimPath, Vector2.zero, FightOpponentConstants.OpponentFullSpriteDepth, allowLoop: false);

            deadAnim = MinigameConstructUtils.InstantiateAndGet(stateMachine.animationPrefabObject, stateMachine.transform,
                opponentInfo.GameOverAnimPath, Vector2.zero, FightOpponentConstants.OpponentFullSpriteDepth, allowLoop: false);

            if (gettingHitAnim != null)
            {
                gettingHitAnim.Done += OnAnimationDone;
            }

            if (deadAnim != null)
            {
                deadAnim.Done += OnAnimationDone;
            }
        }

        private void OnAnimationDone(SpriteAnimatorController controller)
        {
            if (healthController.IsDead)
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
            deadAnim.Disable();
            gettingHitAnim.Disable();
        }

        private void ToggleAnimation(bool enable)
        {
            bool dead = healthController.IsDead;

            if (enable)
            {
                deadAnim.SetEnableState(dead);
                gettingHitAnim.SetEnableState(!dead);
            }
            else
            {
                deadAnim.SetEnableState(!dead);
                gettingHitAnim.SetEnableState(dead);
            }
        }

        public void ReceiveData(IStateMachine sender, object data)
        {
            if (data is int)
            {
                healthController.TakeDamage((int)data);

                if (healthController.IsDead)
                {
                    sender.GiveData(FightAttackResult.KnockedOut);
                }
                else
                {
                    sender.GiveData(FightAttackResult.DealtDamage);
                }

                ToggleAnimation(true);
            }
            else if (data is FightDamage)
            {
                // If the player keep spamming and hit this state, just report a miss
                sender.GiveData(FightAttackResult.Miss);
            }
        }

        public void Update()
        {
        }
    }
}