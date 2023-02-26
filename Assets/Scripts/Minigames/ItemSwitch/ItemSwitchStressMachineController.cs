using System;
using UnityEngine;
using DDEngine.Utils;

namespace DDEngine.Minigame.ItemSwitch
{
    public class ItemSwitchStressMachineController : MonoBehaviour
    {
        [SerializeField]
        private SpriteRenderer backgroundSpriteRenderer;

        [SerializeField]
        private ItemSwitchStressIndicatorController indicatorController;

        public event System.Action<ItemSwitchStressStatus> StressStatusEntered;
        public event System.Action ConfirmPressed;

        public bool Passed => indicatorController.Passed;

        public void Setup(ItemSwitchStressMachineInfo stressInfo, float forceFactor, float maxSpeedFactor)
        {
            transform.localPosition = GameUtils.ToUnityCoordinates(stressInfo.Position);

            backgroundSpriteRenderer.sprite = SpriteManager.Instance.Load(ResourceManager.Instance.GeneralResources,
                stressInfo.BackgroundImagePath);

            indicatorController.Setup(stressInfo, forceFactor, maxSpeedFactor);
            indicatorController.StressStatusEntered += status => StressStatusEntered?.Invoke(status);
            indicatorController.ConfirmPressed += () => ConfirmPressed?.Invoke();
        }

        public void KickOff()
        {
            indicatorController.KickOff();
        }

        public void Freeze()
        {
            indicatorController.Freeze();
        }
    }
}