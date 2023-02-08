using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GUILocationController : MonoBehaviour
{
    public float durationPerUnit = 1.0f;
    private GUIControlSet controlSet;
    private PlayerInput playerInput;

    private void Awake()
    {
        GameManager.Instance.DialogueStateChanged += OnDialogueStateChanged;
        playerInput = GetComponent<PlayerInput>();
    }

    public void Setup(GUIControlSet controlSet)
    {
        this.controlSet = controlSet;
        controlSet.OffsetChanged += OnControlSetOffsetChanged;
    }

    private void OnDialogueStateChanged(bool enabled)
    {
        playerInput.enabled = !enabled;
    }

    private IEnumerator ScrollDoneCoroutine(float duration)
    {
        yield return new WaitForSeconds(duration);
        controlSet.UnregisterPerformingBusyAnimation();
        yield break;
    }

    public void RequestScroll(Vector2 scrollAmount)
    {
        if (scrollAmount == Vector2.zero)
        {
            return;
        }

        float duration = Mathf.Max(scrollAmount.x, scrollAmount.y) * durationPerUnit;
        controlSet.RegisterPerformingBusyAnimation();

        for (int i = 0; i < transform.childCount; i++)
        {
            GameObject child = transform.GetChild(i).gameObject;
            GUILayerController controller = child.GetComponent<GUILayerController>();

            if (controller != null)
            {
                controller.ForceScroll(scrollAmount, duration);
            }
        }

        StartCoroutine(ScrollDoneCoroutine(duration));
    }

    private void ScrollFromOrigin(Vector2 offset)
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            GameObject child = transform.GetChild(i).gameObject;
            GUILayerController controller = child.GetComponent<GUILayerController>();

            if (controller != null)
            {
                controller.ScrollFromOrigin(offset);
            }
        }
    }

    private void OnControlSetOffsetChanged(Vector2 offset)
    {
        ScrollFromOrigin(GameUtils.ToUnityCoordinates(offset));
    }
}
