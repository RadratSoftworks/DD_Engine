using UnityEngine;

using DDEngine.Utils;
using DDEngine.GUI;

namespace DDEngine.Minigame.TakePhoto
{
    public class TakePhotoMinigameLoader : MonoBehaviour
    {
        [SerializeField]
        private GameObject focusRegionPrefabObject;

        [SerializeField]
        private GameObject viewPrefabObject;

        [SerializeField]
        private float scrollDurationPerUnit = 1.3f;

        public static TakePhotoMinigameLoader Instance;

        private void Awake()
        {
            Instance = this;
        }

        private void SetupFocusRegionForLayer(GameObject layer, TakePhotoCameraSideInfo sideInfo, Vector2 focusPoint, Vector2 startPoint)
        {
            GUILayerController layerController = layer.GetComponent<GUILayerController>();

            Vector3 scrollToCenterOfFocusRegion = new Vector2(sideInfo.Distance, 0) + focusPoint;
            Vector3 localCenterFocusRegionPos = layerController.CalculateDestinationScroll(layer.transform.localPosition, -scrollToCenterOfFocusRegion);

            Vector3 depthOfFocusScrolled = layerController.CalculateActualScrollAmount(GameUtils.ToUnitySize(new Vector2(sideInfo.Depth, 0)));

            GameObject regionObject = Instantiate(focusRegionPrefabObject, layer.transform, false);
            TakePhotoFocusRegionController regionController = regionObject.GetComponent<TakePhotoFocusRegionController>();

            if (regionController != null)
            {
                regionController.Setup(layerController, sideInfo.ImageDisplays, GameUtils.ToUnityCoordinates(localCenterFocusRegionPos), depthOfFocusScrolled.x);
            }
        }

        public GUIControlSet Load(TakePhotoMinigameInfo info, Vector2 viewResolution)
        {
            GUIControlSet baseSet = GUIControlSetFactory.Instance.LoadControlSet(info.ControlSetPath, viewResolution, new GUIControlSetInstantiateOptions()
            {
                PanToActiveWhenSelected = false,
                DestroyWhenDisabled = true
            });

            if (baseSet == null)
            {
                return null;
            }

            GUILocationController locationController = baseSet.GameObject.GetComponentInChildren<GUILocationController>();
            if (locationController == null)
            {
                return null;
            }

            locationController.durationPerUnit = scrollDurationPerUnit;

            GameObject locationGameobject = locationController.gameObject;
            GameObject layerFront = locationGameobject.transform.GetChild(0).gameObject;
            GameObject layerBack = locationGameobject.transform.GetChild(1).gameObject;
            GameObject layerCapture = locationGameobject.transform.GetChild(2).gameObject;

            SetupFocusRegionForLayer(layerFront, info.FrontSide, info.FocusPoint, info.StartPoint);
            SetupFocusRegionForLayer(layerBack, info.BackSide, info.FocusPoint, info.StartPoint);

            // Instantiate view collider
            GUILayerController layerCaptureController = layerCapture.GetComponent<GUILayerController>();
            if (layerCaptureController != null)
            {
                GameObject viewObject = Instantiate(viewPrefabObject, layerCapture.transform, false);
                viewObject.transform.localPosition = layerCaptureController.Size * new Vector2(0.5f, -0.5f);

                BoxCollider2D viewCollider = viewObject.GetComponent<BoxCollider2D>();
                viewCollider.size = layerCaptureController.Size;
            }

            locationController.Scroll(GameUtils.ToUnityCoordinates(info.StartPoint));

            GameObject location = locationController.gameObject;
            location.AddComponent<TakePhotoWobbleMovementController>();

            TakePhotoWobbleMovementController wobbleController = location.GetComponent<TakePhotoWobbleMovementController>();
            if (wobbleController != null)
            {
                wobbleController.Setup(GameUtils.ToUnitySize(viewResolution));
            }

            return baseSet;
        }
    }
}
