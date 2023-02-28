using System.Collections;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

using DDEngine.Utils;

namespace DDEngine.Game
{
    // Tried to use vertical layout but it is very inconsistent with pivots...
    public class GameTextBalloonController : MonoBehaviour
    {
        public enum Placement
        {
            Top,
            Middle,
            Bottom
        }

        private const string SkipActionName = "Skip";

        private TMPro.TMP_Text ballonText;
        private RectTransform rectTransform;
        private SpriteRenderer backgroundRenderer;
        private SpriteRenderer balloonRenderer;
        private SpriteRenderer stingerRenderer;

        public GameObject backgroundObject;
        public GameObject textObject;
        public GameObject balloonObject;
        public GameObject stingerObject;

        private string pendingBalloon = null;
        private string pendingStinger = null;
        private Vector2 pendingStingerPos;

        public Color normalTextBackgroundColor = Color.white;
        public Color fullItalicTextBackgroundColor = Color.yellow;
        public Color fullMiddleTextBackgroundColor = Color.clear;
        public Color fullMiddleTextColor = Color.white;
        public float timePerCharacterReveal = 0.015f;
        public float timePerCharacterRevealFast = 0.008f;

        private Placement textPlacement = Placement.Top;
        private Vector2 canvasSize = new Vector2(0, 0);
        private Vector2 stingerPositionRelative = new Vector2(0, 0);

        private IEnumerator currentTextCoroutine;

        private Vector2 GetPivot()
        {
            switch (textPlacement)
            {
                case Placement.Top:
                    return Vector2.up;

                case Placement.Middle:
                    return new Vector2(0.5f, 0.5f);

                case Placement.Bottom:
                    return Vector2.zero;

                default:
                    throw new System.Exception("Unknown text placement type!");
            }
        }

        private Vector2 GetSpritePivot()
        {
            switch (textPlacement)
            {
                case Placement.Top:
                    return Vector2.up;

                case Placement.Middle:
                    return new Vector2(0.5f, 0.5f);

                case Placement.Bottom:
                    return Vector2.zero;

                default:
                    throw new System.Exception("Unknown text placement type!");
            }
        }

        void Awake()
        {
            ballonText = textObject.GetComponent<TMPro.TMP_Text>();
            rectTransform = textObject.GetComponent<RectTransform>();
            backgroundRenderer = backgroundObject.GetComponent<SpriteRenderer>();
            balloonRenderer = balloonObject.GetComponent<SpriteRenderer>();
            stingerRenderer = stingerObject.GetComponent<SpriteRenderer>();

            // Try to expand the balloon to fill horizontally
            Vector2 sizeTransformed = GameUtils.ToUnitySize(canvasSize);
            rectTransform.sizeDelta = new Vector2(sizeTransformed.x, rectTransform.sizeDelta.y);

            rectTransform.pivot = GetPivot();
            transform.localPosition = sizeTransformed * ((textPlacement == Placement.Bottom) ? Vector2.down : (textPlacement == Placement.Top) ? Vector2.zero : new Vector2(0.5f, -0.5f));
        }

        private void Start()
        {
            var dialogueBalloonMap = GameInputManager.Instance.DialogueBalloonActionMap;
            dialogueBalloonMap.Enable();

            InputAction skipAction = dialogueBalloonMap.FindAction(SkipActionName);
            skipAction.performed += OnSkip;
        }

        private void OnDestroy()
        {
            var dialogueBalloonMap = GameInputManager.Instance.DialogueBalloonActionMap;
            dialogueBalloonMap.Disable();

            InputAction skipAction = dialogueBalloonMap.FindAction(SkipActionName);
            skipAction.performed -= OnSkip;
        }

        private void OnEnable()
        {
            GameInputManager.Instance.DialogueBalloonActionMap.Enable();
        }

        private void OnDisable()
        {
            GameInputManager.Instance.DialogueBalloonActionMap.Disable();
        }

        private IEnumerator GraduallyAppearTextCoroutine()
        {
            var waitTime = new WaitForSeconds((GameSettings.TextSpeed == GameTextSpeed.Normal) ? timePerCharacterReveal : timePerCharacterRevealFast);

            while (ballonText.maxVisibleCharacters < ballonText.text.Length)
            {
                ballonText.maxVisibleCharacters = Mathf.Min(ballonText.maxVisibleCharacters + 1, ballonText.text.Length);

                // Force the content fitter to immediately recalculate
                // Else sometimes the text region size will just be delayed and cause flashing!
                LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);

                backgroundRenderer.size = rectTransform.sizeDelta;

                if (textPlacement != Placement.Middle)
                {
                    balloonRenderer.transform.localPosition = rectTransform.sizeDelta * ((textPlacement == Placement.Bottom) ? Vector2.up : Vector2.down);
                }

                if (stingerObject.activeSelf)
                {
                    AdjustStingerPosition();
                }

                yield return waitTime;
            }

            yield break;
        }

        public void Setup(Vector2 size, Placement textPlacement)
        {
            // More data initializations will be done in Awake
            this.textPlacement = textPlacement;
            this.canvasSize = size;
        }

        public void HideText()
        {
            gameObject.SetActive(false);
        }

        public void ChangeText(string newText)
        {
            gameObject.SetActive(true);

            if (currentTextCoroutine != null)
            {
                StopCoroutine(currentTextCoroutine);
                currentTextCoroutine = null;
            }

            if (TextFormatting.IsTextFullItalic(newText))
            {
                backgroundRenderer.color = balloonRenderer.color = fullItalicTextBackgroundColor;
            }
            else if (TextFormatting.IsTextFullMiddle(newText))
            {
                backgroundRenderer.color = balloonRenderer.color = fullMiddleTextBackgroundColor;
                ballonText.color = fullMiddleTextColor;
            }
            else
            {
                backgroundRenderer.color = balloonRenderer.color = normalTextBackgroundColor;
            }

            newText = TextFormatting.PostTransform(newText);

            ballonText.maxVisibleCharacters = (GameSettings.TextSpeed == GameTextSpeed.Instant) ? newText.Length : 0;
            ballonText.text = newText;
            ballonText.font = ResourceManager.Instance.GetFontAssetForLocalization();

            stingerObject.SetActive(false);

            if (GameSettings.TextSpeed == GameTextSpeed.Instant)
            {
                // Rebuild immediately to calculate background size, not rely on coroutine
                LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
            }

            backgroundRenderer.size = rectTransform.sizeDelta;

            if (textPlacement != Placement.Middle)
            {
                balloonRenderer.transform.localPosition = rectTransform.sizeDelta * GetPivot();
            }

            if (pendingBalloon != null)
            {
                UpdateBalloonImpl();
            }

            if (pendingStinger != null)
            {
                UpdateStingerImpl();
            }

            if (GameSettings.TextSpeed != GameTextSpeed.Instant)
            {
                currentTextCoroutine = GraduallyAppearTextCoroutine();
                StartCoroutine(currentTextCoroutine);
            }
        }

        private void UpdateBalloonImpl()
        {
            balloonRenderer.sprite = SpriteManager.Instance.Load(ResourceManager.Instance.GeneralResources,
                pendingBalloon, GetSpritePivot());

            pendingBalloon = null;
        }

        private void UpdateStingerImpl()
        {
            stingerObject.SetActive(true);

            stingerRenderer.sprite = SpriteManager.Instance.Load(ResourceManager.Instance.GeneralResources,
                pendingStinger, GetSpritePivot());

            stingerPositionRelative = GameUtils.ToUnityCoordinates(pendingStingerPos);
            AdjustStingerPosition();

            pendingStinger = null;
        }

        public void SetBalloon(string balloonFilename)
        {
            pendingBalloon = balloonFilename;
            if (gameObject.activeSelf)
            {
                UpdateBalloonImpl();
            }
        }

        public void SetStinger(Vector2 stingerPos, string stingerFilename)
        {
            pendingStingerPos = stingerPos;
            pendingStinger = stingerFilename;

            if (gameObject.activeSelf)
            {
                UpdateStingerImpl();
            }
        }

        private void AdjustStingerPosition()
        {
            stingerObject.transform.localPosition = balloonObject.transform.localPosition + new Vector3(stingerPositionRelative.x, stingerPositionRelative.y * ((textPlacement == Placement.Bottom) ? -1.0f : 1.0f), 0.0f);
        }

        public void OnSkip(InputAction.CallbackContext context)
        {
            if (ballonText.maxVisibleCharacters < ballonText.text.Length)
            {
                if (currentTextCoroutine != null)
                {
                    StopCoroutine(currentTextCoroutine);
                    currentTextCoroutine = null;
                }

                ballonText.maxVisibleCharacters = ballonText.text.Length;
            }
        }

        public bool LastTextFinished => !gameObject.activeSelf || (ballonText.maxVisibleCharacters == ballonText.text.Length);
    }
}