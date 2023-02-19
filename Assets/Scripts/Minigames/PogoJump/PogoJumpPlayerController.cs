using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class PogoJumpPlayerController : MonoBehaviour
{
    private const int PlayerDrawLayer = 0;
    private const string JumpConfirmPressedActionName = "Confirm Pressed";

    private List<SpriteAnimatorController> jumpLevelAnimControllers = new List<SpriteAnimatorController>();
    
    private AudioSource jumpSound;
    private SpriteAnimatorController currentJumpAnim;

    private int difficulty;
    private bool jumpTriggered;
    private bool jumpPressedThisFrame;
    private int currentJumpLevel;
    private string wonMinigameScriptPath;

    private void Awake()
    {
        jumpSound = GetComponent<AudioSource>();
    }

    private void Start()
    {
        var pogoJumpActionMap = GameInputManager.Instance.PogoJumpMinigameActionMap;

        InputAction confirmAction = pogoJumpActionMap.FindAction(JumpConfirmPressedActionName);
        confirmAction.performed += OnConfirmPressed;
    }

    private void UnbindInput()
    {
        var pogoJumpActionMap = GameInputManager.Instance.PogoJumpMinigameActionMap;

        InputAction confirmAction = pogoJumpActionMap.FindAction(JumpConfirmPressedActionName);
        confirmAction.performed -= OnConfirmPressed;
    }

    private void OnDestroy()
    {
        if (GameInputManager.Instance.PogoJumpMinigameActionMap.enabled)
        {
            UnbindInput();
        }
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
    }

    public void OnConfirmPressed(InputAction.CallbackContext context)
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
            UnbindInput();
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
