using System.IO;
using UnityEngine;

using DDEngine.Minigame.TakePhoto;
using DDEngine.Minigame.PogoJump;
using DDEngine.Minigame.ItemSwitch;
using DDEngine.Minigame.Fight;
using DDEngine.Minigame.ConstructionSite;

using DDEngine.GUI;

namespace DDEngine.Minigame
{
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

                case "fight":
                    {
                        FightMinigameInfo infoFight = FightMinigameInfoParser.Parse(parsedResult);
                        if (infoFight == null)
                        {
                            return null;
                        }

                        return FightMinigameLoader.Instance.Load(infoFight, fileStreamPath, viewResolution);
                    }

                case "itemswitch":
                    {
                        ItemSwitchMinigameInfo infoItemSwitch = ItemSwitchMinigameInfoParser.Parse(parsedResult);
                        if (infoItemSwitch == null)
                        {
                            return null;
                        }

                        return ItemSwitchMinigameLoader.Instance.Load(infoItemSwitch, fileStreamPath, viewResolution);
                    }

                case "fly":
                    {
                        ConstructionSiteMinigameInfo infoFly = ConstructionSiteMinigameInfoParser.Parse(parsedResult);
                        if (infoFly == null)
                        {
                            return null;
                        }

                        return ConstructionSiteMinigameLoader.Instance.Load(infoFly, fileStreamPath, viewResolution);
                    }

                case "pogo":
                    {
                        PogoJumpMinigameInfo infoPogo = PogoJumpMinigameInfoParser.Parse(parsedResult);
                        if (infoPogo == null)
                        {
                            return null;
                        }

                        return PogoJumpMinigameLoader.Instance.Load(infoPogo, fileStreamPath, viewResolution);
                    }

                default:
                    Debug.LogErrorFormat("Unsupported minigame type: {0}", gameType);
                    break;
            }

            return null;
        }
    }
}