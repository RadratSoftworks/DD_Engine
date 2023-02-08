using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GUICanvasSetup : MonoBehaviour
{
    public GameObject activeCollideGameObject;

    private BoxCollider2D activeCollider;

    void Awake()
    {
        activeCollider = activeCollideGameObject.GetComponent<BoxCollider2D>();
        activeCollider.enabled = false;
    }

    public void SetCanvasSize(int width, int height)
    {
        activeCollider.size = GameUtils.ToUnitySize(new Vector2(2, 2));
        activeCollider.transform.localPosition = GameUtils.ToUnityCoordinates(new Vector2(width / 2, height / 2));
        activeCollider.enabled = true;
    }
}
