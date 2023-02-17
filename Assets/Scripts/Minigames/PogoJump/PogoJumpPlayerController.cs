using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class PogoJumpPlayerController : MonoBehaviour
{
    private const int PlayerDrawLayer = 0;

    private List<SpriteAnimatorController> jumpLevelAnimControllers = new List<SpriteAnimatorController>();
    
    private AudioSource jumpSound;
    private SpriteAnimatorController currentJumpAnim;
    private PlayerInput playerInput;

    private int difficulty;
    private bool jumpTriggered;
    private bool jumpPressedThisFrame;
    private int currentJumpLevel;
    private string wonMinigameScriptPath;

    private void Awake()
    {
        jumpSound = GetComponent<AudioSource>();
        playerInput = GetComponent<PlayerInput>();
    }

    public void Setup(PogoJumpMinigameInfo minigameInfo, GameObject animationPrefab)
    {
        transform.localPosition = GameUtils.ToUnityCoordinates(minigameInfo.PlayerPosition);

        foreach (string animationPath in minigameInfo.JumpTierAnimations)
        {
            bool isFirst = jumpLevelAnimControllers.Count == 0;

            jumpLevelAnimControllers.Add(MinigameConstructUtils.InstantiateAndGet(animationPrefab, transform, animationPath,
                Vector2.zero, PlayerDrawLayer, allowLoop: false));

            if (isFirst)
            {
                jumpLevelAnimControllers.Last().Enable();
            } else
            {
                jumpLevelAnimControllers.Last().Done += OnJumpAnimationDone;
            }
        }

        currentJumpAnim = jumpLevelAnimControllers[0];

        difficulty = minigameInfo.Difficulty;
        currentJumpLevel = 0;
        jumpTriggered = false;
        wonMinigameScriptPath = minigameInfo.WonScript;

        jumpSound.clip = SoundManager.Instance.GetAudioClip(minigameInfo.JumpSoundPath);
        playerInput.enabled = true;
    }

    private void OnConfirmPressed()
    {
        if (jumpPressedThisFrame)
        {
            return;
        }

        bool passed = (difficulty == 0);

        if (difficulty > 0)
        {
            if (currentJumpAnim != null)
            {
                if (currentJumpAnim.CurrentFrame >= currentJumpAnim.TotalFrame - difficulty)
                {
                    passed = true;
                }
            }
        }

        if (passed)
        {
            jumpSound.Play();
        }

        if (currentJumpLevel == 0)
        {
            currentJumpLevel++;
            UpdateJumpAnimation();
        } else
        {
            jumpTriggered = passed;
            jumpPressedThisFrame = true;
        }
    }

    private void UpdateJumpAnimation()
    {
        if (currentJumpAnim != jumpLevelAnimControllers[currentJumpLevel])
        {
            currentJumpAnim.Disable();

            currentJumpAnim = jumpLevelAnimControllers[currentJumpLevel];
            currentJumpAnim.Enable();
        }
    }

    private void OnJumpAnimationDone(SpriteAnimatorController controller)
    {
        if (controller == jumpLevelAnimControllers.Last())
        {
            // Won the minigame
            playerInput.enabled = false;
            GameManager.Instance.LoadGadget(wonMinigameScriptPath);
        } else
        {
            if (jumpTriggered)
            {
                currentJumpLevel += 1;
            } else
            {
                currentJumpLevel = Math.Max(currentJumpLevel - 1, 0);
            }

            UpdateJumpAnimation();
        }

        jumpTriggered = false;
        jumpPressedThisFrame = false;
    }
}
