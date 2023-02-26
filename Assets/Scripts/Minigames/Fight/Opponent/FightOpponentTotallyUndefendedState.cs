using System.Collections;
using UnityEngine;
using DDEngine.Utils.FSM;

namespace DDEngine.Minigame.Fight
{
    public class FightOpponentTotallyUndefendedState : IState
    {
        private SpriteAnimatorController headConfusedAnim;
        private SpriteAnimatorController headConfusedFxAnim;
        private SpriteAnimatorController bodyConfusedAnim;
        private bool stopped = true;

        public FightOpponentController stateMachine;
        private IEnumerator doneConfusedCoroutine;

        public FightOpponentTotallyUndefendedState(FightOpponentController stateMachine, FightOpponentInfo opponentInfo)
        {
            this.stateMachine = stateMachine;

            bodyConfusedAnim = MinigameConstructUtils.InstantiateAndGet(stateMachine.animationPrefabObject, stateMachine.transform,
                opponentInfo.BodyConfusedAnimPath, Vector2.zero, FightOpponentConstants.OpponentBodyDepth, allowLoop: true);

            headConfusedAnim = MinigameConstructUtils.InstantiateAndGet(stateMachine.animationPrefabObject, stateMachine.transform,
                opponentInfo.HeadConfusedAnimPath, Vector2.zero, FightOpponentConstants.OpponentHeadDepth, allowLoop: true);

            headConfusedFxAnim = MinigameConstructUtils.InstantiateAndGet(stateMachine.animationPrefabObject, stateMachine.transform,
                opponentInfo.HeadConfusedAnimEffectPath, Vector2.zero, FightOpponentConstants.OpponentHeadFxDepth, allowLoop: true);
        }

        private IEnumerator DoneConfusedCoroutine()
        {
            yield return new WaitForSeconds(stateMachine.confusedStateDuration);

            if (!stopped)
            {
                stateMachine.Transition(FighterState.Idle);
            }
        }

        public void Enter()
        {
            headConfusedAnim.Enable();
            headConfusedFxAnim.Enable();
            bodyConfusedAnim.Enable();

            stateMachine.normalLeftHand.Enable();
            stateMachine.normalRightHand.Enable();

            stopped = false;

            doneConfusedCoroutine = DoneConfusedCoroutine();
            stateMachine.StartCoroutine(doneConfusedCoroutine);
        }

        public void Update()
        {
        }

        public void Leave()
        {
            stopped = true;

            headConfusedAnim.Disable();
            headConfusedFxAnim.Disable();
            bodyConfusedAnim.Disable();

            stateMachine.normalLeftHand.Disable();
            stateMachine.normalRightHand.Disable();

            stateMachine.StopCoroutine(doneConfusedCoroutine);
        }

        public void ReceiveData(IStateMachine sender, object data)
        {
            if (data is FightDamage)
            {
                stateMachine.Transition(FighterState.TakingDamage, (data as FightDamage).DamagePoint, sender);
            }
        }
    }
}