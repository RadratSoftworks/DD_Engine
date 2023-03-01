using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;

namespace DDEngine.GUI
{
    public class GUIMenuOptionsController : MonoBehaviour
    {
        public float scrollDuration = 0.5f;
        private int currentChildIndex = 0;
        private bool initializedCurrent = false;

        public delegate void ButtonClicked(string id);
        public event ButtonClicked OnButtonClicked;

        private void RegOrUnregAction(bool reg)
        {
            var actionMap = GameInputManager.Instance.GUIMenuActionMap;

            InputAction navigateDown = actionMap.FindAction("Navigate Down");
            InputAction navigateUp = actionMap.FindAction("Navigate Up");
            InputAction submit = actionMap.FindAction("Submit");
            InputAction leftValue = actionMap.FindAction("Left Value Triggered");
            InputAction rightValue = actionMap.FindAction("Right Value Triggered");

            if (reg)
            {
                navigateDown.performed += OnNavigateDown;
                navigateUp.performed += OnNavigateUp;
                submit.performed += OnSubmit;
                leftValue.performed += OnLeftValueTriggered;
                rightValue.performed += OnRightValueTriggered;

                GameInputManager.Instance.SetGUIMenuTriggerActionMapState(false);
            }
            else
            {
                navigateDown.performed -= OnNavigateDown;
                navigateUp.performed -= OnNavigateUp;
                submit.performed -= OnSubmit;
                leftValue.performed -= OnLeftValueTriggered;
                rightValue.performed -= OnRightValueTriggered;

                GameInputManager.Instance.SetGUIMenuTriggerActionMapState(true);
            }
        }

        private void Start()
        {
            DOTween.Init();
            RegOrUnregAction(true);
        }

        private void OnEnable()
        {
            RegOrUnregAction(true);
        }

        private void OnDisable()
        {
            RegOrUnregAction(false);
        }

        private void Update()
        {
            if (!initializedCurrent)
            {
                SelectOption(currentChildIndex);
                initializedCurrent = true;
            }
        }

        private void DeselectCurrentOption()
        {
            GameObject currentChild = transform.GetChild(currentChildIndex).gameObject;
            GUIMenuItemController controllerOld = currentChild.GetComponent<GUIMenuItemController>();

            if (controllerOld == null)
            {
                GUISettingMultiValuesOptionController controllerOldOld = currentChild.GetComponent<GUISettingMultiValuesOptionController>();
                controllerOldOld.OnOptionDeselected();
            }
            else
            {
                controllerOld.OnOptionDeselected();
            }
        }

        public void SelectOption(int newChildIndex, bool scrolling = false)
        {
            if (scrolling)
            {
                if (newChildIndex == 0)
                {
                    transform.DOLocalMoveY(0.0f, scrollDuration);
                }
                else
                {
                    Transform currentChild = transform.GetChild(currentChildIndex);
                    Transform nextChild = transform.GetChild(newChildIndex);

                    transform.DOLocalMoveY(transform.localPosition.y - (nextChild.localPosition.y - currentChild.localPosition.y), scrollDuration);
                }
            }

            GameObject newChild = transform.GetChild(newChildIndex).gameObject;
            GUIMenuItemController controllerNew = newChild.GetComponent<GUIMenuItemController>();
            if (controllerNew == null)
            {
                GUISettingMultiValuesOptionController controllerNewNew = newChild.GetComponent<GUISettingMultiValuesOptionController>();
                controllerNewNew.OnOptionSelected();
            }
            else
            {
                controllerNew.OnOptionSelected();
            }

            currentChildIndex = newChildIndex;
        }

        public void OnGameOptionChoosen(int newChildIndex)
        {
            DeselectCurrentOption();
            SelectOption(newChildIndex, true);
        }

        public void OnNavigateDown(InputAction.CallbackContext context)
        {
            OnGameOptionChoosen((currentChildIndex + 1) % transform.childCount);
        }

        public void OnNavigateUp(InputAction.CallbackContext context)
        {
            OnGameOptionChoosen((currentChildIndex == 0) ? (transform.childCount - 1) : (currentChildIndex - 1));
        }

        private void OnLeftValueTriggered(InputAction.CallbackContext context)
        {
            GameObject currentChild = transform.GetChild(currentChildIndex).gameObject;
            GUISettingMultiValuesOptionController controller = currentChild.GetComponent<GUISettingMultiValuesOptionController>();

            if (controller != null)
            {
                controller.OnLeftValueTriggered();
            }
        }

        private void OnRightValueTriggered(InputAction.CallbackContext context)
        {
            GameObject currentChild = transform.GetChild(currentChildIndex).gameObject;
            GUISettingMultiValuesOptionController controller = currentChild.GetComponent<GUISettingMultiValuesOptionController>();

            if (controller != null)
            {
                controller.OnRightValueTriggered();
            }
        }

        public void OnSubmit(InputAction.CallbackContext context)
        {
            GameObject currentChild = transform.GetChild(currentChildIndex).gameObject;
            OnButtonClicked?.Invoke(currentChild.name);
        }
    }
}