using UnityEngine;
using DDEngine;

namespace DDEngine.Minigame.Fight
{
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
}
