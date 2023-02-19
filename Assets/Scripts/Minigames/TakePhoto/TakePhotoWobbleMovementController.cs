using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TakePhotoWobbleMovementController : MonoBehaviour
{
    private Vector2 moveAmount;
    private GUILocationController locationController;
    private bool wobbleInactive = true;
    private float startDelayTime = 1.0f;

    private Vector2 boostrapWobbleScroll;
    private Vector2[] wobbleMovementScrollList;

    public void Setup(Vector2 viewSizeInUnits)
    {
        moveAmount = viewSizeInUnits;

        // Most of them is just result of testing
        wobbleMovementScrollList = new Vector2[]
        {
            new Vector2(moveAmount.x / 4 * 3, moveAmount.y / 4),
            new Vector2(moveAmount.x / 4 * 3, -moveAmount.y / 2),
            new Vector2(-moveAmount.x / 4 * 3, -moveAmount.y / 2),
            new Vector2(-moveAmount.x / 2, moveAmount.y / 2),
            new Vector2(moveAmount.x / 2, moveAmount.y / 4),
            new Vector2(moveAmount.x / 2, -moveAmount.y / 3),
            new Vector2(-moveAmount.x / 2, -moveAmount.y / 8),
            new Vector2(-moveAmount.x / 4, moveAmount.y / 24 * 11),
            new Vector2(moveAmount.x / 2, moveAmount.y / 4),
            new Vector2(moveAmount.x / 4 * 3, -moveAmount.y / 2),
            new Vector2(-moveAmount.x / 4 * 3, -moveAmount.y / 4 * 3),
            new Vector2(-moveAmount.x, moveAmount.y)
        };

        boostrapWobbleScroll = new Vector2(-moveAmount.x, moveAmount.y / 2);
    }

    private IEnumerator ScrollAndWait(Vector2 scrollSize, GUILayerController.EaseType easeType = GUILayerController.EaseType.Normal)
    {
        locationController.Scroll(scrollSize, hasDuration: true, accountingScrollFactor: true, busyWhileAnimating: false, ease: easeType);
        yield return new WaitUntil(() => locationController.ScrollAnimationDone);
    }

    public IEnumerator WobbleScrollCoroutine()
    {
        if (startDelayTime != 0)
        {
            yield return new WaitForSeconds(startDelayTime);
            startDelayTime = 0.0f;

            yield return ScrollAndWait(boostrapWobbleScroll, GUILayerController.EaseType.ArcOut);
        }

        for (int i = 0; i < wobbleMovementScrollList.Length; i++)
        {
            yield return ScrollAndWait(wobbleMovementScrollList[i], (i % 2 == 0) ? GUILayerController.EaseType.ArcIn : GUILayerController.EaseType.ArcOut);
        }
        
        wobbleInactive = true;
    }

    private void Start()
    {
        locationController = GetComponent<GUILocationController>();
    }

    void Update()
    {
        if (wobbleInactive)
        {
            wobbleInactive = false;
            StartCoroutine(WobbleScrollCoroutine());
        }
    }
}
