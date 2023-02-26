using UnityEngine;
using UnityEngine.SceneManagement;

namespace DDEngine.Installer {
    public class ProceedButtonController : MonoBehaviour
    {
        private States.InstallDoneState installDoneState;
        private InstallSceneController stateMachine;

        public void Setup(InstallSceneController stateMachine, States.InstallDoneState installDoneState)
        {
            this.installDoneState = installDoneState;
            this.stateMachine = stateMachine;
        }

        public void OnClicked()
        {
            if (!this.installDoneState.EncounteredError)
            {
                SceneManager.LoadScene(Scenes.GameViewSceneIndex);
            } else
            {
                stateMachine.Transition(InstallSceneState.WaitForInstallPick);
            }
        }
    }
}
