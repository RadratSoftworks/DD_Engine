using System.Collections.Generic;
using UnityEngine;

using DDEngine.Utils;
using DDEngine.GUI;

using DG.Tweening;

namespace DDEngine.Minigame.TakePhoto
{
    public class TakePhotoFocusRegionController : MonoBehaviour
    {
        private List<SpriteRenderer> sharpImagesRenderer = new List<SpriteRenderer>();
        private GUILayerController layerController;

        private BoxCollider2D boxCollider;

        [SerializeField]
        private float sharpFadeDuration = 0.3f;

        private void Awake()
        {
            DOTween.Init();
        }

        public void Setup(GUILayerController layerController, List<TakePhotoImageDisplayInfo> displayInfos, Vector2 positionInUnit, float depthOfFocusInUnits)
        {
            this.layerController = layerController;
            this.transform.localPosition = positionInUnit;

            boxCollider = GetComponent<BoxCollider2D>();
            if (boxCollider != null)
            {
                boxCollider.size = new Vector2(depthOfFocusInUnits * 2, depthOfFocusInUnits * 2);
            }

            GameObject layerObject = layerController.gameObject;

            foreach (TakePhotoImageDisplayInfo info in displayInfos)
            {
                Transform sharpImageTransform = layerObject.transform.Find(GameUtils.ToUnityName(info.SharpImagePath));

                if (sharpImageTransform == null)
                {
                    Debug.LogError(string.Format("Sharp image (path={0}) is missing for take photo focus!", info.SharpImagePath));
                }
                else
                {
                    SpriteRenderer renderer = sharpImageTransform.gameObject.GetComponent<SpriteRenderer>(); ;
                    if (renderer != null)
                    {
                        sharpImagesRenderer.Add(renderer);
                    }
                }
            }
        }

        private void SetFocusState(bool focused)
        {
            foreach (SpriteRenderer sharpImageRenderer in sharpImagesRenderer)
            {
                sharpImageRenderer.DOFade(focused ? 1.0f : 0.0f, sharpFadeDuration);
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            SetFocusState(true);
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            SetFocusState(false);
        }
    }
}