using UnityEngine;
using DDEngine.Utils.FSM;
using Cysharp.Threading.Tasks;

namespace DDEngine.Minigame.Fight
{
    public class FightPlayerKnockedOutState : IState
    {
        private SpriteAnimatorController knockedOutAnim;
        private string runScript;

        public FightPlayerKnockedOutState(FightPlayerController stateMachine, FightPlayerInfo opponentInfo, string scriptToRun)
        {
            knockedOutAnim = MinigameConstructUtils.InstantiateAndGet(stateMachine.animationPrefabObject, stateMachine.transform,
                opponentInfo.KnockedOutAnimPath, Vector2.zero);

            runScript = scriptToRun;
        }

        public void Enter()
        {
            knockedOutAnim.Enable();
        }

        public void Leave()
        {
        }

        public void ReceiveData(IStateMachine sender, object data)
        {
            if (!(data is FightEndIntent))
            {
                return;
            }

            UniTask.Action(async () =>
            {
                await GameManager.Instance.SetCurrentGUI(null);
                GameManager.Instance.LoadGadget(runScript);
            })();
        }

        public void Update()
        {
        }
    }
}