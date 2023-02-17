﻿using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;
using static GameManager;

public class GUILayerController : MonoBehaviour
{
    public enum EaseType
    {
        ArcIn = 0,
        ArcOut = 1,
        Normal = 2
    };

    public GameObject boundsObject;

    [Tooltip("Number divided with the scroll amount to get the amount of movement to scroll the layer")]
    public float layerScrollFactor = 100.0f;
    public Vector2 scroll;

    private Vector2 originalPosition;
    private Vector2 size;
    private bool definePan;

    private GUILocationController locationController;
    private GUIControlSet controlSet;
    private Sequence scrollSequence;

    public delegate void OnLayerScrollAnimationFinished(GUILayerController layerController);
    public event OnLayerScrollAnimationFinished ScrollAnimationFinished;

    public Vector2 Size => size;

    private void Start()
    {
        DOTween.Init();
    }

    private void Awake()
    {
        locationController = transform.parent.gameObject.GetComponent<GUILocationController>();
        GameManager.Instance.DialogueStateChanged += OnDialogueStateChanged;
    }

    public void SetProperties(GUIControlSet controlSet, Vector2 position, Vector2 scroll, Vector2 size)
    {
        originalPosition = GameUtils.ToUnityCoordinates(position);

        this.scroll = scroll;
        this.size = GameUtils.ToUnitySize(size);
        this.controlSet = controlSet;

        transform.localPosition = originalPosition;
    }

    public Vector3 CalculateActualScrollAmount(Vector2 scrollAmount)
    {
        return new Vector3((scroll.x / layerScrollFactor) * scrollAmount.x, (scroll.y / layerScrollFactor) * scrollAmount.y, 0.0f);
    }

    public Vector3 CalculateDestinationScroll(Vector3 basePoint, Vector2 scrollAmount)
    {
        return basePoint + CalculateActualScrollAmount(scrollAmount);
    }

    public void ScrollFromOrigin(Vector2 amount, float duration = 0.0f, bool enablePanAnimation = false)
    {
        if (enablePanAnimation)
        {
            scrollSequence = DOTween.Sequence();
            scrollSequence.Append(transform.DOLocalMove(CalculateDestinationScroll(originalPosition, amount), duration));
        } else
        {
            transform.localPosition = CalculateDestinationScroll(originalPosition, amount);
        }
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

    public void ForceScroll(Vector2 scrollAmount, float duration, EaseType ease = EaseType.Normal)
    {
        Vector3 dest = CalculateDestinationScroll(transform.localPosition, scrollAmount);
        if (duration == 0.0f)
        {
            transform.localPosition = dest;
            return;
        }

        scrollSequence = DOTween.Sequence();

        if (ease == EaseType.Normal)
        {
            scrollSequence.Append(transform.DOLocalMove(dest, duration));
        } else if (ease == EaseType.ArcOut)
        {
            scrollSequence.Append(transform.DOLocalMoveX(dest.x, duration).SetEase(Ease.OutQuad));
            scrollSequence.Join(transform.DOLocalMoveY(dest.y, duration).SetEase(Ease.InQuad));
        } else
        {
            scrollSequence.Append(transform.DOLocalMoveX(dest.x, duration).SetEase(Ease.InQuad));
            scrollSequence.Join(transform.DOLocalMoveY(dest.y, duration).SetEase(Ease.OutQuad));
        }

        scrollSequence.OnComplete(() => ScrollAnimationFinished?.Invoke(this));
    }

    public void CancelPendingScroll()
    {
        scrollSequence.Complete();
        ScrollAnimationFinished?.Invoke(this);
    }

    public Vector3 CalculateScrollAmountForLimitedPanFromPos(Vector3 basePoint, Vector2 scrollAmount, bool notAccountingScrollFactor = false)
    {
        Vector3 destPoint = notAccountingScrollFactor ? (basePoint + new Vector3(scrollAmount.x, scrollAmount.y, 0.0f)) : CalculateDestinationScroll(basePoint, scrollAmount);
        destPoint.x = Mathf.Clamp(destPoint.x, controlSet.ViewSize.x - size.x, 0.0f);
        destPoint.y = Mathf.Clamp(destPoint.y, 0.0f, size.y - controlSet.ViewSize.y);

        Vector3 actualMoveAmount = destPoint - basePoint;
        if (scroll.x == 0)
        {
            actualMoveAmount.x = 0.0f;
        } else
        {
            actualMoveAmount.x /= scroll.x / layerScrollFactor;
        }

        if (scroll.y == 0)
        {
            actualMoveAmount.y = 0.0f;
        }
        else
        {
            actualMoveAmount.y /= scroll.y / layerScrollFactor;
        }

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

    private void OnDialogueStateChanged(bool enabled)
    {
        if (scrollSequence == null)
        {
            return;
        }

        if (enabled)
        {
            scrollSequence.Pause();
        } else
        {
            scrollSequence.Play();
        }
    }
}
