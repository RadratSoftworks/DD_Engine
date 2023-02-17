using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GUIBackgroundLabelController : MonoBehaviour
{
    [SerializeField]
    private TMP_Text labelText;

    [SerializeField]
    private SpriteRenderer boxSpriteRenderer;

    public void Setup(GUIControlSet ownSet, GUIControlBackgroundLabelDescription description)
    {
        RectTransform transform = GetComponent<RectTransform>();
        transform.localPosition = GameUtils.ToUnityCoordinates(description.TopPosition);
        Vector2 marginSizeNorm = GameUtils.ToUnitySize(description.MarginSize);

        labelText.text = ownSet.GetLanguageString(description.TextId);
        labelText.color = description.TextColor;
        labelText.font = ResourceManager.Instance.GetFontAssetForLocalization();
        labelText.margin = new Vector4(marginSizeNorm.x, marginSizeNorm.y, marginSizeNorm.x,
            marginSizeNorm.y);

        var labelTextRenderer = labelText.GetComponentInChildren<MeshRenderer>();
        labelTextRenderer.sortingOrder = GameUtils.ToUnitySortingPosition(description.AbsoluteDepth);

        LayoutRebuilder.ForceRebuildLayoutImmediate(transform);

        boxSpriteRenderer.size = transform.sizeDelta;
        boxSpriteRenderer.color = description.FillColor;
        boxSpriteRenderer.sortingOrder = GameUtils.ToUnitySortingPosition(description.AbsoluteDepth + 1);
    }
}