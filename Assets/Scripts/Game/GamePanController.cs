using System.Collections;
using UnityEngine;

namespace DDEngine.Game
{
    public class GamePanController : MonoBehaviour
    {
        private IEnumerator PanCoroutine(Vector2 targetPosition, float duration)
        {
            Vector3 destDelta = new Vector3(targetPosition.x, targetPosition.y, 0) - transform.localPosition;
            Vector3 source = transform.localPosition;
            Vector3 destLocal = transform.localPosition + destDelta;

            float timePassed = 0.0f;

            while (timePassed < duration)
            {
                transform.localPosition = Vector3.Lerp(source, destLocal, timePassed / duration);
                
                yield return null;
                timePassed += Time.deltaTime;
            }

            transform.localPosition = destLocal;
            yield break;
        }

        public void Pan(Vector2 targetPosition, float duration)
        {
            StopAllCoroutines();

            if (duration <= 0.0f)
            {
                transform.localPosition = targetPosition;
                return;
            }

            StartCoroutine(PanCoroutine(targetPosition, duration));
        }
    }
}