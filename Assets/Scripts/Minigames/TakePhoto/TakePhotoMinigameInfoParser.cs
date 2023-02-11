using System;
using System.Collections.Generic;
using UnityEngine;

public class TakePhotoMinigameInfoParser
{
    public static TakePhotoCameraSideInfo ParseCameraSideInfo(MinigameVariable sideVariable)
    {
        TakePhotoCameraSideInfo sideInfo = new TakePhotoCameraSideInfo();
        if (!sideVariable.TryGetValue("distance", out int distanceValue))
        {
            Debug.Log("Unable to get the distance value of the camera side!");
            return null;
        } else
        {
            sideInfo.Distance = distanceValue;
        }

        if (!sideVariable.TryGetValue("depth", out int depthValue))
        {
            Debug.Log("Unable to get the depth value of the camera side!");
            return null;
        }
        else
        {
            sideInfo.Depth = depthValue;
        }

        if (!sideVariable.TryGetValues("sharp", out List<string> sharpImages))
        {
            Debug.Log("Unable to get list of sharp images!");
            return null;
        }

        if (!sideVariable.TryGetValues("blur", out List<string> blurImages))
        {
            Debug.Log("Unable to get list of blur images!");
            return null;
        }

        for (int i = 0; i < Mathf.Min(sharpImages.Count, blurImages.Count); i++)
        {
            TakePhotoImageDisplayInfo imageInfo = new TakePhotoImageDisplayInfo()
            {
                BlurryImagePath = blurImages[i],
                SharpImagePath = sharpImages[i]
            };

            sideInfo.ImageDisplays.Add(imageInfo);
        }

        return sideInfo;
    }

    public static TakePhotoMinigameInfo Parse(MinigameVariable root)
    {
        TakePhotoMinigameInfo gameInfo = new TakePhotoMinigameInfo();

        if (!root.TryGetValue("controlset", out string controlSetPath))
        {
            Debug.Log("Unable to determine control set path from photo minigame file!");
            return null;
        } else
        {
            gameInfo.ControlSetPath = controlSetPath;
        }

        if (!root.TryGetValue("start", out Vector2 startValue))
        {
            Debug.Log("Unable to get start scroll position of the minigame!");
            return null;
        } else
        {
            gameInfo.StartPoint = startValue;
        }

        if (!root.TryGetValue("target", out Vector2 focusValue))
        {
            Debug.Log("Unable to get focus position of the minigame!");
            return null;
        }
        else
        {
            gameInfo.FocusPoint = focusValue;
        }

        if (!root.TryGetValue("front", out MinigameVariable frontSideVar))
        {
            Debug.Log("Unable to find front camera side info!");
            return null;
        }

        if (!root.TryGetValue("back", out MinigameVariable backSideVar))
        {
            Debug.Log("Unable to find back camera side info!");
            return null;
        }

        gameInfo.FrontSide = ParseCameraSideInfo(frontSideVar);
        gameInfo.BackSide = ParseCameraSideInfo(backSideVar);

        if ((gameInfo.FrontSide == null) || (gameInfo.BackSide == null)) {
            return null;
        }

        return gameInfo;
    }
}