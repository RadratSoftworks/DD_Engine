using DDEngine.GUI.Parser;
using DDEngine.Utils;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

namespace DDEngine.GUI
{
    public class GUIScrollingTextController : MonoBehaviour
    {
        private TMPro.TMP_Text scrollingText;
        private string textId;
        private Vector2 scrollSpeed;
        private RectTransform rectTransform;
        private float scrollUnitsPerFrame;

        private void UpdateText(GUIControlSet ownSet)
        {
            scrollingText = GetComponent<TMPro.TMP_Text>();
            scrollingText.font = ResourceManager.Instance.GetFontAssetForLocalization();
            scrollingText.text = TextFormatting.PostTransform(ownSet.GetLanguageString(textId));
        }

        public void Setup(GUIControlSet ownSet, GUIControlScrollingTextDescription description)
        {
            this.textId = description.Text;
            this.scrollSpeed = description.Scroll;

            transform.localPosition = GameUtils.ToUnityCoordinates(description.TopPosition);

            rectTransform = GetComponent<RectTransform>();
            rectTransform.sizeDelta = GameUtils.ToUnitySize(new Vector2(description.Width, 0));

            MeshRenderer textRenderer = GetComponentInChildren<MeshRenderer>();
            textRenderer.sortingOrder = GameUtils.ToUnitySortingPosition(description.AbsoluteDepth);

            UpdateText(ownSet);
            ownSet.LocalizationChanged += UpdateText;
        }

        private void Awake()
        {
            DOTween.Init();
        }

        private void Start()
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);

            Vector2 moveDest = Vector2.zero;
            if (scrollSpeed.x != 0)
            {
                moveDest.x = -rectTransform.sizeDelta.x;
            } else
            {
                moveDest.x = transform.localPosition.x;
            }

            if (scrollSpeed.y != 0)
            {
                moveDest.y = rectTransform.sizeDelta.y;
            } else
            {
                moveDest.y = transform.localPosition.y;
            }

            Vector2 normalizedScrollSpeed = GameUtils.ToUnityCoordinates(scrollSpeed);

            float moveXDuration = (normalizedScrollSpeed.x == 0) ? 0.0f : (moveDest.x - transform.localPosition.x) / (normalizedScrollSpeed.x / 100.0f * Constants.RealScrollFps);
            float moveYDuration = (normalizedScrollSpeed.y == 0) ? 0.0f : (moveDest.y - transform.localPosition.y) / (normalizedScrollSpeed.y / 100.0f * Constants.RealScrollFps);

            transform.DOLocalMove(moveDest, Mathf.Max(moveXDuration, moveYDuration)).SetEase(Ease.Linear);
        }
    }
}
