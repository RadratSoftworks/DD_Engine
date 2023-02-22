using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GUIActiveController : MonoBehaviour
{
    public GameObject arrows;
    private GUIControlSet controlSet;
    private GUILayerController layerController;

    private bool isHovered = false;

    // The pivot of the collider should be the main view point we got to center to!
    private Vector3 actionCenterViewPoint;

    public bool PanToCenterWhenSelect { get; set; } = true;
    private bool dialogueChangeSubscribed = false;

    private void RegOrUnregAction(bool reg)
    {
        var actionMap = GameInputManager.Instance.GUILocationActionMap;
        InputAction activeConfirmed = actionMap.FindAction("Active Confirmed");

        if (reg)
        {
            activeConfirmed.performed += OnSelect;
        }
        else
        {
            activeConfirmed.performed -= OnSelect;
        }
    }

    private void Awake()
    {
        layerController = GetComponentInParent<GUILayerController>();
    }

    private void Start()
    {
        arrows.SetActive(false);
        RegOrUnregAction(true);

        GameManager.Instance.DialogueStateChanged += OnDialogueStateChanged;
        dialogueChangeSubscribed = true;

        controlSet.StateChanged += enabled =>
        {
            if (dialogueChangeSubscribed == enabled)
            {
                return;
            }

            if (enabled)
            {
                GameManager.Instance.DialogueStateChanged += OnDialogueStateChanged;
            } else
            {
                GameManager.Instance.DialogueStateChanged -= OnDialogueStateChanged;
            }

            dialogueChangeSubscribed = enabled;
        };
    }

    private void OnEnable()
    {
        RegOrUnregAction(true);
    }

    private void OnDisable()
    {
        RegOrUnregAction(false);
    }

    public void Setup(GUIControlSet set, Vector2 position, Vector2 size, Rect detectBounds, bool panToWhenSelected = true)
    {
        this.PanToCenterWhenSelect = panToWhenSelected;
        controlSet = set;

        transform.localPosition = GameUtils.ToUnityCoordinates(detectBounds.position + detectBounds.size / 2);
        Vector3 sizeTransformed = size / 2;

        BoxCollider2D collider = GetComponent<BoxCollider2D>();
        if (collider != null)
        {
            collider.size = GameUtils.ToUnitySize(detectBounds.size);
        }

        Vector3[] positionMove =
        {
            Vector3.Scale(sizeTransformed, new Vector3(-1, -1)),
            Vector3.Scale(sizeTransformed, new Vector3(1, -1)),
            Vector3.Scale(sizeTransformed, new Vector3(-1, 1)),
            Vector3.Scale(sizeTransformed, new Vector3(1, 1))
        };

        Vector2[] origin =
        {
            Vector2.right,
            Vector2.zero,
            Vector3.one,
            Vector2.up
        };

        arrows.transform.localPosition = GameUtils.ToUnityCoordinates(position + size / 2) - new Vector2(transform.localPosition.x, transform.localPosition.y);

        for (int i = 0; i < 4; i++)
        {
            GameObject child = arrows.transform.GetChild(i).gameObject;
            SpriteAnimatorController controller = child.GetComponent<SpriteAnimatorController>();
            
            controller.Setup(positionMove[i], 0, FilePaths.ArrowAnimationsPath[i], null, origin[i]);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        actionCenterViewPoint = collision.transform.position;

        arrows.SetActive(true);
        isHovered = true;

        GameManager.Instance.RunPersistentCoroutine(controlSet.HandleAction(name, Constants.OnFocusScriptEventName));
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        arrows.SetActive(false);
        isHovered = false;
    }

    public void OnSelect(InputAction.CallbackContext context)
    {
        if (isHovered)
        {
            if (PanToCenterWhenSelect)
            {
                layerController.ScrollLocation(GUICanvasSetup.CorrectActiveColliderPanning(arrows.transform.InverseTransformPoint(actionCenterViewPoint)));
            }

            GameManager.Instance.RunPersistentCoroutine(controlSet.HandleAction(name, Constants.OnClickScriptEventName));
        }
    }

    private void OnDialogueStateChanged(bool enabled)
    {
        // Disable all interactable actives when dialogue is on!
        gameObject.SetActive(!enabled);
    }
}
