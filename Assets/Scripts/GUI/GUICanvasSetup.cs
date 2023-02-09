using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GUICanvasSetup : MonoBehaviour
{
    public GameObject activeCollideGameObject;
    private BoxCollider2D activeCollider;

    public static Vector2 ActivePositionToViewCenterPoint(Vector2 position)
    {
        return position + new Vector2(1.0f, 0.0f) / Constants.PixelsPerUnit;
    }

    void Awake()
    {
        activeCollider = activeCollideGameObject.GetComponent<BoxCollider2D>();
        activeCollider.enabled = false;
    }

    public void SetCanvasSize(int width, int height)
    {
        activeCollider.size = GameUtils.ToUnitySize(new Vector2(1, 1));
        activeCollider.transform.localPosition = GameUtils.ToUnityCoordinates(new Vector2((width / 2) - 1, height / 2));
        activeCollider.enabled = true;
    }
}
