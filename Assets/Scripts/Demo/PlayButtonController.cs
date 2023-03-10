using UnityEngine;
using UnityEngine.SceneManagement;

namespace DDEngine.Demo
{
    public class PlayButtonController : MonoBehaviour
    {
        public void OnClick()
        {
            SceneManager.LoadScene(Scenes.GameViewSceneIndex);
        }
    }
}
