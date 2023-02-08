using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Tried to use vertical layout but it is very inconsistent with pivots...
public class GameTextBalloonController : MonoBehaviour
{
    private TMPro.TMP_Text ballonText;
    private RectTransform rectTransform;
    private SpriteRenderer backgroundRenderer;
    private SpriteRenderer balloonRenderer;
    private SpriteRenderer stingerRenderer;

    public GameObject backgroundObject;
    public GameObject textObject;
    public GameObject balloonObject;
    public GameObject stingerObject;

    public Color normalTextBackgroundColor = Color.white;
    public Color fullItalicTextBackgroundColor = Color.yellow;

    private bool isBottom = false;
    private Vector2 canvasSize = new Vector2(0, 0);
    private Vector2 stingerPositionRelative = new Vector2(0, 0);

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

        rectTransform.pivot = this.isBottom ? Vector2.zero : Vector2.up;
        transform.localPosition = sizeTransformed * (this.isBottom ? Vector2.down : Vector2.zero);
    }

    private static bool IsTextFullItalic(string text)
    {
        return text.StartsWith("<i>") && text.EndsWith("</i>");
    }

    private IEnumerator GraduallyAppearTextCoroutine()
    {
        while (ballonText.maxVisibleCharacters < ballonText.text.Length)
        {
            ballonText.maxVisibleCharacters = Mathf.Min(ballonText.maxVisibleCharacters + 1, ballonText.text.Length);

            // Force the content fitter to immediately recalculate
            // Else sometimes the text region size will just be delayed and cause flashing!
            LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);

            backgroundRenderer.size = rectTransform.sizeDelta;
            balloonRenderer.transform.localPosition = rectTransform.sizeDelta * (isBottom ? Vector2.up : Vector2.down);

            if (stingerObject.activeSelf)
            {
                AdjustStingerPosition();
            }

            yield return null;
        }

        yield break;
    }

    public void Setup(Vector2 size, bool isBottom)
    {
        // More data initializations will be done in Awake
        this.isBottom = isBottom;
        this.canvasSize = size;
    }

    public void HideText()
    {
        gameObject.SetActive(false);
    }

    public void ChangeText(string newText)
    {
        gameObject.SetActive(true);
        StopCoroutine(GraduallyAppearTextCoroutine());

        if (IsTextFullItalic(newText))
        {
            backgroundRenderer.color = balloonRenderer.color = fullItalicTextBackgroundColor;
        } else
        {
            backgroundRenderer.color = balloonRenderer.color = normalTextBackgroundColor;
        }

        ballonText.maxVisibleCharacters = 0;
        ballonText.text = newText;
        ballonText.font = ResourceManager.Instance.GetFontAssetForLocalization();

        stingerObject.SetActive(false);

        backgroundRenderer.size = rectTransform.sizeDelta;
        balloonRenderer.transform.localPosition = rectTransform.sizeDelta * (isBottom ? Vector2.up :Vector2.down);

        StartCoroutine(GraduallyAppearTextCoroutine());
    }

    public void SetBalloon(string balloonFilename)
    {
        balloonRenderer.sprite = SpriteManager.Instance.Load(ResourceManager.Instance.GeneralResources,
            balloonFilename, isBottom ? Vector2.zero : Vector2.up);
    }

    public void SetStinger(Vector2 stingerPos, string stingerFilename)
    {
        stingerObject.SetActive(true);

        stingerRenderer.sprite = SpriteManager.Instance.Load(ResourceManager.Instance.GeneralResources,
            stingerFilename, isBottom ? Vector2.zero : Vector2.up);

        stingerPositionRelative = GameUtils.ToUnityCoordinates(stingerPos);
        AdjustStingerPosition();
    }

    private void AdjustStingerPosition()
    {
        stingerObject.transform.localPosition = balloonObject.transform.localPosition + new Vector3(stingerPositionRelative.x, stingerPositionRelative.y * (isBottom ? -1.0f : 1.0f), 0.0f);
    }

    public void OnSkip()
    {
        if (ballonText.maxVisibleCharacters < ballonText.text.Length)
        {
            StopCoroutine(GraduallyAppearTextCoroutine());
            ballonText.maxVisibleCharacters = ballonText.text.Length;
        }
    }

    public bool LastTextFinished => !gameObject.activeSelf || (ballonText.maxVisibleCharacters == ballonText.text.Length);
}
