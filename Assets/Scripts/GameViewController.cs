using Cinemachine;
using UnityEngine;
using UnityEngine.UI;

namespace DDEngine
{
    public class GameViewController : MonoBehaviour
    {
        public static GameViewController Instance;

        [SerializeField]
        private Vector3 NonFullViewCameraPosition = new Vector3(30.0f, -30.0f, -5.0f);

        [SerializeField]
        private Vector3 FullViewCameraPosition = new Vector3(3.6f, -4.8f, -5.0f);

        [SerializeField]
        private Camera normalViewCamera;

        [SerializeField]
        private CinemachineVirtualCamera fullscreenCamera;

        [SerializeField]
        private CinemachineConfiner fullscreenCameraConfiner;

        [SerializeField]
        private RawImage uiNormalViewImage;

        [SerializeField]
        private Image uiBackgroundImage;

        private void Start()
        {
            Instance = this;
        }

        public void SetFullViewWithFocus(Transform follow, Collider2D bounds)
        {
            uiNormalViewImage.enabled = false;
            uiBackgroundImage.enabled = false;
            normalViewCamera.enabled = false;

            fullscreenCamera.transform.localPosition = FullViewCameraPosition;

            fullscreenCamera.Follow = follow;
            fullscreenCameraConfiner.m_BoundingShape2D = bounds;
        }

        public void SetNormalView()
        {
            uiNormalViewImage.enabled = true;
            uiBackgroundImage.enabled = true;
            normalViewCamera.enabled = true;

            fullscreenCamera.transform.localPosition = NonFullViewCameraPosition;
            fullscreenCamera.Follow = null;
            fullscreenCameraConfiner.m_BoundingShape2D = null;
        }
    }
}