using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static GUILayerController;

public class GUILocationController : MonoBehaviour
{
    public float durationPerUnit = 0.5f;

    [SerializeField]
    private float moveAmount = 0.02f;

    private GUIControlSet controlSet;
    private PlayerInput playerInput;

    private GUILayerController panLayerController;
    private List<GUILayerController> layerControllers = new List<GUILayerController>();

    private int scrollInProgressCount = 0;
    public float MoveAmount => moveAmount;

    public bool ScrollAnimationDone => (scrollInProgressCount == 0);

    private void Awake()
    {
        GameManager.Instance.DialogueStateChanged += OnDialogueStateChanged;
        playerInput = GetComponent<PlayerInput>();
    }

    private void OnLayerScrollAnimationDone(GUILayerController layerController)
    {
        scrollInProgressCount--;
    }

    public void Setup(GUIControlSet controlSet, GameObject panLayer)
    {
        this.controlSet = controlSet;
        this.panLayerController = panLayer.GetComponent<GUILayerController>();

        controlSet.OffsetChanged += OnControlSetOffsetChanged;
        controlSet.PanRequested += OnPanRequested;
        controlSet.LocationScrollSpeedSetRequested += OnScrollSpeedsSet;

        for (int i = 0; i < transform.childCount; i++)
        {
            GameObject child = transform.GetChild(i).gameObject;
            GUILayerController controller = child.GetComponent<GUILayerController>();

            if (controller != null)
            {
                controller.ScrollAnimationFinished += OnLayerScrollAnimationDone;
                layerControllers.Add(controller);
            }
        }
    }

    private void OnDialogueStateChanged(bool enabled)
    {
        playerInput.enabled = !enabled;
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
    private IEnumerator ScrollDoneCoroutine(bool reportNotBusyAnymore)
    {
        yield return new WaitUntil(() => ScrollAnimationDone);
        if (reportNotBusyAnymore)
        {
            controlSet.UnregisterPerformingBusyAnimation();
        }
        yield break;
    }

    public void Scroll(Vector2 amount, bool hasDuration = false, bool notAccountingPanLayerScrollFactor = false, bool busyWhileAnimating = true, EaseType ease = EaseType.Normal)
    {
        Vector3 targetPanAmount = panLayerController.CalculateScrollAmountForLimitedPan(amount, notAccountingPanLayerScrollFactor);
        if (targetPanAmount == Vector3.zero)
        {
            return;
        }

        float duration = 0.0f;

        if (hasDuration) {
            if (busyWhileAnimating)
            {
                controlSet.RegisterPerformingBusyAnimation();
            }

            scrollInProgressCount = layerControllers.Count;
            duration = Mathf.Max(Mathf.Abs(targetPanAmount.x), Mathf.Abs(targetPanAmount.y)) * durationPerUnit;
        }

        foreach (var controller in layerControllers)
        {
            controller.ForceScroll(targetPanAmount, duration, ease);
        }

        if (hasDuration && busyWhileAnimating)
        {
            StartCoroutine(ScrollDoneCoroutine(busyWhileAnimating));
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

    private void OnScrollSpeedsSet(Vector2[] speeds)
    {
        int index = 0;

        foreach (var layerController in layerControllers)
        {
            if (layerController.scroll == Vector2.zero)
            {
                layerController.scroll = speeds[index++];
            }
        }
    }
}
