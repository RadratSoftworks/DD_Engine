using System;
using UnityEngine;
using UnityEngine.InputSystem;

using DDEngine.Utils;

namespace DDEngine.Minigame.ConstructionSite
{
    public class ConstructionSiteHarryController : MonoBehaviour
    {
        private const string AnimationPath = "ch3/animations/Harry_fly.anim";

        [SerializeField]
        private SpriteAnimatorController animationController;

        [SerializeField]
        private Vector2 flyVelocityCap = new Vector2(2.5f, 1.5f);

        private Rigidbody2D rigidBody;
        private Vector2 moveVector;

        private void RegOrUnregControl(bool register)
        {
            var flyActionMap = GameInputManager.Instance.FlyMinigameActionMap;

            InputAction leftPressed = flyActionMap.FindAction("Left Pressed");
            InputAction rightPressed = flyActionMap.FindAction("Right Pressed");
            InputAction upPressed = flyActionMap.FindAction("Up Pressed");
            InputAction downPressed = flyActionMap.FindAction("Down Pressed");
            InputAction joystickMoved = flyActionMap.FindAction("Joystick Moved");

            if (register)
            {
                leftPressed.performed += OnLeftPressed;
                rightPressed.performed += OnRightPressed;
                upPressed.performed += OnUpPressed;
                downPressed.performed += OnDownPressed;
                joystickMoved.performed += OnJoystickMoved;
            }
            else
            {
                leftPressed.performed -= OnLeftPressed;
                rightPressed.performed -= OnRightPressed;
                upPressed.performed -= OnUpPressed;
                downPressed.performed -= OnDownPressed;
                joystickMoved.performed -= OnJoystickMoved;
            }
        }

        private void Start()
        {
            RegOrUnregControl(true);
        }

        private void OnDestroy()
        {
            RegOrUnregControl(false);
        }

        public void Setup(Vector2 position)
        {
            rigidBody = GetComponent<Rigidbody2D>();

            transform.localPosition = GameUtils.ToUnityCoordinates(position);
            animationController.Setup(Vector2.zero, SpriteAnimatorController.SortOrderNotSet,
                AnimationPath, origin: new Vector2(0.5f, 0.5f));
        }

        private void FixedUpdate()
        {
            // Apply wind force
            rigidBody.AddForce(Vector2.right * 0.3f, ForceMode2D.Impulse);
            rigidBody.AddForce(moveVector, ForceMode2D.Impulse);

            rigidBody.velocity = new Vector2(
                Math.Clamp(rigidBody.velocity.x, -flyVelocityCap.x, flyVelocityCap.x),
                Math.Clamp(rigidBody.velocity.y, -flyVelocityCap.y, flyVelocityCap.y));

            moveVector = Vector2.zero;
        }

        public void OnLeftPressed(InputAction.CallbackContext context)
        {
            moveVector += Vector2.left;
        }

        public void OnRightPressed(InputAction.CallbackContext context)
        {
            moveVector += Vector2.right;
        }

        public void OnUpPressed(InputAction.CallbackContext context)
        {
            moveVector += Vector2.up;
        }

        public void OnDownPressed(InputAction.CallbackContext context)
        {
            moveVector += Vector2.down;
        }

        public void OnJoystickMoved(InputAction.CallbackContext context)
        {
            moveVector += context.ReadValue<Vector2>();
        }

        public void Kill()
        {
            rigidBody.simulated = false;
        }
    }
}