using System;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class FightOpponentMovingHandsController : MonoBehaviour
{
    private Sequence moveSequence;
    private Vector3 originalPosition;

    [SerializeField]
    private float moveDelta = 0.05f;

    [SerializeField]
    private float moveDuration = 1.0f;

    private void Awake()
    {
        originalPosition = transform.position;
    }

    private void Start()
    {
        DOTween.Init();
    }

    private void OnEnable()
    {
        transform.localPosition = originalPosition;

        moveSequence = DOTween.Sequence()
            .Append(transform.DOLocalMove(originalPosition + Vector3.up * moveDelta, moveDuration / 2))
            .Append(transform.DOLocalMove(originalPosition + Vector3.down * moveDelta, moveDuration / 2))
            .SetLoops(-1);
    }

    private void OnDisable()
    {
        moveSequence.Kill();
    }
}