using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static GUILayerController;

public class GUILocationController : MonoBehaviour
{
    public float durationPerUnit = 0.5f;

    [SerializeField]
    private float maxScrollDuration = 0.6f;

    [SerializeField]
    private float moveAmount = 0.02f;

    private GUIControlSet controlSet;

    private GUILayerController panLayerController;
    private List<GUILayerController> layerControllers = new List<GUILayerController>();

    private int scrollInProgressCount = 0;
    public float MoveAmount => moveAmount;

    public bool ScrollAnimationDone => (scrollInProgressCount == 0);

    private void RegOrUnregAction(bool reg)
    {
        var actionMap = GameInputManager.Instance.GUILocationActionMap;

        InputAction moveLeft = actionMap.FindAction("Move Left");
        InputAction moveRight = actionMap.FindAction("Move Right");
        InputAction moveUp = actionMap.FindAction("Move Up");
        InputAction moveDown = actionMap.FindAction("Move Down");
        InputAction moveJoystick = actionMap.FindAction("Move Joystick");

        if (reg)
        {
            moveLeft.performed += OnMoveLeft;
            moveRight.performed += OnMoveRight;
            moveUp.performed += OnMoveUp;
            moveDown.performed += OnMoveDown;
            moveJoystick.performed += OnMoveJoystick;
        } else
        {
            moveLeft.performed -= OnMoveLeft;
            moveRight.performed -= OnMoveRight;
            moveUp.performed -= OnMoveUp;
            moveDown.performed -= OnMoveDown;
            moveJoystick.performed -= OnMoveJoystick;
        }
    }

    private void Start()
    {
        RegOrUnregAction(true);
    }

    private void OnEnable()
    {
        RegOrUnregAction(true);
    }

    private void OnDestroy()
    {
        RegOrUnregAction(false);
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

    public Vector2 GetCurrentScrollOffset()
    {
        return panLayerController.GetCurrentScrollOffset();
    }

    public void ScrollFromOrigin(Vector2 offset, bool enablePanAnimation = false)
    {
        Vector3 targetOffset = panLayerController.CalculateScrollAmountForLimitedPanFromOrigin(offset, false, true);

        if (targetOffset == Vector3.zero)
        {
            return;
        }

        float duration = Mathf.Min(Mathf.Max(Mathf.Abs(targetOffset.x), Mathf.Abs(targetOffset.y)) * durationPerUnit, maxScrollDuration);

        foreach (var controller in layerControllers)
        {
            controller.ScrollFromOrigin(targetOffset, duration, enablePanAnimation);
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

    public void Scroll(Vector2 amount, bool hasDuration = false, bool busyWhileAnimating = true, bool accountingScrollFactor = true, bool forFrameScroll = false, EaseType ease = EaseType.Normal)
    {
        Vector3 targetPanAmount = panLayerController.CalculateScrollAmountForLimitedPan(amount, forFrameScroll, accountingScrollFactor);
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

    public void OnMoveLeft(InputAction.CallbackContext context)
    {
        Scroll(Vector3.right * moveAmount * context.ReadValue<float>(), forFrameScroll: true);
    }

    public void OnMoveRight(InputAction.CallbackContext context)
    {
        Scroll(Vector3.left * moveAmount * context.ReadValue<float>(), forFrameScroll: true);
    }

    public void OnMoveUp(InputAction.CallbackContext context)
    {
        Scroll(Vector3.down * moveAmount * context.ReadValue<float>(), forFrameScroll: true);
    }

    public void OnMoveDown(InputAction.CallbackContext context)
    {
        Scroll(Vector3.up * moveAmount * context.ReadValue<float>(), forFrameScroll: true);
    }

    public void OnMoveJoystick(InputAction.CallbackContext context)
    {
        Scroll(context.ReadValue<Vector2>() * -moveAmount, forFrameScroll: true);
    }

    private void OnControlSetOffsetChanged(Vector2 offset)
    {
        ScrollFromOrigin(GameUtils.ToUnityCoordinates(offset), false);
    }

    private void OnPanRequested(Vector2 amount)
    {
        // Temporary disable inputs
        GameInputManager.Instance.SetGUIInputActionMapState(false);
        ScrollFromOrigin(GameUtils.ToUnityCoordinates(amount), true);
        GameInputManager.Instance.SetGUIInputActionMapState(true);
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
