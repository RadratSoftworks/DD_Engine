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
        }

        public void Leave()
        {
            stateMachine.installingButton.SetActive(false);
        }

        public void ReceiveData(IStateMachine sender, object data)
        {
            if (data is NGageInstallPathIntent)
            {
                string storagePath = Application.persistentDataPath;

                _ = Task.Run(async () =>
                {
                    int errorCode = await GameDataInstaller.Install((data as NGageInstallPathIntent).Path, storagePath);
                    await UniTask.SwitchToMainThread();

                    stateMachine.Transition(InstallSceneState.Done, errorCode);
                })
                    .ContinueWith(async (task) =>
                    {
                        await UniTask.SwitchToMainThread();

                        if (task.IsFaulted)
                        {
                            stateMachine.Transition(InstallSceneState.Done, task.Exception.Message);
                        }
                    })
                    .ConfigureAwait(false);
            }
        }

        public void Update()
        {
        }
    }
}
