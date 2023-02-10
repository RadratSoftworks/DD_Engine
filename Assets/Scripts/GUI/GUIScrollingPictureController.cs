using System;
using UnityEngine;
using UnityEngine.Serialization;

public class GUIScrollingPictureController : MonoBehaviour
{
    private static readonly string ScrollSpeedShaderVarName = "_ScrollSpeed";
    private static readonly string MoveAmountShaderVarName = "_MoveAmount";

    private SpriteRenderer spriteRenderer;
    private Vector2 scroll;

    [SerializeField]
    private float scrollFactor = 100.0f;

    [SerializeField]
    private float moveAmountInPixels = 20.0f;

    private void Awake()
    {
    }

    private void Update()
    {
        // It's time based in the shader at the moment
        // I prefer it that way. But we might need to change to frame-based?
        spriteRenderer.material.SetVector(ScrollSpeedShaderVarName, scroll / scrollFactor);
        spriteRenderer.material.SetFloat(MoveAmountShaderVarName, moveAmountInPixels);
    }

    public void Setup(Sprite picture, Vector2 position, Vector2 scroll, int depth)
    {
        this.scroll = scroll;
        this.transform.localPosition = GameUtils.ToUnityCoordinates(position);

        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = picture;
        spriteRenderer.sortingOrder = GameUtils.ToUnitySortingPosition(depth);
    }
}