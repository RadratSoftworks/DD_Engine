using System;
using UnityEngine;
using DG.Tweening;

public class GUIMenuOptionsController : MonoBehaviour
{
    public float scrollDuration = 0.5f;
    private int currentChildIndex = 0;
    private bool initializedCurrent = false;

    public delegate void ButtonClicked(string id);
    public event ButtonClicked OnButtonClicked;

    private void Start()
    {
        DOTween.Init();
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

    private void OnNavigateDown()
    {
        OnGameOptionChoosen((currentChildIndex + 1) % transform.childCount);
    }

    private void OnNavigateUp()
    {
        OnGameOptionChoosen((currentChildIndex == 0) ? (transform.childCount - 1) : (currentChildIndex - 1));
    }

    private void OnSubmit()
    {
        GameObject currentChild = transform.GetChild(currentChildIndex).gameObject;
        OnButtonClicked?.Invoke(currentChild.name);
    }
}
