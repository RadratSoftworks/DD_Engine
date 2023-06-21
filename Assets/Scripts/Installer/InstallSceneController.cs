using UnityEngine;
using UnityEngine.Localization.Components;

using DDEngine.Installer.States;
using DDEngine.Utils.FSM;

namespace DDEngine.Installer
{
    public enum InstallSceneState
    {
        WaitForInstallPick,
        Installing,
        Done
    };

    public class InstallSceneController : StateMachine<InstallSceneState>
    {
        [SerializeField]
        public LocalizeStringEvent labelText;

        [SerializeField]
        public GameObject retryText;

        [SerializeField]
        public GameObject pickButton;

        [SerializeField]
        public GameObject installingButton;

        [SerializeField]
        public GameObject continueButton;

        private void Awake()
        {
            InstallButtonController controller = pickButton.GetComponent<InstallButtonController>();
            controller.Setup(this);

            InstallDoneState doneState = new States.InstallDoneState(this);
            ProceedButtonController proceedController = continueButton.GetComponent<ProceedButtonController>();

            proceedController.Setup(this, doneState);

            base.AddState(InstallSceneState.WaitForInstallPick, new States.PickInstallFileState(this));
            base.AddState(InstallSceneState.Installing, new States.InstallState(this));
            base.AddState(InstallSceneState.Done, doneState);
        }
        protected override InstallSceneState GetInitialState()
        {
            return InstallSceneState.Installing;
    }
    }
}
