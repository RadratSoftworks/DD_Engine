using UnityEngine;
using DDEngine.GUI;

namespace DDEngine.Minigame.ConstructionSite
{
    public class ConstructionSiteSceneController : MonoBehaviour
    {
        [SerializeField]
        private SpriteRenderer backgroundRenderer;

        [SerializeField]
        private SpriteRenderer signRenderer;

        [SerializeField]
        private SpriteRenderer robotAndGuyRenderer;

        [SerializeField]
        private SpriteRenderer whiskyRenderer;

        [SerializeField]
        private ConstructionSiteHostileController trapHostile;

        [SerializeField]
        private ConstructionSiteHostileController leftBirdHostile;

        [SerializeField]
        private ConstructionSiteHostileController rightBirdHostile;

        [SerializeField]
        private ConstructionSiteHostileController whiskyHostile;

        [SerializeField]
        private ConstructionSiteHostileController manHostile;

        [SerializeField]
        private ConstructionSiteHostileController winHostile;

        [SerializeField]
        private ConstructionSiteHarryController harryController;

        [SerializeField]
        private CompositeCollider2D cameraBounds;

        private void EnableViewStyleOnControlSetStatus(bool enabled)
        {
            if (enabled)
            {
                // Switch to full view
                GameViewController.Instance.SetFullViewWithFocus(harryController.transform, cameraBounds);
            }
            else
            {
                GameViewController.Instance.SetNormalView();
            }
        }

        public void Setup(GUIControlSet ownSet, ConstructionSiteMinigameInfo info)
        {
            backgroundRenderer.sprite = SpriteManager.Instance.Load(ResourceManager.Instance.GeneralResources,
                info.BackgroundImage);
            signRenderer.sprite = SpriteManager.Instance.Load(ResourceManager.Instance.GeneralResources,
                info.SignImage, Vector2.zero);
            robotAndGuyRenderer.sprite = SpriteManager.Instance.Load(ResourceManager.Instance.GeneralResources,
                info.RobotAndGuyImage, Vector2.zero);
            whiskyRenderer.sprite = SpriteManager.Instance.Load(ResourceManager.Instance.GeneralResources,
                info.WhiskyImage, Vector2.zero);

            trapHostile.Setup(info.Trap);
            leftBirdHostile.Setup(info.LeftBird);
            rightBirdHostile.Setup(info.RightBird);
            whiskyHostile.Setup(info.Whisky);
            manHostile.Setup(info.Man);
            winHostile.Setup(info.Win);

            harryController.Setup(info.FlyPosition);

            ownSet.StateChanged += EnableViewStyleOnControlSetStatus;
        }
    }
}