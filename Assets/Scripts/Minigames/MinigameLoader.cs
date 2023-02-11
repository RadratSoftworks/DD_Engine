using System;
using System.IO;
using UnityEngine;

public class MinigameLoader
{
    public static GUIControlSet Load(Stream fileStream, string fileStreamPath, Vector2 viewResolution)
    {
        MinigameVariable parsedResult = MinigameFileParser.Parse(fileStream, fileStreamPath);
        if (parsedResult == null)
        {
            return null;
        }

        if (!parsedResult.TryGetValue("game", out string gameType))
        {
            Debug.Log("Unable to determine minigame type!");
            return null;
        }

        switch (gameType)
        {
            case "photo":
                {
                    TakePhotoMinigameInfo infoPhoto = TakePhotoMinigameInfoParser.Parse(parsedResult);
                    if (infoPhoto == null)
                    {
                        return null;
                    }

                    return TakePhotoMinigameLoader.Instance.Load(infoPhoto, viewResolution);
                }

            default:
                Debug.LogErrorFormat("Unsupported minigame type: {0}", gameType);
                break;
        }

        return null;
    }
}
