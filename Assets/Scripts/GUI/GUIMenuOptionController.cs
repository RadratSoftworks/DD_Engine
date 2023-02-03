using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class GUIMenuOptionController : MonoBehaviour
{
    public float distanceXSinkBack = 0.15f;
    public float sinkDuration = 0.6f;

    private Vector2 originalPosition;

    // Start is called before the first frame update
    void Start()
    {
        DOTween.Init();
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void OnOptionSelected()
    {
        if (originalPosition == null)
        {
            originalPosition = transform.position;
        }

        transform.DOLocalMoveX(originalPosition.x - distanceXSinkBack, sinkDuration);
    }

    public void OnOptionDeselected()
    {
        transform.DOLocalMoveX(originalPosition.x, sinkDuration);
    }
}
