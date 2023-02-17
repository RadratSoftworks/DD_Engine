using System;
using System.Collections.Generic;

public static class PogoJumpMinigameInfoParser
{
    public static PogoJumpImageInfo ParseImage(MinigameVariable imageVar)
    {
        PogoJumpImageInfo info = new PogoJumpImageInfo();
        if (!(imageVar.Value is string))
        {
            throw new Exception("Can't get the image path!");
        }

        info.Path = imageVar.Value as string;

        if (!imageVar.ConvertToVector2(out info.Position))
        {
            throw new Exception("Can't get the image position!");
        }

        return info;
    }

    public static PogoJumpMinigameInfo Parse(MinigameVariable root)
    {
        PogoJumpMinigameInfo info = new PogoJumpMinigameInfo();
        if (!root.TryGetValue("gamewon", out info.WonScript))
        {
            throw new Exception("Can't get path of script to run when won the game!");
        }

        if (!root.TryGetValue("bg", out info.BackgroundImagePath))
        {
            throw new Exception("Can't get background image path!");
        }

        if (!root.TryGetValue("player", out info.PlayerPosition))
        {
            throw new Exception("Can't get player position!");
        }

        if (root.TryGetValue("numAnimations", out int numOfAnimations))
        {
            for (int i = 0; i < numOfAnimations; i++)
            {
                if (root.TryGetValue(string.Format("anim{0}", i), out string animPath))
                {
                    info.JumpTierAnimations.Add(animPath);
                } else
                {
                    throw new Exception(string.Format("Can't get jump animation level {0}", i));
                }
            }
        } else
        {
            throw new Exception("Can't get number of pogo jump animations!");
        }

        if (!root.TryGetValue("difficulty", out info.Difficulty))
        {
            throw new Exception("Can't get jump game difficulty");
        }

        if (!root.TryGetValue("jump", out MinigameVariable jumpVar) || !jumpVar.TryGetValue("sound", out info.JumpSoundPath))
        {
            throw new Exception("Can't get jump sound path!");
        }

        int currentImage = 1;
        while (true)
        {
            string imageVarName = string.Format("image{0}", currentImage);
            if (!root.TryGetValue(imageVarName, out MinigameVariable imageVar))
            {
                break;
            }
            info.Images.Add(ParseImage(imageVar));
            currentImage++;
        }

        return info;
    }
}
