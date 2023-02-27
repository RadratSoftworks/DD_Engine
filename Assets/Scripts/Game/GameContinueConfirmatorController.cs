using UnityEngine;
using UnityEngine.InputSystem;

namespace DDEngine.Game
{
    public class GameContinueConfirmatorController : MonoBehaviour
    {
        public bool Confirmed { get; set; } = false;
        private bool hearing = false;
        private InputAction inputAction = null;

        private void OnDisable()
        {
            if (hearing)
            {
                GameInputManager.Instance.ContinueGameActionMap.Disable();
            }
        }

        private void OnEnable()
        {
            if (hearing)
            {
                GameInputManager.Instance.ContinueGameActionMap.Enable();
            }
        }

        public void StartHearing()
        {
            var continueActionMap = GameInputManager.Instance.ContinueGameActionMap;
            continueActionMap.Enable();

            InputAction continueAction = continueActionMap.FindAction("Continue Confirmed");
            continueAction.performed += OnContinueConfirmed;

            hearing = true;
            Confirmed = false;
        }

        public void OnContinueConfirmed(InputAction.CallbackContext context)
        {
            Confirmed = true;
            hearing = false;

            if (inputAction != null)
            {
                inputAction.performed -= OnContinueConfirmed;
            }

            GameInputManager.Instance.ContinueGameActionMap.Disable();
        }
    }
}