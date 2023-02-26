using Cysharp.Threading.Tasks;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class GUIBackgroundLabelController : MonoBehaviour
{
    [SerializeField]
    private TMP_Text labelText;

    [SerializeField]
    private SpriteRenderer boxSpriteRenderer;

    private RectTransform rectTransform;
    private Color fillColor;
    private string textId;

    private void UpdateText(GUIControlSet ownSet)
    {
        labelText.text = ownSet.GetLanguageString(textId);
        labelText.font = ResourceManager.Instance.GetFontAssetForLocalization();
        labelText.maxVisibleCharacters = labelText.text.Length;

        UpdateBackground();
    }

    private void UpdateBackground()
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);

        boxSpriteRenderer.size = rectTransform.sizeDelta;
        boxSpriteRenderer.color = fillColor;
    }

    private Vector2 GetPivotFromString(string pivot)
    {
        switch (pivot)
        {
            case "c":
                return new Vector2(0.5f, 0.5f);

            case "tl":
                return new Vector2(0.0f, 1.0f);

            case "tr":
                return new Vector2(1.0f, 1.0f);

            default:
                Debug.LogFormat("Unknown pivot type {0}", pivot);
                return new Vector2(0.5f, 0.5f);
        }
    }

    public void Setup(GUIControlSet ownSet, GUIControlBackgroundLabelDescription description)
    {
        this.textId = description.TextId;
        this.fillColor = description.FillColor;

        rectTransform = GetComponent<RectTransform>();
        rectTransform.pivot = GetPivotFromString(description.Pivot);
        rectTransform.localPosition = GameUtils.ToUnityCoordinates(description.Position);
        Vector2 marginSizeNorm = GameUtils.ToUnitySize(description.MarginSize);

        labelText.color = description.TextColor;
        labelText.margin = new Vector4(marginSizeNorm.x, marginSizeNorm.y, marginSizeNorm.x,
            marginSizeNorm.y);

        var labelTextRenderer = labelText.GetComponentInChildren<MeshRenderer>();
        labelTextRenderer.sortingOrder = GameUtils.ToUnitySortingPosition(description.AbsoluteDepth);
        boxSpriteRenderer.sortingOrder = GameUtils.ToUnitySortingPosition(description.AbsoluteDepth + 1);

        UpdateText(ownSet);
        ownSet.LocalizationChanged += UpdateText;
    }

    private void OnEnable()
    {
        UpdateBackgroundTask().Forget();
    }

    private async UniTask UpdateBackgroundTask()
    {
        await UniTask.Yield();
        UpdateBackground();
    }
}