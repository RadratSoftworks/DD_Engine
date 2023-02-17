using System;
using System.Collections.Generic;
using UnityEngine;

public class PogoJumpSceneController : MonoBehaviour
{
    private const int ImageLayer = 1;

    [SerializeField]
    private SpriteRenderer backgroundSpriteRenderer;

    [SerializeField]
    private GameObject imageContainer;

    [SerializeField]
    private PogoJumpPlayerController playerController;

    public void Setup(PogoJumpMinigameInfo jumpGameInfo, GameObject animationPrefab, GameObject imagePrefab)
    {
        backgroundSpriteRenderer.sprite = SpriteManager.Instance.Load(ResourceManager.Instance.GeneralResources,
            jumpGameInfo.BackgroundImagePath);

        foreach (PogoJumpImageInfo image in jumpGameInfo.Images) {
            GameObject imageObj = Instantiate(imagePrefab, imageContainer.transform, false);
            imageObj.transform.localPosition = GameUtils.ToUnityCoordinates(image.Position);

            SpriteRenderer renderer = imageObj.GetComponent<SpriteRenderer>();
            if (renderer != null)
            {
                renderer.sprite = SpriteManager.Instance.Load(ResourceManager.Instance.GeneralResources,
                    image.Path);
                renderer.sortingOrder = GameUtils.ToUnitySortingPosition(ImageLayer);
            }
        }

        playerController.Setup(jumpGameInfo, animationPrefab);
    }
}
