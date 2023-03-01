using System.Collections;
using UnityEngine;

namespace DDEngine.Game
{
    public class GameImageFadeController : MonoBehaviour
    {
        public enum FadeType
        {
            In,
            Out,
            ToValue
        };

        private SpriteRenderer spriteRenderer;

        private IEnumerator FadeCoroutine(float destValue, int frames)
        {
            Color currentColor = spriteRenderer.color;
            float addition = (destValue - currentColor.a) / frames;

            for (int i = 0; i < frames; i++)
            {
                currentColor.a += addition;
                spriteRenderer.color = currentColor;

                yield return null;
            }

            currentColor.a = destValue;
            spriteRenderer.color = currentColor;
        }

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        public void Fade(FadeType fadeType, int frames, int value = -1)
        {
            StopAllCoroutines();

            if (frames == 0)
            {
                Color currentColorImm = spriteRenderer.color;
                switch (fadeType)
                {
                    case FadeType.In:
                        currentColorImm.a = 1.0f;
                        break;

                    case FadeType.Out:
                        currentColorImm.a = 0.0f;
                        break;

                    default:
                        currentColorImm.a = Mathf.Clamp(value / 255.0f, 0.0f, 1.0f);
                        break;
                }

                spriteRenderer.color = currentColorImm;
                return;
            }

            Color currentColor = spriteRenderer.color;
            float targetAlpha = Mathf.Clamp(value / 255.0f, 0.0f, 1.0f);

            switch (fadeType)
            {
                case FadeType.In:
                    currentColor.a = 0.0f;
                    targetAlpha = 1.0f;
                    break;

                case FadeType.Out:
                    currentColor.a = 1.0f;
                    targetAlpha = 0.0f;
                    break;

                default:
                    break;
            }

            spriteRenderer.color = currentColor;
            StartCoroutine(FadeCoroutine(targetAlpha, frames));
        }
    }
}