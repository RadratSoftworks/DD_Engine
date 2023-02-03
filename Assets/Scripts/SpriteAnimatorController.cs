using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class SpriteAnimatorController : MonoBehaviour
{
    class FrameInfo
    {
        public int SpriteIndex { get; set; }
        public int Duration { get; set; }
        public Vector2 Position { get; set; }
    };

    private SpriteRenderer spriteRenderer;
    private List<Sprite> spriteToUse = new List<Sprite>();
    private List<FrameInfo> frameInfos = new List<FrameInfo>();

    public string AnimationFilename { get; set; }

    private void LoadAnimation()
    {
        var generalResources = ResourceManager.Instance.GeneralResources;
        if (!generalResources.Exists(AnimationFilename))
        {
            throw new FileNotFoundException("Unable to find animation file: " + AnimationFilename);
        }
        byte[] animationFileData = generalResources.ReadResourceData(generalResources.Resources[AnimationFilename]);
        using (StreamReader reader = new StreamReader(new MemoryStream(animationFileData)))
        {
            Dictionary<string, int> spriteFileLookup = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            do
            {
                string lineDesc = reader.ReadLine();
                if ((lineDesc == "") || (lineDesc == null))
                {
                    break;
                }
                lineDesc = lineDesc.Trim();
                if (lineDesc.StartsWith('#'))
                {
                    continue;
                }
                var comps = lineDesc.Split(' ');
                if (comps.Length != 4)
                {
                    Debug.Log("Invalid animation instruction: " + lineDesc);
                    continue;
                }
                FrameInfo frame = new FrameInfo()
                {
                    Duration = int.Parse(comps[1]),
                    Position = new Vector2(int.Parse(comps[2]), int.Parse(comps[3]))
                };
                if (spriteFileLookup.ContainsKey(comps[0]))
                {
                    frame.SpriteIndex = spriteFileLookup[comps[0]];
                } else
                {
                    Sprite resultLoad = SpriteManager.Instance.Load(generalResources, comps[0]);
                    if (resultLoad == null)
                    {
                        Debug.LogError("Can't find frame image path: " + comps[0]);
                        continue;
                    }
                    frame.SpriteIndex = spriteToUse.Count;
                    spriteFileLookup.Add(comps[0], frame.SpriteIndex);
                    spriteToUse.Add(resultLoad);
                }
                frameInfos.Add(frame);
            } while (true);
        }
    }

    private IEnumerator AnimateCoroutine()
    {
        if (frameInfos.Count == 0)
        {
            yield break;
        }

        int previousSpriteIndex = -1;
        int currentFrame = 0;

        while (true)
        {
            if (currentFrame == frameInfos.Count)
            {
                currentFrame = 0;
            }

            FrameInfo frame = frameInfos[currentFrame];
            if (previousSpriteIndex != frame.SpriteIndex)
            {
                spriteRenderer.sprite = spriteToUse[frame.SpriteIndex];
                previousSpriteIndex = frame.SpriteIndex;
            }

            transform.localPosition = GameUtils.ToUnityCoordinates(frame.Position);
            currentFrame++;

            if (frame.Duration <= 0)
            {
                break;
            }
            else
            {
                if (frame.Duration == 1)
                {
                    yield return null;
                }
                else
                {
                    // Not sure if that unroll above optimize anything, but well
                    for (int i = 0; i < frame.Duration; i++)
                    {
                        yield return null;
                    }

                }
            }
        }

        yield break;
    }

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        LoadAnimation();
        StartCoroutine(AnimateCoroutine());
    }
}
