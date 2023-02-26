using UnityEngine;
using DG.Tweening;

namespace DDEngine.Minigame.Fight
{
    public class FightHealthBarController : MonoBehaviour
    {
        private const string HeartBeatFrameImagePath = "img/minigames/ch2_fight/Bar_FG_2_Frame{0}";
        private const string BarIndicatorImagePath = "img/minigames/ch2_fight/Bar_Indicator_{0}";
        private const string BarBGImagePath = "img/minigames/ch2_fight/Bar_BG_{0}";
        private const string BarDeathImagePath = "img/minigames/ch2_fight/Bar_FG_{0}_1";
        private const int TotalHeartbeatFrames = 3;
        private const int HeartbeatPerFrameDuration = 6;

        private FighterHealthController healthController;

        [SerializeField]
        private float healthReduceAnimDuration = 0.8f;

        [SerializeField]
        private SpriteAnimatorController heartbeatAnimationController;

        [SerializeField]
        private GameObject healthBarBG;

        [SerializeField]
        private GameObject healthBarIndicator;

        [SerializeField]
        private SpriteRenderer deathSpriteRenderer;

        private bool isLeft;

        private float indicatorLength;

        private Vector3 originalPosition;

        private void Start()
        {
            healthController.HealthChanged += OnHealthChange;
            DOTween.Init();
        }

        private void SetupHeartbeatAnimation()
        {
            for (int i = 0; i < TotalHeartbeatFrames; i++)
            {
                heartbeatAnimationController.AddFrame(string.Format(HeartBeatFrameImagePath, i + 1), Vector2.zero, HeartbeatPerFrameDuration, false);
            }

            heartbeatAnimationController.SetOriginalPositionToCurrent();
        }

        public void Setup(FighterHealthController healthController, bool isLeft)
        {
            this.isLeft = isLeft;
            this.healthController = healthController;

            string pathAppendString = isLeft ? "left" : "right";

            SetupHeartbeatAnimation();

            SpriteRenderer healthBarBgRenderer = healthBarBG.GetComponent<SpriteRenderer>();
            SpriteMask healthBarIndicatorMask = healthBarBG.GetComponent<SpriteMask>();

            healthBarBgRenderer.sprite = SpriteManager.Instance.Load(ResourceManager.Instance.GeneralResources,
                string.Format(BarBGImagePath, pathAppendString));

            healthBarIndicatorMask.sprite = healthBarBgRenderer.sprite;
            deathSpriteRenderer.sprite = SpriteManager.Instance.Load(ResourceManager.Instance.GeneralResources,
                string.Format(BarDeathImagePath, pathAppendString));

            SpriteRenderer healthBarIndicatorRenderer = healthBarIndicator.GetComponent<SpriteRenderer>();
            healthBarIndicatorRenderer.sprite = SpriteManager.Instance.Load(ResourceManager.Instance.GeneralResources,
                string.Format(BarIndicatorImagePath, pathAppendString));

            indicatorLength = healthBarIndicatorRenderer.sprite.bounds.size.x;
            originalPosition = healthBarIndicator.transform.localPosition;
        }

        private void OnHealthChange()
        {
            float percentage = (float)healthController.CurrentHealth / healthController.MaxHealth;
            Vector3 xAdv = (1.0f - percentage) * indicatorLength * (isLeft ? Vector3.left : Vector3.right);

            healthBarIndicator.transform.DOLocalMove(originalPosition + xAdv, healthReduceAnimDuration);
        }
    }
}