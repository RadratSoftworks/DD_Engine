using System;
using System.Collections.Generic;
using UnityEngine;

public static class MinigameConstructUtils
{
    public static SpriteAnimatorController InstantiateAndGet(GameObject prefab, Transform parent,
        string path, Vector2 position, float sortOrder = 0.0f, bool deactiveByDefault = true,
        bool allowLoop = true)
    {
        SpriteAnimatorController controller = UnityUtils.InstantiateAndGetComponent<SpriteAnimatorController>(prefab, parent, path);
        if (controller != null)
        {
            controller.Setup(position, sortOrder, path, allowLoop: allowLoop);
        }
        controller.gameObject.SetActive(deactiveByDefault ? false : true);
        return controller;
    }

    public static BoxCollider2D SetupBoundsObject(GameObject boundsObj, Rect bounds) {
        boundsObj.transform.localPosition = GameUtils.ToUnityCoordinates(bounds.center);
        BoxCollider2D collider = boundsObj.GetComponent<BoxCollider2D>();
        if (collider != null)
        {
            collider.size = GameUtils.ToUnitySize(bounds.size);
        }
        return collider;
    }
}
