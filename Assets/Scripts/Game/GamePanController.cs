using System;
using System.Collections;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

public class GamePanController: MonoBehaviour
{
    private IEnumerator PanCoroutine(Vector2 targetPosition, int frames)
    {
        Vector3 increasePerSe = (new Vector3(targetPosition.x, targetPosition.y, 0) - transform.localPosition) / frames;
        increasePerSe.z = 0.0f;

        for (int i = 0; i < frames; i++)
        {
            transform.localPosition += increasePerSe;
            yield return null;
        }

        transform.localPosition = targetPosition;
        yield break;
    }

    public void Pan(Vector2 targetPosition, int frames)
    {
        StopAllCoroutines();

        if (frames == 0)
        {
            transform.localPosition = targetPosition;
            return;
        }

        StartCoroutine(PanCoroutine(targetPosition, frames));
    }
}
