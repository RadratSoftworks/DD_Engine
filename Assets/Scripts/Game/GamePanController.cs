using System.Collections;
using UnityEngine;

namespace DDEngine.Game
{
    public class GamePanController : MonoBehaviour
    {
        [SerializeField]
        private Transform relativeToTransform;

        private Vector3 GetStartingPosition()
        {
            return (relativeToTransform == null) ? transform.localPosition : (relativeToTransform.localPosition + transform.localPosition);
        }

        private IEnumerator PanCoroutine(Vector2 targetPosition, int frames)
        {
            Vector3 destDelta = (new Vector3(targetPosition.x, targetPosition.y, 0) - GetStartingPosition());
            Vector3 destLocal = transform.localPosition + destDelta;
            Vector3 increasePerSe = destDelta / frames;
            increasePerSe.z = 0.0f;

            for (int i = 0; i < frames; i++)
            {
                transform.localPosition += increasePerSe;
                yield return null;
            }

            transform.localPosition = destLocal;
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
}