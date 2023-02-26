using UnityEngine;

namespace DDEngine.Minigame.Fight
{
    public class FightOpponentPrepareAttackState : FightOpponentBlockStateBase
    {
        private SpriteAnimatorController bodyPreparePunchAnim;

        public FightOpponentPrepareAttackState(FightOpponentController stateMachine, FightOpponentInfo opponentInfo)
            : base(stateMachine, opponentInfo, FighterState.Attacking)
        {
            bodyPreparePunchAnim = MinigameConstructUtils.InstantiateAndGet(stateMachine.animationPrefabObject, stateMachine.transform,
                opponentInfo.BodyPunchPrepAnimPath, Vector2.zero, FightOpponentConstants.OpponentBodyDepth, true, true);
        }

        public override void Enter()
        {
            bodyPreparePunchAnim.Enable();
            base.Enter();
        }

        public override void Leave()
        {
            bodyPreparePunchAnim.Disable();
            base.Leave();
        }
    }
}