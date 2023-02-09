using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GUILocationController : MonoBehaviour
{
    public float durationPerUnit = 1.0f;
    public float moveAmount = 0.02f;

    private GUIControlSet controlSet;
    private PlayerInput playerInput;

    private GUILayerController panLayerController;
    private List<GUILayerController> layerControllers = new List<GUILayerController>();

    private void Awake()
    {
        GameManager.Instance.DialogueStateChanged += OnDialogueStateChanged;
        playerInput = GetComponent<PlayerInput>();
    }

    public void Setup(GUIControlSet controlSet, GameObject panLayer)
    {
        this.controlSet = controlSet;
        this.panLayerController = panLayer.GetComponent<GUILayerController>();

        controlSet.OffsetChanged += OnControlSetOffsetChanged;
        controlSet.PanRequested += OnPanRequested;

        for (int i = 0; i < transform.childCount; i++)
        {
            GameObject child = transform.GetChild(i).gameObject;
            GUILayerController controller = child.GetComponent<GUILayerController>();

            if (controller != null)
            {
                layerControllers.Add(controller);
            }
        }
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

    private void ScrollFromOrigin(Vector2 offset)
    {
        Vector3 targetOffset = panLayerController.CalculateScrollAmountForLimitedPanFromOrigin(offset);

        if (targetOffset == Vector3.zero)
        {
            return;
        }

        foreach (var controller in layerControllers)
        {
            controller.ScrollFromOrigin(targetOffset);
        }
    }

    public void Scroll(Vector2 amount, bool hasDuration = false, bool notAccountingPanLayerScrollFactor = false)
    {
        Vector3 targetPanAmount = panLayerController.CalculateScrollAmountForLimitedPan(amount, notAccountingPanLayerScrollFactor);
        if (targetPanAmount == Vector3.zero)
        {
            return;
        }

        float duration = 0.0f;

        if (hasDuration) {
            controlSet.RegisterPerformingBusyAnimation();
            duration = Mathf.Max(targetPanAmount.x, targetPanAmount.y) * durationPerUnit;
        }

        foreach (var controller in layerControllers)
        {
            controller.ForceScroll(targetPanAmount, duration);
        }

        if (hasDuration)
        {
            StartCoroutine(ScrollDoneCoroutine(duration));
        }
    }

    public void OnMoveLeft(InputValue value)
    {
        Scroll(Vector3.right * moveAmount * value.Get<float>());
    }

    public void OnMoveRight(InputValue value)
    {
        Scroll(Vector3.left * moveAmount * value.Get<float>());
    }

    public void OnMoveUp(InputValue value)
    {
        Scroll(Vector3.down * moveAmount *value.Get<float>());
    }

    public void OnMoveDown(InputValue value)
    {
        Scroll(Vector3.up * moveAmount * value.Get<float>());
    }

    private void OnControlSetOffsetChanged(Vector2 offset)
    {
        ScrollFromOrigin(GameUtils.ToUnityCoordinates(offset));
    }

    private void OnPanRequested(Vector2 amount)
    {
        Scroll(GameUtils.ToUnityCoordinates(amount * new Vector2(-1, 1)), true, true);
    }
}
