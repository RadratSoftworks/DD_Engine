﻿using UnityEngine;
using DG.Tweening;

using DDEngine.Utils;

namespace DDEngine.GUI
{
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

        [Tooltip("How much distance can the layer scroll beyond its boundary")]
        public float extraScrollDelta = 0.0f;

        public Vector2 scroll;

        private Vector2 originalPosition;
        private Vector2 size;
        private bool dialogueStateChangeSubscribed = false;

        private GUILocationController locationController;
        private GUIControlSet controlSet;
        private Sequence scrollSequence;

        public delegate void OnLayerScrollAnimationFinished(GUILayerController layerController);
        public event OnLayerScrollAnimationFinished ScrollAnimationFinished;

        public Vector2 Size => size;
        public GUILocationController Location => locationController;

        private void Start()
        {
            DOTween.Init();
        }

        private void Awake()
        {
            locationController = transform.parent.gameObject.GetComponent<GUILocationController>();
        }

        private void CheckAndSetDialogStateChangeSubscription(bool enabled)
        {
            if (enabled != dialogueStateChangeSubscribed)
            {
                if (enabled)
                {
                    GameManager.Instance.DialogueStateChanged += OnDialogueStateChanged;
                }
                else
                {
                    GameManager.Instance.DialogueStateChanged -= OnDialogueStateChanged;
                }

                enabled = dialogueStateChangeSubscribed;
            }
        }

        public void SetProperties(GUIControlSet controlSet, Vector2 position, Vector2 scroll, Vector2 size)
        {
            originalPosition = GameUtils.ToUnityCoordinates(position);

            this.scroll = scroll;
            this.size = GameUtils.ToUnitySize(size);
            this.controlSet = controlSet;

            transform.localPosition = originalPosition;

            dialogueStateChangeSubscribed = false;
            controlSet.StateChanged += enabled =>
            {
                CheckAndSetDialogStateChangeSubscription(enabled);
            };
        }

        public Vector3 CalculateActualScrollAmount(Vector2 scrollAmount, bool perFrameScroll = false)
        {
            float scaleFactorFrame = perFrameScroll ? (1.0f / Time.deltaTime) : 1.0f;
            return new Vector3((scroll.x / layerScrollFactor / scaleFactorFrame) * scrollAmount.x, (scroll.y / layerScrollFactor / scaleFactorFrame) * scrollAmount.y, 0.0f);
        }

        public Vector3 CalculateDestinationScroll(Vector3 basePoint, Vector2 scrollAmount, bool perFrameScroll = false)
        {
            return basePoint + CalculateActualScrollAmount(scrollAmount, perFrameScroll);
        }

        public void ScrollFromOrigin(Vector2 amount, float duration = 0.0f, bool enablePanAnimation = false)
        {
            if (enablePanAnimation)
            {
                scrollSequence = DOTween.Sequence();
                scrollSequence.Append(transform.DOLocalMove(CalculateDestinationScroll(originalPosition, amount), duration));
            }
            else
            {
                transform.localPosition = CalculateDestinationScroll(originalPosition, amount);
            }
        }

        public Vector2 GetCurrentScrollOffset()
        {
            Vector2 amountRaw = new Vector2(transform.localPosition.x, transform.localPosition.y) - originalPosition;

            if (scroll.x == 0)
            {
                amountRaw.x = 0;
            }
            else
            {
                amountRaw.x /= scroll.x / layerScrollFactor;
            }

            if (scroll.y == 0)
            {
                amountRaw.y = 0;
            }
            else
            {
                amountRaw.y /= scroll.y / layerScrollFactor;
            }

            return amountRaw;
        }

        public Vector3 ReAdjustPanAmountToAccurate(Vector3 limitedPanAmount)
        {
            Vector3 originalPositionGame = GameUtils.FromUnityCoordinates(originalPosition);
            Vector3 currentGame = GameUtils.FromUnityCoordinates(new Vector2(transform.localPosition.x, transform.localPosition.y));
            Vector3 moveGame = GameUtils.FromUnityCoordinates(limitedPanAmount * scroll / layerScrollFactor);
            Vector3 destRounded = new((int)(currentGame.x + moveGame.x), (int)(currentGame.y + moveGame.y));

            // The distance must not make the position go over the destination. All are clamped down using int conversion
            // This seems to be the math on phone, since on each frame they scrolled scroll / 100 pixels, but it must be compared and converted to int because
            // the game works on pixel coordinates (which is integer).
            if (scroll.x != 0)
            {
                int distance = (int)(destRounded.x - originalPositionGame.x);
                float moveAmount = scroll.x / layerScrollFactor;

                distance = (int)((int)(distance / moveAmount) * moveAmount);
                moveGame.x = (originalPositionGame.x + distance - currentGame.x);
            }

            if (scroll.y != 0)
            {
                int distance = (int)(destRounded.y - originalPositionGame.y);
                float moveAmount = scroll.y / layerScrollFactor;

                distance = (int)((int)(distance / moveAmount) * moveAmount);
                moveGame.y = (originalPositionGame.y + distance - currentGame.y);
            }

            limitedPanAmount = GameUtils.ToUnityCoordinates(moveGame);

            if (scroll.x == 0)
            {
                limitedPanAmount.x = 0;
            }
            else
            {
                limitedPanAmount.x /= scroll.x / layerScrollFactor;
            }

            if (scroll.y == 0)
            {
                limitedPanAmount.y = 0;
            }
            else
            {
                limitedPanAmount.y /= scroll.y / layerScrollFactor;
            }

            return limitedPanAmount;
        }

        public void AccurateScrollLocation(Vector2 amountRaw)
        {
            if (scroll.x == 0)
            {
                amountRaw.x = 0;
            }
            else
            {
                amountRaw.x /= scroll.x / layerScrollFactor;
            }

            if (scroll.y == 0)
            {
                amountRaw.y = 0;
            }
            else
            {
                amountRaw.y /= scroll.y / layerScrollFactor;
            }

            locationController.Scroll(amountRaw, hasDuration: true, allowExtraScroll: true, readjustPanAmountCallback: value => ReAdjustPanAmountToAccurate(value));
        }

        public void ForceScroll(Vector2 scrollAmount, float duration, EaseType ease = EaseType.Normal, bool forFrameScroll = false)
        {
            Vector3 dest = CalculateDestinationScroll(transform.localPosition, scrollAmount, forFrameScroll);

            if (duration == 0.0f)
            {
                transform.localPosition = dest;
                return;
            }

            scrollSequence = DOTween.Sequence();

            if (ease == EaseType.Normal)
            {
                scrollSequence.Append(transform.DOLocalMove(dest, duration));
            }
            else if (ease == EaseType.ArcOut)
            {
                scrollSequence.Append(transform.DOLocalMoveX(dest.x, duration).SetEase(Ease.OutQuad));
                scrollSequence.Join(transform.DOLocalMoveY(dest.y, duration).SetEase(Ease.InQuad));
            }
            else
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

        public Vector3 CalculateScrollAmountForLimitedPanFromPos(Vector3 basePoint, Vector2 scrollAmount, bool perFrameScroll = false, bool accountingScrollFactor = true, bool allowExtraScroll = false)
        {
            float extraScrollDeltaFin = allowExtraScroll ? extraScrollDelta : 0.0f;

            Vector3 destPoint = accountingScrollFactor ? CalculateDestinationScroll(basePoint, scrollAmount, perFrameScroll) : basePoint + new Vector3(scrollAmount.x, scrollAmount.y);
            destPoint.x = Mathf.Clamp(destPoint.x, controlSet.ViewSize.x - size.x - extraScrollDeltaFin, extraScrollDeltaFin);
            destPoint.y = Mathf.Clamp(destPoint.y, -extraScrollDeltaFin, size.y - controlSet.ViewSize.y + extraScrollDeltaFin);

            Vector3 actualMoveAmount = (destPoint - basePoint) * (perFrameScroll ? (1.0f / Time.deltaTime) : 1.0f);

            if (scroll.x == 0)
            {
                actualMoveAmount.x = 0.0f;
            }
            else
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

        public Vector3 CalculateScrollAmountForLimitedPan(Vector2 scrollAmount, bool perFrameScroll = false, bool accountingScaleFactor = true, bool allowExtraScroll = false)
        {
            return CalculateScrollAmountForLimitedPanFromPos(transform.localPosition, scrollAmount, perFrameScroll, accountingScaleFactor, allowExtraScroll);
        }

        public Vector3 CalculateScrollAmountForLimitedPanFromOrigin(Vector2 scrollAmount, bool perFrameScroll = false, bool accountingScaleFactor = true, bool allowExtraScroll = false)
        {
            return CalculateScrollAmountForLimitedPanFromPos(originalPosition, scrollAmount, perFrameScroll, accountingScaleFactor, allowExtraScroll);
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
            }
            else
            {
                scrollSequence.Play();
            }
        }
    }
}