using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using DDEngine.Utils;

namespace DDEngine
{
    public class SpriteAnimatorController : MonoBehaviour
    {
        public static readonly int SortOrderNotSet = -1;

        class FrameInfo
        {
            public int SpriteIndex { get; set; }
            public int Duration { get; set; }
            public Vector2 Position { get; set; }
            public bool InGameCoordinates { get; set; } = true;
        };

        private SpriteRenderer spriteRenderer;
        private List<Sprite> spriteToUse = new List<Sprite>();
        private List<FrameInfo> frameInfos = new List<FrameInfo>();
        private Dictionary<string, int> spriteFileLookup = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        private Vector2? animationImageOrigin;
        private Vector2 originalPosition;
        private bool allowLoop = true;
        private bool disableOnDone = true;
        private IEnumerator currentAnimateCoroutine = null;
        private int currentFrame = 0;
        private bool started = false;

        public event Action<SpriteAnimatorController> Done;

        public int TotalFrame => frameInfos.Count;
        public int CurrentFrame => currentFrame;

        public bool Enabled => gameObject.activeSelf;

        public void Setup(Vector2 position, float sortOrder, string animationFilename, string layerName = null, Vector2? origin = null, bool allowLoop = true, bool disableOnDone = false)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();

            if (sortOrder >= 0)
            {
                spriteRenderer.sortingOrder = GameUtils.ToUnitySortingPosition(sortOrder);
            }

            if (layerName != null)
            {
                spriteRenderer.sortingLayerName = layerName;
            }

            LoadImplementation(position, animationFilename, origin, allowLoop, disableOnDone);
        }

        public void Reload(Vector2 position, string animationFilename, Vector2? origin = null, bool allowLoop = true, bool disableOnDone = false)
        {
            if (currentAnimateCoroutine != null)
            {
                StopCoroutine(currentAnimateCoroutine);
                currentAnimateCoroutine = null;
            }

            spriteToUse.Clear();
            frameInfos.Clear();

            LoadImplementation(position, animationFilename, origin, allowLoop, disableOnDone);

            currentAnimateCoroutine = AnimateCoroutine();
            StartCoroutine(currentAnimateCoroutine);
        }

        private void LoadImplementation(Vector2 position, string animationFilename, Vector2? origin = null, bool allowLoop = true, bool disableOnDone = false)
        {
            animationImageOrigin = origin;
            this.allowLoop = allowLoop;
            this.disableOnDone = disableOnDone;

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
                spriteFileLookup.Clear();

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
                        if ((comps.Length >= 1) && comps[0].StartsWith("name", StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }
                        Debug.Log("Invalid animation instruction: " + lineDesc);
                        continue;
                    }
                    AddFrame(comps[0], new Vector2(int.Parse(comps[2]), int.Parse(comps[3])), int.Parse(comps[1]));
                } while (true);
            }
        }

        public void AddFrame(string frameImagePath, Vector2 position, int durationInBaseGameFrames, bool inGameCoordinates = true)
        {
            FrameInfo frame = new FrameInfo()
            {
                Duration = durationInBaseGameFrames,
                Position = position,
                InGameCoordinates = inGameCoordinates
            };
            if (spriteFileLookup.ContainsKey(frameImagePath))
            {
                frame.SpriteIndex = spriteFileLookup[frameImagePath];
            }
            else
            {
                ResourceFile resourcesToChoose = ResourceManager.Instance.PickBestResourcePackForFile(frameImagePath);
                Sprite resultLoad = SpriteManager.Instance.Load(resourcesToChoose, frameImagePath, animationImageOrigin);
                if (resultLoad == null)
                {
                    Debug.LogError("Can't find frame image path: " + frameImagePath);
                    return;
                }
                frame.SpriteIndex = spriteToUse.Count;
                spriteFileLookup.Add(frameImagePath, frame.SpriteIndex);
                spriteToUse.Add(resultLoad);
            }
            frameInfos.Add(frame);
        }

        public void SetOriginalPosition(Vector2 position)
        {
            originalPosition = position;
        }

        public void SetOriginalPositionToCurrent()
        {
            originalPosition = transform.localPosition;
        }

        private IEnumerator AnimateCoroutine()
        {
            if (frameInfos.Count == 0)
            {
                currentAnimateCoroutine = null;
                Done?.Invoke(this);

                yield break;
            }

            int previousSpriteIndex = -1;
            currentFrame = 0;

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

                transform.localPosition = basePosition + (frame.InGameCoordinates ? GameUtils.ToUnityCoordinates(frame.Position) : frame.Position);
                currentFrame++;

                if (frame.Duration <= 0)
                {
                    break;
                }
                else
                {
                    int durationReal = GameManager.Instance.GetRealFrames(frame.Duration);

                    if (durationReal == 1)
                    {
                        yield return null;
                    }
                    else
                    {
                        // Not sure if that unroll above optimize anything, but well
                        for (int i = 0; i < durationReal; i++)
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

            Done?.Invoke(this);

            currentAnimateCoroutine = null;
            yield break;
        }

        private void RestartUnityCoords(Vector2 basePosition)
        {
            StopAnimating();
            transform.localPosition = basePosition;

            currentAnimateCoroutine = AnimateCoroutine();
            StartCoroutine(currentAnimateCoroutine);
        }

        public void Restart(Vector2 basePosition)
        {
            RestartUnityCoords(GameUtils.ToUnityCoordinates(basePosition));
        }

        public void Restart()
        {
            RestartUnityCoords(originalPosition);
        }

        public void Enable()
        {
            if (gameObject.activeSelf == false)
            {
                currentAnimateCoroutine = null;
            }

            gameObject.SetActive(true);
        }

        public void Disable()
        {
            StopAnimating();
            gameObject.SetActive(false);
        }

        public void SetEnableState(bool enabled)
        {
            if (!enabled)
            {
                StopAnimating();
            }

            gameObject.SetActive(enabled);
        }

        public void StopAnimating()
        {
            if (currentAnimateCoroutine != null)
            {
                StopCoroutine(currentAnimateCoroutine);
                currentAnimateCoroutine = null;
            }
        }

        private void Start()
        {
            if (currentAnimateCoroutine == null)
            {
                StartAnimatingIfNotExists();
            }

            started = true;
        }

        private void OnEnable()
        {
            if (!started)
            {
                return;
            }
            Restart();
        }

        private void StartAnimatingIfNotExists()
        {
            if (currentAnimateCoroutine != null)
            {
                return;
            }

            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }

            transform.localPosition = originalPosition;

            currentAnimateCoroutine = AnimateCoroutine();
            StartCoroutine(currentAnimateCoroutine);
        }
    }
}