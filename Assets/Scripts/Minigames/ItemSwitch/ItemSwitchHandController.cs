using DG.Tweening;
using UnityEngine;

using DDEngine.Utils;

namespace DDEngine.Minigame.ItemSwitch
{
    public class ItemSwitchHandController : MonoBehaviour
    {
        private SpriteAnimatorController handAnimatorController;
        private Sequence shakeSequence;
        private Vector2 position;

        [SerializeField]
        private float shakePatternDuration = 10.0f;

        [SerializeField]
        private float stableRevertDuration = 0.2f;

        [SerializeField]
        private float bitStressShakeStrength = 0.05f;

        [SerializeField]
        private float hardStressShakeStrength = 0.2f;

        [SerializeField]
        private int bitStressShakeVibrato = 3;

        [SerializeField]
        private int hardStressShakeVibrato = 5;

        private void Awake()
        {
            handAnimatorController = GetComponent<SpriteAnimatorController>();
        }

        private void Start()
        {
            DOTween.Init();
        }

        public void Setup(ItemSwitchStressMachineController indicator, ItemSwitchDirkHandInfo handInfo)
        {
            transform.localPosition = GameUtils.ToUnityCoordinates(handInfo.OriginalPosition);
            handAnimatorController.Setup(handInfo.OriginalPosition, SpriteAnimatorController.SortOrderNotSet, handInfo.AnimationPath);

            indicator.StressStatusEntered += OnStressStatusEntered;
            shakeSequence = DOTween.Sequence();
            position = transform.localPosition;
        }

        public void Shutdown()
        {
            shakeSequence?.Kill();
        }

        private void OnStressStatusEntered(ItemSwitchStressStatus status)
        {
            shakeSequence?.Kill();

            shakeSequence = DOTween.Sequence();
            shakeSequence.Append(transform.DOLocalMove(position, stableRevertDuration));

            switch (status)
            {
                case ItemSwitchStressStatus.Stable:
                    break;

                case ItemSwitchStressStatus.Average:
                    shakeSequence.Append(transform.DOShakePosition(shakePatternDuration, strength: bitStressShakeStrength, vibrato: bitStressShakeVibrato).SetEase(Ease.Unset))
                        .SetLoops(-1);
                    break;

                case ItemSwitchStressStatus.Hard:
                    shakeSequence.Append(transform.DOShakePosition(shakePatternDuration, strength: hardStressShakeStrength, vibrato: hardStressShakeVibrato).SetEase(Ease.Unset))
                        .SetLoops(-1);
                    break;

                case ItemSwitchStressStatus.Shutdown:
                    Shutdown();
                    break;
            }
        }
    }
}