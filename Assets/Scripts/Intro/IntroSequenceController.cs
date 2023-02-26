using UnityEngine;
using UnityEngine.SceneManagement;

using Cysharp.Threading.Tasks;
using UnityEngine.EventSystems;

using DG.Tweening;
using DDEngine.Installer;

namespace DDEngine.Intro
{
    public class IntroSequenceController : MonoBehaviour
    {
        private Animator animator;

        [SerializeField]
        private GameObject loadingNotice;

        [SerializeField]
        private float fadeLoadingNoticeDuration = 0.5f;

        private void Start()
        {
            DOTween.Init();

            GameSettings.RestoreSettings();
            animator = GetComponent<Animator>();
        }

        private async UniTask DoneFadeOut()
        {
            loadingNotice.SetActive(true);
            TMPro.TMP_Text textRenderer = loadingNotice.GetComponent<TMPro.TMP_Text>();

            await textRenderer.DOFade(1.0f, fadeLoadingNoticeDuration).Play();

            AsyncOperation operation = null;

            if (GameDataInstaller.IsGameDataInstalled(Application.persistentDataPath))
            {
                operation = SceneManager.LoadSceneAsync(Scenes.GameViewSceneIndex);
            }
            else
            {
                operation = SceneManager.LoadSceneAsync(Scenes.InstallerSceneIndex);
            }

            operation.allowSceneActivation = false;
            while (operation.progress < 0.9f)
            {
                await UniTask.Yield();
            }

            await textRenderer.DOFade(0.0f, fadeLoadingNoticeDuration).Play().ToUniTask();
            operation.allowSceneActivation = true;
        }

        public void OnContinuePressed(BaseEventData _)
        {
            animator.SetBool("continueToGame", true);
        }
    }
}
