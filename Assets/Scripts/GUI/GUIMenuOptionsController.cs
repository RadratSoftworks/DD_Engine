using System;
using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;
using Unity.VisualScripting;

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

        if (reg)
        {
            navigateDown.performed += OnNavigateDown;
            navigateUp.performed += OnNavigateUp;
            submit.performed += OnSubmit;
        }
        else
        {
            navigateDown.performed -= OnNavigateDown;
            navigateUp.performed -= OnNavigateUp;
            submit.performed -= OnSubmit;
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
        GUIMenuOptionController controllerOld = currentChild.GetComponent<GUIMenuOptionController>();

        controllerOld.OnOptionDeselected();
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
        GUIMenuOptionController controllerNew = newChild.GetComponent<GUIMenuOptionController>();

        controllerNew.OnOptionSelected();
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

    public void OnSubmit(InputAction.CallbackContext context)
    {
        GameObject currentChild = transform.GetChild(currentChildIndex).gameObject;
        OnButtonClicked?.Invoke(currentChild.name);
    }
}
