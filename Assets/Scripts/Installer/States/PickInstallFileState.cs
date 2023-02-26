namespace DDEngine.Installer.States
{
    public class PickInstallFileState : IState
    {
        private InstallSceneController stateMachine;

        public PickInstallFileState(InstallSceneController stateMachine)
        {
            this.stateMachine = stateMachine;
        }

        public void Enter()
        {
            stateMachine.labelText.SetEntry("Instruction");
            stateMachine.pickButton.SetActive(true);
        }

        public void Leave()
        {
            stateMachine.pickButton.SetActive(false);
        }

        public void ReceiveData(IStateMachine sender, object data)
        {
            if (data is NGageInstallPathIntent)
            {
                stateMachine.Transition(InstallSceneState.Installing, data);
            }
        }

        public void Update()
        {
        }
    }
}
