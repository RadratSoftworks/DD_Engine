using System;

namespace DDEngine.Minigame.ConstructionSite
{
    public static class ConstructionSiteMinigameInfoParser
    {
        public static void ParseHostile(MinigameVariable hostile, ref ConstructionSiteHostileInfo hostileInfo)
        {
            hostile.TryGetValue("image", out hostileInfo.HostImagePath);

            if (!hostile.TryGetValue("idle", out MinigameVariable idleVar) || !idleVar.TryGetValue("anim", out hostileInfo.IdleAnimationPath))
            {
                throw new Exception("Can't get idle animation path!");
            }

            if (!hostile.TryGetValue("animation", out MinigameVariable animOffsetVar) ||
                !animOffsetVar.TryGetValue("offset", out MinigameVariable offsetVar) ||
                !offsetVar.ConvertToVector2(out hostileInfo.IdlePosition))
            {
                throw new Exception("Can't get animation offset!");
            }

            if (!hostile.TryGetValue("danger", out MinigameVariable dangerVar) || !dangerVar.ConvertToRect(out hostileInfo.DangerBounds))
            {
                throw new Exception("Can't parse danger bounds!");
            }

            if (!dangerVar.TryGetValue("anim", out hostileInfo.DangerAnimationPath))
            {
                throw new Exception("Can't parse danger animation path");
            }

            if (!hostile.TryGetValue("death", out MinigameVariable deathVar) || !deathVar.ConvertToRect(out hostileInfo.DeathBounds))
            {
                throw new Exception("Can't parse death bounds!");
            }

            if (!deathVar.TryGetValue("anim", out hostileInfo.DeathAnimationPath))
            {
                throw new Exception("Can't parse death animation path");
            }

            if (!hostile.TryGetValue("action", out hostileInfo.DeathScriptPath))
            {
                throw new Exception("Can't get death script path!");
            }
        }

        public static ConstructionSiteMinigameInfo Parse(MinigameVariable root)
        {
            ConstructionSiteMinigameInfo minigameInfo = new ConstructionSiteMinigameInfo();

            if (!root.TryGetValue("bg", out MinigameVariable bgVar) || !bgVar.TryGetValue("image", out minigameInfo.BackgroundImage))
            {
                throw new Exception("Can't get background image path!");
            }

            if (!root.TryGetValue("fly", out MinigameVariable flyVar) || !flyVar.TryGetValue("start1", out minigameInfo.FlyPosition))
            {
                throw new Exception("Can't get fly spawn position!");
            }

            if (!root.TryGetValue("whisky", out MinigameVariable whiskyBgVar) || !whiskyBgVar.TryGetValue("image", out minigameInfo.WhiskyImage))
            {
                throw new Exception("Can't get whisky image path!");
            }

            if (!root.TryGetValue("swat", out MinigameVariable swatVar) || !swatVar.TryGetValue("image", out minigameInfo.RobotAndGuyImage))
            {
                throw new Exception("Can't get robot and guy image path!");
            }

            if (!root.TryGetValue("sign", out MinigameVariable signVar) || !signVar.TryGetValue("image", out minigameInfo.SignImage))
            {
                throw new Exception("Can't get sign image path!");
            }

            if (!root.TryGetValue("trap", out MinigameVariable trapVar))
            {
                throw new Exception("Can't get trap hostile variable!");
            }

            if (!root.TryGetValue("right_bird", out MinigameVariable rightBirdVar))
            {
                throw new Exception("Can't get right bird hostile variable!");
            }

            if (!root.TryGetValue("left_bird", out MinigameVariable leftBirdVar))
            {
                throw new Exception("Can't get left bird hostile variable!");
            }

            if (!root.TryGetValue("whisky", out MinigameVariable whiskyVar))
            {
                throw new Exception("Can't get whisky hostile variable!");
            }

            if (!root.TryGetValue("man", out MinigameVariable manVar))
            {
                throw new Exception("Can't get man hostile variable!");
            }

            if (!root.TryGetValue("win", out MinigameVariable winVar))
            {
                throw new Exception("Can't get win destination variable!");
            }

            ParseHostile(trapVar, ref minigameInfo.Trap);
            ParseHostile(rightBirdVar, ref minigameInfo.RightBird);
            ParseHostile(leftBirdVar, ref minigameInfo.LeftBird);
            ParseHostile(whiskyVar, ref minigameInfo.Whisky);
            ParseHostile(manVar, ref minigameInfo.Man);
            ParseHostile(winVar, ref minigameInfo.Win);

            return minigameInfo;
        }
    }
}