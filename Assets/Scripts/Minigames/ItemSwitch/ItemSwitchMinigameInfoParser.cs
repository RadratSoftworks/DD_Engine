using System;
using System.IO;

namespace DDEngine.Minigame.ItemSwitch
{
    public static class ItemSwitchMinigameInfoParser
    {
        private static void ParseBackgroundInfo(MinigameVariable backgroundVar, ref ItemSwitchBackgroundInfo backgroundInfo)
        {
            if (!(backgroundVar.Value is string))
            {
                throw new InvalidDataException("Switch minigame background path is not valid!");
            }

            backgroundInfo.ImagePath = backgroundVar.Value as string;

            if (!backgroundVar.TryGetValue("anim", out MinigameVariable animVar))
            {
                throw new Exception("Can't find the effect animation for switch minigame background!");
            }

            if (!(animVar.Value is string))
            {
                throw new InvalidDataException("Switch minigame background effect path is not valid!");
            }

            backgroundInfo.EffectAnimationPath = animVar.Value as string;

            if (!animVar.ConvertToVector2(out backgroundInfo.EffectPosition))
            {
                throw new InvalidDataException("Can't get the background effect position for switch minigame!");
            }
        }

        private static void ParseHandInfo(MinigameVariable handVar, ref ItemSwitchDirkHandInfo handInfo)
        {
            if (!handVar.TryGetValue("anim", out handInfo.AnimationPath))
            {
                throw new Exception("Can't get hand animation path!");
            }

            if (!handVar.ConvertToVector2(out handInfo.OriginalPosition))
            {
                throw new Exception("Can't get hand position!");
            }
        }

        private static void ParseStressMachineInfo(MinigameVariable stressVar, ref ItemSwitchStressMachineInfo stressInfo)
        {
            if (!stressVar.TryGetValue("bg", out stressInfo.BackgroundImagePath))
            {
                throw new Exception("Can't get stress indicator background image path!");
            }

            if (!stressVar.TryGetValue("indicator", out stressInfo.IndicatorImagePath))
            {
                throw new Exception("Can't get stress indicator image path!");
            }

            if (!stressVar.ConvertToVector2(out stressInfo.Position))
            {
                throw new Exception("Can't get stress indicator position!");
            }
        }

        private static void ParseTimerInfo(MinigameVariable timeVar, ref ItemSwitchTimerInfo timeInfo)
        {
            if (!timeVar.TryGetValue("anim", out timeInfo.CountdownAnimationPath))
            {
                throw new Exception("Can't get timer countdown animation path!");
            }

            if (!timeVar.ConvertToVector2(out timeInfo.Position))
            {
                throw new Exception("Can't get timer position!");
            }

            if (!timeVar.TryGetValue("won", out MinigameVariable wonVar) || !wonVar.TryGetValue("anim", out timeInfo.WonAnimationPath))
            {
                throw new Exception("Can't get timer won animation path!");
            }

            if (!timeVar.TryGetValue("lost", out MinigameVariable lostVar) || !lostVar.TryGetValue("anim", out timeInfo.LostAnimationPath))
            {
                throw new Exception("Can't get timer lost animation path!");
            }

            if (!timeVar.TryGetValue("pre", out MinigameVariable preVar) || !preVar.TryGetValue("anim", out timeInfo.ReadyAnimationPath))
            {
                throw new Exception("Can't get timer ready animation path!");
            }
        }

        public static ItemSwitchMinigameInfo Parse(MinigameVariable root)
        {
            ItemSwitchMinigameInfo minigameInfo = new ItemSwitchMinigameInfo();

            if (!root.TryGetValue("gamewon", out minigameInfo.WonScript))
            {
                throw new Exception("Can't find script to run when the game is won!");
            }

            if (!root.TryGetValue("gamelost", out minigameInfo.LoseScript))
            {
                throw new Exception("Can't find script to run when the game is lost!");
            }

            if (!root.TryGetValue("moveAmplitude", out minigameInfo.ForcePercentage))
            {
                throw new Exception("Can't get the move amplitude!");
            }

            if (!root.TryGetValue("maxMomentum", out minigameInfo.MaxSpeedPercentage))
            {
                throw new Exception("Can't get the maxmum momentum!");
            }

            if (!root.TryGetValue("winPercentage", out minigameInfo.WinPercentage))
            {
                throw new Exception("Can't find the win percentage");
            }

            if (!root.TryGetValue("bg", out MinigameVariable bgVar))
            {
                throw new Exception("Can't find switch minigame background info!");
            }

            if (!root.TryGetValue("left", out MinigameVariable leftHandVar))
            {
                throw new Exception("Can't find left hand info!");
            }

            if (!root.TryGetValue("right", out MinigameVariable rightHandVar))
            {
                throw new Exception("Can't find right hand info!");
            }

            if (!root.TryGetValue("stress", out MinigameVariable stressVar))
            {
                throw new Exception("Can't find stress indicator info!");
            }

            if (!root.TryGetValue("time", out MinigameVariable timerVar))
            {
                throw new Exception("Can't find time info!");
            }

            ParseBackgroundInfo(bgVar, ref minigameInfo.BackgroundInfo);
            ParseHandInfo(leftHandVar, ref minigameInfo.LeftHandInfo);
            ParseHandInfo(rightHandVar, ref minigameInfo.RightHandInfo);
            ParseStressMachineInfo(stressVar, ref minigameInfo.StressMachineInfo);
            ParseTimerInfo(timerVar, ref minigameInfo.TimerInfo);

            return minigameInfo;
        }
    }
}