using System;
using UnityEngine;

namespace DDEngine.Minigame.ConstructionSite
{
    public class ConstructionSiteRegionController : MonoBehaviour
    {
        public event Action<GameObject, bool> FlyStatusChanged;

        /// <summary>
        /// Setup the construction site region.
        /// </summary>
        /// <param name="bounds">The trigger box for the region, in raw screen coordinates.</param>
        public void Setup(Rect bounds)
        {
            MinigameConstructUtils.SetupBoundsObject(gameObject, bounds);
        }

        public void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.gameObject.layer == ConstructionSiteHostileConstants.FlyLayer)
            {
                FlyStatusChanged(collision.gameObject, true);
            }
        }

        public void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.gameObject.layer == ConstructionSiteHostileConstants.FlyLayer)
            {
                FlyStatusChanged(collision.gameObject, false);
            }
        }
    }
}