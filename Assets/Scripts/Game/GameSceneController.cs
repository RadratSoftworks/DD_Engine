using UnityEngine;
using DDEngine.Utils;

namespace DDEngine.Game
{
    public class GameSceneController : MonoBehaviour
    {
        [SerializeField]
        private SpriteRenderer backgroundRenderer;
        private int gadgetOrder;

        public void Setup(Vector2 size, int gadgetOrder)
        {
            backgroundRenderer.size = GameUtils.ToUnitySize(size);
            backgroundRenderer.sortingOrder = gadgetOrder * Constants.TotalGameLayers;

            this.gadgetOrder = gadgetOrder;
        }

        public int GetSortingOrder(char layer)
        {
            return (gadgetOrder + 1) * Constants.TotalGameLayers - (layer - 'a') - 1;
        }

        public int GadgetOrder => gadgetOrder;

        public Color BackgroundColor
        {
            get => backgroundRenderer.color;
            set => backgroundRenderer.color = value;
        }
    }
}
