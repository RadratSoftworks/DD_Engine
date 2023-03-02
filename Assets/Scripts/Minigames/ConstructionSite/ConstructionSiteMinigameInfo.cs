using UnityEngine;

namespace DDEngine.Minigame.ConstructionSite
{
    public class ConstructionSiteMinigameInfo
    {
        public string BackgroundImage;
        public string RobotAndGuyImage;
        public string WhiskyImage;
        public string SignImage;

        public ConstructionSiteHostileInfo Trap = new ConstructionSiteHostileInfo();
        public ConstructionSiteHostileInfo RightBird = new ConstructionSiteHostileInfo();
        public ConstructionSiteHostileInfo LeftBird = new ConstructionSiteHostileInfo();
        public ConstructionSiteHostileInfo Whisky = new ConstructionSiteHostileInfo();
        public ConstructionSiteHostileInfo Man = new ConstructionSiteHostileInfo();
        public ConstructionSiteHostileInfo Win = new ConstructionSiteHostileInfo();

        public Vector2[] ArrowPositions = new Vector2[4];
        public Vector2 FlyPosition = Vector2.zero;
    }
}
