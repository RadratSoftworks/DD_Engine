using UnityEngine;

public class ConstructionSiteHostileController : MonoBehaviour
{
    // Since these animations are not one-time, and can trigger many time, better preload it to
    // separate objects
    [SerializeField]
    private SpriteAnimatorController idleAnimation;

    [SerializeField]
    private SpriteAnimatorController dangerAnimation;

    [SerializeField]
    private SpriteAnimatorController deathAnimation;

    [SerializeField]
    private ConstructionSiteRegionController dangerRegionController;

    [SerializeField]
    private ConstructionSiteRegionController deathRegionController;

    private string deathScriptPath;

    public void Setup(ConstructionSiteHostileInfo hostileInfo)
    {
        transform.localPosition = GameUtils.ToUnityCoordinates(hostileInfo.IdlePosition);

        idleAnimation.Setup(Vector2.zero, SpriteAnimatorController.SortOrderNotSet, hostileInfo.IdleAnimationPath);
        dangerAnimation.Setup(Vector2.zero, SpriteAnimatorController.SortOrderNotSet, hostileInfo.DangerAnimationPath);
        deathAnimation.Setup(Vector2.zero, SpriteAnimatorController.SortOrderNotSet, hostileInfo.DeathAnimationPath, allowLoop: false);

        // Disable these by default
        dangerAnimation.Disable();
        deathAnimation.Disable();

        // Rebase the position of those region, since they are supposed to be our children
        dangerRegionController.Setup(new Rect(hostileInfo.DangerBounds.position - hostileInfo.IdlePosition, hostileInfo.DangerBounds.size));
        deathRegionController.Setup(new Rect(hostileInfo.DeathBounds.position - hostileInfo.IdlePosition, hostileInfo.DeathBounds.size));

        dangerRegionController.FlyStatusChanged += OnFlyStatusChangedInDangerRegion;
        deathRegionController.FlyStatusChanged += OnFlyStatusChangedInDeathRegion;

        deathScriptPath = hostileInfo.DeathScriptPath;
    }

    public void OnFlyStatusChangedInDangerRegion(GameObject victim, bool entered)
    {
        if (deathAnimation.Enabled)
        {
            return;
        }

        // It would have been more complicated, but luckily we died when we enter the death region
        // Else would have to make an idle region trigger around the danger too! (what I think, but someone more smart maybe know more..)
        idleAnimation.SetEnableState(!entered);
        dangerAnimation.SetEnableState(entered);
    }

    public void OnFlyStatusChangedInDeathRegion(GameObject victim, bool entered)
    {
        if (entered)
        {
            // Run death script!
            idleAnimation.Disable();
            dangerAnimation.Disable();

            var harryController = victim.GetComponent<ConstructionSiteHarryController>();
            if (harryController != null)
            {
                harryController.Kill();
            }

            deathAnimation.Done += controller =>
            {
                GameManager.Instance.SetCurrentGUI(null);
                GameManager.Instance.LoadGadget(deathScriptPath);
            };

            deathAnimation.Enable();
        }
    }
}
