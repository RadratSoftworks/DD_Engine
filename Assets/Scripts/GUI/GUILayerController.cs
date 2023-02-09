using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;

public class GUILayerController : MonoBehaviour
{
    public GameObject boundsObject;

    [Tooltip("Number divided with the scroll amount to get the amount of movement to scroll the layer")]
    public float layerScrollFactor = 100.0f;

    private Vector2 originalPosition;
    private Vector2 size;
    private Vector2 scroll;
    private bool definePan;

    private GUILocationController locationController;
    private GUIControlSet controlSet;


    private void Start()
    {
        DOTween.Init();
    }

    private void Awake()
    {
        locationController = transform.parent.gameObject.GetComponent<GUILocationController>();
    }

    public void SetProperties(GUIControlSet controlSet, Vector2 position, Vector2 scroll, Vector2 size)
    {
        originalPosition = GameUtils.ToUnityCoordinates(position);

        this.scroll = scroll;
        this.size = GameUtils.ToUnitySize(size);
        this.controlSet = controlSet;

        transform.localPosition = originalPosition;
    }

    private Vector3 CalculateDestinationScroll(Vector3 basePoint, Vector2 scrollAmount)
    {
        return basePoint + new Vector3((scroll.x / layerScrollFactor) * scrollAmount.x, (scroll.y / layerScrollFactor) * scrollAmount.y, 0.0f);
    }

    public void ScrollFromOrigin(Vector2 amount)
    {
        transform.localPosition = CalculateDestinationScroll(originalPosition, amount);
    }


    // Return the amount of time until scrolling is done!
    public void ScrollLocation(Vector2 amountRaw)
    {
        if (scroll.x == 0)
        {
            amountRaw.x = 0;
        } else
        {
            amountRaw.x /= scroll.x / layerScrollFactor;
        }

        if (scroll.y == 0)
        {
            amountRaw.y = 0;
        } else
        {
            amountRaw.y /= scroll.y / layerScrollFactor;
        }

        locationController.Scroll(amountRaw, true);
    }

    public void ForceScroll(Vector2 scrollAmount, float duration)
    {
        Vector3 dest = CalculateDestinationScroll(transform.localPosition, scrollAmount);
        if (duration == 0.0f)
        {
            transform.localPosition = dest;
            return;
        }

        transform.DOLocalMove(dest, duration);
    }

    public Vector3 CalculateScrollAmountForLimitedPanFromPos(Vector3 basePoint, Vector2 scrollAmount, bool notAccountingScrollFactor = false)
    {
        Vector3 destPoint = notAccountingScrollFactor ? (basePoint + new Vector3(scrollAmount.x, scrollAmount.y, 0.0f)) : CalculateDestinationScroll(basePoint, scrollAmount);
        destPoint.x = Mathf.Clamp(destPoint.x, controlSet.ViewSize.x - size.x, 0.0f);
        destPoint.y = Mathf.Clamp(destPoint.y, 0.0f, size.y - controlSet.ViewSize.y);

        Vector3 actualMoveAmount = destPoint - basePoint;
        actualMoveAmount.x /= ((scroll.x == 0) ? 1 : scroll.x / layerScrollFactor);
        actualMoveAmount.y /= ((scroll.y == 0) ? 1 : scroll.y / layerScrollFactor);

        return actualMoveAmount;
    }

    public Vector3 CalculateScrollAmountForLimitedPan(Vector2 scrollAmount, bool notAccountingScrollFactor = false)
    {
        return CalculateScrollAmountForLimitedPanFromPos(transform.localPosition, scrollAmount, notAccountingScrollFactor);
    }

    public Vector3 CalculateScrollAmountForLimitedPanFromOrigin(Vector2 scrollAmount)
    {
        return CalculateScrollAmountForLimitedPanFromPos(originalPosition, scrollAmount);
    }
}
