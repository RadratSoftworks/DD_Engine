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

        private IEnumerator FadeCoroutine(float destValue, float duration)
        {
            Color currentColor = spriteRenderer.color;
            float sourceAlpha = currentColor.a;

            float elapsedTime = 0.0f;

            while (elapsedTime < duration)
            {
                currentColor.a = Mathf.Lerp(sourceAlpha, destValue, elapsedTime / duration);
                spriteRenderer.color = currentColor;

                yield return null;
                elapsedTime += Time.deltaTime;
            }

            currentColor.a = destValue;
            spriteRenderer.color = currentColor;
        }

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        public void Fade(FadeType fadeType, float duration, int value = -1)
        {
            StopAllCoroutines();

            if (duration <= 0.0f)
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
            StartCoroutine(FadeCoroutine(targetAlpha, duration));
        }
    }
}