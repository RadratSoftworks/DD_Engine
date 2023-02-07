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

    private void Awake()
    {
        layerController = GetComponentInParent<GUILayerController>();
    }

    private void Start()
    {
        arrows.SetActive(false);
    }

    private void OnDestroy()
    {
        GameManager.Instance.DialogueStateChanged -= OnDialogueStateChanged;
    }

    public void Setup(GUIControlSet set, Vector2 position, Vector2 size, Rect detectBounds)
    {
        controlSet = set;
        transform.localPosition = GameUtils.ToUnityCoordinates(detectBounds.position + detectBounds.size / 2);
        Vector3 sizeTransformed = GameUtils.ToUnitySize(size);

        BoxCollider2D collider = GetComponent<BoxCollider2D>();
        if (collider != null)
        {
            collider.size = GameUtils.ToUnitySize(detectBounds.size);
        }

        Vector3[] positionMove =
        {
            Vector3.Scale(sizeTransformed / 2, new Vector3(-1, 1)),
            Vector3.Scale(sizeTransformed / 2, new Vector3(1, 1)),
            Vector3.Scale(sizeTransformed / 2, new Vector3(-1, -1)),
            Vector3.Scale(sizeTransformed / 2, new Vector3(1, -1))
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

        GameManager.Instance.DialogueStateChanged += OnDialogueStateChanged;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        actionCenterViewPoint = collision.transform.position;

        arrows.SetActive(true);
        isHovered = true;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        arrows.SetActive(false);
        isHovered = false;
    }

    public void OnSelect()
    {
        if (isHovered)
        {
            layerController.ScrollLocation(arrows.transform.InverseTransformPoint(actionCenterViewPoint));
            StartCoroutine(controlSet.HandleAction(name, Constants.OnClickScriptEventName));
        }
    }

    private void OnDialogueStateChanged(bool enabled)
    {
        // Disable all interactable actives when dialogue is on!
        gameObject.SetActive(!enabled);
    }
}
