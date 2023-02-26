using UnityEngine;
using UnityEngine.UI;

using System.Collections;

namespace DDEngine.RealGUI
{
    class SceneFadeInController : MonoBehaviour
    {
        [SerializeField]
        private float fadeDuration = 0.6f;

        private void Awake()
        {
            Image img = GetComponent<Image>();

            img.color = new Color(0.0f, 0.0f, 0.0f, 1.0f);
            img.CrossFadeAlpha(0.0f, fadeDuration, false);

            StartCoroutine(OnFadeComplete());
        }

        private IEnumerator OnFadeComplete()
        {
            yield return new WaitForSeconds(fadeDuration);
            GameObject.Destroy(gameObject);
        }
    }
}