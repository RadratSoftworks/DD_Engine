using UnityEngine;
using DG.Tweening;

namespace DDEngine.Installer
{
    public class AboutPopupController : MonoBehaviour
    {
        [SerializeField]
        private CanvasGroup canvasGroup;

        [SerializeField]
        private float fadeDuration = 0.3f;

        private void Awake()
        {
            DOTween.Init();
        }

        private void Start()
        {
            OnEnable();
        }

        private void OnEnable()
        {
            canvasGroup.DOFade(1.0f, fadeDuration);
        }

        public void OnClick()
        {
            canvasGroup.DOFade(0.0f, fadeDuration)
                .OnComplete(() => gameObject.SetActive(false));
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }
    }
}
