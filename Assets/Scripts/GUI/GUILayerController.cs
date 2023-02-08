using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;

public class GUILayerController : MonoBehaviour
{
    public float moveAmount = 0.01f;
    public GameObject boundsObject;

    [Tooltip("Number divided with the scroll amount to get the amount of movement to scroll the layer")]
    public float layerScrollFactor = 100.0f;

    private Vector2 originalPosition;
    private Vector2 size;
    private Vector2 scroll;
    private bool definePan;

    private GUILocationController locationController;
    private BoxCollider2D boundsTrigger;


    private void Start()
    {
        DOTween.Init();
    }

    private void Awake()
    {
        locationController = transform.parent.gameObject.GetComponent<GUILocationController>();
    }

    public void SetProperties(Vector2 position, Vector2 initialScroll, Vector2 size, bool definePan)
    {
        originalPosition = GameUtils.ToUnityCoordinates(position);
        scroll = initialScroll;

        this.size = GameUtils.ToUnitySize(size);
        this.definePan = definePan;

        transform.localPosition = originalPosition;
        boundsTrigger = boundsObject.GetComponent<BoxCollider2D>();

        if (definePan)
        {
            boundsObject.transform.localPosition = GameUtils.ToUnityCoordinates(size / 2);
            boundsTrigger.size = this.size;
        } else
        {
            boundsObject.SetActive(false);
        }
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

        locationController.RequestScroll(amountRaw);
    }

    public void ForceScroll(Vector2 scrollAmount, float duration)
    {
        transform.DOLocalMove(CalculateDestinationScroll(transform.localPosition, scrollAmount), duration);
    }

    private bool PanningDisabled()
    {
        return !definePan && locationController.PanLocked;
    }

    public void OnMoveLeft(InputValue value)
    {
        if (PanningDisabled())
        {
            return;
        }

        transform.localPosition += Vector3.right * (scroll.x / layerScrollFactor) * moveAmount * value.Get<float>();
    }

    public void OnMoveRight(InputValue value)
    {
        if (PanningDisabled())
        {
            return;
        }

        transform.localPosition += Vector3.left * (scroll.x / layerScrollFactor) * moveAmount * value.Get<float>();
    }

    public void OnMoveUp(InputValue value)
    {
        if (PanningDisabled())
        {
            return;
        }

        transform.localPosition += Vector3.down * (scroll.y / layerScrollFactor) * moveAmount * value.Get<float>();
    }

    public void OnMoveDown(InputValue value)
    {
        if (PanningDisabled())
        {
            return;
        }

        transform.localPosition += Vector3.up * (scroll.y / layerScrollFactor) * moveAmount * value.Get<float>();
    }
}
