using System;
using System.Collections.Generic;
using UnityEngine;

public static class UnityUtils
{
    public static T InstantiateAndGetComponent<T>(GameObject prefab, Transform parent, string name = null)
    {
        GameObject obj = GameObject.Instantiate(prefab, parent, false);
        obj.transform.localPosition = Vector3.zero;
        obj.SetActive(false);

        if (name != null)
        {
            obj.name = GameUtils.ToUnityName(name);
        }

        return obj.GetComponent<T>();
    }
}
