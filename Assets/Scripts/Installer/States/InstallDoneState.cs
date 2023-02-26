using UnityEngine.Localization.SmartFormat.PersistentVariables;
using DDEngine.Utils.FSM;

namespace DDEngine.Installer.States
{
    public class InstallDoneState : IState
    {
        private InstallSceneController stateMachine;
        private StringVariable messageVar;
        private bool encounteredError = false;

        public bool EncounteredError => encounteredError;

        public InstallDoneState(InstallSceneController stateMachine)
        {
            this.stateMachine = stateMachine;

            messageVar = stateMachine.labelText.StringReference["message"] as StringVariable;
        }

        public void Enter()
        {
            stateMachine.labelText.SetEntry("DoneInstruction");
            stateMachine.continueButton.SetActive(true);
            encounteredError = false;
        }

        public void Leave()
        {
            stateMachine.continueButton.SetActive(false);
            stateMachine.retryText.SetActive(false);
        }

        private void NotifyErrorEncountered()
        {
            stateMachine.retryText.SetActive(true);
            encounteredError = true;
        }

        private void DisplayNiceError(int errorCode)
        {
            string entryText;

            switch (errorCode)
            {
                case GameDataInstaller.ErrorCodeNone:
                    return;

                case GameDataInstaller.ErrorCodeCorrupted:
                    entryText = "ErrorInstallFileCorrupted";
                    break;

                case GameDataInstaller.ErrorCodeDataTooLarge:
                    entryText = "ErrorInstallFileTooLarge";
                    break;

                case GameDataInstaller.ErrorCodeFileNotFound:
                    entryText = "ErrorInstallFileCantOpen";
                    break;

                case GameDataInstaller.ErrorCodeInstallFailed:
                    entryText = "ErrorInstallFailed";
                    break;

                default:
                    throw new System.Exception("Unknown install error code!");
            }

            stateMachine.labelText.SetEntry(entryText);
            NotifyErrorEncountered();
        }

        public void ReceiveData(IStateMachine sender, object data)
        {
            if (data is string)
            {
                stateMachine.labelText.SetEntry("ExceptionInstruction");
                messageVar.Value = data as string;

                NotifyErrorEncountered();
            }
            else if (data is int)
            {
                DisplayNiceError((int)data);
            }
        }

        public void Update()
        {
        }
    }
}
