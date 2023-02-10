using System;
using Unity.VisualScripting;
using UnityEngine;

public class GUILabelController : MonoBehaviour
{
    private TMPro.TMP_Text labelText;
    private MeshRenderer labelTextRenderer;

    public void Setup(Vector2 position, string text, int depth)
    {
        transform.localPosition = GameUtils.ToUnityCoordinates(position);

        labelText = GetComponent<TMPro.TMP_Text>();
        labelText.font = ResourceManager.Instance.GetFontAssetForLocalization();
        labelText.text = text;

        labelTextRenderer = GetComponentInChildren<MeshRenderer>();
        labelTextRenderer.sortingOrder = GameUtils.ToUnitySortingPosition(depth);
    }
}
