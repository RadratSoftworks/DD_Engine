using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class GUIMenuOptionController : MonoBehaviour
{
    public float distanceXSinkBack = 0.15f;
    public float sinkDuration = 0.6f;

    private Vector2 originalPosition;
    private AudioSource selectedAudio;
    private Sequence selectedSequence;

    // Start is called before the first frame update
    void Start()
    {
        selectedAudio = GetComponent<AudioSource>();
        selectedAudio.clip = SoundManager.Instance.GetAudioClip(FilePaths.MenuOptionSwitchSFXFileName);

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

        selectedAudio.Play();

        selectedSequence = DOTween.Sequence();
        selectedSequence.AppendInterval(selectedAudio.clip.length)
            .Append(transform.DOLocalMoveX(originalPosition.x - distanceXSinkBack, sinkDuration));
    }

    public void OnOptionDeselected()
    {
        selectedAudio.Stop();
        selectedSequence.Kill();

        transform.DOLocalMoveX(originalPosition.x, sinkDuration);
    }
}
