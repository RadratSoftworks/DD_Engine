using Cysharp.Threading.Tasks;
using System.Threading.Tasks;
using UnityEngine;

using DDEngine.Utils.FSM;

namespace DDEngine.Installer.States
{
    public class InstallState : IState
    {
        private InstallSceneController stateMachine;

        public InstallState(InstallSceneController stateMachine)
        {
            this.stateMachine = stateMachine;
        }

        public void Enter()
        {
            stateMachine.installingButton.SetActive(true);

#if FULL_GAME_IN_RESOURCES
            ReceiveData(null, null);
#endif
        }

        public void Leave()
        {
            stateMachine.installingButton.SetActive(false);
        }

        public void ReceiveData(IStateMachine sender, object data)
        {
#if !FULL_GAME_IN_RESOURCES
            if (data is NGageInstallPathIntent)
            {
#endif
                string storagePath = Application.persistentDataPath;

#if !FULL_GAME_IN_RESOURCES
                _ = Task.Run(async () =>
#else
                UniTask.Action(async () =>
#endif
                {
#if FULL_GAME_IN_RESOURCES
                    int errorCode = await GameDataInstaller.Install(null, storagePath);
#else
                    int errorCode = await GameDataInstaller.Install((data as NGageInstallPathIntent).Path, storagePath);
#endif

                    stateMachine.Transition(InstallSceneState.Done, errorCode);
                })
#if !FULL_GAME_IN_RESOURCES
                    .ContinueWith(async (task) =>
                    {
                        await UniTask.SwitchToMainThread();

                        if (task.IsFaulted)
                        {
                            stateMachine.Transition(InstallSceneState.Done, task.Exception.Message);
                        }
                    })
                    .ConfigureAwait(false);
#else
                ();
#endif
#if !FULL_GAME_IN_RESOURCES
            }
#endif
        }

        public void Update()
        {
        }
    }
}
