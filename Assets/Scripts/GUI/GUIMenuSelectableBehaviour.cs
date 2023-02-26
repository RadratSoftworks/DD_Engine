using UnityEngine;
using DG.Tweening;

namespace DDEngine.GUI
{
    public class GUIMenuSelectableBehaviour : MonoBehaviour
    {
        public float distanceXSinkBack = 0.15f;
        public float sinkDuration = 0.6f;

        private Vector2 originalPosition;
        private AudioSource selectedAudio;
        protected Sequence selectedSequence;

        // Start is called before the first frame update
        void Start()
        {
            DOTween.Init();
        }

        // Update is called once per frame
        void Update()
        {
        }

        public virtual void OnOptionSelected()
        {
            if (originalPosition == null)
            {
                originalPosition = transform.position;
            }

            if (selectedAudio == null)
            {
                selectedAudio = GetComponent<AudioSource>();
                selectedAudio.clip = SoundManager.Instance.GetAudioClip(FilePaths.MenuOptionSwitchSFXFileName);
            }

            // We can play later
            selectedAudio.Play();

            selectedSequence = DOTween.Sequence();
            selectedSequence.AppendInterval(selectedAudio.clip.length)
                .Append(transform.DOLocalMoveX(originalPosition.x - distanceXSinkBack, sinkDuration));
        }

        public virtual void OnOptionDeselected()
        {
            selectedAudio.Stop();
            selectedSequence.Kill();

            transform.DOLocalMoveX(originalPosition.x, sinkDuration);
        }
    }
}
