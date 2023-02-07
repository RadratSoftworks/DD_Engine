using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using Unity.VisualScripting;

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

    private Vector2? animationImageOrigin;
    private Vector2 originalPosition;
    private bool allowLoop = true;
    private bool disableOnDone = true;

    public void Setup(Vector2 position, float sortOrder, string animationFilename, string layerName = null, Vector2? origin = null, bool allowLoop = true, bool disableOnDone = false)
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sortingOrder = GameUtils.ToUnitySortingPosition(sortOrder);

        animationImageOrigin = origin;
        this.allowLoop = allowLoop;
        this.disableOnDone = disableOnDone;

        if (layerName != null)
        {
            spriteRenderer.sortingLayerName = layerName;
        }

        originalPosition = GameUtils.ToUnityCoordinates(position);
        LoadAnimation(animationFilename);

        transform.localPosition = originalPosition;
    }

    private void LoadAnimation(string filename)
    {
        var resourcePack = ResourceManager.Instance.PickBestResourcePackForFile(filename);
        if (!resourcePack.Exists(filename))
        {
            throw new FileNotFoundException("Unable to find animation file: " + filename);
        }
        byte[] animationFileData = resourcePack.ReadResourceData(resourcePack.Resources[filename]);
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
                    if ((comps.Length >= 1) && comps[0].StartsWith("name", StringComparison.OrdinalIgnoreCase)) {
                        continue;
                    }
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
                    ResourceFile resourcesToChoose = ResourceManager.Instance.PickBestResourcePackForFile(comps[0]);
                    Sprite resultLoad = SpriteManager.Instance.Load(resourcesToChoose, comps[0], animationImageOrigin);
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

        Vector2 basePosition = transform.localPosition;

        while (true)
        {
            if (currentFrame == frameInfos.Count)
            {
                if (!allowLoop)
                {
                    break;
                }

                currentFrame = 0;
            }

            FrameInfo frame = frameInfos[currentFrame];
            if (previousSpriteIndex != frame.SpriteIndex)
            {
                spriteRenderer.sprite = spriteToUse[frame.SpriteIndex];
                previousSpriteIndex = frame.SpriteIndex;
            }

            transform.localPosition = basePosition + GameUtils.ToUnityCoordinates(frame.Position);
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

        if (this.disableOnDone)
        {
            gameObject.SetActive(false);
        }

        yield break;
    }

    private void RestartUnityCoords(Vector2 basePosition)
    {
        StopAllCoroutines();
        transform.localPosition = basePosition;

        StartCoroutine(AnimateCoroutine());
    }

    public void Restart(Vector2 basePosition)
    {
        RestartUnityCoords(GameUtils.ToUnityCoordinates(basePosition));
    }

    private void Start()
    {
        transform.localPosition = originalPosition;
        StartCoroutine(AnimateCoroutine());
    }

    private void OnEnable()
    {
        RestartUnityCoords(originalPosition);
    }
}
