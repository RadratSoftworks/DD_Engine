using System.Collections;
using UnityEngine;

namespace DDEngine.Game
{
    public class GameImageFadeController : MonoBehaviour
    {
        private SpriteRenderer spriteRenderer;

        private IEnumerator FadeCoroutine(bool isFadeIn, int frames)
        {
            Color currentColor = spriteRenderer.color;

            float addition = 1.0f / frames;
            if (!isFadeIn)
            {
                addition *= -1.0f;
                currentColor.a = 1.0f;
            }
            else
            {
                currentColor.a = 0.0f;
            }

            for (int i = 0; i < frames; i++)
            {
                currentColor.a += addition;
                spriteRenderer.color = currentColor;

                yield return null;
            }

            currentColor.a = (isFadeIn) ? 1.0f : 0.0f;
            spriteRenderer.color = currentColor;
        }

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        public void Fade(bool isFadeIn, int frames)
        {
            StopAllCoroutines();

            if (frames == 0)
            {
                Color currentColor = spriteRenderer.color;
                currentColor.a = (isFadeIn) ? 1.0f : 0.0f;
                spriteRenderer.color = currentColor;

                return;
            }

            StartCoroutine(FadeCoroutine(isFadeIn, frames));
        }
    }
}